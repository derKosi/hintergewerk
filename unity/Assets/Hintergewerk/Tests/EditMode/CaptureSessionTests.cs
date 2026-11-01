// Tests für die Capture-Zustandsmaschine: legale/ilegale Übergänge, Confirm-Ergebnis.
using NUnit.Framework;
using Hintergewerk.Capture;
using Hintergewerk.Data;

namespace Hintergewerk.Tests
{
    public class CaptureSessionTests
    {
        static CaptureSession InZeichnen()
        {
            var s = new CaptureSession();
            s.BeginTypwahl();
            s.SelectType(ObjectType.Power);
            return s;
        }

        [Test]
        public void IllegalerStart_OhneTypwahl()
        {
            var s = new CaptureSession();
            Assert.IsFalse(s.SelectType(ObjectType.Power));
            Assert.IsFalse(s.AddPoint(0, 0, 0));
            Assert.AreEqual(CapturePhase.Idle, s.Phase);
        }

        [Test]
        public void GluecklicherFlow_IdeeBisConfirm()
        {
            var s = InZeichnen();
            Assert.AreEqual(CapturePhase.Zeichnen, s.Phase);
            Assert.AreEqual(4.0, s.DepthCm, 1e-9); // Power-Preset

            s.AddPoint(0.1, 1.2, 0.0);
            s.AddPoint(0.8, 1.2, 0.0);
            Assert.IsTrue(s.UndoLastPoint()); // Wischen
            Assert.AreEqual(1, s.Punkte.Count);
            s.AddPoint(0.8, 1.2, 0.0);

            Assert.IsTrue(s.EndDraw());
            Assert.AreEqual(CapturePhase.Tiefe, s.Phase);

            Assert.IsTrue(s.SetDepth(7.5));
            Assert.AreEqual(7.5, s.DepthCm, 1e-9);
            Assert.IsTrue(s.SetDepth(99));     // Clamp
            Assert.AreEqual(30.0, s.DepthCm, 1e-9);

            Assert.IsTrue(s.BeginBestaetigen());
            Assert.IsTrue(s.SetConfidence(Confidence.Measured));

            var o = s.Confirm("anc_1", "raum_1");
            Assert.IsNotNull(o);
            Assert.AreEqual(ObjectType.Power, o.type);
            Assert.AreEqual("anc_1", o.anchor_id);
            Assert.AreEqual("raum_1", o.room_id);
            Assert.AreEqual(2, o.geometry.Count);
            Assert.AreEqual(30.0, o.geometry.depth_cm, 1e-9);
            Assert.AreEqual(Confidence.Measured, o.confidence);
            Assert.IsFalse(string.IsNullOrEmpty(o.meta.created_at)); // created_at gesetzt
            Assert.AreEqual(CapturePhase.Idle, s.Phase); // Session zurückgesetzt
        }

        [Test]
        public void EndDraw_Mindestens2Punkte()
        {
            var s = InZeichnen();
            s.AddPoint(0, 0, 0);
            Assert.IsFalse(s.EndDraw());
            s.AddPoint(1, 0, 0);
            Assert.IsTrue(s.EndDraw());
        }

        [Test]
        public void UndoLastPoint_NurInZeichnenUndNurMitPunkten()
        {
            var s = InZeichnen();
            Assert.IsFalse(s.UndoLastPoint()); // leer
            s.AddPoint(0.1, 0.2, 0.3);
            Assert.IsTrue(s.UndoLastPoint());
            Assert.AreEqual(0, s.Punkte.Count);
            s.EndDraw();
            Assert.IsFalse(s.UndoLastPoint()); // falsche Phase
        }

        [Test]
        public void SetConfidence_NurSpätImFlow()
        {
            var s = InZeichnen();
            Assert.IsFalse(s.SetConfidence(Confidence.Measured)); // Zeichnen: nicht erlaubt
            s.AddPoint(0, 0, 0);
            s.AddPoint(1, 0, 0);
            s.EndDraw();
            Assert.IsTrue(s.SetConfidence(Confidence.Measured)); // Tiefe: erlaubt
        }

        [Test]
        public void Confirm_NurAusBestaetigen()
        {
            var s = InZeichnen();
            Assert.IsNull(s.Confirm("anc_1", "raum_1"));
        }

        [Test]
        public void Cancel_RäumtAlles()
        {
            var s = InZeichnen();
            s.AddPoint(0, 0, 0);
            Assert.IsTrue(s.Cancel());
            Assert.AreEqual(CapturePhase.Idle, s.Phase);
            Assert.AreEqual(0, s.Punkte.Count);
            Assert.IsNull(s.Type);
        }
    }
}
