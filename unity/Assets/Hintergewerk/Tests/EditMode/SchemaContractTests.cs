// Vertrags-Tests: Die schema-v1.json ist der formale Vertrag (Fremd-Tools lesen den Export).
// Dieser Test prüft die maschinell prüfbare Teilmenge — ohne Schema-Library-Abhängigkeit.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Hintergewerk.Data;

namespace Hintergewerk.Tests
{
    public class SchemaContractTests
    {
        static string Fixture(string name) =>
            Path.Combine(Application.dataPath, "Hintergewerk/Tests/EditMode/Fixtures", name);

        static void PruefeStruktur(HouseData h)
        {
            Assert.GreaterOrEqual(h.schema_version, 1);

            var rooms = new HashSet<string>();
            var anchors = new HashSet<string>();
            var ids = new HashSet<string>();

            foreach (var r in h.rooms) Assert.IsTrue(rooms.Add(r.id), "Raum-ID doppelt: " + r.id);
            foreach (var a in h.anchors) Assert.IsTrue(anchors.Add(a.id), "Anker-ID doppelt: " + a.id);

            foreach (var o in h.objects)
            {
                Assert.IsTrue(ids.Add(o.id), "Objekt-ID doppelt: " + o.id);
                Assert.GreaterOrEqual(o.geometry.depth_cm, 0.0);
                Assert.GreaterOrEqual(o.safety_corridor_cm, 0.0);
                foreach (var p in o.geometry.points_lokal)
                    Assert.AreEqual(3, p.Length, "Punkt braucht genau [x,y,z]");
            }
        }

        [Test]
        public void Beispielhaus_ErfuelltDenVertrag()
        {
            var h = SimpleJson.Parse(File.ReadAllText(Fixture("beispielhaus.json")));
            PruefeStruktur(h);
            Assert.Greater(h.objects.Count, 0);
        }

        [Test]
        public void ZweiRaeume_ErfuelltDenVertrag()
        {
            var h = SimpleJson.Parse(File.ReadAllText(Fixture("zwei_raeume.json")));
            PruefeStruktur(h);
            Assert.AreEqual(2, h.rooms.Count);
            Assert.Greater(h.objects.Count, 0);
        }

        [Test]
        public void VerwaisteAnker_WerdenMarkiertNichtGelöscht()
        {
            var store = new HouseDataStore(Fixture("verwaiste_anker.json"));
            Assert.Greater(store.Data.objects.Count, 0);
            Assert.Greater(store.OrphanedObjects().Count, 0); // Ehrlichkeitsregel: sichtbar führen
        }

        [Test]
        public void LeeresHaus_IstGueltig()
        {
            var h = SimpleJson.Parse(File.ReadAllText(Fixture("leer.json")));
            PruefeStruktur(h);
        }

        [Test]
        public void Randfaelle_ParsenSauber()
        {
            var h = SimpleJson.Parse(File.ReadAllText(Fixture("randfaelle.json")));
            PruefeStruktur(h);
            Assert.Greater(h.objects.Count, 0);
        }
    }
}
