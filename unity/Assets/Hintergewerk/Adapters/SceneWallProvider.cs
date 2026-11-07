// Hintergewerk — SDK-Adapter: Scene API (Wandebenen).
// LIEGT AUSSERHALB der Runtime-asmdef (Assembly-CSharp).
// v207-API, gegen den SDK-Quellcode verifiziert (07.11.):
//   OVRSceneAnchor.GetSceneAnchors(List<OVRSceneAnchor>) — alle von OVRSceneManager instanzierten Anker
//   OVRSemanticClassification (Sibling-Komponente) — .Contains(label), Labels aus dem Room Setup
//   OVRSceneManager.Classification.WallFace = "WALL_FACE" (unsichtbare Wände sind ebenfalls WallFace)
// Ohne Scene-Daten (Editor/XR-Simulator, oder Room Setup nicht gemacht): Fallback-Ebene.
// Ehrlichkeit: Der Adapter liefert nur, was das OS wirklich liefert — keine erfundenen Wände.
using System.Collections.Generic;
using UnityEngine;
using Hintergewerk.Core;

namespace Hintergewerk.Adapters
{
    public class SceneWallProvider : MonoBehaviour, IWallProvider
    {
        [SerializeField] Transform fallbackPlaneOrigin; // manuell kalibrierte Ebene (Testraum), optional
        [SerializeField] Vector3 fallbackPlaneNormal = Vector3.forward;
        [Tooltip("Debug: Anzahl Scene-Wände ins Log (nur bei Änderung)")]
        [SerializeField] bool logWallCount;

        readonly List<OVRSceneAnchor> sceneAnchors = new();
        readonly List<WallPlane> walls = new();
        int lastWallCount = -1;

        public List<WallPlane> GetWalls()
        {
            walls.Clear();

            OVRSceneAnchor.GetSceneAnchors(sceneAnchors);
            foreach (var a in sceneAnchors)
            {
                if (!a.IsTracked) continue;
                var sem = a.GetComponent<OVRSemanticClassification>();
                if (sem == null || !sem.Contains(OVRSceneManager.Classification.WallFace)) continue;
                // Scene-Planes: Ebene liegt lokal in XY, +Z ist die Normale (zeigt in den Raum)
                var t = a.transform;
                walls.Add(new WallPlane
                {
                    Point = t.position,
                    Normal = t.forward,
                    Label = "wall"
                });
            }

            // Fallback: kalibrierte Ebene, solange kein Room Setup / keine Scene-Anker da sind
            if (walls.Count == 0 && fallbackPlaneOrigin != null)
                walls.Add(new WallPlane
                {
                    Point = fallbackPlaneOrigin.position,
                    Normal = fallbackPlaneNormal.normalized,
                    Label = "wall"
                });

            if (logWallCount && walls.Count != lastWallCount)
            {
                lastWallCount = walls.Count;
                Debug.Log($"[SceneWallProvider] {walls.Count} Wände (Scene-API: {sceneAnchors.Count} Anker gesamt)");
            }
            return walls;
        }

        public bool TryFindPlane(Vector3 point, Vector3 direction, float maxDist, out WallPlane plane)
        {
            plane = default;
            foreach (var w in GetWalls())
            {
                float denom = Vector3.Dot(w.Normal, direction);
                if (Mathf.Abs(denom) < 1e-5f) continue;
                float t = Vector3.Dot(w.Point - point, w.Normal) / denom;
                if (t < 0f || t > maxDist) continue;
                plane = w;
                return true;
            }
            return false;
        }
    }
}
