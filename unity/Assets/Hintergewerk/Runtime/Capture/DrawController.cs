// Hintergewerk — der MonoBehaviour am Gerät: füttert den CaptureDriver mit echten Händen.
// Keine Logik hier — nur Verkabelung (Interfaces, Inspector) und Status-Anzeige.
// Die Logik lebt in CaptureSession + CaptureDriver (getestet, SDK-frei).
// Menü: Handfläche zum Körper öffnet, Pinch zählt den Typ weiter, Handfläche weg wählt.
//
// 07.11. (M2-Finish):
//  - ADR 0002: Driver bekommt (Anker-Id, Anker-Pose) — Punkte werden anker-lokal gespeichert
//  - Zeichnen-Vorschau: Linie folgt der Hand (docs/03 §1) + Snap-Marker, in Typ-Farbe
//  - Auto-Start nur EINMAL beim Start (vorher: nach jedem Confirm sofort neuer Capture-Loop)
//  - Beweisfoto (D8): optionales IEvidenceCapture — fehlgeschlagen wird ehrlich gemeldet
using UnityEngine;
using Hintergewerk.Core;
using Hintergewerk.Data;
using Hintergewerk.Localization;
using Hintergewerk.Recall;
using Hintergewerk.UI;

namespace Hintergewerk.Capture
{
    public class DrawController : MonoBehaviour
    {
        [Header("Adapter (MonoBehaviours, implementieren die Interfaces)")]
        [SerializeField] MonoBehaviour handSourceBehaviour;     // OvrHandSource
        [SerializeField] MonoBehaviour wallProviderBehaviour;   // SceneWallProvider (optional)
        [SerializeField] MonoBehaviour anchorProviderBehaviour; // OvrAnchorProvider
        [SerializeField] MonoBehaviour evidenceBehaviour;       // AppSnapshotEvidence (optional, D8)

        [Header("Capture")]
        [SerializeField] ObjectType startType = ObjectType.Power;
        [SerializeField] float gridCm = 5f;
        [SerializeField] float magnetRadiusCm = 8f;
        [SerializeField] string roomId = "raum_1";
        [SerializeField] TextMesh statusText;
        [SerializeField] float previewWidthM = 0.018f;

        IHandSource hands;
        IWallProvider walls;
        IAnchorProvider anchors;
        IEvidenceCapture evidence;
        HouseDataStore store;
        SnapEngine snap;
        UndoStack undo;
        CaptureSession session;
        CaptureDriver driver;
        HandMenuLogic menu;

        Vector3 lastTip;
        bool pinchWasDown;
        bool menuWasOpen;
        bool autoStarted;
        SnapResult shownSnap;
        LineRenderer preview;
        GameObject snapMarker;

        void Start()
        {
            hands = handSourceBehaviour as IHandSource;
            walls = wallProviderBehaviour as IWallProvider;
            anchors = anchorProviderBehaviour as IAnchorProvider;
            evidence = evidenceBehaviour as IEvidenceCapture;
            if (hands == null || anchors == null)
            {
                Debug.LogError("[DrawController] Adapter fehlen oder implementieren die Interfaces nicht.");
                enabled = false;
                return;
            }

            store = new HouseDataStore();
            snap = new SnapEngine { GridCm = gridCm, MagnetRadiusCm = magnetRadiusCm };
            undo = new UndoStack();
            session = new CaptureSession();
            session.PhaseChanged += p => { SetStatus(PhaseText(p)); if (p == CapturePhase.Idle) FadeOutPreview(); };
            menu = new HandMenuLogic();

            // CaptureDriver mit Anker-Pose (ADR 0002): Anker lazy beim ersten Punkt,
            // Punkte → anker-lokal. Zeichnen snappt auf die Wand, die der Hand am nächsten liegt.
            driver = new CaptureDriver(session, store, snap, undo,
                walls != null ? (System.Func<WallPlane>)(() =>
                {
                    var ws = walls.GetWalls();
                    if (ws == null || ws.Count == 0)
                        return new WallPlane { Point = Vector3.zero, Normal = Vector3.forward, Label = "default" };
                    if (ws.Count == 1) return ws[0];
                    // Mehrere Wände (Scene API): die Ebene mit dem geringsten Abstand zur Hand
                    WallPlane best = ws[0];
                    float bestD = float.MaxValue;
                    foreach (var w in ws)
                    {
                        float d = Mathf.Abs(Vector3.Dot(lastTip - w.Point, w.Normal));
                        if (d < bestD) { bestD = d; best = w; }
                    }
                    return best;
                }) : null,
                () => (System.Collections.Generic.IReadOnlyList<(Vector3 pos, string id)>)
                    CollectEndpoints(),
                ResolveAnchor,
                roomId,
                plane => plane.RoomId); // D43: Raum kommt von der gesnappten Wand (Scene/Mock); leer → Standard

            // Beweisfoto (D8): nach Confirm anhängen — fehlgeschlagen wird ehrlich gemeldet.
            // (NACH der Driver-Konstruktion — NRE-Fix 07.11.: Subscription saß vorher davor.)
            driver.ObjectConfirmed += o =>
            {
                var roomNow = driver.ActiveRoomId; // D43: zählt im Raum des Objekts, nicht im Szenen-Standard
                Debug.Log("[DrawController] Objekt in Raum " + roomNow + " gespeichert");
                var count = store.ObjectsInRoom(roomNow).Count;
                if (evidence == null) { SetStatus(Strings.Format("capture.saved_room", count)); return; }
                evidence.CaptureFor(o.id, path =>
                {
                    if (path != null && store.AttachPhoto(o.id, path))
                        SetStatus(Strings.Get("capture.photo_saved") + " · " + Strings.Format("capture.saved_room", count));
                    else
                        SetStatus(Strings.Get("capture.photo_unavailable"));
                });
            };

            anchors.LoadAll(ok => SetStatus(ok
                ? Strings.Format("recall.loaded", store.Data.objects.Count)
                : Strings.Get("demo.load_failed")));
        }

        /// <summary>Raum-Anker lazily erzeugen + im Store registrieren; Liefert (Id, Pose) für den Driver.</summary>
        (string id, Pose pose) ResolveAnchor()
        {
            var id = GetOrCreateAnchorId();
            if (string.IsNullOrEmpty(id))
                return ("", new Pose(Vector3.zero, Quaternion.identity));
            var pose = anchors.TryGetPose(id, out var p) ? p : new Pose(Camera.main != null ? Camera.main.transform.position : transform.position, Quaternion.identity);
            return (id, pose);
        }

        /// <summary>
        /// Magnetische Endpunkte bereits erfasster Objekte des Raums (docs/03 §1: „magnetische
        /// Endpunkte") — verortbare Objekte, erster/letzter Punkt in Welt. Anker-lokal → Welt
        /// mit derselben Mathematik wie der Recall (CorridorFactory.ToWorld).
        /// </summary>
        System.Collections.Generic.IReadOnlyList<(Vector3 pos, string id)> CollectEndpoints()
        {
            var list = new System.Collections.Generic.List<(Vector3, string)>();
            foreach (var o in store.ObjectsInRoom(driver != null ? driver.ActiveRoomId : roomId))
            {
                if (o.orphaned || o.geometry.Count < 2) continue;
                if (!anchors.TryGetPose(o.anchor_id, out var pose)) continue;
                var world = Recall.CorridorFactory.ToWorld(o, pose);
                list.Add((world[0], o.id));
                list.Add((world[world.Count - 1], o.id));
            }
            return list;
        }

        void Update()
        {
            var right = hands.GetHand(left: false);
            var left = hands.GetHand(left: true);
            lastTip = right.IndexTip;

            // Menü: Handfläche zum Körper öffnet/schließt
            bool open = menu.UpdatePalm(hands.PalmFacingBody(left: true));

            bool pinchDown = right.Tracked && right.Pinching;
            bool rising = pinchDown && !pinchWasDown;

            if (open)
            {
                if (rising) menu.Cycle(1); // Pinch im Menü = nächster Typ
                SetStatus(Strings.Get("menu.cycle_hint") + " — " + (menu.CurrentType.HasValue ? Strings.Get("type." + menu.CurrentType.Value.ToKey()) : ""));
                FinishFrame(pinchDown, open);
                return;
            }
            if (menuWasOpen)
            {
                var t = menu.Close(); // Handfläche weg = Typ wählen
                if (t.HasValue && session.Phase == CapturePhase.Idle)
                {
                    session.BeginTypwahl();
                    session.SelectType(t.Value);
                }
            }

            // Auto-Start EINMAL beim Start mit Standard-Typ (danach: Menü startet jeden Capture)
            if (session.Phase == CapturePhase.Idle && !autoStarted)
            {
                autoStarted = true;
                session.BeginTypwahl();
                session.SelectType(startType);
            }

            driver.Tick(right, left);

            UpdatePreview(right);

            if (session.Phase == CapturePhase.Tiefe)
                SetStatus(Strings.Format("phase.tiefe", session.DepthCm));
            else if (session.Phase == CapturePhase.Zeichnen && driver.LastSnap != null && driver.LastSnap != shownSnap)
            {
                shownSnap = driver.LastSnap;
                SetStatus(Strings.Get("capture.point_snapped") + " · " + Strings.Format("list.count", store.ObjectsInRoom(driver.ActiveRoomId).Count, driver.ActiveRoomId));
            }

            FinishFrame(pinchDown, open);
        }

        // ---------------- Zeichnen-Vorschau („Linie folgt der Hand", docs/03 §1) ----------------

        void UpdatePreview(HandFrame right)
        {
            var phase = session.Phase;
            if (phase != CapturePhase.Zeichnen && phase != CapturePhase.Tiefe && phase != CapturePhase.Bestaetigen)
                return;

            var pose = driver.ActiveAnchorPose ?? new Pose(Vector3.zero, Quaternion.identity);
            if (preview == null)
            {
                var go = new GameObject("DrawPreview");
                preview = go.AddComponent<LineRenderer>();
                preview.startWidth = previewWidthM;
                preview.endWidth = previewWidthM;
                preview.useWorldSpace = true;
                preview.numCornerVertices = 2;
            }
            var c = RecallPalette.TypeColor(session.Type ?? ObjectType.Power);
            preview.startColor = c;
            preview.endColor = c;

            var pts = new System.Collections.Generic.List<Vector3>();
            foreach (var p in session.Punkte)
                pts.Add(CaptureDriver.LocalToWorld(pose, new Vector3((float)p[0], (float)p[1], (float)p[2])));
            if (session.Phase == CapturePhase.Zeichnen && right.Tracked) pts.Add(right.IndexTip); // Linie folgt der Hand

            if (pts.Count >= 2)
            {
                preview.positionCount = pts.Count;
                preview.SetPositions(pts.ToArray());
            }

            // Snap-Marker am letzten eingerasteten Punkt
            if (driver.LastSnap != null)
            {
                if (snapMarker == null)
                {
                    snapMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Destroy(snapMarker.GetComponent<Collider>());
                    snapMarker.transform.localScale = Vector3.one * 0.035f;
                    snapMarker.name = "SnapMarker";
                }
                snapMarker.transform.position = driver.LastSnap.Point;
                snapMarker.SetActive(phase == CapturePhase.Zeichnen);
            }
        }

        /// <summary>Nach Confirm/Cancel: Vorschau versetzt ausblenden — das Foto (D8) soll die Linie noch zeigen.</summary>
        void FadeOutPreview()
        {
            if (preview != null) { Destroy(preview.gameObject, 0.6f); preview = null; }
            if (snapMarker != null) { Destroy(snapMarker, 0.6f); snapMarker = null; }
        }

        void FinishFrame(bool pinchDown, bool menuOpen)
        {
            pinchWasDown = pinchDown;
            menuWasOpen = menuOpen;
        }

        /// <summary>Raum-Anker lazily beim ersten Bedarf (erster Punkt) erzeugen + im Store registrieren.
        /// D43: im AUFGELOSTEN Raum des Objekts (driver.ActiveRoomId — ResolveRoom läuft VOR ResolveAnchor).</summary>
        string GetOrCreateAnchorId()
        {
            if (anchors == null) return "";
            var room = driver != null ? driver.ActiveRoomId : roomId;
            var head = Camera.main != null ? Camera.main.transform : transform;
            var id = anchors.CreateAnchor(new Pose(head.position, Quaternion.identity), room);
            store.RegisterAnchor(id, room);
            Debug.Log("[DrawController] Raum-Anker erstellt: " + id + " (Raum " + room + ")");
            return id;
        }

        string PhaseText(CapturePhase p)
        {
            switch (p)
            {
                case CapturePhase.Idle: return Strings.Get("phase.idle");
                case CapturePhase.Typwahl: return Strings.Get("phase.typwahl");
                case CapturePhase.Zeichnen: return Strings.Get("phase.zeichnen");
                case CapturePhase.Bestaetigen: return Strings.Get("phase.bestaetigen");
                default: return Strings.Format("phase.tiefe", session.DepthCm);
            }
        }

        void SetStatus(string s)
        {
            if (statusText != null) statusText.text = s;
            Debug.Log("[DrawController] " + s);
        }
    }
}
