// Hintergewerk — Bauplan-Panel in die offene Szene verdrahten (D39, ADR-0005).
// Idempotent: vorhandenes BauplanPanel wird neu verdrahtet, nicht dupliziert.
// Die Szene wird GESPEICHERT (Gerät-Konfiguration: Panel gehört zur Ausstattung).
// Aufruf: Menü Hintergewerk → Bauplan verdrahten (offene Szene) — oder script eval
// 'Hintergewerk.EditorTools.BauplanWiring.WireOpenScene();'
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Hintergewerk.Recall;

namespace Hintergewerk.EditorTools
{
    public static class BauplanWiring
    {
        [MenuItem("Hintergewerk/Bauplan verdrahten (offene Szene)")]
        public static void WireOpenScene() => Wire(null);

        /// <summary>roomId null/leer = bestehenden Wert des Panels BEHALTEN (Szene 04 läuft auf
        /// „alle Räume" — Fix 09.11.: das harte „raum_1" hatte das zurückgesetzt).</summary>
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
            EnsureSceneManager(wallProvider);

            // DrawController (Capture-Szenen) auf DENSELBEN Provider setzen, wenn leer —
            // Snap-Wände = Plan-Wände (Konsistenz); am Gerät echte Scene-Wände (D36).
            var dcGo = GameObject.Find("DrawController");
            if (dcGo != null)
            {
                var dcComp = dcGo.GetComponent("DrawController");
                if (dcComp != null)
                {
                    var dso = new SerializedObject(dcComp);
                    var dwp = dso.FindProperty("wallProviderBehaviour");
                    if (dwp != null && dwp.objectReferenceValue == null)
                    {
                        dwp.objectReferenceValue = wallProvider;
                        dso.ApplyModifiedPropertiesWithoutUndo();
                        Debug.Log("[Bauplan] DrawController auf Wall-Provider gesetzt (Snap = Plan-Wände).");
                    }
                }
            }
            var existing = GameObject.Find("BauplanPanel");
            var panelGo = existing != null ? existing : new GameObject("BauplanPanel");
            var panel = panelGo.GetComponent<BauplanPanel>() ?? panelGo.AddComponent<BauplanPanel>();

            var so = new SerializedObject(panel);
            Set(so, "anchorProviderBehaviour", anchorProvider);
            Set(so, "wallProviderBehaviour", wallProvider); // D41: Raum-Umriss im Plan
            Set(so, "followTarget", leftHand);
            if (!string.IsNullOrEmpty(roomId))
                so.FindProperty("roomId").stringValue = roomId;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("[Bauplan] roomId=" + so.FindProperty("roomId").stringValue + " ('' = alle Räume)");

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
            wso.FindProperty("logWallCount").boolValue = true; // Spur für Gerätetest: Quelle im Log
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

        /// <summary>OVRSceneAnchor entstehen NUR durch einen OVRSceneManager (der instanziiert
        /// die Scene-Anker der Session als GameObjects) — Fix 09.11.: Ohne ihn blieb GetSceneAnchors
        /// leer und der Plan fiel immer in den Mock-Fallback, auch am Gerät mit Room Setup.
        /// Der SceneManager wird an den Wall-Provider gehängt (SceneWallProvider am selben GO).
        /// Am Gerät fragt er beim ersten Start ggf. den System-Scene-Capture-Dialog ab — das ist
        /// der D16-Flow (Onboarding über OS-Room-Setup).</summary>
        static void EnsureSceneManager(Component wallProvider)
        {
            var smType = System.Type.GetType("OVRSceneManager, Oculus.VR");
            if (smType == null) { Debug.LogError("[Bauplan] OVRSceneManager-Typ nicht gefunden (Oculus.VR)."); return; }

            var host = wallProvider != null ? wallProvider.gameObject : null;
            if (host == null) { Debug.LogError("[Bauplan] Kein Host für den SceneManager."); return; }

            if (Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .Any(mb => mb != null && mb.GetType() == smType))
            { Debug.Log("[Bauplan] OVRSceneManager vorhanden."); return; }

            host.AddComponent(smType);
            Debug.Log("[Bauplan] OVRSceneManager an " + host.name + " gesetzt (instanziiert die Scene-Anker).");
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
