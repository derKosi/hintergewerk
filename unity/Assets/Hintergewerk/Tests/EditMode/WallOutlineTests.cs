// Tests für WallOutline (D41): Grundkanten aus Wand-Ebenen + Nähe-Filter.
// Ehrlichkeitsregel wird mitgetestet: ohne Größe / ohne horizontale Richtung → keine Kante.
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Hintergewerk.Core;
using Hintergewerk.Recall;

namespace Hintergewerk.Tests
{
    public class WallOutlineTests
    {
        static WallPlane Wand(Vector3 point, Vector3 normal, float w, float h) =>
            new WallPlane { Point = point, Normal = normal.normalized, Label = "wall", Size = new Vector2(w, h) };

        [Test]
        public void AchsenWand_LiefertGrundkante_AufBoden()
        {
            // 4 m breite Wand bei z=0, Fläche 2,6 m hoch, Zentrum bei y=1.3 → Grundkante y=0 (nur x/z)
            var walls = new List<WallPlane> { Wand(new Vector3(2f, 1.3f, 0f), Vector3.forward, 4f, 2.6f) };

            var segs = WallOutline.FloorSegments(walls);

            Assert.AreEqual(1, segs.Count);
            Assert.AreEqual(0f, segs[0][0].x, 1e-4f); // a.x = 2 − 2
            Assert.AreEqual(0f, segs[0][0].y, 1e-4f); // a.z = 0
            Assert.AreEqual(4f, segs[0][1].x, 1e-4f); // b.x = 2 + 2
            Assert.AreEqual(0f, segs[0][1].y, 1e-4f);
        }

        [Test]
        public void OhneGroesse_KeineKante_NichtsErfunden()
        {
            var walls = new List<WallPlane>
            {
                new WallPlane { Point = Vector3.zero, Normal = Vector3.forward, Label = "wall", Size = Vector2.zero },
                new WallPlane { Point = Vector3.zero, Normal = Vector3.forward, Label = "wall" } // default = 0/0
            };

            Assert.AreEqual(0, WallOutline.FloorSegments(walls).Count);
        }

        [Test]
        public void VertikaleNormale_BodenDecke_KeineKante()
        {
            var walls = new List<WallPlane> { Wand(new Vector3(0f, 0f, 0f), Vector3.up, 4f, 4f) };

            Assert.AreEqual(0, WallOutline.FloorSegments(walls).Count);
        }

        [Test]
        public void GedrehteWand_LaengeUndRichtungKorrekt()
        {
            // Normale um 30° um Y gedreht → Wandrichtung (cos30°, −sin30°) bzw. negiert; Länge = Breite
            float rad = 30f * Mathf.Deg2Rad;
            var normal = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
            var walls = new List<WallPlane> { Wand(new Vector3(1f, 1.3f, 1f), normal, 3f, 2.5f) };

            var segs = WallOutline.FloorSegments(walls);

            Assert.AreEqual(1, segs.Count);
            var d = segs[0][1] - segs[0][0];
            Assert.AreEqual(3f, d.magnitude, 1e-3f);                      // Wandlänge = Size.x
            var expected = (Vector2)new Vector2(Mathf.Cos(rad), -Mathf.Sin(rad)); // Wandrichtung = up × Normal
            Assert.Less(Mathf.Abs(Vector2.Dot(d.normalized, expected) - 1f), 0.001f);
        }

        [Test]
        public void RaumAusVierWaenden_VierKanten()
        {
            var walls = Mocks.MockWallProvider.Room(new Vector3(0f, 0f, 0f), 4f, 3f, 2.6f).GetWalls();

            var segs = WallOutline.FloorSegments(walls);

            Assert.AreEqual(4, segs.Count);
            // Grundriss-Bounds = Raumbreite/-tiefe (unabhängig von Reihenfolge)
            float minx = float.MaxValue, maxx = float.MinValue, minz = float.MaxValue, maxz = float.MinValue;
            foreach (var s in segs)
                foreach (var p in s)
                {
                    minx = Mathf.Min(minx, p.x); maxx = Mathf.Max(maxx, p.x);
                    minz = Mathf.Min(minz, p.y); maxz = Mathf.Max(maxz, p.y);
                }
            Assert.AreEqual(0f, minx, 1e-4f);
            Assert.AreEqual(4f, maxx, 1e-4f);
            Assert.AreEqual(0f, minz, 1e-4f);
            Assert.AreEqual(3f, maxz, 1e-4f);
        }

        [Test]
        public void FilterNahe_OhneObjekte_Leer()
        {
            var segs = new List<Vector2[]> { new[] { new Vector2(0f, 0f), new Vector2(4f, 0f) } };
            Assert.AreEqual(0, WallOutline.FilterNearSegments(segs, new List<Vector3>(), 2.5f).Count);
        }

        [Test]
        public void FilterNahe_PunktAnWandmitte_Drin_Fern_Raus()
        {
            var segs = new List<Vector2[]>
            {
                new[] { new Vector2(0f, 0f), new Vector2(4f, 0f) },      // Wand A (nahe)
                new[] { new Vector2(0f, 10f), new Vector2(4f, 10f) },    // Wand B (10 m weg)
            };
            var objs = new List<Vector3> { new Vector3(2f, 0f, 0.05f) }; // Objekt an Wand A

            var kept = WallOutline.FilterNearSegments(segs, objs, 2.5f);

            Assert.AreEqual(1, kept.Count);
            Assert.AreEqual(0f, kept[0][0].y, 1e-4f); // Wand A blieb
        }

        [Test]
        public void FilterNahe_NaheWandende_ZaehltAuch()
        {
            // Objekt hinter dem Wandende: nächstgelegener Segmentpunkt = Endpunkt
            var segs = new List<Vector2[]> { new[] { new Vector2(0f, 0f), new Vector2(4f, 0f) } };
            var objs = new List<Vector3> { new Vector3(5f, 0f, 1f) }; // Distanz zu (4,0) = √2 ≈ 1.41

            Assert.AreEqual(1, WallOutline.FilterNearSegments(segs, objs, 2.5f).Count);
        }

        [Test]
        public void Distanz_EntartetesSegment_IstPunktdistanz_SenkrechterFall()
        {
            Assert.AreEqual(5f, WallOutline.DistancePointSegment(new Vector2(3f, 4f), Vector2.zero, Vector2.zero), 1e-4f);
            Assert.AreEqual(1f, WallOutline.DistancePointSegment(new Vector2(2f, 1f), new Vector2(1f, 0f), new Vector2(3f, 0f)), 1e-4f); // senkrechter Fußpunkt
            Assert.AreEqual(0f, WallOutline.DistancePointSegment(new Vector2(2f, 0f), new Vector2(1f, 0f), new Vector2(3f, 0f)), 1e-4f); // Punkt auf dem Segment
        }
    }
}
