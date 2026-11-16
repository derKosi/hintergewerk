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
        readonly Func<WallPlane, string> roomIdSource; // D43: Raum aus der gesnappten Wand (Scene/Mock), optional
        bool pinchWasDown;
        Vector3 letzterWeltPunkt;

        /// <summary>Abstand, nach dem ein gehaltener Pinch den nächsten Punkt nachschiebt
        /// (= Rasterweite; doc03 „Linie folgt der Hand", Snap glättet).</summary>
        public float DragSchwelleM { get; set; } = 0.05f;

        void PunktSetzen(Vector3 tipWelt)
        {
            // Reihenfolge wichtig (D43): Wand → Raum → Anker. Der Anker wird mit der
            // AUFGELOSTEN Raum-Id registriert (DrawController liest ActiveRoomId).
            var plane = planeSource?.Invoke() ?? new WallPlane
            {
                Point = Vector3.zero,
                Normal = Vector3.forward,
                Label = "wall"
            };
            ResolveRoom(plane); // lazy beim ersten Punkt (danach fixiert fürs Objekt)
            ResolveAnchor();    // lazy beim ersten Punkt (danach fixiert fürs Objekt)
            var r = snap.Snap(tipWelt, plane, endpointsSource?.Invoke());
            var local = WorldToLocal(activeAnchorPose ?? new Pose(Vector3.zero, Quaternion.identity), r.Point);
            session.AddPoint(local.x, local.y, local.z);
            LastSnap = r;
            letzterWeltPunkt = tipWelt;
            undo?.Push("Punkt entfernen", () => session.UndoLastPoint());
        }

        // Lazy je Objekt aufgelöst: erster gezeichneter Punkt fixiert Anker + Pose + Raum.
        string activeAnchorId;
        Pose? activeAnchorPose;
        string objectRoomId; // null = noch nicht aufgelöst (Raum wird mit dem ersten Punkt fixiert)

        /// <summary>Anker-Pose des laufenden Objekts (nach erstem Punkt) — für die Zeichnen-Vorschau.</summary>
        public Pose? ActiveAnchorPose => activeAnchorPose;
        /// <summary>Anker-Id des laufenden Objekts (leer, bis der erste Punkt gesetzt wurde).</summary>
        public string ActiveAnchorId => activeAnchorId;

        /// <summary>Raum des laufenden Objekts (D43): nach dem ersten Punkt die Wand-Raum-Id,
        /// vorher der Szenen-Standard. Statuszeilen/Anker-Registrierung lesen das.</summary>
        public string ActiveRoomId => objectRoomId ?? roomId;

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
            string roomId,
            Func<WallPlane, string> roomIdSource = null)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.store = store;
            this.snap = snap ?? throw new ArgumentNullException(nameof(snap));
            this.undo = undo;
            this.planeSource = planeSource;
            this.endpointsSource = endpointsSource;
            this.anchorSource = anchorSource;
            this.roomId = roomId ?? "";
            this.roomIdSource = roomIdSource;
        }

        public void Tick(HandFrame right, HandFrame left)
        {
            bool pinch = right.Tracked && right.Pinching;

            switch (session.Phase)
            {
                case CapturePhase.Zeichnen:
                    if (pinch && !pinchWasDown)
                    {
                        PunktSetzen(right.IndexTip);
                    }
                    else if (pinch && (Vector3.Distance(right.IndexTip, letzterWeltPunkt) >= DragSchwelleM))
                    {
                        // Fix 16.11. (Simulator-Befund): Die Tap-Grammatik alone erzwang 2-Punkte-
                        // Objekte — die fallende Flanke des 2. Taps endete den Draw, bevor Ecke 3
                        // kommen konnte (docs/03: „Linie folgt der Hand" = Drag ist der Hauptweg).
                        // Jetzt: gehaltener Pinch fügt alle DragSchwelleM Punkte nach (snappt weiter).
                        PunktSetzen(right.IndexTip);
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
                        var o = session.Confirm(activeAnchorId, objectRoomId ?? roomId);
                        if (o != null && store != null)
                        {
                            store.AddTracked(o);
                            undo?.Push("Objekt entfernen", () => store.RemoveObject(o.id));
                            ObjectConfirmed?.Invoke(o);
                        }
                        activeAnchorId = null;
                        activeAnchorPose = null; // nächstes Objekt löst frisch auf
                        objectRoomId = null;     // nächstes Objekt wählt den Raum neu (Raumwechsel beim Umgehen)
                    }
                    break;
            }

            // Nach JEDER Phase (auch Idle!) buchen — sonst friert die Flanke ein und der
            // erste Pinch nach einem Confirm wird verschluckt (Bug aus Test ZweiObjekte_…).
            pinchWasDown = pinch;
        }

        /// <summary>
        /// D43 — automatische Raum-Zuordnung: Die gesnappte Wand trägt RoomId/RoomLabel (Scene API:
        /// OVRSceneRoom-Zuordnung + semantisches Label; Mock: Mock-Raum). Der Raum wird beim ersten
        /// Punkt fixiert und im Store angelegt (nur Räume MIT Objekten existieren → kein leerer
        /// Raum im Export). Ohne RoomId an der Wand gilt der Szenen-Standard-Raum — nichts erfunden.
        /// </summary>
        void ResolveRoom(WallPlane plane)
        {
            if (objectRoomId != null) return; // einmal pro Objekt fixiert
            var id = roomIdSource?.Invoke(plane);
            objectRoomId = string.IsNullOrEmpty(id) ? roomId : id;
            var label = string.IsNullOrEmpty(plane.RoomLabel) ? objectRoomId : plane.RoomLabel;
            store?.EnsureRoom(objectRoomId, label, 0); // idempotent; bestehendes Label bleibt
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
