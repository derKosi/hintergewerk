// Tests: Jeder String-Key muss in beiden Sprachen existieren — sonst fällt es als roter Test auf,
// nicht erst als halber Satz im Jury-Video.
using NUnit.Framework;
using Hintergewerk.Localization;

namespace Hintergewerk.Tests
{
    public class StringsTests
    {
        [Test]
        public void Alle_Keys_Haben_Beide_Sprachen()
        {
            foreach (var key in Strings.Keys)
            {
                Strings.Language = AppLanguage.De;
                Assert.That(Strings.Get(key), Is.Not.Empty);
                Strings.Language = AppLanguage.En;
                Assert.That(Strings.Get(key), Is.Not.Empty);
            }
            Strings.Language = AppLanguage.De; // Zustand zurücksetzen für andere Tests
        }

        [Test]
        public void FehlenderKey_WirdSichtbarZurückgegeben()
        {
            Assert.AreEqual("kein.dieser.key", Strings.Get("kein.dieser.key"));
        }

        [Test]
        public void Format_EsetztParameter()
        {
            Strings.Language = AppLanguage.En;
            StringAssert.Contains("7 objects", Strings.Format("list.count", 7, "Bad EG"));
            Strings.Language = AppLanguage.De;
            StringAssert.Contains("7 Objekte", Strings.Format("list.count", 7, "Bad EG"));
        }
    }
}
