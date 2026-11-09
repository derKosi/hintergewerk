// Hintergewerk — Bauplan-Panel in die offene Szene verdrahten (D39, ADR-0005).
// Idempotent: vorhandenes BauplanPanel wird neu verdrahtet, nicht dupliziert.
// Die Szene wird GESPEICHERT (Gerät-Konfiguration: Panel gehört zur Ausstattung).
// Aufruf: Menü Hintergewerk → Bauplan verdrahten (offene Szene) — oder script eval
// 'Hintergewerk.EditorTools.BauplanWiring.WireOpenScene();'
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Hintergewerk.Recall;

namespace Hintergewerk.EditorTools
{
    public static class BauplanWiring
    {
        [MenuItem("Hintergewerk/Bauplan verdrahten (offene Szene)")]
        public static void WireOpenScene() => Wire("raum_1");

        public static void Wire(string roomId)
        {
            var anchorProvider = FindAnchorProvider();
            if (anchorProvider == null)
            {
                Debug.LogError("[Bauplan] Kein OvrAnchorProvider gefunden (HGW_Adapters oder sonstwo in der Szene).");
                return;
            }

            // Linke Hand-Anchor: OVR-Rig-Konvention TrackingSpace/LeftHandAnchor
            var rig = GameObject.Find("OVRCameraRig");
            var leftHand = rig != null ? rig.transform.Find("TrackingSpace/LeftHandAnchor") : null;
            if (leftHand == null) { Debug.LogError("[Bauplan] TrackingSpace/LeftHandAnchor nicht gefunden."); return; }

            var wallProvider = FindOrCreateWallProvider();
            var existing = GameObject.Find("BauplanPanel");
            var panelGo = existing != null ? existing : new GameObject("BauplanPanel");
            var panel = panelGo.GetComponent<BauplanPanel>() ?? panelGo.AddComponent<BauplanPanel>();

            var so = new SerializedObject(panel);
            Set(so, "anchorProviderBehaviour", anchorProvider);
            Set(so, "wallProviderBehaviour", wallProvider); // D41: Raum-Umriss im Plan
            Set(so, "followTarget", leftHand);
            so.FindProperty("roomId").stringValue = roomId;
            so.ApplyModifiedPropertiesWithoutUndo();

            // ExportController (falls in der Szene) bekommt denselben Provider → Umriss im PDF
            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb == null || mb.GetType().Name != "ExportController") continue;
                var eso = new SerializedObject(mb);
                Set(eso, "wallProviderBehaviour", wallProvider);
                eso.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[Bauplan] ExportController verdrahtet (Umriss im Raum-PDF, D41).");
                break;
            }

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Bauplan] Verdrahtet + Szene gespeichert: Panel folgt linker Hand (Raum " + roomId + ").");
        }

        /// <summary>SceneWallProvider finden — oder erzeugen: Mock-Raum (3×3×2.6 m) als
        /// Simulator-Fallback hinter dem SceneWallProvider. Am Gerät mit Room Setup liefern
        /// die echten Scene-Wände (Fallback greift nicht); das Log meldet die Quelle.
        /// Die Mock-Wände liegen SERIALIZED am MockWallProviderBehaviour (überleben Save/Load).</summary>
        static Component FindOrCreateWallProvider()
        {
            Component sceneWalls = null;
            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (mb != null && mb.GetType().Name == "SceneWallProvider") { sceneWalls = mb; break; }

            bool created = false;
            if (sceneWalls == null)
            {
                var adapters = GameObject.Find("HGW_Adapters");
                var host = adapters != null ? adapters : new GameObject("HGW_Walls");
                var wallType = FindAdapterType("Hintergewerk.Adapters.SceneWallProvider");
                if (wallType == null) { Debug.LogError("[Bauplan] SceneWallProvider-Typ nicht gefunden."); return null; }
                sceneWalls = host.AddComponent(wallType);
                created = true;
            }

            // Mock-Fallback (Demo/Simulator): finden oder anlegen; Wände jedes Mal frisch
            // setzen (idempotent) — Raum so gelegt, dass die Nord-Wand auf die Demo-Wand passt.
            var mockGo = GameObject.Find("MockRaume_ohneRoomSetup");
            if (mockGo == null) mockGo = new GameObject("MockRaume_ohneRoomSetup");
            var mock = mockGo.GetComponent<Hintergewerk.Mocks.MockWallProviderBehaviour>()
                       ?? mockGo.AddComponent<Hintergewerk.Mocks.MockWallProviderBehaviour>();
            mock.walls = Hintergewerk.Mocks.MockWallProvider.Room(
                new Vector3(-1.5f, 0f, -2.5f), 3f, 3f, 2.6f).Walls;

            var wso = new SerializedObject(sceneWalls);
            Set(wso, "fallbackWallProviderBehaviour", mock);
            wso.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[Bauplan] SceneWallProvider " + (created ? "angelegt" : "gefunden") +
                      " + Mock-Raum als Fallback (3×3×2,6 m).");
            return sceneWalls;
        }


        static void Set(SerializedObject so, string field, Object val)
        {
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogError("[Bauplan] Feld fehlt: " + field); return; }
            p.objectReferenceValue = val;
        }

        static System.Type FindAdapterType(string name)
        {
            foreach (var asmName in new[] { "Assembly-CSharp" })
            {
                var t = System.Type.GetType(name + ", " + asmName);
                if (t != null) return t;
            }
            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (mb != null && mb.GetType().Name == name) return mb.GetType();
            return null;
        }

        /// <summary>OvrAnchorProvider suchen: bevorzugt an HGW_Adapters, sonst erste Instanz
        /// in der Szene (Szene 03 trägt ihn unter anderem Namen).</summary>
        static Component FindAnchorProvider()
        {
            var adapters = GameObject.Find("HGW_Adapters");
            if (adapters != null)
            {
                var c = adapters.GetComponent("OvrAnchorProvider");
                if (c != null) return c;
            }
            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (mb != null && mb.GetType().Name == "OvrAnchorProvider") return mb;
            return null;
        }
    }
}
#endif
