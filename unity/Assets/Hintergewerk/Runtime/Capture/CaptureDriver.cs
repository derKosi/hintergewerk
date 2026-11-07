// Hintergewerk — CaptureDriver: die Klebstelle zwischen Eingabe (Hände), Snapping,
// Zustandsmaschine und Store. Rein testbar: Frames hinein (Tick), Objekte im Store raus.
// Der MonoBehaviour-DrawController (auf dem Gerät) ruft nur Tick() auf — keine Logik im MB.
//
// Gesten-Mapping (docs/03 §1, Skeleton-Version):
//   Zeichnen:    Pinch steigend  → Punkt snappen + adden · Pinch fallend → EndDraw
//   Tiefe:       Zwei-Hand-Distanz setzt Tiefe · Pinch steigend → Bestaetigen
//   Bestaetigen: Pinch steigend  → Confirm → Store
// Jede Aktion pusht auf den Undo-Stapel („Wischen = rückgängig").
//
// ADR 0002 (Invarianten §2): Punkte werden ANKER-LOKAL gespeichert. Der Driver löst beim
// ersten Punkt Anker + Pose auf (lazy) und transformiert jeden Weltpunkt mit Pose⁻¹ nach
// lokal. Recall löst später lokal → Welt auf (CorridorFactory.ToWorld) — Roundtrip bleibt
// deckungsgleich, auch wenn die Anker-Pose nach Re-Lokalisierung abweicht.
// (Fix 07.11.: vorher wurden Welt-Punkte gespeichert und beim Recall NOCHMAL transformiert
//  → doppelter Versatz um die Anker-Pose.)
using System;
using System.Collections.Generic;
using UnityEngine;
using Hintergewerk.Core;
using Hintergewerk.Data;

namespace Hintergewerk.Capture
{
    public class CaptureDriver
    {
        readonly CaptureSession session;
        readonly HouseDataStore store;
        readonly SnapEngine snap;
        readonly UndoStack undo;
        readonly Func<WallPlane> planeSource;
        readonly Func<IReadOnlyList<(Vector3 pos, string id)>> endpointsSource;
        readonly Func<(string anchorId, Pose pose)> anchorSource;
        readonly string roomId;
        bool pinchWasDown;

        // Lazy je Objekt aufgelöst: erster gezeichneter Punkt fixiert Anker + Pose.
        string activeAnchorId;
        Pose? activeAnchorPose;

        /// <summary>Anker-Pose des laufenden Objekts (nach erstem Punkt) — für die Zeichnen-Vorschau.</summary>
        public Pose? ActiveAnchorPose => activeAnchorPose;
        /// <summary>Anker-Id des laufenden Objekts (leer, bis der erste Punkt gesetzt wurde).</summary>
        public string ActiveAnchorId => activeAnchorId;

        public SnapResult LastSnap { get; private set; }

        /// <summary>Frisch bestätigtes Objekt (nach Store-Eintrag) — UI hängt hier z. B. das Beweisfoto an (D8).</summary>
        public event Action<Data.TrackedObject> ObjectConfirmed;

        public CaptureDriver(CaptureSession session, HouseDataStore store, SnapEngine snap, UndoStack undo,
            Func<WallPlane> planeSource,
            Func<IReadOnlyList<(Vector3 pos, string id)>> endpointsSource,
            Func<string> anchorIdSource,
            string roomId)
            : this(session, store, snap, undo, planeSource, endpointsSource,
                anchorIdSource == null
                    ? null
                    : (Func<(string anchorId, Pose pose)>)(() => (anchorIdSource(), new Pose(Vector3.zero, Quaternion.identity))),
                roomId) { }

        public CaptureDriver(CaptureSession session, HouseDataStore store, SnapEngine snap, UndoStack undo,
            Func<WallPlane> planeSource,
            Func<IReadOnlyList<(Vector3 pos, string id)>> endpointsSource,
            Func<(string anchorId, Pose pose)> anchorSource,
            string roomId)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.store = store;
            this.snap = snap ?? throw new ArgumentNullException(nameof(snap));
            this.undo = undo;
            this.planeSource = planeSource;
            this.endpointsSource = endpointsSource;
            this.anchorSource = anchorSource;
            this.roomId = roomId ?? "";
        }

        public void Tick(HandFrame right, HandFrame left)
        {
            bool pinch = right.Tracked && right.Pinching;

            switch (session.Phase)
            {
                case CapturePhase.Zeichnen:
                    if (pinch && !pinchWasDown)
                    {
                        ResolveAnchor(); // lazy beim ersten Punkt (danach fixiert fürs Objekt)
                        var plane = planeSource?.Invoke() ?? new WallPlane
                        {
                            Point = Vector3.zero,
                            Normal = Vector3.forward,
                            Label = "wall"
                        };
                        var r = snap.Snap(right.IndexTip, plane, endpointsSource?.Invoke());
                        var local = WorldToLocal(activeAnchorPose ?? new Pose(Vector3.zero, Quaternion.identity), r.Point);
                        session.AddPoint(local.x, local.y, local.z);
                        LastSnap = r;
                        undo?.Push("Punkt entfernen", () => session.UndoLastPoint());
                    }
                    if (!pinch && pinchWasDown) session.EndDraw();
                    break;

                case CapturePhase.Tiefe:
                    if (right.Tracked && left.Tracked)
                        session.SetDepth(SnapEngine.DepthFromHandDistance(left.IndexTip, right.IndexTip));
                    if (pinch && !pinchWasDown) session.BeginBestaetigen();
                    break;

                case CapturePhase.Bestaetigen:
                    if (pinch && !pinchWasDown)
                    {
                        var o = session.Confirm(activeAnchorId, roomId);
                        if (o != null && store != null)
                        {
                            store.AddTracked(o);
                            undo?.Push("Objekt entfernen", () => store.RemoveObject(o.id));
                            ObjectConfirmed?.Invoke(o);
                        }
                        activeAnchorId = null;
                        activeAnchorPose = null; // nächstes Objekt löst frisch auf
                    }
                    break;
            }

            // Nach JEDER Phase (auch Idle!) buchen — sonst friert die Flanke ein und der
            // erste Pinch nach einem Confirm wird verschluckt (Bug aus Test ZweiObjekte_…).
            pinchWasDown = pinch;
        }

        /// <summary>Anker + Pose lazy auflösen (einmal pro Objekt). Ohne Quelle: Identity (Punkte bleiben wie gezeichnet).</summary>
        void ResolveAnchor()
        {
            if (activeAnchorPose != null) return;
            if (anchorSource == null) { activeAnchorId = ""; activeAnchorPose = new Pose(Vector3.zero, Quaternion.identity); return; }
            var (id, pose) = anchorSource();
            activeAnchorId = id ?? "";
            activeAnchorPose = pose;
        }

        /// <summary>ADR 0002: Welt → anker-lokal (Punkt_world = AnchorPose × Punkt_lokal).</summary>
        public static Vector3 WorldToLocal(Pose anchorPose, Vector3 world) =>
            Quaternion.Inverse(anchorPose.rotation) * (world - anchorPose.position);

        /// <summary>Gegenstück für Vorschau/Recall: anker-lokal → Welt.</summary>
        public static Vector3 LocalToWorld(Pose anchorPose, Vector3 local) =>
            anchorPose.position + anchorPose.rotation * local;
    }
}
