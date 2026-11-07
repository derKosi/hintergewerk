// Hintergewerk — der MonoBehaviour am Gerät: füttert den CaptureDriver mit echten Händen.
// Keine Logik hier — nur Verkabelung (Interfaces, Inspector) und Status-Anzeige.
// Die Logik lebt in CaptureSession + CaptureDriver (getestet, SDK-frei).
// Menü: Handfläche zum Körper öffnet, Pinch zählt den Typ weiter, Handfläche weg wählt.
using UnityEngine;
using Hintergewerk.Core;
using Hintergewerk.Data;
using Hintergewerk.Localization;
using Hintergewerk.UI;

namespace Hintergewerk.Capture
{
    public class DrawController : MonoBehaviour
    {
        [Header("Adapter (MonoBehaviours, implementieren die Interfaces)")]
        [SerializeField] MonoBehaviour handSourceBehaviour;     // OvrHandSource
        [SerializeField] MonoBehaviour wallProviderBehaviour;   // SceneWallProvider (optional)
        [SerializeField] MonoBehaviour anchorProviderBehaviour; // OvrAnchorProvider

        [Header("Capture")]
        [SerializeField] ObjectType startType = ObjectType.Power;
        [SerializeField] float gridCm = 5f;
        [SerializeField] float magnetRadiusCm = 8f;
        [SerializeField] string roomId = "raum_1";
        [SerializeField] TextMesh statusText;

        IHandSource hands;
        IWallProvider walls;
        IAnchorProvider anchors;
        HouseDataStore store;
        SnapEngine snap;
        UndoStack undo;
        CaptureSession session;
        CaptureDriver driver;
        HandMenuLogic menu;

        Vector3 lastTip;
        bool pinchWasDown;
        bool menuWasOpen;
        SnapResult shownSnap;
        string currentAnchorId;

        void Start()
        {
            hands = handSourceBehaviour as IHandSource;
            walls = wallProviderBehaviour as IWallProvider;
            anchors = anchorProviderBehaviour as IAnchorProvider;
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
            session.PhaseChanged += p => SetStatus(PhaseText(p));
            menu = new HandMenuLogic();

            // CaptureDriver verdrahten (fehlte im Skeleton — NRE in Update, gefixt 07.11.):
            // Plane aus Scene-Walls (oder Default-Frontalebene), Endpoint-Snapping v2 (M2),
            // Anchor lazily beim ersten Confirm. M2-TODO: Welt→Anker-lokal-Konversion über
            // AnchorPose⁻¹ (ADR 0002) — solange Raum-Anker nahe Ursprung liegt, lokal≈Welt.
            driver = new CaptureDriver(session, store, snap, undo,
                walls != null ? (System.Func<WallPlane>)(() =>
                {
                    var ws = walls.GetWalls();
                    return ws != null && ws.Count > 0
                        ? ws[0]
                        : new WallPlane { Point = Vector3.zero, Normal = Vector3.forward, Label = "default" };
                }) : null,
                () => (System.Collections.Generic.IReadOnlyList<(Vector3 pos, string id)>)
                    System.Array.Empty<(Vector3 pos, string id)>(),
                GetOrCreateAnchorId,
                roomId);

            anchors.LoadAll(ok => SetStatus(ok
                ? Strings.Format("recall.loaded", store.Data.objects.Count)
                : Strings.Get("demo.load_failed")));
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

            // Idle + noch kein Typ: mit Standard-Typ automatisch starten (Menü kommt später feiner)
            if (session.Phase == CapturePhase.Idle)
            {
                session.BeginTypwahl();
                session.SelectType(startType);
            }

            driver.Tick(right, left);

            if (session.Phase == CapturePhase.Tiefe)
                SetStatus(Strings.Format("phase.tiefe", session.DepthCm));
            else if (driver.LastSnap != null && driver.LastSnap != shownSnap)
            {
                shownSnap = driver.LastSnap;
                SetStatus(Strings.Get("capture.point_snapped"));
            }

            FinishFrame(pinchDown, open);
        }

        void FinishFrame(bool pinchDown, bool menuOpen)
        {
            pinchWasDown = pinchDown;
            menuWasOpen = menuOpen;
        }

        /// <summary>Raum-Anker lazily beim ersten Bedarf (Confirm) erzeugen + im Store registrieren.</summary>
        string GetOrCreateAnchorId()
        {
            if (!string.IsNullOrEmpty(currentAnchorId)) return currentAnchorId;
            if (anchors == null) return "";
            var head = Camera.main != null ? Camera.main.transform : transform;
            currentAnchorId = anchors.CreateAnchor(new Pose(head.position, Quaternion.identity), roomId);
            store.RegisterAnchor(currentAnchorId, roomId);
            Debug.Log("[DrawController] Raum-Anker erstellt: " + currentAnchorId);
            return currentAnchorId;
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
