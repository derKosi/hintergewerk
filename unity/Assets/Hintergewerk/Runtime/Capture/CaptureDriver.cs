// Hintergewerk — CaptureDriver: die Klebstelle zwischen Eingabe (Hände), Snapping,
// Zustandsmaschine und Store. Rein testbar: Frames hinein (Tick), Objekte im Store raus.
// Der MonoBehaviour-DrawController (auf dem Gerät) ruft nur Tick() auf — keine Logik im MB.
//
// Gesten-Mapping (docs/03 §1, Skeleton-Version):
//   Zeichnen:    Pinch steigend  → Punkt snappen + adden · Pinch fallend → EndDraw
//   Tiefe:       Zwei-Hand-Distanz setzt Tiefe · Pinch steigend → Bestaetigen
//   Bestaetigen: Pinch steigend  → Confirm → Store
// Jede Aktion pusht auf den Undo-Stapel („Wischen = rückgängig").
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
        readonly Func<string> anchorIdSource;
        readonly string roomId;
        bool pinchWasDown;

        public SnapResult LastSnap { get; private set; }

        public CaptureDriver(CaptureSession session, HouseDataStore store, SnapEngine snap, UndoStack undo,
            Func<WallPlane> planeSource,
            Func<IReadOnlyList<(Vector3 pos, string id)>> endpointsSource,
            Func<string> anchorIdSource,
            string roomId)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.store = store;
            this.snap = snap ?? throw new ArgumentNullException(nameof(snap));
            this.undo = undo;
            this.planeSource = planeSource;
            this.endpointsSource = endpointsSource;
            this.anchorIdSource = anchorIdSource;
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
                        var plane = planeSource?.Invoke() ?? new WallPlane
                        {
                            Point = Vector3.zero,
                            Normal = Vector3.forward,
                            Label = "wall"
                        };
                        var r = snap.Snap(right.IndexTip, plane, endpointsSource?.Invoke());
                        session.AddPoint(r.Point.x, r.Point.y, r.Point.z);
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
                        var o = session.Confirm(anchorIdSource?.Invoke() ?? "", roomId);
                        if (o != null && store != null)
                        {
                            store.AddTracked(o);
                            undo?.Push("Objekt entfernen", () => store.RemoveObject(o.id));
                        }
                    }
                    break;
            }

            // Nach JEDER Phase (auch Idle!) buchen — sonst friert die Flanke ein und der
            // erste Pinch nach einem Confirm wird verschluckt (Bug aus Test ZweiObjekte_…).
            pinchWasDown = pinch;
        }
    }
}
