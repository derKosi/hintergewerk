// Tests für die Warn-Mathematik (Recall): Segment-/Polylinien-Distanz, Eskalation.
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Hintergewerk.Recall;

namespace Hintergewerk.Tests
{
    public class WarningMathTests
    {
        [Test]
        public void Segment_Senkrecht()
        {
            var d = WarningMath.DistanceToSegment(new Vector3(0f, 1f, 0f), new Vector3(-1f, 0f, 0f), new Vector3(1f, 0f, 0f));
            Assert.AreEqual(1f, d, 1e-4f);
        }

        [Test]
        public void Segment_HinterEndpunkt()
        {
            var d = WarningMath.DistanceToSegment(new Vector3(3f, 0f, 0f), new Vector3(-1f, 0f, 0f), new Vector3(1f, 0f, 0f));
            Assert.AreEqual(2f, d, 1e-4f);
        }

        [Test]
        public void Polylinie_NimmtMinimum()
        {
            var line = new List<Vector3> { new Vector3(0f, 0f, 0f), new Vector3(2f, 0f, 0f), new Vector3(2f, 2f, 0f) };
            float d = WarningMath.DistanceToPolyline(new Vector3(3f, 0.5f, 0f), line, out int seg);
            Assert.AreEqual(1f, d, 1e-4f);
            Assert.AreEqual(1, seg);
        }

        [Test]
        public void Warnsystem_Eskaliert_Nah_Kritisch_Frei()
        {
            var sys = new WarningSystem { NearCm = 15f, CriticalCm = 7f };
            var line = new List<Vector3> { new Vector3(0f, 0f, 0f), new Vector3(2f, 0f, 0f) };
            var corridors = new List<(string id, IReadOnlyList<Vector3> line)> { ("obj_1", line) };

            string id; float cm;
            var near = sys.Evaluate(new Vector3(0.10f, 0f, 0f), corridors, out id, out cm);
            Assert.AreEqual(WarningLevel.Near, near);
            Assert.AreEqual("obj_1", id);
            Assert.AreEqual(10f, cm, 1e-3f);

            var critical = sys.Evaluate(new Vector3(0.05f, 0f, 0f), corridors, out _, out _);
            Assert.AreEqual(WarningLevel.Critical, critical);

            var none = sys.Evaluate(new Vector3(1.0f, 0.5f, 0f), corridors, out _, out _);
            Assert.AreEqual(WarningLevel.None, none);
        }
    }
}
