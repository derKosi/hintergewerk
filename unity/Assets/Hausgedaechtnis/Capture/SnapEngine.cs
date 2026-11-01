// Hausgedächtnis — SnapEngine: macht Handtracking-Präzision (1–2 cm Zittern) irrelevant.
// Reine Mathematik, kein SDK. Dreistufige Kette (03-interaktion.md §2):
//   1. Ebene: Punkt auf Wand-/Deckenebene projizieren
//   2. Raster: 5-cm-Einrasten in der Ebene
//   3. Magnet: bestehende Endpunkte/Anschlüsse ziehen den Punkt an
using System.Collections.Generic;
using UnityEngine;
using Hausgedächtnis.Core;

namespace Hausgedächtnis.Capture
{
    public class SnapResult
    {
        public Vector3 Point;
        public bool SnappedToPlane;
        public bool SnappedToGrid;
        public bool SnappedToMagnet;
        public string MagnetId; // Id des angezogenen Objekts, falls Magnet aktiv
    }

    public class SnapEngine
    {
        public float GridCm = 5f;        // Rasterweite in der Ebene
        public float MagnetRadiusCm = 8f; // Anziehungsradius für bestehende Endpunkte

        PlaneBasis basis = new PlaneBasis();

        /// <summary>
        /// Rohpunkt (z. B. Zeigefingerspitze) snappen. existingEndpoints: Weltpositionen
        /// bereits erfasster Endpunkte (für magnetisches Verbinden), mit optionaler Objekt-Id.
        /// </summary>
        public SnapResult Snap(Vector3 raw, WallPlane plane, IReadOnlyList<(Vector3 pos, string id)> existingEndpoints)
        {
            var res = new SnapResult();
            var p = raw;

            // 1) Auf Ebene projizieren
            var n = plane.Normal.normalized;
            float d = Vector3.Dot(p - plane.Point, n);
            p -= n * d;
            res.SnappedToPlane = Mathf.Abs(d) > 0.0005f;

            // 2) Raster in der Ebene
            basis.Update(plane.Point, n);
            float grid = Mathf.Max(0.01f, GridCm * 0.01f);
            float u = Mathf.Round(Vector3.Dot(p - plane.Point, basis.U) / grid) * grid;
            float v = Mathf.Round(Vector3.Dot(p - plane.Point, basis.V) / grid) * grid;
            var gridded = plane.Point + basis.U * u + basis.V * v;
            if ((gridded - p).sqrMagnitude > 1e-8f) res.SnappedToGrid = true;
            p = gridded;

            // 3) Magnetische Endpunkte (gewinnen über Raster)
            if (existingEndpoints != null)
            {
                float best = MagnetRadiusCm * 0.01f;
                best *= best; // sqrMagnitude-Vergleich
                Vector3 bestPos = p; string bestId = null;
                for (int i = 0; i < existingEndpoints.Count; i++)
                {
                    var sq = (existingEndpoints[i].pos - p).sqrMagnitude;
                    if (sq < best) { best = sq; bestPos = existingEndpoints[i].pos; bestId = existingEndpoints[i].id; }
                }
                if (bestId != null)
                {
                    p = bestPos;
                    res.SnappedToMagnet = true;
                    res.MagnetId = bestId;
                    res.SnappedToGrid = false;
                }
            }

            res.Point = p;
            return res;
        }

        /// <summary>Tiefe aus Zwei-Hand-Distanz (Tiefe setzen §1: Zwei-Hand-Distanz).</summary>
        public static float DepthFromHandDistance(Vector3 a, Vector3 b, float clampMaxCm = 30f) =>
            Mathf.Clamp((a - b).magnitude * 100f, 0f, clampMaxCm);

        /// <summary>Orthonormalbasis einer Ebene (stabil: bevorzugt Welt-X, fällt auf Welt-Z zurück).</summary>
        class PlaneBasis
        {
            public Vector3 U, V;

            public void Update(Vector3 point, Vector3 normal)
            {
                var n = normal.normalized;
                var refv = Mathf.Abs(n.x) > 0.9f ? Vector3.forward : Vector3.right;
                U = Vector3.Cross(n, refv).normalized;
                V = Vector3.Cross(n, U).normalized;
            }
        }
    }
}
