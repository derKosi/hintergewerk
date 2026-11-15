// Hintergewerk — SDK-Adapter: Scene API (Wandebenen).
// LIEGT AUSSERHALB der Runtime-asmdef (Assembly-CSharp).
// v207-API, gegen den SDK-Quellcode verifiziert (07.11.):
//   OVRSceneAnchor.GetSceneAnchors(List<OVRSceneAnchor>) — alle von OVRSceneManager instanzierten Anker
//   OVRSemanticClassification (Sibling-Komponente) — .Contains(label), Labels aus dem Room Setup
//   OVRSceneManager.Classification.WallFace = "WALL_FACE" (unsichtbare Wände sind ebenfalls WallFace)
//   OVRScenePlane.Width/Height/Offset — Flächenmaße der WallFace (D41: Grundriss-Kanten)
//   OVRSceneRoom (Raum-GO ÜBER den Plane-Ankern) + OVRSceneAnchor.Uuid — Raum-Zuordnung (D43)
// Ohne Scene-Daten (Editor/XR-Simulator, oder Room Setup nicht gemacht): Fallback —
// ein ganzer IWallProvider (z. B. MockWallProvider-Raum für Simulator/Demo) oder die
// manuell kalibrierte Einzel-Ebene. Log meldet aktiv genutzte Quelle (kein stilles Fake).
// Ehrlichkeit: Der Adapter liefert nur, was das OS wirklich liefert — keine erfundenen Wände
// oder Räume. Wände ohne erkannten Raum tragen RoomId="" → Capture fällt auf den Standard-Raum.
using System.Collections.Generic;
using UnityEngine;
using Hintergewerk.Core;

namespace Hintergewerk.Adapters
{
    /// <summary>Raum aus der Scene API (D43): stabile Id (Room-Anchor-UUID) + menschliches Label
    /// (Semantic Classification, z. B. "LIVING_ROOM") oder Mock-Namen. Floor bleibt 0 — die Scene
    /// API liefert kein Stockwerk (Mehrstock ist Roadmap, docs/02 §7.1).</summary>
    public struct RoomInfo
    {
        public string Id;
        public string Label;
        public int Floor;
    }

    public class SceneWallProvider : MonoBehaviour, IWallProvider
    {
        [SerializeField] MonoBehaviour fallbackWallProviderBehaviour; // ganzer Provider (z. B. Mock-Raum), optional
        [SerializeField] Transform fallbackPlaneOrigin; // manuell kalibrierte Ebene (Testraum), optional
        [SerializeField] Vector3 fallbackPlaneNormal = Vector3.forward;
        [Tooltip("Maße der Fallback-Ebene (Breite/Höhe in m); 0 = unbekannt → keine Umriss-Kante")]
        [SerializeField] Vector2 fallbackPlaneSize = Vector2.zero;
        [Tooltip("Debug: Anzahl Scene-Wände + Quelle ins Log (nur bei Änderung)")]
        [SerializeField] bool logWallCount;

        readonly List<OVRSceneAnchor> sceneAnchors = new();
        readonly List<WallPlane> walls = new();
        readonly List<RoomInfo> rooms = new();
        int lastWallCount = -1;
        string lastSource = "";
        string lastRoomSummary = "";
        IWallProvider fallbackProvider;

        /// <summary>Im letzten GetWalls()-Durchlauf gefundene Räume (D43) — nur was das OS liefert.</summary>
        public IReadOnlyList<RoomInfo> Rooms => rooms;

        void Start()
        {
            // Fix 09.11. (Schwäche #2): OVRSceneManager lädt das Scene-Model NICHT von allein
            // (Start() nur Telemetrie) — ohne expliziten LoadSceneModel()-Aufruf entstehen keine
            // OVRSceneAnchor und der Plan fiel immer in den Mock-Fallback, auch am Gerät mit
            // Room Setup. Ergebnis ehrlich im Log (Success / NoSceneModelToLoad / Permission).
            var sceneManager = GetComponent<OVRSceneManager>();
            if (sceneManager == null) { Debug.Log("[SceneWallProvider] Kein OVRSceneManager am GO — Scene-Anker bleiben aus."); return; }
            sceneManager.VerboseLogging = true;
            // Fix 15.11. (Befund A): OVRCameraRig hat requestScenePermissionOnStartup=0 und nichts
            // sonst fordert USE_SCENE an → LoadSceneModel() scheiterte still, ohne Room-Setup-Daten
            // griff der Mock-Raum (erfundene Wände — Verstoß gegen die Ehrlichkeitsregel). Jetzt:
            // fehlende Berechtigung erkennen, ANFRAGEN, bei Erteilung das Scene-Model laden.
            sceneManager.LoadSceneModelFailedPermissionNotGranted += OnScenePermissionMissing;
            var loaded = sceneManager.LoadSceneModel();
            Debug.Log("[SceneWallProvider] LoadSceneModel(): " + (loaded ? "angefordert" : "abgelehnt"));
        }

        void OnScenePermissionMissing()
        {
            StartCoroutine(RequestScenePermissionThenLoad());
        }

        System.Collections.IEnumerator RequestScenePermissionThenLoad()
        {
            var sceneManager = GetComponent<OVRSceneManager>();
            if (sceneManager == null) yield break;
            var permission = OVRPermissionsRequester.ScenePermission;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(permission))
            {
                Debug.Log("[SceneWallProvider] USE_SCENE fehlt — Berechtigung wird angefragt …");
                var granted = false; var done = false;
                var callbacks = new UnityEngine.Android.PermissionCallbacks();
                callbacks.PermissionGranted += _ => { granted = true; done = true; };
                callbacks.PermissionDenied += _ => done = true;
                UnityEngine.Android.Permission.RequestUserPermission(permission, callbacks);
                yield return new WaitUntil(() => done);
                if (!granted)
                {
                    // Ehrlich: ohne Scene-Daten bleibt nur der sichtbar geloggte Fallback — kein Fake.
                    Debug.LogWarning("[SceneWallProvider] USE_SCENE verweigert — Fallback-Ebene aktiv (siehe Log-Quelle).");
                    yield break;
                }
                Debug.Log("[SceneWallProvider] USE_SCENE erteilt.");
            }
#else
            Debug.Log("[SceneWallProvider] Editor/Simulator: USE_SCENE-Anfrage übersprungen.");
#endif
            var loaded = sceneManager.LoadSceneModel();
            Debug.Log("[SceneWallProvider] LoadSceneModel() nach Berechtigung: " + (loaded ? "angefordert" : "abgelehnt"));
        }

        public List<WallPlane> GetWalls()
        {
            walls.Clear();
            rooms.Clear();
            string source = "scene";

            OVRSceneAnchor.GetSceneAnchors(sceneAnchors);
            foreach (var a in sceneAnchors)
            {
                if (!a.IsTracked) continue;
                var sem = a.GetComponent<OVRSemanticClassification>();
                if (sem == null || !sem.Contains(OVRSceneManager.Classification.WallFace)) continue;
                // Scene-Planes: Ebene liegt lokal in XY, +Z ist die Normale (zeigt in den Raum)
                var t = a.transform;
                var plane = a.GetComponent<OVRScenePlane>(); // Größe für Grundriss-Kanten (D41)
                var (room, roomLabel) = ResolveRoom(a);
                var wall = new WallPlane
                {
                    Point = plane != null
                        ? t.TransformPoint(new Vector3(plane.Offset.x, plane.Offset.y, 0f)) // Offset meist 0 (Pivot = Zentrum)
                        : t.position,
                    Normal = t.forward,
                    Label = "wall",
                    Size = plane != null && plane.Width > 0f && plane.Height > 0f
                        ? new Vector2(plane.Width, plane.Height)
                        : Vector2.zero, // unbekannt → WallOutline leitet bewusst KEINE Kante ab
                    RoomId = room,      // D43: automatische Raum-Zuordnung im Capture
                    RoomLabel = roomLabel
                };
                walls.Add(wall);
            }

            // Fallback 1: ganzer Provider — NUR im Editor/Simulator (D44, 15.11. Gerätebefund:
            // am Gerät mit Passthrough snappten Pinches auf unsichtbare Mock-Wände am Tracking-
            // Ursprung → „Zeichnen geht nicht". Erfundene Wände im echten Raum = Verstoß gegen
            // die Ehrlichkeitsregel). Am Gerät gilt: ohne Room Setup gibt es KEINE Wände.
#if UNITY_ANDROID && !UNITY_EDITOR
            bool mockFallbackErlaubt = false;
#else
            bool mockFallbackErlaubt = true;
#endif
            if (walls.Count == 0 && mockFallbackErlaubt && fallbackWallProviderBehaviour != null)
            {
                fallbackProvider = fallbackWallProviderBehaviour as IWallProvider;
                if (fallbackProvider != null)
                {
                    walls.AddRange(fallbackProvider.GetWalls());
                    source = fallbackWallProviderBehaviour.name;
                    foreach (var w in walls)
                        if (!string.IsNullOrEmpty(w.RoomId)) TrackRoom(w.RoomId, w.RoomLabel);
                }
            }

            // Fallback 2: kalibrierte Einzel-Ebene, solange kein Room Setup / keine Scene-Anker
            if (walls.Count == 0 && fallbackPlaneOrigin != null)
            {
                walls.Add(new WallPlane
                {
                    Point = fallbackPlaneOrigin.position,
                    Normal = fallbackPlaneNormal.normalized,
                    Label = "wall",
                    Size = fallbackPlaneSize
                }); // RoomId leer → Capture nutzt den Szenen-Standard-Raum (nichts erfunden)
                source = "fallbackPlane";
            }

            var roomSummary = RoomSummary();
            if (logWallCount && (walls.Count != lastWallCount || source != lastSource || roomSummary != lastRoomSummary))
            {
                lastWallCount = walls.Count;
                lastSource = source;
                lastRoomSummary = roomSummary;
                Debug.Log($"[SceneWallProvider] {walls.Count} Wände via {source} · Räume: {roomSummary} (Scene-API: {sceneAnchors.Count} Anker gesamt)");
            }
            return walls;
        }

        /// <summary>Raum-Zuordnung einer Scene-Wand: OVRSceneRoom am Eltern-GO (Room-Layout der
        /// SDK-Instanziierung), Raum-Id = Room-Anchor-UUID (stabil über Sessions), Label aus der
        /// semantischen Klassifikation des Raums. Ohne Raum: ("", "") → Standard-Raum im Capture.</summary>
        (string id, string label) ResolveRoom(OVRSceneAnchor wallAnchor)
        {
            var sceneRoom = wallAnchor.GetComponentInParent<OVRSceneRoom>();
            if (sceneRoom == null) return ("", "");
            var ra = sceneRoom.GetComponent<OVRSceneAnchor>();
            var id = ra != null ? ra.Uuid.ToString("N") : sceneRoom.name;
            var label = RoomLabelOf(sceneRoom);
            TrackRoom(id, label);
            return (id, label);
        }

        void TrackRoom(string id, string label)
        {
            if (string.IsNullOrEmpty(id) || rooms.Exists(r => r.Id == id)) return;
            rooms.Add(new RoomInfo { Id = id, Label = label, Floor = 0 });
        }

        string RoomSummary()
        {
            if (rooms.Count == 0) return "keine erkannt (Standard-Raum im Capture)";
            var parts = new List<string>();
            foreach (var r in rooms)
                parts.Add(Short(r.Id) + (string.IsNullOrEmpty(r.Label) ? "" : "(" + r.Label + ")"));
            return parts.Count + ": " + string.Join(", ", parts);
        }

        static string Short(string id) => string.IsNullOrEmpty(id) || id.Length <= 8 ? id : id.Substring(0, 8) + "…";

        /// <summary>Semantischen Raumnamen aus der Klassifikation ziehen: bevorzugt bekannte
        /// Raumtypen, sonst das erste Label mit "ROOM". Sonst "" (ehrlich unbekannt).</summary>
        static string RoomLabelOf(OVRSceneRoom room)
        {
            var sem = room.GetComponent<OVRSemanticClassification>();
            if (sem == null) return "";
            string firstRoomish = null;
            foreach (var l in sem.Labels)
            {
                if (string.IsNullOrEmpty(l)) continue;
                if (RoomLabels.Contains(l)) return l;
                if (firstRoomish == null && l.Contains("ROOM")) firstRoomish = l;
            }
            return firstRoomish ?? "";
        }

        static readonly HashSet<string> RoomLabels = new()
        {
            "LIVING_ROOM", "KITCHEN", "BEDROOM", "BATHROOM", "OFFICE", "HALLWAY",
            "DINING_ROOM", "GARAGE", "UTILITY", "BALCONY", "STAIRWELL", "CLOSET", "STORAGE"
        };

        public bool TryFindPlane(Vector3 point, Vector3 direction, float maxDist, out WallPlane plane)
        {
            plane = default;
            foreach (var w in GetWalls())
            {
                float denom = Vector3.Dot(w.Normal, direction);
                if (Mathf.Abs(denom) < 1e-5f) continue;
                float t = Vector3.Dot(w.Point - point, w.Normal) / denom;
                if (t < 0f || t > maxDist) continue;
                plane = w;
                return true;
            }
            return false;
        }
    }
}
