// Hintergewerk — SDK-Adapter: Scene API (Wandebenen). Kommt nach M1 an die Reihe.
// LIEGT AUSSERHALB der Runtime-asmdef (Assembly-CSharp).
// Skeleton: Fallback-Ebene (manuell kalibriert) funktioniert bereits; das Auslesen
// von OVRSceneAnchor/OVRSceneManager (Label == wall) kommt mit der Scene-API-Integration.
using System.Collections.Generic;
using UnityEngine;
using Hintergewerk.Core;

namespace Hintergewerk.Adapters
{
    public class SceneWallProvider : MonoBehaviour, IWallProvider
    {
        [SerializeField] Transform fallbackPlaneOrigin; // manuell kalibrierte Ebene (Testraum)
        [SerializeField] Vector3 fallbackPlaneNormal = Vector3.forward;

        readonly List<WallPlane> walls = new();

        public List<WallPlane> GetWalls()
        {
            // TODO(nach M1): OVRSceneAnchor/OVRSceneManager auslesen (Semantik-Label „wall")
            walls.Clear();
            if (fallbackPlaneOrigin != null)
                walls.Add(new WallPlane
                {
                    Point = fallbackPlaneOrigin.position,
                    Normal = fallbackPlaneNormal.normalized,
                    Label = "wall"
                });
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
