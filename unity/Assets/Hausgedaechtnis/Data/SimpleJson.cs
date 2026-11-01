// Hausgedächtnis — minimaler JSON-Writer/Parser für genau das HouseData-Schema.
// Bewusst handgeschrieben statt JsonUtility: stabiles menschenlesbares Format, Enums als
// Strings, nulls bleiben erhalten, Invariant Culture (Dezimalpunkt, auch auf deutschen Systemen!),
// unbekannte Keys werden beim Lesen übersprungen (Forward-Kompatibilität für schema_version > 1).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Hausgedächtnis.Data
{
    public static class SimpleJson
    {
        // =============================== SCHREIBEN ===============================

        public static string Write(HouseData h)
        {
            var sb = new StringBuilder(8192);
            sb.Append("{\"schema_version\":").Append(h.schema_version);

            sb.Append(",\"rooms\":[");
            for (int i = 0; i < h.rooms.Count; i++)
            {
                var r = h.rooms[i];
                if (i > 0) sb.Append(',');
                sb.Append("{\"id\":").Append(Esc(r.id))
                  .Append(",\"label\":").Append(Esc(r.label))
                  .Append(",\"floor\":").Append(r.floor).Append('}');
            }
            sb.Append(']');

            sb.Append(",\"anchors\":[");
            for (int i = 0; i < h.anchors.Count; i++)
            {
                var a = h.anchors[i];
                if (i > 0) sb.Append(',');
                sb.Append("{\"id\":").Append(Esc(a.id))
                  .Append(",\"room_id\":").Append(Esc(a.room_id))
                  .Append(",\"confidence\":").Append(Esc(a.confidence))
                  .Append(",\"created_at\":").Append(Esc(a.created_at)).Append('}');
            }
            sb.Append(']');

            sb.Append(",\"objects\":[");
            for (int i = 0; i < h.objects.Count; i++)
            {
                var o = h.objects[i];
                if (i > 0) sb.Append(',');
                WriteObject(sb, o);
            }
            sb.Append(']');

            sb.Append('}');
            return sb.ToString();
        }

        static void WriteObject(StringBuilder sb, TrackedObject o)
        {
            sb.Append("{\"id\":").Append(Esc(o.id))
              .Append(",\"type\":").Append(Esc(o.type.ToKey()))
              .Append(",\"room_id\":").Append(Esc(o.room_id))
              .Append(",\"anchor_id\":").Append(Esc(o.anchor_id))
              .Append(",\"geometry\":{\"points_lokal\":[");

            for (int i = 0; i < o.geometry.points_lokal.Count; i++)
            {
                var p = o.geometry.points_lokal[i];
                if (i > 0) sb.Append(',');
                sb.Append('[').Append(Num(p[0])).Append(',').Append(Num(p[1])).Append(',').Append(Num(p[2])).Append(']');
            }
            sb.Append("],\"depth_cm\":").Append(Num(o.geometry.depth_cm))
              .Append(",\"diameter_cm\":").Append(Num(o.geometry.diameter_cm))
              .Append("},\"confidence\":").Append(Esc(o.confidence.ToKey()))
              .Append(",\"safety_corridor_cm\":").Append(Num(o.safety_corridor_cm))
              .Append(",\"evidence\":{\"photos\":[");

            for (int i = 0; i < o.evidence.photos.Count; i++)
            { if (i > 0) sb.Append(','); sb.Append(Esc(o.evidence.photos[i])); }
            sb.Append("],\"voice_note\":");
            sb.Append(o.evidence.voice_note == null ? "null" : Esc(o.evidence.voice_note));

            sb.Append("},\"meta\":{\"created_at\":").Append(Esc(o.meta.created_at))
              .Append(",\"author\":").Append(Esc(o.meta.author))
              .Append(",\"note\":").Append(Esc(o.meta.note)).Append("}}");
        }

        static string Num(double d) => d.ToString("0.###", CultureInfo.InvariantCulture);

        static string Esc(string s)
        {
            if (string.IsNullOrEmpty(s)) return "\"\"";
            var sb = new StringBuilder(s.Length + 8);
            sb.Append('"');
            foreach (var c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
            return sb.ToString();
        }

        // =============================== LESEN ===============================

        public static HouseData Parse(string json)
        {
            var p = new Parser(json);
            var h = p.House();
            p.ExpectEnd();
            return h;
        }

        sealed class Parser
        {
            readonly string s;
            int i;

            public Parser(string s) { this.s = s ?? throw new ArgumentNullException(nameof(s)); }

            Exception Err(string msg) => new FormatException($"JSON-Fehler bei Offset {i}: {msg}");

            void Ws() { while (i < s.Length && (s[i] == ' ' || s[i] == '\n' || s[i] == '\r' || s[i] == '\t')) i++; }

            char Peek() { Ws(); if (i >= s.Length) throw Err("unerwartetes Ende"); return s[i]; }

            void Expect(char c)
            {
                Ws();
                if (i >= s.Length || s[i] != c) throw Err($"'{c}' erwartet");
                i++;
            }

            bool TryEat(char c) { Ws(); if (i < s.Length && s[i] == c) { i++; return true; } return false; }

            string Str()
            {
                Expect('"');
                var sb = new StringBuilder();
                while (true)
                {
                    if (i >= s.Length) throw Err("String ohne Ende");
                    var c = s[i++];
                    if (c == '"') break;
                    if (c == '\\')
                    {
                        if (i >= s.Length) throw Err("Escape ohne Zeichen");
                        var e = s[i++];
                        switch (e)
                        {
                            case '"': sb.Append('"'); break;
                            case '\\': sb.Append('\\'); break;
                            case '/': sb.Append('/'); break;
                            case 'b': sb.Append('\b'); break;
                            case 'f': sb.Append('\f'); break;
                            case 'n': sb.Append('\n'); break;
                            case 'r': sb.Append('\r'); break;
                            case 't': sb.Append('\t'); break;
                            case 'u':
                                if (i + 4 > s.Length) throw Err("kurzer \\u-Escape");
                                sb.Append((char)Convert.ToInt32(s.Substring(i, 4), 16));
                                i += 4;
                                break;
                            default: throw Err($"unbekanntes Escape \\{e}");
                        }
                    }
                    else sb.Append(c);
                }
                return sb.ToString();
            }

            string NullOrString(out bool isNull)
            {
                Ws();
                if (i + 4 <= s.Length && s.Substring(i, 4) == "null") { i += 4; isNull = true; return null; }
                isNull = false;
                return Str();
            }

            double Num()
            {
                Ws();
                int st = i;
                if (i < s.Length && (s[i] == '-' || s[i] == '+')) i++;
                while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.' || s[i] == 'e' || s[i] == 'E' || s[i] == '-' || s[i] == '+')) i++;
                if (i == st) throw Err("Zahl erwartet");
                return double.Parse(s.Substring(st, i - st), CultureInfo.InvariantCulture);
            }

            string Key() { var k = Str(); Expect(':'); return k; }

            void SkipValue()
            {
                Ws();
                if (i >= s.Length) throw Err("Wert erwartet");
                var c = s[i];
                if (c == '{' || c == '[')
                {
                    var close = c == '{' ? '}' : ']';
                    i++;
                    int depth = 1;
                    bool inStr = false;
                    while (i < s.Length && depth > 0)
                    {
                        var d = s[i];
                        if (inStr) { if (d == '\\') i++; else if (d == '"') inStr = false; }
                        else if (d == '"') inStr = true;
                        else if (d == c) depth++;
                        else if (d == close) depth--;
                        i++;
                    }
                    if (depth != 0) throw Err("unbalancierte Klammer");
                }
                else if (c == '"') Str();
                else if (c == 'n' || (i + 4 <= s.Length && s.Substring(i, 4) == "null")) i += 4;
                else if (c == 't' || (i + 4 <= s.Length && s.Substring(i, 4) == "true")) i += 4;
                else if (c == 'f' || (i + 5 <= s.Length && s.Substring(i, 5) == "false")) i += 5;
                else Num();
            }

            public void ExpectEnd() { Ws(); if (i != s.Length) throw Err("Zeichen nach JSON-Ende"); }

            public HouseData House()
            {
                var h = new HouseData();
                Expect('{');
                while (!TryEat('}'))
                {
                    switch (Key())
                    {
                        case "schema_version": h.schema_version = (int)Num(); break;
                        case "rooms": h.rooms = RoomList(); break;
                        case "anchors": h.anchors = AnchorList(); break;
                        case "objects": h.objects = ObjectList(); break;
                        default: SkipValue(); break;
                    }
                    if (!TryEat(',')) { Expect('}'); break; }
                }
                if (h.schema_version > Schema.Version)
                    throw new FormatException($"schema_version {h.schema_version} ist neuer als unterstützt ({Schema.Version})");
                return h;
            }

            List<RoomData> RoomList()
            {
                var list = new List<RoomData>();
                Expect('[');
                while (!TryEat(']'))
                {
                    var r = new RoomData();
                    Expect('{');
                    while (!TryEat('}'))
                    {
                        switch (Key())
                        {
                            case "id": r.id = Str(); break;
                            case "label": r.label = Str(); break;
                            case "floor": r.floor = (int)Num(); break;
                            default: SkipValue(); break;
                        }
                        if (!TryEat(',')) { Expect('}'); break; }
                    }
                    list.Add(r);
                    if (!TryEat(',')) { Expect(']'); break; }
                }
                return list;
            }

            List<AnchorData> AnchorList()
            {
                var list = new List<AnchorData>();
                Expect('[');
                while (!TryEat(']'))
                {
                    var a = new AnchorData();
                    Expect('{');
                    while (!TryEat('}'))
                    {
                        switch (Key())
                        {
                            case "id": a.id = Str(); break;
                            case "room_id": a.room_id = Str(); break;
                            case "confidence": a.confidence = Str(); break;
                            case "created_at": a.created_at = Str(); break;
                            default: SkipValue(); break;
                        }
                        if (!TryEat(',')) { Expect('}'); break; }
                    }
                    list.Add(a);
                    if (!TryEat(',')) { Expect(']'); break; }
                }
                return list;
            }

            List<TrackedObject> ObjectList()
            {
                var list = new List<TrackedObject>();
                Expect('[');
                while (!TryEat(']'))
                {
                    list.Add(OneObject());
                    if (!TryEat(',')) { Expect(']'); break; }
                }
                return list;
            }

            TrackedObject OneObject()
            {
                var o = new TrackedObject();
                Expect('{');
                while (!TryEat('}'))
                {
                    switch (Key())
                    {
                        case "id": o.id = Str(); break;
                        case "type": o.type = Schema.ObjectTypeFromKey(Str()); break;
                        case "room_id": o.room_id = Str(); break;
                        case "anchor_id": o.anchor_id = Str(); break;
                        case "geometry": Geometry(o); break;
                        case "confidence": o.confidence = Schema.ConfidenceFromKey(Str()); break;
                        case "safety_corridor_cm": o.safety_corridor_cm = Num(); break;
                        case "evidence": Evidence(o); break;
                        case "meta": Meta(o); break;
                        default: SkipValue(); break;
                    }
                    if (!TryEat(',')) { Expect('}'); break; }
                }
                return o;
            }

            void Geometry(TrackedObject o)
            {
                Expect('{');
                while (!TryEat('}'))
                {
                    switch (Key())
                    {
                        case "points_lokal":
                            Expect('[');
                            while (!TryEat(']'))
                            {
                                Expect('[');
                                var x = Num(); Expect(','); var y = Num(); Expect(','); var z = Num(); Expect(']');
                                o.geometry.AddPoint(x, y, z);
                                if (!TryEat(',')) { Expect(']'); break; }
                            }
                            break;
                        case "depth_cm": o.geometry.depth_cm = Num(); break;
                        case "diameter_cm": o.geometry.diameter_cm = Num(); break;
                        default: SkipValue(); break;
                    }
                    if (!TryEat(',')) { Expect('}'); break; }
                }
            }

            void Evidence(TrackedObject o)
            {
                Expect('{');
                while (!TryEat('}'))
                {
                    switch (Key())
                    {
                        case "photos":
                            Expect('[');
                            while (!TryEat(']'))
                            {
                                o.evidence.photos.Add(Str());
                                if (!TryEat(',')) { Expect(']'); break; }
                            }
                            break;
                        case "voice_note": o.evidence.voice_note = NullOrString(out _); break;
                        default: SkipValue(); break;
                    }
                    if (!TryEat(',')) { Expect('}'); break; }
                }
            }

            void Meta(TrackedObject o)
            {
                Expect('{');
                while (!TryEat('}'))
                {
                    switch (Key())
                    {
                        case "created_at": o.meta.created_at = Str(); break;
                        case "author": o.meta.author = Str(); break;
                        case "note": o.meta.note = NullOrString(out _); break;
                        default: SkipValue(); break;
                    }
                    if (!TryEat(',')) { Expect('}'); break; }
                }
            }
        }
    }
}
