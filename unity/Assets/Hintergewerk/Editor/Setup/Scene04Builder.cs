// Hintergewerk — Szene 04_Recall bauen (M3): OVRCameraRig (Instanz aus Szene 03 —
// Hände inkl. AutoMapBones-Overrides wandern mit), Wand-Quad, Status-TextMesh,
// RecallController + ExportController + SceneSwitcher + RecallDemoSeeder (aus),
// Korridor-Material, Build-Liste [02, 03, 04, 01].
// Ausführen: Menü Hintergewerk → Szene 04 Recall bauen — oder per unity command script eval
// 'Hintergewerk.EditorTools.Scene04Builder.Build();' Idempotent (überschreibt die Szene).
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Hintergewerk.Recall;
using Hintergewerk.Export;
using Hintergewerk.UI;

// ObjectInspector lebt in Hintergewerk.UI (dort ist auch SceneSwitcher)

namespace Hintergewerk.EditorTools
{
    public static class Scene04Builder
    {
        const string ScenePath = "Assets/Hintergewerk/Scenes/04_Recall.unity";
        const string SourceScene = "Assets/Hintergewerk/Scenes/03_CaptureAutoTest.unity";
        const string MaterialPath = "Assets/Hintergewerk/Scenes/CorridorMaterial.mat";

        [MenuItem("Hintergewerk/Szene 04 Recall bauen")]
        public static void Build()
        {
            // ---- 1) Rig aus Szene 03 übernehmen (Hände mit Overrides kommen mit) ----
            var scene03 = EditorSceneManager.OpenScene(SourceScene, OpenSceneMode.Single);
            var rig = FindRigRoot(scene03);
            if (rig == null) { Debug.LogError("[Scene04] OVRCameraRig nicht in Szene 03 gefunden."); return; }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.MoveGameObjectToScene(rig, scene);
            EditorSceneManager.CloseScene(scene03, true);

            // ---- 2) Wand-Quad (Demo-/Testwand, normal zeigt zur Kamera) ----
            var wall = GameObject.CreatePrimitive(PrimitiveType.Quad);
            wall.name = "DemoWall";
            wall.transform.SetPositionAndRotation(new Vector3(0f, 1.1f, 0.5f), Quaternion.identity);
            wall.transform.localScale = new Vector3(3f, 2.2f, 1f);

            // ---- 3) Status-TextMesh unter der TrackingSpace (folgt dem Tracking-Origin) ----
            var tracking = rig.transform.Find("TrackingSpace");
            var textGo = new GameObject("StatusText");
            if (tracking != null)
            {
                textGo.transform.SetParent(tracking, false);
                textGo.transform.localPosition = new Vector3(0f, 1.45f, 1.2f);
            }
            var tm = textGo.AddComponent<TextMesh>();
            tm.fontSize = 100;
            tm.characterSize = 0.012f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;
            tm.text = "Recall";

            // ---- 4) Korridor-Material (Asset, damit der Shader im Build mitkommt) ----
            var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                mat = new Material(shader != null ? shader : Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, MaterialPath);
            }

            // ---- 5) Adapter + Controller ----
            var adapters = new GameObject("HGW_Adapters");
            var anchorProvider = AddAdapter(adapters, "Hintergewerk.Adapters.OvrAnchorProvider");
            var handSource = AddAdapter(adapters, "Hintergewerk.Adapters.OvrHandSource");
            if (anchorProvider == null || handSource == null) { Debug.LogError("[Scene04] Adapter fehlen — Abbruch."); return; }

            var recallGo = new GameObject("RecallController");
            var recall = recallGo.AddComponent<RecallController>();
            Wire(recall, "anchorProviderBehaviour", anchorProvider);
            Wire(recall, "handSourceBehaviour", handSource);
            Wire(recall, "corridorMaterial", mat);
            Wire(recall, "statusText", tm);
            SetString(recall, "roomId", "raum_1");

            var exportGo = new GameObject("ExportController");
            var export = exportGo.AddComponent<ExportController>();
            Wire(export, "anchorProviderBehaviour", anchorProvider);
            Wire(export, "handSourceBehaviour", handSource);
            Wire(export, "statusText", tm);
            SetString(export, "roomId", "raum_1");

            var seederGo = new GameObject("RecallDemoSeeder");
            var seeder = seederGo.AddComponent<RecallDemoSeeder>();
            Wire(seeder, "anchorProviderBehaviour", anchorProvider);
            SetString(seeder, "roomId", "raum_1");
            SetFloat(seeder, "wallZ", 0.5f);
            SetFloat(seeder, "anchorZ", -0.3f);
            seeder.enabled = false; // Gerät: AUS. Nur Simulator-Test aktivieren.

            var switcherGo = new GameObject("SceneSwitcher");
            var switcher = switcherGo.AddComponent<SceneSwitcher>();
            Wire(switcher, "handSourceBehaviour", handSource);

            var inspectorGo = new GameObject("ObjectInspector");
            var inspector = inspectorGo.AddComponent<ObjectInspector>();
            Wire(inspector, "anchorProviderBehaviour", anchorProvider);
            Wire(inspector, "handSourceBehaviour", handSource);
            Wire(inspector, "statusText", tm);
            SetString(inspector, "roomId", "raum_1");

            // ---- 6) Speichern + Build-Liste [02, 03, 04, 01] ----
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                "Assets/Hintergewerk/Scenes/02_AnkerTest.unity",
                "Assets/Hintergewerk/Scenes/03_CaptureAutoTest.unity",
                "Assets/Hintergewerk/Scenes/04_Recall.unity",
                "Assets/Hintergewerk/Scenes/01_Smoketest.unity",
            }.Select(p => new EditorBuildSettingsScene(p, true)).ToArray();

            Debug.Log("[Scene04] FERTIG: " + ScenePath + " · Build-Liste: 02, 03, 04, 01");
        }

        // ---------------- Helfer ----------------

        static Type FindTypeIn(string name)
        {
            foreach (var asm in new[] { "Assembly-CSharp", "Oculus.VR" })
            {
                var t = Type.GetType(name + ", " + asm);
                if (t != null) return t;
            }
            return null;
        }

        static GameObject FindRigRoot(UnityEngine.SceneManagement.Scene scene)
        {
            var rigType = FindTypeIn("OVRCameraRig");
            if (rigType == null) { Debug.LogError("[Scene04] OVRCameraRig-Typ nicht gefunden (Assembly-CSharp/Oculus.VR)."); return null; }
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.transform.parent != null) continue;
                if (root.GetComponent(rigType) != null) return root;
                // evtl. als PrefabInstance umbenannt — auch Kinder prüfen (Root-Position)
                var comp = root.GetComponentsInChildren(rigType, true).FirstOrDefault();
                if (comp != null) return root;
            }
            return null;
        }

        static Component AddAdapter(GameObject go, string typeName)
        {
            var t = FindTypeIn(typeName);
            if (t == null) { Debug.LogError("[Scene04] Adapter-Typ nicht gefunden: " + typeName); return null; }
            return go.AddComponent(t);
        }

        static void Wire(Component c, string field, UnityEngine.Object val)
        {
            var so = new SerializedObject(c);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogError($"[Scene04] Feld fehlt an {c.GetType().Name}: {field}"); return; }
            p.objectReferenceValue = val;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetString(Component c, string field, string val)
        {
            var so = new SerializedObject(c);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogError($"[Scene04] Feld fehlt an {c.GetType().Name}: {field}"); return; }
            p.stringValue = val;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetFloat(Component c, string field, float val)
        {
            var so = new SerializedObject(c);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogError($"[Scene04] Feld fehlt an {c.GetType().Name}: {field}"); return; }
            p.floatValue = val;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
