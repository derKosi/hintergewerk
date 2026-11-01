// Hintergewerk — Wand-Ebenen aus dem Mock (Editor-Iteration ohne Scene API).
using System.Collections.Generic;
using UnityEngine;
using Hintergewerk.Core;

namespace Hintergewerk.Mocks
{
    public class MockWallProvider : IWallProvider
    {
        public List<WallPlane> Walls = new()
        {
            new WallPlane { Point = Vector3.zero, Normal = Vector3.forward, Label = "wall" }
        };

        public List<WallPlane> GetWalls() => Walls;

        public bool TryFindPlane(Vector3 point, Vector3 direction, float maxDist, out WallPlane plane)
        {
            plane = default;
            foreach (var w in Walls)
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
