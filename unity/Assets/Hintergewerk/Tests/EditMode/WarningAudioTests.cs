// Hintergewerk — Tests: FoV-aware Audio-Panning (Politur 08.11., docs/03 §2.5
// „räumlicher Audio-Ping"). Rein rechnerisch — kein Audio-System im Test nötig.
using NUnit.Framework;
using UnityEngine;
using Hintergewerk.Recall;

namespace Hintergewerk.Tests
{
    public class WarningAudioTests
    {
        readonly Vector3 head = new Vector3(0f, 1.2f, 0f);

        [Test]
        public void Gefahr_Rechts_Pingt_Nach_Rechts()
        {
            var pan = WarningAudio.PanFor(head, Vector3.right, new Vector3(2f, 1.2f, 1f));
            Assert.Greater(pan, 0.5f, "Gefahr rechts vom Kopf → Pan deutlich positiv.");
        }

        [Test]
        public void Gefahr_Links_Pingt_Nach_Links()
        {
            var pan = WarningAudio.PanFor(head, Vector3.right, new Vector3(-2f, 1.2f, 1f));
            Assert.Less(pan, -0.5f, "Gefahr links → Pan deutlich negativ.");
        }

        [Test]
        public void Gefahr_Vorne_Pingt_Mitte()
        {
            var pan = WarningAudio.PanFor(head, Vector3.right, new Vector3(0f, 1.2f, 2f));
            Assert.That(pan, Is.EqualTo(0f).Within(0.05f), "Gefahr direkt voraus → Mitte.");
        }

        [Test]
        public void Pan_Bleibt_Geklemmt_MinusEins_Bis_PlusEins()
        {
            var p1 = WarningAudio.PanFor(head, Vector3.right, new Vector3(9f, 1.2f, 0.01f));
            Assert.LessOrEqual(p1, 1f);
            var p2 = WarningAudio.PanFor(head, Vector3.right, new Vector3(-9f, 1.2f, 0.01f));
            Assert.GreaterOrEqual(p2, -1f);
        }

        [Test]
        public void Gefahr_Auf_Kopfposition_Pingt_Mitte()
        {
            Assert.That(WarningAudio.PanFor(head, Vector3.right, head), Is.EqualTo(0f),
                "Kein Richtungvektor → neutral, kein Crash.");
        }

        [Test]
        public void Kopfrechte_Nahe_Null_Faellt_Auf_Weltrechte_Zurueck()
        {
            var pan = WarningAudio.PanFor(head, Vector3.zero, new Vector3(2f, 1.2f, 0f));
            Assert.Greater(pan, 0.5f, "Degenerierter Kopf-Frame → deterministischer Fallback.");
        }
    }
}
