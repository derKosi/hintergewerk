// Hintergewerk — Detail-Panel-Text (docs/03 §1: „Highlight + Detail-Panel (wer, wann, Tiefe, Foto)").
// Rein statisch + testbar. Ehrlichkeitsregel: Konfidenz IST IMMER sichtbar; fehlendes Foto
// wird benannt, nicht versteckt; verwaiste Objekte zeigen ihren Zustand.
using System;
using System.Globalization;
using Hintergewerk.Data;
using Hintergewerk.Localization;

namespace Hintergewerk.UI
{
    public static class InspectorPanel
    {
        /// <summary>Mehrzeiliger Paneltext für ein Objekt (ohne HTML/Farbcodes — TextMesh-roh).</summary>
        public static string Format(TrackedObject o)
        {
            if (o == null) return Strings.Get("panel.not_found");

            var type = Strings.Get("type." + o.type.ToKey());
            var conf = Strings.Get("confidence." + o.confidence.ToKey());
            string state;
            if (o.orphaned) state = Strings.Get("recall.orphaned");
            else state = Strings.Format("panel.korridor", o.safety_corridor_cm);

            var date = FormatDate(o.meta.created_at);
            var meta = Strings.Format("panel.meta_line", o.meta.author, date);
            var photo = Strings.Format("panel.photo_line",
                o.evidence.photos.Count > 0 ? o.evidence.photos.Count.ToString() : Strings.Get("panel.no_photo"));

            string note = string.IsNullOrEmpty(o.meta.note) ? "" : "\n" + o.meta.note;

            return $"{type} · {conf} · {Strings.Format("phase.tiefe", o.geometry.depth_cm)}\n"
                 + state + "\n"
                 + meta + " · " + photo
                 + note;
        }

        /// <summary>ISO-8601 (UTC, Schema v1) → kurzes Datum tt.mm.jjjj; unlesbar bleibt unlesbar (ehrlich).</summary>
        public static string FormatDate(string isoUtc)
        {
            if (DateTime.TryParse(isoUtc, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal,
                    out var dt))
                return dt.ToUniversalTime().ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            return isoUtc ?? "";
        }
    }
}
