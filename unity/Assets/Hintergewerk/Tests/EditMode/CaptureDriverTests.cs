// Integrationstest ohne Gerät: Skript-Hände → CaptureDriver → State Machine → Store → JSON.
// Das ist der komplette Capture-Flow, wie er morgen am Gerät läuft — nur mit Mocks.
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
    public class CaptureDriverTests
    {
        HouseDataStore store;
        CaptureSession session;
        CaptureDriver driver;
        InMemoryAnchorProvider provider;
        string anchorId;

        [SetUp]
        public void Setup()
        {
            InMemoryAnchorProvider.ResetSharedStore();

            var path = Path.Combine(Application.temporaryCachePath, "hg_driver_" + Guid.NewGuid().ToString("N") + ".json");
            store = new HouseDataStore(path);
            store.EnsureRoom("raum_1", "Testraum", 0);

            provider = new InMemoryAnchorProvider();
            anchorId = provider.CreateAnchor(new Pose(Vector3.zero, Quaternion.identity), "raum_1");
            store.RegisterAnchor(anchorId, "raum_1");

            session = new CaptureSession();
            driver = new CaptureDriver(session, store, new SnapEngine { GridCm = 5f, MagnetRadiusCm = 8f },
                new UndoStack(),
                () => new WallPlane { Point = Vector3.zero, Normal = Vector3.forward, Label = "wall" },
                () => Array.Empty<(Vector3 pos, string id)>(),
                () => anchorId,
                "raum_1");

            session.BeginTypwahl();
            session.SelectType(ObjectType.Power);
        }

        static HandFrame Right(Vector3 tip, bool pinching) => ScriptedHandSource.Frame(tip, pinching);
        static HandFrame Left(Vector3 tip) => ScriptedHandSource.Frame(tip, false);

        /// <summary>Ein vollständiges Objekt: 2× (Pinch down/up) → Tiefe → Bestaetigen → Confirm.</summary>
        void ZeichneObjekt(Vector3 p1, Vector3 p2, Vector3 leftTip, ObjectType? t = null)
        {
            if (t.HasValue)
            {
                session.BeginTypwahl();
                session.SelectType(t.Value);
            }
            driver.Tick(Right(p1, true), Left(Vector3.zero));    // Pinch down → Punkt 1 (gesnappt)
            driver.Tick(Right(p1, false), Left(Vector3.zero));   // Pinch up
            driver.Tick(Right(p2, true), Left(Vector3.zero));    // Pinch down → Punkt 2
            driver.Tick(Right(p2, false), Left(Vector3.zero));   // Pinch up → EndDraw → Tiefe
            driver.Tick(Right(Vector3.zero, true), Left(leftTip));  // Pinch → Bestaetigen
            driver.Tick(Right(Vector3.zero, false), Left(leftTip));
            driver.Tick(Right(Vector3.zero, true), Left(leftTip));  // Pinch → Confirm
            driver.Tick(Right(Vector3.zero, false), Left(Vector3.zero));
        }

        [Test]
        public void KompletterFlow_PinchBisStore()
        {
            Assert.AreEqual(CapturePhase.Zeichnen, session.Phase);

            // Pinch steigend: Punkt snappen und adden (z auf Ebene, x/y aufs 5-cm-Raster)
            driver.Tick(Right(new Vector3(0.12f, 0.13f, 0.4f), true), Left(Vector3.zero));
            Assert.AreEqual(1, session.Punkte.Count);
            Assert.AreEqual(0.10, session.Punkte[0][0], 1e-4);
            Assert.AreEqual(0.15, session.Punkte[0][1], 1e-4);
            Assert.AreEqual(0.0, session.Punkte[0][2], 1e-4);

            // Pinch gehalten, Hand bewegt: KEIN neuer Punkt (nur steigende Flanke addet)
            driver.Tick(Right(new Vector3(0.5f, 0.5f, 0.4f), true), Left(Vector3.zero));
            Assert.AreEqual(1, session.Punkte.Count);

            // Pinch fallend: EndDraw bricht ab (nur 1 Punkt) — Phase bleibt Zeichnen
            driver.Tick(Right(new Vector3(0.5f, 0.5f, 0.4f), false), Left(Vector3.zero));
            Assert.AreEqual(CapturePhase.Zeichnen, session.Phase);

            // Zweiter Pinch: Punkt 2 → danach EndDraw → Tiefe
            driver.Tick(Right(new Vector3(0.52f, 0.53f, 0.4f), true), Left(Vector3.zero));
            Assert.AreEqual(2, session.Punkte.Count);
            driver.Tick(Right(new Vector3(0.52f, 0.53f, 0.4f), false), Left(Vector3.zero));
            Assert.AreEqual(CapturePhase.Tiefe, session.Phase);

            // Zwei-Hand-Distanz setzt Tiefe: 12 cm
            driver.Tick(Right(Vector3.zero, false), Left(new Vector3(0.12f, 0f, 0f)));
            Assert.AreEqual(12.0, session.DepthCm, 1e-3);

            // Pinch → Bestaetigen, Freigabe, Pinch → Confirm → Store
            driver.Tick(Right(Vector3.zero, true), Left(new Vector3(0.12f, 0f, 0f)));
            Assert.AreEqual(CapturePhase.Bestaetigen, session.Phase);
            driver.Tick(Right(Vector3.zero, false), Left(Vector3.zero));
            driver.Tick(Right(Vector3.zero, true), Left(Vector3.zero));

            Assert.AreEqual(CapturePhase.Idle, session.Phase);
            Assert.AreEqual(1, store.Data.objects.Count);

            var o = store.Data.objects[0];
            Assert.AreEqual(ObjectType.Power, o.type);
            Assert.AreEqual(anchorId, o.anchor_id);
            Assert.AreEqual(12.0, o.geometry.depth_cm, 1e-9);
            Assert.AreEqual(2, o.geometry.Count);
            Assert.AreEqual(0.10, o.geometry.Point(0)[0], 1e-6);
        }

        [Test]
        public void ZweiObjekte_UeberlebenStoreReload()
        {
            ZeichneObjekt(new Vector3(0.10f, 0.15f, 0.2f), new Vector3(0.80f, 0.15f, 0.2f), Vector3.zero, ObjectType.Power);
            ZeichneObjekt(new Vector3(0.20f, 0.25f, 0.2f), new Vector3(0.90f, 0.25f, 0.2f), new Vector3(0.12f, 0f, 0f), ObjectType.Water);

            Assert.AreEqual(2, store.Data.objects.Count);

            var reloaded = new HouseDataStore(store.Path);
            Assert.AreEqual(2, reloaded.Data.objects.Count);
            Assert.AreEqual(0, reloaded.OrphanedObjects().Count);
            Assert.AreEqual(ObjectType.Power, reloaded.Data.objects[0].type);
            Assert.AreEqual(ObjectType.Water, reloaded.Data.objects[1].type);
        }

        [Test]
        public void Undo_EntferntGespeichertesObjekt()
        {
            ZeichneObjekt(new Vector3(0.10f, 0.15f, 0.2f), new Vector3(0.80f, 0.15f, 0.2f), Vector3.zero);
            Assert.AreEqual(1, store.Data.objects.Count);

            var stack = new UndoStack();
            var o = store.Data.objects[0];
            stack.Push("Objekt entfernen", () => store.RemoveObject(o.id));
            stack.TryUndo(out var label);
            Assert.AreEqual("Objekt entfernen", label);
            Assert.AreEqual(0, store.Data.objects.Count);
        }

        [Test]
        public void MockProvider_SimuliertNeustart()
        {
            provider.CreateAnchor(new Pose(new Vector3(1f, 0f, 0f), Quaternion.identity), "raum_1");
            Assert.AreEqual(2, provider.KnownAnchorIds is ICollection<string> c ? c.Count : 0);

            // „Neustart": neue Instanz, gleicher geteilter Speicher
            var neu = new InMemoryAnchorProvider();
            Assert.AreEqual(0, ((ICollection<string>)neu.KnownAnchorIds).Count);
            neu.LoadAll(ok => { });
            Assert.AreEqual(2, ((ICollection<string>)neu.KnownAnchorIds).Count);
            Assert.IsTrue(neu.TryGetPose(anchorId, out var pose));
            Assert.AreEqual(Vector3.zero, pose.position);
        }

        [Test]
        public void MockProvider_FailLoadAll_TriggertFallback()
        {
            provider.FailLoadAll = true;
            bool? result = null;
            provider.LoadAll(ok => result = ok);
            Assert.AreEqual(false, result.Value);
        }

        [Test]
        public void MockWallProvider_RaycastTrifftEbene()
        {
            var walls = new MockWallProvider();
            Assert.IsTrue(walls.TryFindPlane(new Vector3(0f, 0f, 1f), Vector3.back, 2f, out var plane));
            Assert.AreEqual("wall", plane.Label);
            Assert.IsFalse(walls.TryFindPlane(new Vector3(0f, 0f, 1f), Vector3.forward, 2f, out _)); // weg von der Ebene
        }
    }
}
