// Tests für Export: PDF-Struktur (Header, xref, Transliteration) und Grundriss-Aufbau.
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using Hintergewerk.Data;
using Hintergewerk.Export;

namespace Hintergewerk.Tests
{
    public class ExportTests
    {
        [Test]
        public void Pdf_Header_Und_Struktur()
        {
            var doc = new PdfDoc();
            doc.Lines.Add(new PdfLine { X1 = 10f, Y1 = 10f, X2 = 100f, Y2 = 10f, Width = 2f, R = 1f });
            doc.Texts.Add(new PdfText { X = 10f, Y = 20f, Size = 10f, S = "Prüfung äöüß (Test)", Bold = true });

            var bytes = PdfEmitter.Emit(doc);
            var text = Encoding.ASCII.GetString(bytes);

            StringAssert.StartsWith("%PDF-1.4", text);
            StringAssert.Contains("/Type /Catalog", text);
            StringAssert.Contains("%%EOF", text);
            // Transliteration: Umlaute raus, Klammern escaped
            StringAssert.Contains("Pruefung aeoeuess", text);
            StringAssert.Contains("\\(", text);
        }

        [Test]
        public void Pdf_XrefOffset_ZeigtAufXref()
        {
            var doc = new PdfDoc();
            doc.Lines.Add(new PdfLine());
            var text = Encoding.ASCII.GetString(PdfEmitter.Emit(doc));

            int startIdx = text.LastIndexOf("startxref");
            string tail = text.Substring(startIdx + "startxref".Length).Trim();
            int off = int.Parse(tail.Split('\n')[0]);
            StringAssert.StartsWith("xref", text.Substring(off));
        }

        [Test]
        public void Grundriss_ErzeugtZeichnung_und_PDF()
        {
            var walls = new List<Vector2> { new Vector2(0f, 0f), new Vector2(4f, 0f), new Vector2(4f, 3f), new Vector2(0f, 3f) };
            var objs = new List<MappedObject>
            {
                new MappedObject
                {
                    Type = ObjectType.Power, DepthCm = 4.5f, Confidence = Confidence.Measured, CorridorCm = 5f,
                    WorldPoints = new List<Vector3> { new Vector3(0.2f, 0f, 2.8f), new Vector3(1.5f, 0f, 2.8f) }
                }
            };

            float scale;
            var doc = MapRenderer.BuildDoc("Bad EG", "Mathias", walls, objs, out scale);
            Assert.Greater(doc.Lines.Count, 0);
            Assert.Greater(doc.Texts.Count, 0);
            Assert.Greater(scale, 0f);

            var bytes = PdfEmitter.Emit(doc);
            Assert.Greater(bytes.Length, 500);
        }
    }
}
