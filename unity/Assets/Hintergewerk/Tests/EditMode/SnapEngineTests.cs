// Tests für die Snap-Kette: Ebene → Raster → Magnet, plus Zwei-Hand-Tiefe.
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Hintergewerk.Core;
using Hintergewerk.Capture;

namespace Hintergewerk.Tests
{
    public class SnapEngineTests
    {
        static WallPlane WandXY() => new WallPlane { Point = Vector3.zero, Normal = Vector3.forward, Label = "wall" };

        [Test]
        public void Punkt_WirdAufEbeneProjiziert()
        {
            var r = new SnapEngine().Snap(new Vector3(0.12f, 0.13f, 0.4f), WandXY(), null);
            Assert.AreEqual(0f, r.Point.z, 1e-4f);
            Assert.IsTrue(r.SnappedToPlane);
        }

        [Test]
        public void Raster_5cm()
        {
            var r = new SnapEngine { GridCm = 5f }.Snap(new Vector3(0.12f, 0.13f, 0f), WandXY(), null);
            Assert.AreEqual(0.10f, r.Point.x, 1e-4f);
            Assert.AreEqual(0.15f, r.Point.y, 1e-4f);
            Assert.IsTrue(r.SnappedToGrid);
        }

        [Test]
        public void Magnet_Gewinnt_Innerhalb_Radius()
        {
            var endpunkte = new List<(Vector3 pos, string id)> { (new Vector3(0.11f, 0.14f, 0f), "obj_1") };
            var r = new SnapEngine { GridCm = 5f, MagnetRadiusCm = 8f }.Snap(new Vector3(0.12f, 0.13f, 0f), WandXY(), endpunkte);
            Assert.IsTrue(r.SnappedToMagnet);
            Assert.AreEqual("obj_1", r.MagnetId);
            Assert.AreEqual(0.11f, r.Point.x, 1e-4f);
            Assert.AreEqual(0.14f, r.Point.y, 1e-4f);
        }

        [Test]
        public void KeinMagnet_WennZuWeit()
        {
            var endpunkte = new List<(Vector3 pos, string id)> { (new Vector3(0.50f, 0.50f, 0f), "obj_2") };
            var r = new SnapEngine { GridCm = 5f, MagnetRadiusCm = 8f }.Snap(new Vector3(0.12f, 0.13f, 0f), WandXY(), endpunkte);
            Assert.IsFalse(r.SnappedToMagnet);
        }

        [Test]
        public void Tiefe_AusHanddistanz_WirdGeclampt()
        {
            Assert.AreEqual(12f, SnapEngine.DepthFromHandDistance(Vector3.zero, new Vector3(0.12f, 0f, 0f)), 1e-3f);
            Assert.AreEqual(30f, SnapEngine.DepthFromHandDistance(Vector3.zero, new Vector3(1f, 0f, 0f)), 1e-3f);
        }
    }
}
