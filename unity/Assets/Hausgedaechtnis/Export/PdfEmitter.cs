// Hausgedächtnis — minimaler PDF-Emitter (Risiko R4 ausräumen, 02-architektur.md §5).
// PDF 1.4, A4, Base14 Helvetica (keine externen Assets, keine Dependencies, IL2CPP-sicher).
// Bewusst simpel: Linien + Text + Farben. Umlaute werden transliteriert (ae/oe/ue/ss),
// damit WinAnsi/ASCII garantiert passt. Byte-Offsets des xref werden exakt mitgezählt.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Hausgedächtnis.Export
{
    public class PdfLine
    {
        public float X1, Y1, X2, Y2, Width = 1f;
        public float R = 0f, G = 0f, B = 0f;
    }

    public class PdfText
    {
        public float X, Y, Size = 9f;
        public bool Bold;
        public string S = "";
        public float R = 0f, G = 0f, B = 0f;
    }

    public class PdfDoc
    {
        public float Width = 595f;   // A4 hoch
        public float Height = 842f;
        public readonly List<PdfLine> Lines = new List<PdfLine>();
        public readonly List<PdfText> Texts = new List<PdfText>();
    }

    public static class PdfEmitter
    {
        public static byte[] Emit(PdfDoc doc)
        {
            var content = BuildContent(doc);
            var contentBytes = Encoding.ASCII.GetBytes(content);

            // Indirekte Objekte: 1 Catalog, 2 Pages, 3 Page, 4/5 Fonts, 6 Content
            var objs = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 " + F(doc.Width) + " " + F(doc.Height) + "]"
                  + " /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>",
                "<< /Length " + contentBytes.Length + " >>\nstream\n" + content + "\nendstream"
            };

            var bytes = new List<byte>(contentBytes.Length + 4096);

            // Header + Binary-Comment (markiert Binär-Inhalt, manche Viewer bestehen drauf)
            Add(bytes, Encoding.ASCII.GetBytes("%PDF-1.4\n%"));
            bytes.Add(0xE2); bytes.Add(0xE3); bytes.Add(0xCF); bytes.Add(0xD3);
            bytes.Add((byte)'\n');

            var offsets = new int[objs.Count + 1]; // 0 unbenutzt
            for (int i = 0; i < objs.Count; i++)
            {
                offsets[i + 1] = bytes.Count;
                Add(bytes, Encoding.ASCII.GetBytes((i + 1) + " 0 obj\n" + objs[i] + "\nendobj\n"));
            }

            int xrefPos = bytes.Count;
            var xref = new StringBuilder();
            xref.Append("xref\n0 ").Append(objs.Count + 1).Append('\n');
            xref.Append("0000000000 65535 f \n");                       // Free-Head, exakt 20 Bytes
            for (int i = 1; i <= objs.Count; i++)
                xref.Append(offsets[i].ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
            xref.Append("trailer\n<< /Size ").Append(objs.Count + 1).Append(" /Root 1 0 R >>\n");
            xref.Append("startxref\n").Append(xrefPos).Append('\n');
            xref.Append("%%EOF");
            Add(bytes, Encoding.ASCII.GetBytes(xref.ToString()));

            return bytes.ToArray();
        }

        static string BuildContent(PdfDoc d)
        {
            var sb = new StringBuilder(8192);
            sb.Append("1 j 1 J\n"); // runde Kappen/Verbindungen

            foreach (var l in d.Lines)
            {
                sb.Append(F(l.R)).Append(' ').Append(F(l.G)).Append(' ').Append(F(l.B)).Append(" RG\n");
                sb.Append(F(l.Width)).Append(" w ")
                  .Append(F(l.X1)).Append(' ').Append(F(l.Y1)).Append(" m ")
                  .Append(F(l.X2)).Append(' ').Append(F(l.Y2)).Append(" l S\n");
            }

            foreach (var t in d.Texts)
            {
                sb.Append(F(t.R)).Append(' ').Append(F(t.G)).Append(' ').Append(F(t.B)).Append(" rg\n");
                sb.Append("BT /").Append(t.Bold ? "F2" : "F1").Append(' ').Append(F(t.Size))
                  .Append(" Tf ").Append(F(t.X)).Append(' ').Append(F(t.Y))
                  .Append(" Td (").Append(T(t.S)).Append(") Tj ET\n");
            }
            return sb.ToString();
        }

        static void Add(List<byte> target, byte[] data) { for (int i = 0; i < data.Length; i++) target.Add(data[i]); }

        static string F(float v) => v.ToString("0.##", CultureInfo.InvariantCulture);

        /// <summary>ASCII-sicher machen: Umlaute transliterieren, ( ) \ escapen, Steuerzeichen weg.</summary>
        static string T(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length + 4);
            foreach (var c in s)
            {
                switch (c)
                {
                    case '(': sb.Append("\\("); break;
                    case ')': sb.Append("\\)"); break;
                    case '\\': sb.Append("\\\\"); break;
                    case 'ä': sb.Append("ae"); break;
                    case 'ö': sb.Append("oe"); break;
                    case 'ü': sb.Append("ue"); break;
                    case 'Ä': sb.Append("Ae"); break;
                    case 'Ö': sb.Append("Oe"); break;
                    case 'Ü': sb.Append("Ue"); break;
                    case 'ß': sb.Append("ss"); break;
                    default:
                        if (c >= 32 && c <= 126) sb.Append(c);
                        break; // alles andere: weglassen (steuerfrei halten)
                }
            }
            return sb.ToString();
        }
    }
}
