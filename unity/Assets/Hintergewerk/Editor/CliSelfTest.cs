// Hintergewerk — CLI-Selbsttest: gleiche Prüfungen wie die Menü-Einträge in CoreSelfTest,
// aber ohne DisplayDialog/RevealInFinder — für Agent-/Batch-Ausführung via
// `unity command script eval 'Hintergewerk.EditorTools.CliSelfTest.RunAll();'`.
// Ergebnis steht als „[CliSelfTest] … BESTANDEN|FEHLGESCHLAGEN" im Editor-Log.
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Hintergewerk.Data;
using Hintergewerk.Export;

namespace Hintergewerk.EditorTools
{
    public static class CliSelfTest
    {
        public static void RunAll()
        {
            var jsonOk = JsonRoundtrip();
            var pdfPath = SamplePdf(out long pdfBytes);
            Debug.Log($"[CliSelfTest] JSON-Roundtrip: {(jsonOk ? "BESTANDEN" : "FEHLGESCHLAGEN")} · " +
                      $"PDF: {(pdfBytes > 0 ? pdfPath + " (" + pdfBytes + " Bytes)" : "FEHLGESCHLAGEN")}");
        }

        static bool JsonRoundtrip()
        {
            // Eindeutiger Pfad pro Lauf — der Selbsttest muss jederzeit wiederholbar sein
            // (Fix 07.11.: fester Pfad kumulierte Objekte über Läufe → zweiter Lauf scheiterte).
            var store = new HouseDataStore(Path.Combine(Application.temporaryCachePath,
                "hg_selftest_cli_" + Guid.NewGuid().ToString("N") + ".json"));
            store.EnsureRoom("raum_test", "Testraum", 0);
            store.RegisterAnchor("anc_test_1", "raum_test", "high");

            var o = store.AddObject(ObjectType.Power, "raum_test", "anc_test_1", 4.5, Confidence.Measured);
            o.geometry.AddPoint(0.1f, 1.2f, 0f);
            o.geometry.AddPoint(0.8f, 1.2f, 0f);
            o.geometry.AddPoint(0.8f, 2.1f, 0f);
            o.evidence.photos.Add("photos/test_1.jpg");
            o.meta.note = "Steigeinleitung, offen sichtbar";

            var o2 = store.AddObject(ObjectType.NoDrillZone, "raum_test", "anc_test_1", 0.0);
            o2.geometry.AddPoint(0f, 0f, 0.1f);
            o2.geometry.AddPoint(2f, 0f, 0.1f);

            var reloaded = new HouseDataStore(store.Path);
            var ok = reloaded.Data.objects.Count == 2
                  && reloaded.Data.rooms.Count == 1
                  && reloaded.Data.anchors.Count == 1
                  && reloaded.Data.objects[0].geometry.Count == 3
                  && reloaded.Data.objects[0].type == ObjectType.Power
                  && reloaded.Data.objects[0].confidence == Confidence.Measured
                  && reloaded.Data.objects[0].evidence.photos.Count == 1
                  && reloaded.Data.objects[1].type == ObjectType.NoDrillZone
                  && reloaded.Data.objects[0].evidence.voice_note == null; // null-Erhaltung
            return ok;
        }

        static string SamplePdf(out long bytes)
        {
            var walls = new System.Collections.Generic.List<Vector2>
            {
                new Vector2(0, 0), new Vector2(4, 0), new Vector2(4, 3), new Vector2(0, 3)
            };
            var objects = new System.Collections.Generic.List<MappedObject>
            {
                Obj(ObjectType.Power, 4.5f, Confidence.Measured,
                    new Vector3(0.2f, 0f, 2.8f), new Vector3(1.5f, 0f, 2.8f), new Vector3(1.5f, 0f, 1.9f)),
                Obj(ObjectType.Water, 6f, Confidence.Estimated,
                    new Vector3(0.2f, 0f, 0.2f), new Vector3(3.8f, 0f, 0.2f)),
                Obj(ObjectType.Stud, 0f, Confidence.Measured,
                    new Vector3(2.0f, 0f, 0.2f), new Vector3(2.0f, 0f, 2.8f)),
                Obj(ObjectType.NoDrillZone, 0f, Confidence.Estimated,
                    new Vector3(1.0f, 0f, 1.5f), new Vector3(3.0f, 0f, 1.5f)),
            };
            var doc = MapRenderer.BuildDoc("Bad EG (Beispiel)", HouseDataStore.DefaultAuthor, walls, objects, out _, out _);
            var pdf = PdfEmitter.Emit(doc);
            var outPath = Path.Combine(Application.temporaryCachePath, "hintergewerk_beispiel.pdf");
            File.WriteAllBytes(outPath, pdf);
            bytes = new FileInfo(outPath).Length;
            return outPath;
        }

        static MappedObject Obj(ObjectType t, float depth, Confidence c, params Vector3[] pts)
        {
            var m = new MappedObject { Type = t, DepthCm = depth, Confidence = c, Id = Guid.NewGuid().ToString("N").Substring(0, 8) };
            m.WorldPoints.AddRange(pts);
            return m;
        }
    }
}
#endif
