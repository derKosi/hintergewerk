// Hintergewerk — Autotest-Vorbereitung für Szene 04 (Recall): FakeHandSource (RecallApproach)
// einklinken, Seeder aktivieren — und danach alles zurücksetzen. Nur für den Editor-Test;
// die Szene wird danach WIEDER GESPEICHERT (Gerät-Konfiguration: echter Adapter, Seeder aus).
// Aufruf: script eval 'Hintergewerk.EditorTools.RecallAutotest.Prepare();'  …  Cleanup();
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Hintergewerk.Recall;
using Hintergewerk.Export;
using Hintergewerk.UI;

namespace Hintergewerk.EditorTools
{
    public static class RecallAutotest
    {
        public static void Prepare()
        {
            // Store-Reset für wiederholbare Läufe (nur Editor-Testdaten des Simulators):
            var dataDir = Application.persistentDataPath;
            foreach (var f in new[] { "hintergewerk.json", "anker_map.json" })
            {
                var p = System.IO.Path.Combine(dataDir, f);
                if (System.IO.File.Exists(p)) System.IO.File.Delete(p);
            }
            Debug.Log("[RecallAutotest] Store-Reset: " + dataDir);

            var fakeGo = new GameObject("FakeHands");
            var fakeType = Type.GetType("Hintergewerk.Adapters.FakeHandSource, Assembly-CSharp");
            var fake = fakeGo.AddComponent(fakeType) as MonoBehaviour;
            var fso = new SerializedObject(fake);
            fso.FindProperty("scenario").stringValue = "RecallApproach";
            fso.ApplyModifiedPropertiesWithoutUndo();

            Rewire("RecallController", "handSourceBehaviour", fake);
            Rewire("ExportController", "handSourceBehaviour", fake);
            Rewire("SceneSwitcher", "handSourceBehaviour", fake);

            var seeder = GameObject.Find("RecallDemoSeeder")?.GetComponent<RecallDemoSeeder>();
            if (seeder == null) { Debug.LogError("[RecallAutotest] Seeder nicht gefunden."); return; }
            seeder.enabled = true;
            Debug.Log("[RecallAutotest] Vorbereitet: FakeHands (RecallApproach) + Seeder an.");
        }

        public static void Cleanup()
        {
            var adapters = GameObject.Find("HGW_Adapters");
            var handType = Type.GetType("Hintergewerk.Adapters.OvrHandSource, Assembly-CSharp");
            Component real = adapters != null && handType != null ? adapters.GetComponent(handType) : null;
            if (real == null) { Debug.LogError("[RecallAutotest] OvrHandSource an HGW_Adapters nicht gefunden."); return; }

            Rewire("RecallController", "handSourceBehaviour", real);
            Rewire("ExportController", "handSourceBehaviour", real);
            Rewire("SceneSwitcher", "handSourceBehaviour", real);

            var seederGo = GameObject.Find("RecallDemoSeeder");
            if (seederGo != null) seederGo.GetComponent<RecallDemoSeeder>().enabled = false;

            var fake = GameObject.Find("FakeHands");
            if (fake != null) UnityEngine.Object.DestroyImmediate(fake);

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[RecallAutotest] Aufgeräumt + Szene gespeichert (Gerät-Konfiguration).");
        }

        static void Rewire(string gameObjectName, string field, UnityEngine.Object value)
        {
            var go = GameObject.Find(gameObjectName);
            if (go == null) { Debug.LogError("[RecallAutotest] GameObject fehlt: " + gameObjectName); return; }
            Component c = gameObjectName switch
            {
                "RecallController" => go.GetComponent<RecallController>(),
                "ExportController" => go.GetComponent<ExportController>(),
                "SceneSwitcher" => go.GetComponent<SceneSwitcher>(),
                _ => null
            };
            if (c == null) { Debug.LogError("[RecallAutotest] Komponente fehlt an " + gameObjectName); return; }
            var so = new SerializedObject(c);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogError("[RecallAutotest] Feld fehlt: " + field); return; }
            p.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
