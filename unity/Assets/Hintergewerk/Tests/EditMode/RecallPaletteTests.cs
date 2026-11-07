// Tests für den Farbkanon (RecallPalette): Typ-Farben unterscheidbar, Eskalation signalrot, Puls im Range.
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Hintergewerk.Data;
using Hintergewerk.Recall;

namespace Hintergewerk.Tests
{
    public class RecallPaletteTests
    {
        [Test]
        public void TypFarben_SindAlleUnterscheidbar()
        {
            var colors = new List<Color>
            {
                RecallPalette.TypeColor(ObjectType.Power),
                RecallPalette.TypeColor(ObjectType.Water),
                RecallPalette.TypeColor(ObjectType.Heating),
                RecallPalette.TypeColor(ObjectType.Stud),
                RecallPalette.TypeColor(ObjectType.NoDrillZone)
            };
            for (int i = 0; i < colors.Count; i++)
                for (int j = i + 1; j < colors.Count; j++)
                    Assert.AreNotEqual(colors[i], colors[j], $"Typ {i} und {j} haben dieselbe Farbe");
        }

        [Test]
        public void Eskalation_NaheUndKritischHebenSichAb()
        {
            foreach (ObjectType t in System.Enum.GetValues(typeof(ObjectType)))
            {
                var baseC = RecallPalette.TypeColor(t);
                var nearC = RecallPalette.NearColor(t);
                var critC = RecallPalette.CriticalColor(t);
                Assert.AreNotEqual(baseC, nearC, $"{t}: Near-Basis unterscheidet sich nicht");
                Assert.Greater(critC.r, 0.8f, $"{t}: Kritisch ist signalrot (rot-Kanal)");
            }
        }

        [Test]
        public void Puls_BleibtInNullEins_UndIstStufenweiseRuheloser()
        {
            float maxNear = 0f, maxCritical = 0f, minNear = 1f;
            for (float t = 0f; t < 20f; t += 0.05f)
            {
                var n = RecallPalette.Pulse(t, WarningLevel.Near);
                var c = RecallPalette.Pulse(t, WarningLevel.Critical);
                Assert.That(n, Is.InRange(0f, 1f));
                Assert.That(c, Is.InRange(0f, 1f));
                maxNear = Mathf.Max(maxNear, n); minNear = Mathf.Min(minNear, n);
                maxCritical = Mathf.Max(maxCritical, c);
            }
            Assert.Greater(maxNear, minNear, "Near pulsiert");
            Assert.Greater(maxCritical, maxNear, "Kritisch pulsiert schneller");
        }
    }
}
