// Tests für den JSON-Layer: Roundtrip, null-Erhaltung, Forward-Kompatibilität.
using System;
using NUnit.Framework;
using Hintergewerk.Data;

namespace Hintergewerk.Tests
{
    public class SimpleJsonTests
    {
        static HouseData Sample()
        {
            var h = new HouseData();
            h.rooms.Add(new RoomData("raum_1", "Bad EG — äöü", 0));
            h.anchors.Add(new AnchorData("anc_1", "raum_1", "high", "2026-11-02T09:14:00Z"));

            var o = new TrackedObject
            {
                id = "obj_1",
                type = ObjectType.Heating,
                room_id = "raum_1",
                anchor_id = "anc_1",
                confidence = Confidence.Measured,
                safety_corridor_cm = 6.0
            };
            o.geometry.AddPoint(0.1f, 1.2f, -0.3f);
            o.geometry.AddPoint(1.55f, 1.2f, -0.3f);
            o.geometry.depth_cm = 4.5;
            o.geometry.diameter_cm = 3.5;
            o.evidence.photos.Add("photos/obj_1.jpg");
            o.meta.created_at = "2026-11-02T09:15:00Z";
            o.meta.author = "Mathias";
            o.meta.note = "Achtung \"Zitat\" — linie 2";
            h.objects.Add(o);
            return h;
        }

        [Test]
        public void Roundtrip_ErhaeltAlleFelder()
        {
            var back = SimpleJson.Parse(SimpleJson.Write(Sample()));
            Assert.AreEqual(1, back.rooms.Count);
            Assert.AreEqual("Bad EG — äöü", back.rooms[0].label);
            Assert.AreEqual(1, back.anchors.Count);
            Assert.AreEqual("high", back.anchors[0].confidence);
            Assert.AreEqual(1, back.objects.Count);

            var o = back.objects[0];
            Assert.AreEqual(ObjectType.Heating, o.type);
            Assert.AreEqual(Confidence.Measured, o.confidence);
            Assert.AreEqual(4.5, o.geometry.depth_cm, 1e-9);
            Assert.AreEqual(2, o.geometry.Count);
            Assert.AreEqual(0.1, o.geometry.Point(0)[0], 1e-6);
            Assert.AreEqual(-0.3, o.geometry.Point(0)[2], 1e-6);
            Assert.AreEqual(1, o.evidence.photos.Count);
            Assert.AreEqual("photos/obj_1.jpg", o.evidence.photos[0]);
        }

        [Test]
        public void VoiceNote_BleibtNull()
        {
            var back = SimpleJson.Parse(SimpleJson.Write(Sample()));
            Assert.IsNull(back.objects[0].evidence.voice_note);
        }

        [Test]
        public void Anfuehrungszeichen_Ueberleben()
        {
            var back = SimpleJson.Parse(SimpleJson.Write(Sample()));
            Assert.AreEqual("Achtung \"Zitat\" — linie 2", back.objects[0].meta.note);
        }

        [Test]
        public void UnbekannterKey_WirdUebersprungen()
        {
            const string json = "{\"schema_version\":1,\"rooms\":[],\"anchors\":[],\"objects\":[],\"zukunft\":{\"x\":1}}";
            var h = SimpleJson.Parse(json);
            Assert.AreEqual(0, h.objects.Count);
        }

        [Test]
        public void SchemaVersion_ZuNeu_Wirft()
        {
            const string json = "{\"schema_version\":99,\"rooms\":[],\"anchors\":[],\"objects\":[]}";
            Assert.Throws<FormatException>(() => SimpleJson.Parse(json));
        }

        [Test]
        public void VerwaistesFlag_WirdNichtSerialisiert()
        {
            var h = Sample();
            h.objects[0].orphaned = true;
            StringAssert.DoesNotContain("orphaned", SimpleJson.Write(h));
        }
    }
}
