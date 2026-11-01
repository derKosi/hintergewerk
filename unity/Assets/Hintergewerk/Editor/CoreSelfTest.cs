// Hintergewerk — Editor-Selbsttest: verifiziert den SDK-freien Kern OHNE Gerät.
// Menü: Hintergewerk → Kern-Selbsttest (JSON-Roundtrip) / PDF-Beispiel erzeugen.
// Damit ist morgen nach dem Import in 10 Sekunden klar, ob Data + Export layer funktionieren,
// bevor auch nur eine Zeile Meta-SDK-Code existiert.
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Hintergewerk.Data;
using Hintergewerk.Export;

namespace Hintergewerk.EditorTools
{
    public static class CoreSelfTest
    {
        [MenuItem("Hintergewerk/Kern-Selbsttest (JSON-Roundtrip)")]
        public static void JsonRoundtrip()
        {
            var store = new HouseDataStore(Path.Combine(Application.temporaryCachePath, "hg_selftest.json"));
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

            // Roundtrip über eine NEUE Store-Instanz (frisch von Platte)
            var reloaded = new HouseDataStore(store.Path);
            var ok = reloaded.Data.objects.Count == 2
                  && reloaded.Data.rooms.Count == 1
                  && reloaded.Data.anchors.Count == 1
                  && reloaded.Data.objects[0].geometry.Count == 3
                  && reloaded.Data.objects[0].type == ObjectType.Power
                  && reloaded.Data.objects[0].confidence == Confidence.Measured
                  && reloaded.Data.objects[0].evidence.photos.Count == 1
                  && reloaded.Data.objects[1].type == ObjectType.NoDrillZone;

            // Null-Erhaltung prüfen (voice_note muss null bleiben)
            var nullOk = reloaded.Data.objects[0].evidence.voice_note == null;

            Debug.Log($"[Selbsttest] JSON-Roundtrip: {(ok && nullOk ? "BESTANDEN" : "FEHLGESCHLAGEN")}\n{File.ReadAllText(store.Path)}");
            if (!ok || !nullOk) EditorUtility.DisplayDialog("Hintergewerk", "JSON-Roundtrip FEHLGESCHLAGEN — Log ansehen!", "Ok");
            else EditorUtility.DisplayDialog("Hintergewerk", "JSON-Roundtrip bestanden.\n(Objekte, Räume, Anker, null-Erhaltung, Enums)", "Ok");
        }

        [MenuItem("Hintergewerk/PDF-Beispiel erzeugen")]
        public static void SamplePdf()
        {
            // Fake-Raum: 4×3 m, zwei Wände offene Polygonliste (im Uhrzeigersinn)
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

            var doc = MapRenderer.BuildDoc("Bad EG (Beispiel)", HouseDataStore.DefaultAuthor, walls, objects, out float scale, out int ratio);
            var bytes = MapRenderer.EmitPdf(doc);
            var outPath = Path.Combine(Application.temporaryCachePath, "hintergewerk_beispiel.pdf");
            File.WriteAllBytes(outPath, bytes);
            Debug.Log($"[Selbsttest] PDF geschrieben ({bytes.Length} Bytes, 1:{ratio}, {scale:0.##} pt/m): {outPath}");
            EditorUtility.RevealInFinder(outPath);
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
