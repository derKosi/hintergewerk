// Tests für die Menü-Logik: Flanken-Toggle, Wrap-Around, Close-Verhalten.
using NUnit.Framework;
using Hintergewerk.Data;
using Hintergewerk.UI;

namespace Hintergewerk.Tests
{
    public class HandMenuTests
    {
        [Test]
        public void SteigendeFlanke_ToggeltNurEinmal()
        {
            var m = new HandMenuLogic();
            Assert.IsFalse(m.UpdatePalm(true));  // steigend: öffnet
            Assert.IsTrue(m.UpdatePalm(true));   // gehalten: bleibt offen
            Assert.IsTrue(m.UpdatePalm(false));  // fallend: schließt
            Assert.IsFalse(m.UpdatePalm(false)); // gehalten: bleibt zu
        }

        [Test]
        public void Cycle_WrappedUmFünfTypen()
        {
            var m = new HandMenuLogic();
            m.UpdatePalm(true);
            Assert.AreEqual(ObjectType.Power, m.CurrentType.Value);
            m.Cycle(1);
            Assert.AreEqual(ObjectType.Water, m.CurrentType.Value);
            m.Cycle(5); // volle Runde
            Assert.AreEqual(ObjectType.Water, m.CurrentType.Value);
            m.Cycle(-1);
            Assert.AreEqual(ObjectType.Power, m.CurrentType.Value);
        }

        [Test]
        public void Close_LiefertTypUndSchließt()
        {
            var m = new HandMenuLogic();
            m.UpdatePalm(true);
            m.Cycle(2); // Heating
            var t = m.Close();
            Assert.AreEqual(ObjectType.Heating, t.Value);
            Assert.IsFalse(m.IsOpen);
            Assert.IsFalse(m.CurrentType.HasValue);
        }

        [Test]
        public void Geschlossen_Close_GibtNull()
        {
            var m = new HandMenuLogic();
            Assert.IsFalse(m.Close().HasValue);
        }

        [Test]
        public void Cycle_Geschlossen_MachtNichts()
        {
            var m = new HandMenuLogic();
            m.Cycle(3);
            Assert.IsFalse(m.IsOpen);
        }
    }
}
