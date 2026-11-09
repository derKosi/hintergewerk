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

        /// <summary>
        /// Rechteckiger Raum (4 Wände mit Maßen) — für Simulator/Demo, wenn kein Room Setup
        /// vorliegt. Origin = Raum-Eckpunkt (min x/z), y = Boden. Wände zeigen ins Innere.
        /// </summary>
        public static MockWallProvider Room(Vector3 originMin, float widthX, float depthZ, float heightY)
        {
            var h = Vector3.up * (heightY * 0.5f);
            var p = new MockWallProvider();
            p.Walls = new List<WallPlane>
            {
                // Süd (z = min), Normale +z (nach innen); Zentrum = Kantenmitte + halbe Höhe
                new WallPlane { Point = originMin + new Vector3(widthX * 0.5f, 0f, 0f) + h, Normal = Vector3.forward,  Label = "wall", Size = new Vector2(widthX, heightY) },
                // Nord (z = max), Normale −z
                new WallPlane { Point = originMin + new Vector3(widthX * 0.5f, 0f, depthZ) + h, Normal = Vector3.back,     Label = "wall", Size = new Vector2(widthX, heightY) },
                // West (x = min), Normale +x
                new WallPlane { Point = originMin + new Vector3(0f, 0f, depthZ * 0.5f) + h, Normal = Vector3.right,   Label = "wall", Size = new Vector2(depthZ, heightY) },
                // Ost (x = max), Normale −x
                new WallPlane { Point = originMin + new Vector3(widthX, 0f, depthZ * 0.5f) + h, Normal = Vector3.left,    Label = "wall", Size = new Vector2(depthZ, heightY) },
            };
            return p;
        }

        public List<WallPlane> GetWalls() => Walls;

        public bool TryFindPlane(Vector3 point, Vector3 direction, float maxDist, out WallPlane plane) =>
            TryFindPlane(point, direction, maxDist, Walls, out plane);

        /// <summary>Statische Variante — rechnet gegen die ÜBERGEBENE Liste (nie gegen einen
        /// veralteten Cache; Fix 09.11.: MockWallProviderBehaviour tauscht walls zur Laufzeit).</summary>
        public static bool TryFindPlane(Vector3 point, Vector3 direction, float maxDist,
            List<WallPlane> walls, out WallPlane plane)
        {
            plane = default;
            foreach (var w in walls)
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
