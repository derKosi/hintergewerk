// Hintergewerk — zentrale UI-Texte DE/EN (Entscheidung D20: DE primär, Video EN).
// Alle UI-Strings über Strings.Get/Format — nichts hardcoded im Code verstreuen.
// Fehlender Key wird sichtbar zurückgegeben (der Key selbst), nie stillschweigend leer.
using System.Collections.Generic;

namespace Hintergewerk.Localization
{
    public enum AppLanguage { De, En }

    public static class Strings
    {
        public static AppLanguage Language = AppLanguage.De;

        static readonly Dictionary<string, (string De, string En)> T = new()
        {
            // Menü & Objekttypen
            ["menu.open"] = ("Handfläche zum Körper — Menü", "Palm to body — open menu"),
            ["menu.close"] = ("Menü geschlossen", "Menu closed"),
            ["type.power"] = ("Elektro", "Power"),
            ["type.water"] = ("Wasser", "Water"),
            ["type.heating"] = ("Heizung", "Heating"),
            ["type.stud"] = ("Ständerwerk/Balken", "Studs/Joists"),
            ["type.no_drill_zone"] = ("Nicht-bohren-Zone", "No-drill zone"),

            // Capture
            ["capture.draw_hint"] = ("Pinch am Startpunkt, Linie nachziehen", "Pinch at the start, trace the run"),
            ["capture.point_snapped"] = ("Punkt eingerastet", "Point snapped"),
            ["capture.end_hint"] = ("Endpunkt setzen — mindestens 2 Punkte", "Set the end — at least 2 points"),
            ["capture.depth_hint"] = ("Zwei-Hand-Distanz = Tiefe", "Two-hand distance = depth"),
            ["capture.confirm_hint"] = ("Pinch zum Bestätigen", "Pinch to confirm"),
            ["capture.saved"] = ("Gespeichert: {0}", "Saved: {0}"),
            ["capture.saved_room"] = ("Gespeichert · {0} Objekte im Raum", "Saved · {0} objects in room"),
            ["capture.photo_saved"] = ("Gespeichert · Foto angehängt", "Saved · photo attached"),
            ["capture.photo_unavailable"] = ("Gespeichert · Foto nicht verfügbar (ehrlich: kein Bild, kein Fake)", "Saved · photo unavailable (honest: no image, no fake)"),
            ["capture.undo_hint"] = ("Wischen = rückgängig", "Swipe to undo"),
            ["capture.undo_done"] = ("Rückgängig: {0}", "Undone: {0}"),
            ["capture.cancelled"] = ("Abgebrochen", "Cancelled"),
            ["capture.min_points"] = ("Mindestens 2 Punkte nötig", "At least 2 points required"),

            // Recall & Warnung
            ["recall.loaded"] = ("Geladen: {0} Objekte", "Loaded: {0} objects"),
            ["recall.located_status"] = ("{0} verortbar · {1} nicht verortbar", "{0} located · {1} not locatable"),
            ["recall.near"] = ("Vorsicht — Leitung in der Nähe", "Careful — utility nearby"),
            ["recall.critical"] = ("STOPP — Sicherheitskorridor!", "STOP — safety corridor!"),
            ["recall.orphaned"] = ("Nicht verortbar (Anker fehlt)", "Cannot locate (anchor missing)"),
            ["recall.anchor_lost"] = ("Anker verloren: {0}", "Anchor lost: {0}"),
            ["recall.empty_room"] = ("Dieser Raum: noch nichts dokumentiert", "This room: nothing documented yet"),

            // Konfidenz
            ["confidence.measured"] = ("gemessen", "measured"),
            ["confidence.estimated"] = ("geschätzt", "estimated"),
            ["confidence.scanner"] = ("Sucher", "scanner"),

            // Export
            ["export.started"] = ("Export läuft…", "Exporting…"),
            ["export.done"] = ("Export fertig: {0}", "Export done: {0}"),
            ["export.done_json"] = ("JSON fertig: {0}", "JSON done: {0}"),
            ["export.hint"] = ("Linke Hand: Pinch = Export (PDF + JSON)", "Left hand: pinch = export (PDF + JSON)"),
            ["export.failed"] = ("Export fehlgeschlagen", "Export failed"),

            // Objektliste
            ["list.title"] = ("Objektliste", "Object list"),
            ["list.empty"] = ("Noch nichts dokumentiert", "Nothing documented yet"),
            ["list.count"] = ("{0} Objekte in {1}", "{0} objects in {1}"),

            // Demo/M1 (Szene 02_AnkerTest)
            ["demo.anchors_loaded"] = ("Geladen: {0} Anker", "Loaded: {0} anchors"),
            ["demo.anchor_placed"] = ("Anker gesetzt: {0}", "Anchor placed: {0}"),
            ["demo.load_failed"] = ("Laden fehlgeschlagen — Fallback (docs/07)", "Load failed — fallback (docs/07)"),

            // Capture-Phasen
            ["phase.idle"] = ("Bereit — Handfläche zum Körper öffnet das Menü", "Ready — palm to body opens the menu"),
            ["phase.typwahl"] = ("Typ wählen", "Choose type"),
            ["phase.zeichnen"] = ("Zeichnen — Pinch setzt Punkte", "Drawing — pinch places points"),
            ["phase.tiefe"] = ("Tiefe: {0} cm", "Depth: {0} cm"),
            ["phase.bestaetigen"] = ("Pinch zum Speichern", "Pinch to save"),

            // Menü
            ["menu.cycle_hint"] = ("Pinch = nächster Typ", "Pinch = next type"),
            ["menu.close_hint"] = ("Handfläche weg = Typ wählen", "Palm away = pick type"),
        };

        public static string Get(string key)
        {
            if (!T.TryGetValue(key, out var pair)) return key; // fehlender Key sichtbar, nicht still leer
            return Language == AppLanguage.En ? pair.En : pair.De;
        }

        public static string Format(string key, params object[] args) => string.Format(Get(key), args);

        public static IEnumerable<string> Keys => T.Keys;
    }
}
