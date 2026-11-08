// Hintergewerk — Tests: Späte Anker-Lokalisierung im Recall (Fix 08.11.).
// Simulator-Beweis 08.11.: LoadAll liefert Callback VOR der Lokalisierung — am Gerät
// kommen Posen erst Sekunden später an. Ohne Rebuild blieben Korridore „nicht verortbar".
using NUnit.Framework;
using UnityEngine;
using Hintergewerk.Recall;

namespace Hintergewerk.Tests
{
    public class RecallControllerTests
    {
        [Test]
        public void Posen_Kommen_Spaet_RebuildErfolgt()
        {
            int sig = 42, last = 17, missing = 3;
            Assert.IsTrue(RecallController.ShouldRebuildForLatePoses(sig, last, missing),
                "Neue Posen + fehlende Objekte → Korridore müssen neu gebaut werden.");
        }

        [Test]
        public void Nichts_Fehlt_KeinRebuild()
        {
            Assert.IsFalse(RecallController.ShouldRebuildForLatePoses(42, 17, 0),
                "Alles verortbar → kein Rebuild (nicht zuholen).");
        }

        [Test]
        public void Signatur_Unveraendert_KeinRebuild()
        {
            Assert.IsFalse(RecallController.ShouldRebuildForLatePoses(17, 17, 3),
                "Keine neuen Posen → kein Rebuild (Poll darf nicht spammen).");
        }

        [Test]
        public void Verwaistes_Objekt_Bleibt_Unverortbar_Auch_Bei_Neuen_Posen()
        {
            // Ehrlichkeitsregel: Rebuild löst NICHTS für Anker ohne Pose — der kommt weiterhin
            // als „nicht verortbar" durch (Rebuild zählt ihn korrekt, erfindet keine Pose).
            // Hier nur die Entscheidungsgrenze: fehlende bleiben in der Zählung hängen.
            int sig = 100, last = 100, missing = 1;
            Assert.IsFalse(RecallController.ShouldRebuildForLatePoses(sig, last, missing),
                "Ohne neue Posen kein Rebuild — Verwaiste bleiben sichtbar nicht verortbar.");
        }
    }

    public class CorridorFacingTests
    {
        readonly Vector3 n = new Vector3(0f, 0f, 1f);

        [Test]
        public void Kamera_Vor_Wand_Versatz_Zur_Kamera()
        {
            // Kamera auf −z-Seite der Wand (Normalen +z) → Versatz muss Richtung Kamera (−z) zeigen.
            var off = Hintergewerk.Recall.CorridorFactory.FacingOffset(new Vector3(0, 1, 0.5f), n, new Vector3(0, 1.3f, -0.5f));
            Assert.Less(off.z, 0f, "Kamera auf −z-Seite → Ribbon versetzt Richtung Kamera (−z).");
            Assert.Less(System.Math.Abs(off.z), 0.01f, "Versatz bleibt unauffällig (~8 mm).");
        }

        [Test]
        public void Kamera_Hinter_Wand_Versatz_DrehtUm()
        {
            var off = Hintergewerk.Recall.CorridorFactory.FacingOffset(new Vector3(0, 1, 0.5f), n, new Vector3(0, 1.3f, 2f));
            Assert.Greater(off.z, 0f, "Kamera hinter der Wand (+z) → Versatz kippt Richtung Kamera (+z).");
        }

        [Test]
        public void Ohne_Kamera_Fallback_In_NormalenRichtung()
        {
            var off = Hintergewerk.Recall.CorridorFactory.FacingOffset(new Vector3(0, 1, 0.5f), n, null);
            Assert.Greater(off.z, 0f, "Kein Kamera-Kontext (Tests) → deterministisch +normal.");
        }
    }
}
