// Hintergewerk — SDK-Adapter: Scene API (Wandebenen).
// LIEGT AUSSERHALB der Runtime-asmdef (Assembly-CSharp).
// v207-API, gegen den SDK-Quellcode verifiziert (07.11.):
//   OVRSceneAnchor.GetSceneAnchors(List<OVRSceneAnchor>) — alle von OVRSceneManager instanzierten Anker
//   OVRSemanticClassification (Sibling-Komponente) — .Contains(label), Labels aus dem Room Setup
//   OVRSceneManager.Classification.WallFace = "WALL_FACE" (unsichtbare Wände sind ebenfalls WallFace)
//   OVRScenePlane.Width/Height/Offset — Flächenmaße der WallFace (D41: Grundriss-Kanten)
// Ohne Scene-Daten (Editor/XR-Simulator, oder Room Setup nicht gemacht): Fallback —
// ein ganzer IWallProvider (z. B. MockWallProvider-Raum für Simulator/Demo) oder die
// manuell kalibrierte Einzel-Ebene. Log meldet aktiv genutzte Quelle (kein stilles Fake).
// Ehrlichkeit: Der Adapter liefert nur, was das OS wirklich liefert — keine erfundenen Wände.
using System.Collections.Generic;
using UnityEngine;
using Hintergewerk.Core;

namespace Hintergewerk.Adapters
{
    public class SceneWallProvider : MonoBehaviour, IWallProvider
    {
        [SerializeField] MonoBehaviour fallbackWallProviderBehaviour; // ganzer Provider (z. B. Mock-Raum), optional
        [SerializeField] Transform fallbackPlaneOrigin; // manuell kalibrierte Ebene (Testraum), optional
        [SerializeField] Vector3 fallbackPlaneNormal = Vector3.forward;
        [Tooltip("Maße der Fallback-Ebene (Breite/Höhe in m); 0 = unbekannt → keine Umriss-Kante")]
        [SerializeField] Vector2 fallbackPlaneSize = Vector2.zero;
        [Tooltip("Debug: Anzahl Scene-Wände + Quelle ins Log (nur bei Änderung)")]
        [SerializeField] bool logWallCount;

        readonly List<OVRSceneAnchor> sceneAnchors = new();
        readonly List<WallPlane> walls = new();
        int lastWallCount = -1;
        string lastSource = "";
        IWallProvider fallbackProvider;

        public List<WallPlane> GetWalls()
        {
            walls.Clear();
            string source = "scene";

            OVRSceneAnchor.GetSceneAnchors(sceneAnchors);
            foreach (var a in sceneAnchors)
            {
                if (!a.IsTracked) continue;
                var sem = a.GetComponent<OVRSemanticClassification>();
                if (sem == null || !sem.Contains(OVRSceneManager.Classification.WallFace)) continue;
                // Scene-Planes: Ebene liegt lokal in XY, +Z ist die Normale (zeigt in den Raum)
                var t = a.transform;
                var plane = a.GetComponent<OVRScenePlane>(); // Größe für Grundriss-Kanten (D41)
                var wall = new WallPlane
                {
                    Point = plane != null
                        ? t.TransformPoint(new Vector3(plane.Offset.x, plane.Offset.y, 0f)) // Offset meist 0 (Pivot = Zentrum)
                        : t.position,
                    Normal = t.forward,
                    Label = "wall",
                    Size = plane != null && plane.Width > 0f && plane.Height > 0f
                        ? new Vector2(plane.Width, plane.Height)
                        : Vector2.zero // unbekannt → WallOutline leitet bewusst KEINE Kante ab
                };
                walls.Add(wall);
            }

            // Fallback 1: ganzer Provider (Simulator/Demo ohne Room Setup — z. B. Mock-Raum)
            if (walls.Count == 0 && fallbackWallProviderBehaviour != null)
            {
                fallbackProvider = fallbackWallProviderBehaviour as IWallProvider;
                if (fallbackProvider != null)
                {
                    walls.AddRange(fallbackProvider.GetWalls());
                    source = fallbackWallProviderBehaviour.name;
                }
            }

            // Fallback 2: kalibrierte Einzel-Ebene, solange kein Room Setup / keine Scene-Anker
            if (walls.Count == 0 && fallbackPlaneOrigin != null)
            {
                walls.Add(new WallPlane
                {
                    Point = fallbackPlaneOrigin.position,
                    Normal = fallbackPlaneNormal.normalized,
                    Label = "wall",
                    Size = fallbackPlaneSize
                });
                source = "fallbackPlane";
            }

            if (logWallCount && (walls.Count != lastWallCount || source != lastSource))
            {
                lastWallCount = walls.Count;
                lastSource = source;
                Debug.Log($"[SceneWallProvider] {walls.Count} Wände via {source} (Scene-API: {sceneAnchors.Count} Anker gesamt)");
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
