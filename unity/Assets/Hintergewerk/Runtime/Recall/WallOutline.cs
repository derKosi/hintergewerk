// Hintergewerk — Wand-Grundriss (D41): aus Wand-Ebenen (Point/Normal/Size) werden
// Grundkanten (Bodenlinien) als Top-Down-Segmente (x/z) abgeleitet. Speist Bauplan-Panel
// und PDF (MapRenderer.wallSegments) — ohne Scene-Daten/Größenangabe kein Umriss
// (Ehrlichkeitsregel: die Ebene ist dann unendlich, Kanten würden wir nur erfinden).
// SDK-frei, rein rechnerisch (EditMode-testbar). D15 bleibt: Grundriss ist OUTPUT.
using System.Collections.Generic;
using UnityEngine;
using Hintergewerk.Core;

namespace Hintergewerk.Recall
{
    public static class WallOutline
    {
        /// <summary>
        /// Grundkanten der Wände als Segment-Paare (a, b) in Welt (x/z). Wände ohne
        /// Größe (Size == 0) und Ebenen ohne horizontale Richtung (Boden/Decke-Normalen)
        /// werden übersprungen — nichts erfunden.
        /// </summary>
        public static List<Vector2[]> FloorSegments(IReadOnlyList<WallPlane> walls)
        {
            var segments = new List<Vector2[]>();
            if (walls == null) return segments;

            foreach (var w in walls)
            {
                if (w.Size.x <= 0f || w.Size.y <= 0f) continue;          // Größe unbekannt → keine Kante
                var dir = Vector3.Cross(Vector3.up, w.Normal);           // horizontale Wandrichtung
                if (dir.sqrMagnitude < 1e-6f) continue;                  // Normale vertikal (Boden/Decke) → keine Grundkante
                dir.Normalize();

                var bottomCenter = w.Point - Vector3.up * (w.Size.y * 0.5f); // Flächenzentrum → Unterkante
                var a = bottomCenter - dir * (w.Size.x * 0.5f);
                var b = bottomCenter + dir * (w.Size.x * 0.5f);
                segments.Add(new[] { new Vector2(a.x, a.z), new Vector2(b.x, b.z) });
            }
            return segments;
        }

        /// <summary>
        /// Segmente filtern: nur die, die einem Objektpunkt näher als maxDistM sind
        /// (Abstand Punkt→Segment, x/z). Für Raum-PDFs ohne Raum-Wand-Zuordnung:
        /// Wände, die zu den dokumentierten Objekten passen. Keine Objektpunkte → leer
        /// (kein Raumbezug bekannt → kein Umriss im Raum-PDF).
        /// </summary>
        public static List<Vector2[]> FilterNearSegments(
            List<Vector2[]> segments, IReadOnlyList<Vector3> objectPoints, float maxDistM)
        {
            var kept = new List<Vector2[]>();
            if (segments == null || objectPoints == null || objectPoints.Count == 0) return kept;

            foreach (var s in segments)
            {
                float best = float.MaxValue;
                foreach (var p in objectPoints)
                    best = Mathf.Min(best, DistancePointSegment(new Vector2(p.x, p.z), s[0], s[1]));
                if (best <= maxDistM) kept.Add(s);
            }
            return kept;
        }

        /// <summary>Punkt-zu-Segment-Abstand in 2D (x/z).</summary>
        public static float DistancePointSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float lenSq = ab.sqrMagnitude;
            if (lenSq < 1e-10f) return Vector2.Distance(p, a); // entartetes Segment = Punkt
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lenSq);
            return Vector2.Distance(p, a + ab * t);
        }
    }
}
