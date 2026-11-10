// D43 — Automatische Raum-Zuordnung: Die gesnappte Wand bestimmt den Raum des Objekts.
// SDK-frei getestet (Driver + Store + Mock); der Scene-Teil (OVRSceneRoom→RoomId) läuft
// im Adapter und wird am Gerät verifiziert (docs/07 §G-Erweiterung).
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Hintergewerk.Capture;
using Hintergewerk.Core;
using Hintergewerk.Data;
using Hintergewerk.Mocks;

namespace Hintergewerk.Tests
{
    public class RoomAssignmentTests
    {
        HouseDataStore store;
        CaptureSession session;
        InMemoryAnchorProvider provider;
        string anchorId;
        SnapEngine snap;

        [SetUp]
        public void Setup()
        {
            InMemoryAnchorProvider.ResetSharedStore();
            var path = Path.Combine(Application.temporaryCachePath, "hg_room_" + Guid.NewGuid().ToString("N") + ".json");
            store = new HouseDataStore(path);
            provider = new InMemoryAnchorProvider();
            anchorId = provider.CreateAnchor(new Pose(Vector3.zero, Quaternion.identity), "raum_fallback");
            store.RegisterAnchor(anchorId, "raum_fallback");
            session = new CaptureSession();
            snap = new SnapEngine { GridCm = 5f, MagnetRadiusCm = 8f };
            session.BeginTypwahl();
            session.SelectType(ObjectType.Power);
        }

        CaptureDriver NewDriver(Func<WallPlane> plane, string fallbackRoomId,
            Func<WallPlane, string> roomIdSource = null)
        {
            return new CaptureDriver(session, store, snap, new UndoStack(),
                plane, () => Array.Empty<(Vector3 pos, string id)>(),
                () => (anchorId, new Pose(Vector3.zero, Quaternion.identity)),
                fallbackRoomId, roomIdSource);
        }

        static HandFrame Right(Vector3 tip, bool pinching) => ScriptedHandSource.Frame(tip, pinching);
        static HandFrame Left(Vector3 tip) => ScriptedHandSource.Frame(tip, false);

        /// <summary>Kompletter Objekt-Durchlauf am Driver (2 Punkte → Tiefe → Confirm).</summary>
        void ZeichneObjekt(CaptureDriver d, Vector3 p1, Vector3 p2)
        {
            d.Tick(Right(p1, true), Left(Vector3.zero));
            d.Tick(Right(p1, false), Left(Vector3.zero));
            d.Tick(Right(p2, true), Left(Vector3.zero));
            d.Tick(Right(p2, false), Left(Vector3.zero));
            d.Tick(Right(Vector3.zero, true), Left(new Vector3(0.12f, 0f, 0f)));  // Tiefe 12 cm
            d.Tick(Right(Vector3.zero, false), Left(new Vector3(0.12f, 0f, 0f)));
            d.Tick(Right(Vector3.zero, true), Left(new Vector3(0.12f, 0f, 0f)));  // Confirm
            d.Tick(Right(Vector3.zero, false), Left(Vector3.zero));
            Assert.AreEqual(CapturePhase.Idle, session.Phase);
        }

        [Test]
        public void WandMitRaumId_ObjektLandetImWandRaum()
        {
            var d = NewDriver(
                () => new WallPlane { Point = Vector3.zero, Normal = Vector3.forward, Label = "wall", RoomId = "uuid_szene_a", RoomLabel = "LIVING_ROOM" },
                "raum_fallback",
                plane => plane.RoomId);

            d.Tick(Right(new Vector3(0.10f, 0.15f, 0.2f), true), Left(Vector3.zero));  // Punkt 1 → Raum aktiv
            Assert.AreEqual("uuid_szene_a", d.ActiveRoomId); // während des Zeichnens (nach Confirm fällt ActiveRoomId bewusst auf den Standard zurück)
            d.Tick(Right(new Vector3(0.10f, 0.15f, 0.2f), false), Left(Vector3.zero));
            d.Tick(Right(new Vector3(0.80f, 0.15f, 0.2f), true), Left(Vector3.zero));
            d.Tick(Right(new Vector3(0.80f, 0.15f, 0.2f), false), Left(Vector3.zero));
            d.Tick(Right(Vector3.zero, true), Left(new Vector3(0.12f, 0f, 0f)));  // Tiefe
            d.Tick(Right(Vector3.zero, false), Left(new Vector3(0.12f, 0f, 0f)));
            d.Tick(Right(Vector3.zero, true), Left(Vector3.zero));                // Confirm
            d.Tick(Right(Vector3.zero, false), Left(Vector3.zero));
            Assert.AreEqual(CapturePhase.Idle, session.Phase);

            Assert.AreEqual(1, store.Data.objects.Count);
            Assert.AreEqual("uuid_szene_a", store.Data.objects[0].room_id);
            var room = store.Data.rooms.Find(r => r.id == "uuid_szene_a");
            Assert.IsNotNull(room);
            Assert.AreEqual("LIVING_ROOM", room.label);
        }

        [Test]
        public void WandOhneRaumId_FaelltAufStandardRaum()
        {
            var d = NewDriver(
                () => new WallPlane { Point = Vector3.zero, Normal = Vector3.forward, Label = "wall" },
                "raum_fallback",
                plane => plane.RoomId);

            ZeichneObjekt(d, new Vector3(0.10f, 0.15f, 0.2f), new Vector3(0.80f, 0.15f, 0.2f));

            Assert.AreEqual(1, store.Data.objects.Count);
            Assert.AreEqual("raum_fallback", store.Data.objects[0].room_id);
        }

        [Test]
        public void OhneRaumQuelle_VerhaeltSichWieVorher() // Regressions-Wächter: alter 8-Arg-CTOR
        {
            var d = NewDriver(
                () => new WallPlane { Point = Vector3.zero, Normal = Vector3.forward, Label = "wall", RoomId = "uuid_szene_a" },
                "raum_fallback",
                roomIdSource: null);

            ZeichneObjekt(d, new Vector3(0.10f, 0.15f, 0.2f), new Vector3(0.80f, 0.15f, 0.2f));

            Assert.AreEqual("raum_fallback", store.Data.objects[0].room_id);
        }

        [Test]
        public void ObjektWechselRaum_WandBestimmtJeObjekt()
        {
            var wallA = new WallPlane { Point = Vector3.zero, Normal = Vector3.forward, Label = "wall", RoomId = "uuid_szene_a", RoomLabel = "LIVING_ROOM" };
            var wallB = new WallPlane { Point = Vector3.one, Normal = Vector3.right, Label = "wall", RoomId = "uuid_szene_b", RoomLabel = "BEDROOM" };
            var current = wallA;
            var d = NewDriver(() => current, "raum_fallback", plane => plane.RoomId);

            ZeichneObjekt(d, new Vector3(0.10f, 0.15f, 0.2f), new Vector3(0.80f, 0.15f, 0.2f));
            current = wallB;
            session.BeginTypwahl();
            session.SelectType(ObjectType.Water);
            ZeichneObjekt(d, new Vector3(0.20f, 0.25f, 0.2f), new Vector3(0.90f, 0.25f, 0.2f));

            Assert.AreEqual("uuid_szene_a", store.Data.objects[0].room_id);
            Assert.AreEqual("uuid_szene_b", store.Data.objects[1].room_id);
            Assert.AreEqual(2, store.Data.rooms.Count); // nur Räume MIT Objekten existieren
        }

        [Test]
        public void RaumNurBeimErstenPunktFixiert()
        {
            var wallA = new WallPlane { Point = Vector3.zero, Normal = Vector3.forward, Label = "wall", RoomId = "uuid_szene_a" };
            var wallB = new WallPlane { Point = Vector3.one, Normal = Vector3.right, Label = "wall", RoomId = "uuid_szene_b" };
            var current = wallA;
            var d = NewDriver(() => current, "raum_fallback", plane => plane.RoomId);

            d.Tick(Right(new Vector3(0.10f, 0.15f, 0.2f), true), Left(Vector3.zero));  // Punkt 1 → Raum fixiert
            d.Tick(Right(new Vector3(0.10f, 0.15f, 0.2f), false), Left(Vector3.zero)); // Flanke sauber lösen
            current = wallB;
            d.Tick(Right(new Vector3(0.80f, 0.15f, 0.2f), true), Left(Vector3.zero));  // Punkt 2 an anderer Wand
            d.Tick(Right(new Vector3(0.80f, 0.15f, 0.2f), false), Left(Vector3.zero)); // EndDraw → Tiefe
            d.Tick(Right(Vector3.zero, true), Left(new Vector3(0.12f, 0f, 0f)));       // Tiefe → Bestaetigen
            d.Tick(Right(Vector3.zero, false), Left(Vector3.zero));
            d.Tick(Right(Vector3.zero, true), Left(Vector3.zero));                     // Confirm
            d.Tick(Right(Vector3.zero, false), Left(Vector3.zero));

            Assert.AreEqual(CapturePhase.Idle, session.Phase);
            Assert.AreEqual("uuid_szene_a", store.Data.objects[0].room_id); // erster Punkt gewinnt
            Assert.IsFalse(store.Data.rooms.Exists(r => r.id == "uuid_szene_b"));
        }

        [Test]
        public void EnsureRoom_Idempotent_LabelBleibt()
        {
            store.EnsureRoom("r1", "Erster", 0);
            store.EnsureRoom("r1", "Zweiter", 0);
            Assert.AreEqual("Erster", store.Data.rooms.Find(r => r.id == "r1").label);
            Assert.AreEqual(1, store.Data.rooms.Count);
        }

        [Test]
        public void MockRaum_TraegtRaumAufAllenWaenden()
        {
            var mock = MockWallProvider.Room(new Vector3(-1.5f, 0f, -2.5f), 3f, 3f, 2.6f);
            Assert.AreEqual(4, mock.Walls.Count);
            foreach (var w in mock.Walls)
            {
                Assert.AreEqual("raum_1", w.RoomId);
                Assert.AreEqual("Mock-Raum", w.RoomLabel);
            }

            // Capture am Mock-Wand-Raum: landet automatisch in raum_1 (wie bisher — aber jetzt aus der Wand)
            store.EnsureRoom("raum_fallback", "Fallback", 0);
            var d = NewDriver(() => mock.Walls[0], "raum_fallback", plane => plane.RoomId);
            ZeichneObjekt(d, new Vector3(0.10f, 0.15f, 0.2f), new Vector3(0.80f, 0.15f, 0.2f));
            Assert.AreEqual("raum_1", store.Data.objects[0].room_id);
        }
    }
}
