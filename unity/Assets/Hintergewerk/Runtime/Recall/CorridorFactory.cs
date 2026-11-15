// Hintergewerk — Korridore statt Haarlinien (03-interaktion.md §2.5).
// Baut ein leuchtendes Ribbon entlang der Polylinie, Breite = Sicherheitskorridor (± cm).
// Rein Unity (Mesh-API), kein SDK — Material wird vom Aufrufer zugewiesen (UI-Farbsystem).
using System.Collections.Generic;
using UnityEngine;

namespace Hintergewerk.Recall
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
                // Beidseitig sichtbar: beide Wicklungen ausgeben (Fix 08.11. — vorher nur eine
                // Wicklung; URP/Unlit culled Backfaces → Korridore von der Kameraseite unsichtbar,
                // Simulator-Beweis xr_ribbons.png). Unabhängig vom Material korrekt.
                tris.Add(baseIdx); tris.Add(baseIdx + 1); tris.Add(baseIdx + 2);
                tris.Add(baseIdx); tris.Add(baseIdx + 2); tris.Add(baseIdx + 3);
                tris.Add(baseIdx + 2); tris.Add(baseIdx + 1); tris.Add(baseIdx);
                tris.Add(baseIdx + 3); tris.Add(baseIdx + 2); tris.Add(baseIdx);
            }

            var mesh = new Mesh { name = "CorridorRibbon" };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Fertiges Overlay-GameObject (MeshFilter + MeshRenderer, Material vom Aufrufer).
        /// Vertices bekommen einen ~8 mm kamera-seitigen Versatz — sonst z-fightet das Ribbon
        /// koplanar mit der echten Wand (Simulator-Beweis 08.11: Korridore unsichtbar hinter
        /// dem Wand-Quad). Nur Anzeige-Offset, keine Datenänderung (ADR-0002 unberührt).</summary>
        public static GameObject CreateOverlay(string name, IReadOnlyList<Vector3> points,
                                                float halfWidthM, Vector3 planeNormal, Material material)
        {
            var go = new GameObject(name);
            var mf = go.AddComponent<MeshFilter>();
            var mesh = BuildRibbon(points, halfWidthM, planeNormal);
            // Flache Ribbons (0 Ausdehnung in Normalen-Richtung) werden vom XR-Frustum-Culling
            // fälschlich aussortiert (Beweis 08.11: isVisible=false trotz Sichtfeld). Bounds
            // minimal aufblähen — nur Culling-Betroffenheit, keine Geometrie-Änderung.
            var b = mesh.bounds;
            b.size += Vector3.one * 0.02f;
            mesh.bounds = b;
            mf.sharedMesh = mesh;
            var cam = Camera.main;
            go.transform.position = FacingOffset(points[0], planeNormal, cam != null ? cam.transform.position : (Vector3?)null);
            var mr = go.AddComponent<MeshRenderer>();
            if (material != null) mr.sharedMaterial = material;
            return go;
        }

        /// <summary>Signierter Anzeige-Versatz entlang der Ebenen-Normalen, zur Kamera gewandt.
        /// Ohne Kamera (EditMode-Test) zeigt der Versatz in Normalen-Richtung.</summary>
        public static Vector3 FacingOffset(Vector3 center, Vector3 normal, Vector3? cameraPos, float eps = 0.008f)
        {
            var n = normal.normalized;
            var sign = cameraPos.HasValue && Vector3.Dot(cameraPos.Value - center, n) < 0f ? -1f : 1f;
            return n * (sign * eps);
        }

        /// <summary>Ebenen-Normale fürs Band ansehen-korrigieren (Fix 15.11., Befund B): Das
        /// Capture erzeugt genau 2 Punkte → die Wand-Normale ist aus den Punkten nicht ableitbar.
        /// Bei senkrechten Leitungen fiel der Fallback auf Welt-X → das Band stand SENKRECHT von
        /// der Wand ab und war von vorn nur als Kante sichtbar (gerade der Bohrer-Szenen-Fall!).
        /// Für senkrechte Linien liegt die beste verfügbare Wandnähe in der horizontalen Richtung
        /// zur Kamera. Nicht-senkrechte Linien bleiben unverändert (Simulator-Beweis 08.11.).
        /// Reine Geometrie, ohne Kamera-Zugriff — unit-testbar.</summary>
        public static Vector3 NormalForView(IReadOnlyList<Vector3> pts, Vector3 planeNormal, Vector3? cameraPos)
        {
            if (pts == null || pts.Count < 2 || !cameraPos.HasValue) return planeNormal;
            var dir = pts[pts.Count - 1] - pts[0];
            if (dir.sqrMagnitude < 1e-8f) return planeNormal;
            if (Mathf.Abs(Vector3.Dot(dir.normalized, Vector3.up)) <= 0.9f) return planeNormal;
            var toCam = cameraPos.Value - pts[0];
            toCam.y = 0f;
            if (toCam.sqrMagnitude < 1e-6f) return planeNormal;
            return toCam.normalized;
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
