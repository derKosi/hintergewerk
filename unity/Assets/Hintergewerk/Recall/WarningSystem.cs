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
