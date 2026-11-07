// Hintergewerk — Warnsystem: Hand nähert sich Korridor → Eskalation.
// Reine Distanzmathematik (Punkt↔Polylinie), der SDK-Adapter füttert nur Handpositionen.
// FoV-aware-Erweiterung (Rand-Glow + räumlicher Audio-Ping) sitzt im UI-Adapter auf.
using System.Collections.Generic;
using UnityEngine;

namespace Hintergewerk.Recall
{
    public enum WarningLevel { None, Near, Critical }

    public static class WarningMath
    {
        /// <summary>Minimaler Abstand Punkt↔Polylinie (Meter) + Index des nächsten Segments.</summary>
        public static float DistanceToPolyline(Vector3 p, IReadOnlyList<Vector3> line, out int segIdx)
        {
            segIdx = -1;
            float best = float.MaxValue;
            if (line == null || line.Count < 2) return best;
            for (int i = 0; i < line.Count - 1; i++)
            {
                var d = DistanceToSegment(p, line[i], line[i + 1]);
                if (d < best) { best = d; segIdx = i; }
            }
            return best;
        }

        public static float DistanceToSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            var ab = b - a;
            float lenSq = ab.sqrMagnitude;
            if (lenSq < 1e-10f) return Vector3.Distance(p, a);
            float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / lenSq);
            return Vector3.Distance(p, a + ab * t);
        }

        /// <summary>
        /// Minimale Distanz Strahl ↔ Polylinie (Meter) — für „Blick + Pinch" (docs/03 §1:
        /// Objekt auswählen). Strahl = Blickrichtung; Segment-Parameter wird geclampt (kein Hinternauswahl).
        /// </summary>
        public static float DistanceRayToPolyline(Vector3 rayOrigin, Vector3 rayDir, IReadOnlyList<Vector3> line, float maxDist)
        {
            if (line == null || line.Count < 2) return float.MaxValue;
            var d = rayDir.normalized;
            float best = float.MaxValue;
            for (int i = 0; i < line.Count - 1; i++)
            {
                var a = line[i];
                var b = line[i + 1];
                var seg = b - a;
                float C = seg.sqrMagnitude;
                if (C < 1e-10f) continue;

                // Geschlossene Lösung Strahl↔Linie (Ericson), dann s clampen und t nachlösen:
                var w0 = rayOrigin - a;
                float B = Vector3.Dot(d, seg);
                float D = Vector3.Dot(d, w0);
                float E = Vector3.Dot(seg, w0);
                float denom = C - B * B;                // A = 1 (normalisiert)
                float s, t;
                if (denom < 1e-10f)
                {
                    s = 0f;                             // parallel: Strahlstart ist der Kandidat
                    t = Mathf.Clamp01(E / C);
                }
                else
                {
                    s = (B * E - C * D) / denom;
                    t = (E - B * D) / denom;
                    if (s < 0f)
                    {
                        s = 0f;                         // nur vor dem Blick
                        t = Mathf.Clamp01(E / C);       // t für s=0 nachlösen
                    }
                }
                t = Mathf.Clamp01(t);
                var pRay = rayOrigin + d * s;
                var pSeg = a + seg * t;
                float dist = Vector3.Distance(pRay, pSeg);
                if (s <= maxDist && dist < best) best = dist;
            }
            return best;
        }
    }

    public class WarningSystem
    {
        public float NearCm = 15f;     // gelb: Hand im Warnbereich
        public float CriticalCm = 7f;  // rot: Stopp!

        /// <summary>
        /// Bewertet eine Handposition gegen alle Korridore. Rückgabe: schlimmster Level
        /// + Id des nächsten Objekts (für Detail-Panel/Hervorhebung).
        /// </summary>
        public WarningLevel Evaluate(Vector3 handPos,
                                     IReadOnlyList<(string id, IReadOnlyList<Vector3> line)> corridors,
                                     out string nearestId, out float nearestCm)
        {
            nearestId = null;
            nearestCm = float.MaxValue;
            var level = WarningLevel.None;

            if (corridors == null) return level;

            for (int i = 0; i < corridors.Count; i++)
            {
                float cm = WarningMath.DistanceToPolyline(handPos, corridors[i].line, out _) * 100f;
                if (cm < nearestCm) { nearestCm = cm; nearestId = corridors[i].id; }
            }

            if (nearestCm <= CriticalCm) level = WarningLevel.Critical;
            else if (nearestCm <= NearCm) level = WarningLevel.Near;
            return level;
        }
    }
}
