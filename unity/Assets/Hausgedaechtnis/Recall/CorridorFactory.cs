// Hausgedächtnis — Korridore statt Haarlinien (03-interaktion.md §2.5).
// Baut ein leuchtendes Ribbon entlang der Polylinie, Breite = Sicherheitskorridor (± cm).
// Rein Unity (Mesh-API), kein SDK — Material wird vom Aufrufer zugewiesen (UI-Farbsystem).
using System.Collections.Generic;
using UnityEngine;

namespace Hausgedächtnis.Recall
{
    public static class CorridorFactory
    {
        /// <summary>
        /// Ribbon-Mesh entlang einer Polylinie in der Wandebene. halfWidthM = Korridor in Metern
        /// (z. B. 0.05 für ±5 cm). Segmente werden einzeln gequads — Ecken absichtlich offen,
        /// der Korridor soll sichtbar „großzügig" wirken, nicht millimeterpassgenau.
        /// </summary>
        public static Mesh BuildRibbon(IReadOnlyList<Vector3> points, float halfWidthM, Vector3 planeNormal)
        {
            if (points == null || points.Count < 2)
            {
                Debug.LogWarning("[Corridor] Polylinie mit <2 Punkten — leeres Mesh");
                return new Mesh();
            }

            var n = planeNormal.normalized;
            var verts = new List<Vector3>(points.Count * 4);
            var tris = new List<int>((points.Count - 1) * 6);

            for (int i = 0; i < points.Count - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                var dir = (b - a);
                if (dir.sqrMagnitude < 1e-8f) continue;
                dir.Normalize();

                // Enden um halbe Breite überstehen (Deckel)
                var a2 = a - dir * halfWidthM;
                var b2 = b + dir * halfWidthM;

                // In-Ebene-Senkrechte: liegt im Ribbon-Band
                var left = Vector3.Cross(dir, n).normalized * halfWidthM;

                int baseIdx = verts.Count;
                verts.Add(a2 - left); verts.Add(a2 + left);
                verts.Add(b2 + left); verts.Add(b2 - left);
                // Zwei Dreiecke (beidseitig sichtbar via Material, Reihenfolge egal)
                tris.Add(baseIdx); tris.Add(baseIdx + 1); tris.Add(baseIdx + 2);
                tris.Add(baseIdx); tris.Add(baseIdx + 2); tris.Add(baseIdx + 3);
            }

            var mesh = new Mesh { name = "CorridorRibbon" };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Fertiges Overlay-GameObject (MeshFilter + MeshRenderer, Material vom Aufrufer).</summary>
        public static GameObject CreateOverlay(string name, IReadOnlyList<Vector3> points,
                                                float halfWidthM, Vector3 planeNormal, Material material)
        {
            var go = new GameObject(name);
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = BuildRibbon(points, halfWidthM, planeNormal);
            var mr = go.AddComponent<MeshRenderer>();
            if (material != null) mr.sharedMaterial = material;
            return go;
        }

        /// <summary>Anker-lokale Punkte in Weltpositionen auflösen (Konvention §2 Architektur).</summary>
        public static List<Vector3> ToWorld(Data.TrackedObject o, Pose anchorPose)
        {
            var result = new List<Vector3>(o.geometry.Count);
            for (int i = 0; i < o.geometry.Count; i++)
            {
                var p = o.geometry.Point(i);
                result.Add(anchorPose.position + anchorPose.rotation * new Vector3((float)p[0], (float)p[1], (float)p[2]));
            }
            return result;
        }
    }
}
