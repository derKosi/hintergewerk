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

            float scale; int ratio;
            var doc = MapRenderer.BuildDoc("Bad EG", "Mathias", walls, objs, out scale, out ratio);
            Assert.Greater(doc.Lines.Count, 0);
            Assert.Greater(doc.Texts.Count, 0);
            Assert.Greater(scale, 0f);
            Assert.AreEqual(25, ratio); // 4×3-m-Raum: 1:20 passt nicht mehr, 1:25 schon

            var bytes = PdfEmitter.Emit(doc);
            Assert.Greater(bytes.Length, 500);
        }

        [Test]
        public void Grundriss_WandSegmente_D41_SkizzierenRaumOhneObjekte()
        {
            // Zimmer-Ermittlung (D41): nur Wand-Grundkanten, noch kein Objekt dokumentiert
            var segs = new List<Vector2[]>
            {
                new[] { new Vector2(0f, 0f), new Vector2(4f, 0f) },
                new[] { new Vector2(4f, 0f), new Vector2(4f, 3f) },
                new[] { new Vector2(4f, 3f), new Vector2(0f, 3f) },
                new[] { new Vector2(0f, 3f), new Vector2(0f, 0f) },
            };

            float scale; int ratio;
            var doc = MapRenderer.BuildDoc("Bad EG", "Mathias", null, new List<MappedObject>(), out scale, out ratio, segs);

            Assert.Greater(doc.Lines.Count, 0);          // Wand-Segmente wurden gezeichnet (kein reiner Rahmen)
            Assert.AreEqual(25, ratio);                  // 4×3-m-Raum → 1:25 (Segmente bestimmen die Bounds!)
            Assert.Greater(PdfEmitter.Emit(doc).Length, 500);
        }

        [Test]
        public void Normblatt_KleinerRaum_Nimmt120()
        {
            var walls = new List<Vector2> { new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0.5f) };
            float scale; int ratio;
            MapRenderer.BuildDoc("Tiny", "T", walls, new List<MappedObject>(), out scale, out ratio);
            Assert.AreEqual(20, ratio);
            Assert.AreEqual(141.73f, scale, 0.5f);
        }

        [Test]
        public void Normblatt_RiesigerRaum_FreierMassstab()
        {
            var walls = new List<Vector2> { new Vector2(0f, 0f), new Vector2(40f, 0f), new Vector2(40f, 30f), new Vector2(0f, 30f) };
            float scale; int ratio;
            MapRenderer.BuildDoc("Halle", "T", walls, new List<MappedObject>(), out scale, out ratio);
            Assert.AreEqual(0, ratio); // größer als 1:200 → freier Maßstab
            Assert.Less(scale, 2834.65f / 200f);
        }

        // ---------------- JSON-Export (M3, ergänzt 07.11.) ----------------

        [Test]
        public void ExportJson_Roundtrip_ErhaeltObjekteUndSchema()
        {
            var path = System.IO.Path.Combine(Application.temporaryCachePath, "hg_exp_" + System.Guid.NewGuid().ToString("N") + ".json");
            var store = new HouseDataStore(System.IO.Path.Combine(Application.temporaryCachePath, "hg_st_" + System.Guid.NewGuid().ToString("N") + ".json"));
            store.EnsureRoom("raum_1", "Testraum", 0);
            store.RegisterAnchor("anc_x", "raum_1");
            store.AddObject(ObjectType.Water, "raum_1", "anc_x", 6.0);
            store.AddObject(ObjectType.Power, "raum_1", "anc_missing", 4.0); // Anker fehlt → orphaned

            var written = ExportRunner.ExportJson(store, out var count, path);
            Assert.AreEqual(2, count);
            Assert.IsTrue(System.IO.File.Exists(written));

            var parsed = SimpleJson.Parse(System.IO.File.ReadAllText(written));
            Assert.AreEqual(Schema.Version, parsed.schema_version);
            Assert.AreEqual(2, parsed.objects.Count);

            // Ehrlichkeit: verwaistes Objekt bleibt im Export drin (Beweis, kein Filter).
            // orphaned wird bewusst NICHT serialisiert — der Store berechnet es beim Laden neu.
            var reloaded = new HouseDataStore(written);
            Assert.AreEqual(1, reloaded.OrphanedObjects().Count);
            Assert.AreEqual("anc_missing", reloaded.OrphanedObjects()[0].anchor_id);
        }

        [Test]
        public void AttachPhoto_HaengtAnUndSpeichert()
        {
            var store = new HouseDataStore(System.IO.Path.Combine(Application.temporaryCachePath, "hg_st_" + System.Guid.NewGuid().ToString("N") + ".json"));
            var o = store.AddObject(ObjectType.Power, "raum_1", "anc_x", 4.0);

            Assert.IsFalse(store.AttachPhoto("obj_gibtsnicht", "photos/x.png")); // unbekanntes Objekt
            Assert.IsFalse(store.AttachPhoto(o.id, null));                        // kein Pfad
            Assert.IsTrue(store.AttachPhoto(o.id, "photos/test.png"));
            Assert.IsTrue(store.AttachPhoto(o.id, "photos/test.png"));            // Idempotent (kein Duplikat)

            var reloaded = new HouseDataStore(store.Path);
            Assert.AreEqual(1, reloaded.Data.objects[0].evidence.photos.Count);
            Assert.AreEqual("photos/test.png", reloaded.Data.objects[0].evidence.photos[0]);
        }
    }
}
