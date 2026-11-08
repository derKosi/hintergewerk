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

            var existing = GameObject.Find("BauplanPanel");
            var panelGo = existing != null ? existing : new GameObject("BauplanPanel");
            var panel = panelGo.GetComponent<BauplanPanel>() ?? panelGo.AddComponent<BauplanPanel>();

            var so = new SerializedObject(panel);
            Set(so, "anchorProviderBehaviour", anchorProvider);
            Set(so, "followTarget", leftHand);
            so.FindProperty("roomId").stringValue = roomId;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Bauplan] Verdrahtet + Szene gespeichert: Panel folgt linker Hand (Raum " + roomId + ").");
        }

        static void Set(SerializedObject so, string field, Object val)
        {
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogError("[Bauplan] Feld fehlt: " + field); return; }
            p.objectReferenceValue = val;
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
