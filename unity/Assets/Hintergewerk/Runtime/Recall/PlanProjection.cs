// Hintergewerk — Bauplan-Projektion (D39, ADR-0005): Welt (x/z, Top-Down) → Panel-lokal.
// SDK-frei, rein rechnerisch (testbar). Der Bauplan ist OUTPUT — dieselbe Top-Down-Idee
// wie der MapRenderer (PDF), nur live statt auf Papier.
using System.Collections.Generic;
using UnityEngine;

namespace Hintergewerk.Recall
{
    /// <summary>Einheitliche Skalierung+Verschiebung für die Plan-Projektion eines Raums.</summary>
    public struct PlanTransform
    {
        public float Scale;      // Meter Welt → Meter Panel (uniform, kein Aspekt-Verzug)
        public Vector2 Center;   // Welt-Bounds-Zentrum (x/z)
        public Vector2 PanelSize;// Panel-Größe in Metern (x = Breite, y = Höhe)

        public Vector2 ToLocal(Vector3 world)
        {
            return new Vector2(
                (world.x - Center.x) * Scale + PanelSize.x * 0.5f,
                (world.z - Center.y) * Scale + PanelSize.y * 0.5f);
        }
    }

    public static class PlanProjection
    {
        /// <summary>Passt die Welt-Punkte (Top-Down x/z) mit Rand in das Panel ein.</summary>
        public static PlanTransform Fit(IReadOnlyList<Vector3> worldPoints, Vector2 panelSize, float paddingM = 0.02f)
        {
            var t = new PlanTransform { PanelSize = panelSize, Scale = 0f, Center = Vector2.zero };
            if (worldPoints == null || worldPoints.Count == 0)
            {
                t.Scale = 1f; // Leerer Plan: neutrale Skalierung (1 m Welt = 1 m Panel)
                return t;
            }

            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            foreach (var p in worldPoints)
            {
                if (p.x < min.x) min.x = p.x; if (p.x > max.x) max.x = p.x;
                if (p.z < min.y) min.y = p.z; if (p.z > max.y) max.y = p.z;
            }

            // Ein Punkt: fester, ruhiger Maßstab (kein Springen beim ersten Objekt)
            var span = Vector2.Max(max - min, new Vector2(0.5f, 0.5f)) + new Vector2(paddingM * 2f, paddingM * 2f);
            t.Center = (min + max) * 0.5f;
            t.Scale = Mathf.Min((panelSize.x - paddingM * 2f) / span.x, (panelSize.y - paddingM * 2f) / span.y);
            return t;
        }
    }
}
