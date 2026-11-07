// Tests für das Detail-Panel: Konfidenz IMMER sichtbar (Ehrlichkeit), Verwaist-Zustand,
// Foto-ehrlichkeit, Datums-Parsing. Plus Ray-Distanz für „Blick + Pinch“.
using NUnit.Framework;
using UnityEngine;
using Hintergewerk.Data;
using Hintergewerk.Localization;
using Hintergewerk.Recall;
using Hintergewerk.UI;

namespace Hintergewerk.Tests
{
    public class InspectorPanelTests
    {
        static TrackedObject Obj()
        {
            var o = new TrackedObject
            {
                id = "obj_test",
                type = ObjectType.Water,
                room_id = "raum_1",
                anchor_id = "anc_1",
                confidence = Confidence.Measured,
                safety_corridor_cm = 6
            };
            o.geometry.depth_cm = 6;
            o.meta.created_at = "2026-11-07T14:32:00Z";
            o.meta.author = "Mathias";
            o.meta.note = "Steigeinleitung";
            return o;
        }

        [Test]
        public void Panel_Konfidenz_IST_Immer_Sichtbar()
        {
            foreach (Confidence c in System.Enum.GetValues(typeof(Confidence)))
            {
                var o = Obj();
                o.confidence = c;
                var text = InspectorPanel.Format(o);
                StringAssert.Contains(
                    Strings.Get("confidence." + c.ToKey()), text,
                    $"Konfidenz {c} fehlt im Panel — Ehrlichkeitsregel!");
            }
        }

        [Test]
        public void Panel_Zeigt_Typ_Tiefe_Monteur_Foto()
        {
            var o = Obj();
            o.evidence.photos.Add("photos/x.png");
            var text = InspectorPanel.Format(o);
            StringAssert.Contains(Strings.Get("type.water"), text);
            StringAssert.Contains("Mathias", text);
            StringAssert.Contains("07.11.2026", text);
            StringAssert.Contains("Steigeinleitung", text);
            StringAssert.Contains("1", text); // Fotoanzahl
        }

        [Test]
        public void Panel_FehlendesFoto_Wird_Benannt()
        {
            var text = InspectorPanel.Format(Obj());
            StringAssert.Contains(Strings.Get("panel.no_photo"), text);
        }

        [Test]
        public void Panel_VerwaistesObjekt_Zeigt_NichtVerortbar()
        {
            var o = Obj();
            o.orphaned = true;
            var text = InspectorPanel.Format(o);
            StringAssert.Contains(Strings.Get("recall.orphaned"), text);
        }

        [Test]
        public void Panel_Null_Und_UnlesbaresDatum_Ehrlich()
        {
            StringAssert.Contains(Strings.Get("panel.not_found"), InspectorPanel.Format(null));
            var o = Obj();
            o.meta.created_at = "kaputtes-datum";
            StringAssert.Contains("kaputtes-datum", InspectorPanel.Format(o));
        }

        // ---------------- Ray-Distanz (Blick + Pinch) ----------------

        [Test]
        public void Ray_Distanz_Trifft_Polylinie()
        {
            var line = new System.Collections.Generic.List<Vector3>
            {
                new Vector3(0f, 1f, 1f), new Vector3(1f, 1f, 1f)
            };
            // Blick (−Z) auf die Linie → Abstand = 0
            float d = WarningMath.DistanceRayToPolyline(new Vector3(0.5f, 1f, 3f), Vector3.back, line, 4f);
            Assert.AreEqual(0f, d, 1e-4f);

            // 20 cm daneben
            d = WarningMath.DistanceRayToPolyline(new Vector3(0.5f, 1.2f, 3f), Vector3.back, line, 4f);
            Assert.AreEqual(0.2f, d, 1e-3f);

            // Strahl zeigt von der Linie WEG → nächster Kandidat ist der Strahlstart (2 m) —
            // über selectRadius (0,15 m) → keine Auswahl
            d = WarningMath.DistanceRayToPolyline(new Vector3(0.5f, 1f, 3f), Vector3.forward, line, 4f);
            Assert.AreEqual(2f, d, 1e-3f);

            // Blick PAST die Linie (maxDist überschritten) → keine Auswahl
            d = WarningMath.DistanceRayToPolyline(new Vector3(0.5f, 1f, 10f), Vector3.back, line, 4f);
            Assert.AreEqual(float.MaxValue, d);
        }
    }
}
