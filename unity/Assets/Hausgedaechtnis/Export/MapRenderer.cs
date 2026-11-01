// Hausgedächtnis — MapRenderer: Grundriss als EXPORT (nicht Input, D15).
// Projiziert Weltpunkte top-down (x/z) auf A4, zeichnet Wände, Objekte mit Korridor-Halo,
// Maßstab-Balken, Legende, Titel. Speist den PdfEmitter. Das ist der Teil, den ein
// Bauherr / die nächste Firma tatsächlich annimmt.
using System;
using System.Collections.Generic;
using UnityEngine;
using Hausgedächtnis.Data;

namespace Hausgedächtnis.Export
{
    public class MappedObject
    {
        public ObjectType Type;
        public List<Vector3> WorldPoints = new List<Vector3>();
        public float DepthCm;
        public Confidence Confidence;
        public float CorridorCm = 5f;
        public string Id;
    }

    public static class MapRenderer
    {
        // Seitenlayout (A4 hoch, Punkte)
        const float MarginL = 42f, MarginR = 42f, MarginT = 70f, MarginB = 170f;

        public static readonly Dictionary<ObjectType, Color> TypeColors = new Dictionary<ObjectType, Color>
        {
            { ObjectType.Power,      new Color(0.82f, 0.16f, 0.16f) },
            { ObjectType.Water,      new Color(0.13f, 0.32f, 0.82f) },
            { ObjectType.Heating,    new Color(0.92f, 0.52f, 0.10f) },
            { ObjectType.Stud,       new Color(0.45f, 0.33f, 0.20f) },
            { ObjectType.NoDrillZone,new Color(0.68f, 0.12f, 0.68f) },
        };

        /// <summary>
        /// Baut das PdfDoc für einen Raum. wallOutline: geschlossenes Polygon in Meter (x/z),
        /// darf null sein (dann nur Rahmen). Rückgabe: scale = Punkte pro Meter für den Balken.
        /// </summary>
        public static PdfDoc BuildDoc(string roomLabel, string author, List<Vector2> wallOutline,
                                      List<MappedObject> objects, out float scale)
        {
            var doc = new PdfDoc();

            // ---- Bounds über Wände + Objekte ----
            float minx = float.MaxValue, maxx = float.MinValue, minz = float.MaxValue, maxz = float.MinValue;
            void Ext(Vector2 p) { minx = Mathf.Min(minx, p.x); maxx = Mathf.Max(maxx, p.x); minz = Mathf.Min(minz, p.y); maxz = Mathf.Max(maxz, p.y); }
            if (wallOutline != null) foreach (var p in wallOutline) Ext(p);
            if (objects != null) foreach (var o in objects) foreach (var p in o.WorldPoints) Ext(new Vector2(p.x, p.z));
            if (minx > maxx) { minx = 0; maxx = 3; minz = 0; maxz = 3; } // leerer Raum

            float aw = doc.Width - MarginL - MarginR;
            float ah = doc.Height - MarginT - MarginB;
            float w = Mathf.Max(maxx - minx, 0.01f), d = Mathf.Max(maxz - minz, 0.01f);
            scale = Mathf.Min(aw / w, ah / d);

            Vector2 M(Vector3 p) => new Vector2(MarginL + (p.x - minx) * scale, MarginB + (p.z - minz) * scale);

            // ---- Wände (geschlossener Zug) ----
            var wallCol = new Color(0.25f, 0.25f, 0.25f);
            if (wallOutline != null && wallOutline.Count >= 2)
            {
                for (int i = 0; i < wallOutline.Count; i++)
                {
                    var a = wallOutline[i];
                    var b = wallOutline[(i + 1) % wallOutline.Count];
                    Line(doc, MarginL + (a.x - minx) * scale, MarginB + (a.y - minz) * scale,
                                MarginL + (b.x - minx) * scale, MarginB + (b.y - minz) * scale, 1.4f, wallCol);
                }
            }
            else
            {
                // Fallback: Zeichnungsrahmen
                Line(doc, MarginL, MarginB, doc.Width - MarginR, MarginB, 0.5f, wallCol);
                Line(doc, doc.Width - MarginR, MarginB, doc.Width - MarginR, doc.Height - MarginT, 0.5f, wallCol);
                Line(doc, doc.Width - MarginR, doc.Height - MarginT, MarginL, doc.Height - MarginT, 0.5f, wallCol);
                Line(doc, MarginL, doc.Height - MarginT, MarginL, MarginB, 0.5f, wallCol);
            }

            // ---- Objekte: Korridor-Halo (breit, hell) + Mittellinie (dünn, kräftig) + Endpunkte ----
            if (objects != null)
            {
                foreach (var o in objects)
                {
                    if (o.WorldPoints == null || o.WorldPoints.Count < 2) continue;
                    var c = TypeColors.TryGetValue(o.Type, out var col) ? col : Color.magenta;
                    var halo = new Color(c.r * 0.4f + 0.6f, c.g * 0.4f + 0.6f, c.b * 0.4f + 0.6f);
                    float haloW = Mathf.Max(2f * o.CorridorCm * 0.01f * scale, 2.5f);

                    for (int i = 0; i < o.WorldPoints.Count - 1; i++)
                    {
                        var a = M(o.WorldPoints[i]);
                        var b = M(o.WorldPoints[i + 1]);
                        Line(doc, a.x, a.y, b.x, b.y, haloW, halo);
                    }
                    for (int i = 0; i < o.WorldPoints.Count - 1; i++)
                    {
                        var a = M(o.WorldPoints[i]);
                        var b = M(o.WorldPoints[i + 1]);
                        Line(doc, a.x, a.y, b.x, b.y, 1.1f, c);
                    }
                    foreach (var p in o.WorldPoints)
                    {
                        var q = M(p);
                        Cross(doc, q, 2.6f, c);
                    }

                    // Label am Mittelpunkt: Typ-Kürzel + Tiefe (+Konfidenz-Marker)
                    var mid = M(o.WorldPoints[0]);
                    Text(doc, mid.x + 4, mid.y + 4, 6.5f, $"{Short(o.Type)} {o.DepthCm:0.#}cm{ConfMark(o.Confidence)}", c);
                }
            }

            // ---- Maßstab-Balken (1 m) ----
            float barY = MarginB - 16f;
            Line(doc, MarginL, barY, MarginL + scale, barY, 1.2f, Color.black);
            Line(doc, MarginL, barY - 3f, MarginL, barY + 3f, 1f, Color.black);
            Line(doc, MarginL + scale, barY - 3f, MarginL + scale, barY + 3f, 1f, Color.black);
            Text(doc, MarginL + scale + 6, barY - 2f, 7.5f, "1 m", Color.black);

            // ---- Titel ----
            Text(doc, MarginL, doc.Height - 34f, 15f, "Hausgedaechtnis — " + (roomLabel ?? "Raum"), true, Color.black);
            Text(doc, MarginL, doc.Height - 50f, 8.5f,
                 $"Grundriss mit dokumentierten Objekten · erfasst von {author ?? "unbekannt"} · {DateTime.UtcNow:yyyy-MM-dd} · Schema v{Schema.Version}",
                 Color.gray);

            // ---- Legende ----
            float ly = MarginB - 44f;
            Text(doc, MarginL, ly + 14f, 9f, "Legende", true, Color.black);
            int shown = 0;
            var counts = new Dictionary<ObjectType, int>();
            if (objects != null) foreach (var o in objects) counts[o.Type] = counts.TryGetValue(o.Type, out var n) ? n + 1 : 1;
            foreach (var kv in counts)
            {
                float y = ly - shown * 13f;
                var c = TypeColors.TryGetValue(kv.Key, out var col) ? col : Color.magenta;
                Line(doc, MarginL, y + 3f, MarginL + 16f, y + 3f, 3f, c);
                Text(doc, MarginL + 22f, y, 8f, $"{kv.Key.Label()} ({kv.Value})", Color.black);
                shown++;
            }
            Text(doc, MarginL + 180f, ly + 14f - 0f, 8f, "Konfidenz:", true, Color.black);
            Text(doc, MarginL + 180f, ly + 1f, 8f, "(m) = gemessen", Color.black);
            Text(doc, MarginL + 180f, ly - 12f, 8f, "kein Marker = geschaetzt", Color.black);
            Text(doc, MarginL + 180f, ly - 25f, 8f, "breite Bahn = Sicherheitskorridor", Color.black);
            Text(doc, MarginL, 20f, 7f,
                 "Dieses Dokument zeigt ausschliesslich erfasste Objekte. Nicht dargestellte Bereiche sind ungeprueft — nicht frei von Leitungen.",
                 Color.gray);

            return doc;
        }

        public static byte[] EmitPdf(PdfDoc doc) => PdfEmitter.Emit(doc);

        // ---- Helpers ----

        static void Line(PdfDoc d, float x1, float y1, float x2, float y2, float width, Color c) =>
            d.Lines.Add(new PdfLine { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Width = width, R = c.r, G = c.g, B = c.b });

        static void Text(PdfDoc d, float x, float y, float size, string s, Color c, bool bold = false) =>
            d.Texts.Add(new PdfText { X = x, Y = y, Size = size, S = s, Bold = bold, R = c.r, G = c.g, B = c.b });

        static void Text(PdfDoc d, float x, float y, float size, string s, bool bold, Color c) =>
            Text(d, x, y, size, s, c, bold);

        static void Cross(PdfDoc d, Vector2 p, float r, Color c)
        {
            Line(d, p.x - r, p.y - r, p.x + r, p.y + r, 0.8f, c);
            Line(d, p.x - r, p.y + r, p.x + r, p.y - r, 0.8f, c);
        }

        static string Short(ObjectType t) => t switch
        {
            ObjectType.Power => "E", ObjectType.Water => "W", ObjectType.Heating => "H",
            ObjectType.Stud => "S", _ => "X"
        };

        static string ConfMark(Confidence c) => c == Confidence.Measured ? " (m)" : "";
    }
}
