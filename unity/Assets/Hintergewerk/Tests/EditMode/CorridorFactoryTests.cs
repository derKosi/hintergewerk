// Hintergewerk — Tests: Ribbon beidseitig sichtbar (Fix 08.11., Simulator xr_ribbons.png)
// + Kamera-seitiger Anzeige-Offset + Aufgeblähte Bounds gegen XR-Frustum-Culling.
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Hintergewerk.Recall;

namespace Hintergewerk.Tests
{
    public class CorridorRibbonTests
    {
        static List<Vector3> Line(float x0, float x1)
        {
            return new List<Vector3> { new Vector3(x0, 0.9f, 0.5f), new Vector3(x1, 0.9f, 0.5f) };
        }

        [Test]
        public void Ribbon_Hat_Beide_Wicklungen()
        {
            var mesh = CorridorFactory.BuildRibbon(Line(-0.6f, 0.5f), 0.06f, Vector3.forward);
            var tris = mesh.triangles;
            Assert.AreEqual(12, tris.Length, "Ein Segment × 2 Dreiecke × 2 Wicklungen.");
            bool forward = false, reverse = false;
            for (int i = 0; i + 2 < tris.Length; i += 3)
            {
                if (tris[i] == 0 && tris[i + 1] == 1 && tris[i + 2] == 2) forward = true;
                if (tris[i] == 2 && tris[i + 1] == 1 && tris[i + 2] == 0) reverse = true;
            }
            Assert.IsTrue(forward && reverse, "Beide Wicklungen müssen vorhanden sein (Backface-Culling-Schutz).");
        }

        [Test]
        public void Zu_Wenig_Punkte_LeeresMesh_KeinCrash()
        {
            var mesh = CorridorFactory.BuildRibbon(new List<Vector3> { new Vector3(0, 0, 0) }, 0.05f, Vector3.forward);
            Assert.AreEqual(0, mesh.triangles.Length, "<2 Punkte → leeres Mesh, kein Wurf.");
        }

        [Test]
        public void FacingOffset_Ist_Klein_Und_An_Ebene()
        {
            var off = CorridorFactory.FacingOffset(new Vector3(0, 1, 0.5f), Vector3.forward, null);
            Assert.That(off.magnitude, Is.EqualTo(0.008f).Within(0.001f), "Versatz ~8 mm.");
        }
    }

    public class PlanProjectionTests
    {
        [Test]
        public void Breiter_Raum_Skaliert_Gleichmaessig()
        {
            var pts = new List<Vector3> { new Vector3(0, 0, 0), new Vector3(4, 2, 1) };
            var t = PlanProjection.Fit(pts, new Vector2(2f, 1f), 0.02f);
            Assert.Less(t.Scale, 0.5f, "4 m Spanne in ~1.96 m Panel → Maßstab < 0,5.");
            Assert.That(t.Center.x, Is.EqualTo(2f).Within(0.01f));
            Assert.That(t.Center.y, Is.EqualTo(0.5f).Within(0.01f));
        }

        [Test]
        public void Zentrum_Landet_Auf_Panel_Mitte()
        {
            var pts = new List<Vector3> { new Vector3(-1, 1, -0.5f), new Vector3(1, 1.6f, 0.5f) };
            var t = PlanProjection.Fit(pts, new Vector2(0.22f, 0.16f));
            var center = t.ToLocal(new Vector3(0, 1.3f, 0f));
            Assert.That(center.x, Is.EqualTo(0.11f).Within(0.01f), "Panel-Breite/2 = Mittelpunkt.");
            Assert.That(center.y, Is.EqualTo(0.08f).Within(0.01f), "Panel-Höhe/2 = Mittelpunkt.");
        }

        [Test]
        public void Alle_Punkte_Bleiben_Im_Panel()
        {
            var pts = new List<Vector3>
            {
                new Vector3(-2.3f, 0.7f, -1.8f), new Vector3(2.1f, 1.9f, 0.4f),
                new Vector3(0f, 1.2f, 1.2f)
            };
            var size = new Vector2(0.22f, 0.16f);
            var t = PlanProjection.Fit(pts, size);
            foreach (var p in pts)
            {
                var l = t.ToLocal(p);
                Assert.GreaterOrEqual(l.x, 0f);
                Assert.LessOrEqual(l.x, size.x);
                Assert.GreaterOrEqual(l.y, 0f);
                Assert.LessOrEqual(l.y, size.y);
            }
        }

        [Test]
        public void Leerer_Plan_Neutrale_Skalierung()
        {
            var t = PlanProjection.Fit(new List<Vector3>(), new Vector2(0.22f, 0.16f));
            Assert.That(t.Scale, Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void Einzelner_Punkt_Ruhiger_Massstab()
        {
            var t = PlanProjection.Fit(new List<Vector3> { new Vector3(3, 1, 7) }, new Vector2(0.22f, 0.16f));
            Assert.Greater(t.Scale, 0f);
            Assert.Less(t.Scale, 10f, "Mindest-Spanne 0,5 m verhindert Extrem-Zoom.");
        }

        // --- NormalForView (Fix 15.11., Befund B: senkrechte Leitungen standen als Kante ab) ---

        static readonly List<Vector3> VertikaleLinie = new()
        {
            new Vector3(0f, 0.8f, 0.5f), new Vector3(0f, 1.6f, 0.5f)
        };

        [Test]
        public void NormalForView_SenkrechteLinie_NimmtKameraRichtung()
        {
            // Kamera vor der Wand (−z): Normale muss horizontal zur Kamera zeigen —
            // vorher fiel sie auf Welt-X → Band stand senkrecht von der Wand ab.
            var n = CorridorFactory.NormalForView(VertikaleLinie, new Vector3(1, 0, 0), new Vector3(0f, 1.6f, -2f));
            Assert.That(n.x, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(n.z, Is.LessThan(-0.99f), "Normale zur Kamera (−z), nicht Welt-X.");
        }

        [Test]
        public void NormalForView_HorizontaleLinie_Unveraendert()
        {
            var waende = new Vector3(0, 1, 0);
            var pts = new List<Vector3> { new Vector3(-0.5f, 1.2f, 0.5f), new Vector3(0.5f, 1.2f, 0.5f) };
            var n = CorridorFactory.NormalForView(pts, waende, new Vector3(0f, 1.6f, -2f));
            Assert.That(n, Is.EqualTo(waende), "Nicht-senkrechte Linien: Bewährtes Verhalten bleibt.");
        }

        [Test]
        public void NormalForView_OhneKamera_Unveraendert()
        {
            var fallback = new Vector3(1, 0, 0);
            var n = CorridorFactory.NormalForView(VertikaleLinie, fallback, null);
            Assert.That(n, Is.EqualTo(fallback), "Ohne Kamera (EditMode-Test) kein Verhalten ändern.");
        }
    }
}
