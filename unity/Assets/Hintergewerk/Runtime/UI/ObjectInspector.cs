// Hintergewerk — Detail-Panel (docs/03 §1, Geste 3: „Blick + Pinch → Objekt auswählen,
// Highlight + Detail-Panel"). SDK-frei: Blickstrahl (Camera.main) + Korridor-Distanz
// (WarningMath, getestet), Pinch wählt aus, Pinch aufs Leere wählt ab.
// Panel erscheint am Objekt, blickt zur Kamera. Konfidenz IMMER sichtbar (Ehrlichkeit).
using System.Collections.Generic;
using UnityEngine;
using Hintergewerk.Core;
using Hintergewerk.Data;
using Hintergewerk.Localization;
using Hintergewerk.Recall;

namespace Hintergewerk.UI
{
    public class ObjectInspector : MonoBehaviour
    {
        [SerializeField] MonoBehaviour anchorProviderBehaviour;  // OvrAnchorProvider (Posen der Korridore)
        [SerializeField] MonoBehaviour handSourceBehaviour;      // OvrHandSource (Pinch)
        [SerializeField] string roomId = "raum_1";
        [SerializeField] TextMesh panelText;                     // Welt-Panel (leer = nur Log)
        [SerializeField] TextMesh statusText;                    // optional: Hinweise
        [SerializeField] float selectRadiusM = 0.15f;            // „Trefferschärfe" des Blicks
        [SerializeField] float gazeMaxDistM = 4f;
        [SerializeField] float panelScale = 0.012f;

        IAnchorProvider anchors;
        IHandSource hands;
        HouseDataStore store;
        readonly List<TrackedObject> candidates = new();
        readonly List<(string id, IReadOnlyList<Vector3> lines)> corridorLines = new();
        TrackedObject selected;
        bool pinchWasDown;
        GameObject panelGo;
        float nextRebuildTime;

        void Start()
        {
            anchors = anchorProviderBehaviour as IAnchorProvider;
            hands = handSourceBehaviour as IHandSource;
            if (anchors == null || hands == null)
            {
                Debug.LogError("[Inspector] Adapter fehlen oder implementieren die Interfaces nicht.");
                enabled = false;
                return;
            }
            store = new HouseDataStore();
            if (panelText == null)
            {
                panelGo = new GameObject("InspectorPanel");
                var tm = panelGo.AddComponent<TextMesh>();
                tm.fontSize = 64;
                tm.characterSize = panelScale * 0.5f;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Left;
                tm.color = Color.white;
                panelText = tm;
                panelGo.SetActive(false);
            }
            SetStatus(Strings.Get("panel.hint"));
        }

        void Update()
        {
            var right = hands.GetHand(left: false);
            bool pinch = right.Tracked && right.Pinching;
            bool rising = pinch && !pinchWasDown;
            pinchWasDown = pinch;

            // Korridor-Geometrie alle 2 s frisch aufbauen (Anker-Drift, neue Objekte)
            if (Time.time >= nextRebuildTime) { RebuildLines(); nextRebuildTime = Time.time + 2f; }

            if (rising)
            {
                var hit = FindByGaze();
                selected = hit;
                ShowPanel(hit);
            }

            if (selected != null && panelGo != null && panelGo.activeSelf && Camera.main != null)
                panelGo.transform.LookAt(Camera.main.transform, Vector3.up); // Panel blickt zur Kamera
        }

        /// <summary>Objekt unter dem Blickstrahl wählen (nächstes Korridor-Segment innerhalb selectRadiusM).</summary>
        TrackedObject FindByGaze()
        {
            var cam = Camera.main;
            if (cam == null) return null;
            var origin = cam.transform.position;
            var dir = cam.transform.forward;

            string bestId = null;
            float best = selectRadiusM;
            foreach (var (id, line) in corridorLines)
            {
                float d = WarningMath.DistanceRayToPolyline(origin, dir, line, gazeMaxDistM);
                if (d < best) { best = d; bestId = id; }
            }
            if (bestId == null) return null;
            return store.Data.objects.Find(o => o.id == bestId);
        }

        void RebuildLines()
        {
            corridorLines.Clear();
            candidates.Clear();
            foreach (var o in store.ObjectsInRoom(roomId))
            {
                if (o.orphaned || !anchors.TryGetPose(o.anchor_id, out var pose)) continue;
                corridorLines.Add((o.id, CorridorFactory.ToWorld(o, pose)));
                candidates.Add(o);
            }
        }

        void ShowPanel(TrackedObject o)
        {
            var text = InspectorPanel.Format(o);
            if (panelText != null) panelText.text = text;
            SetStatus(o != null ? Strings.Get("type." + o.type.ToKey()) : Strings.Get("panel.not_found"));

            if (panelGo != null && o != null)
            {
                // Am Objekt platzieren (erster Korridor-Punkt), leicht vor die Wand
                var (id, line) = corridorLines.Find(x => x.id == o.id);
                if (line != null && line.Count > 0)
                {
                    panelGo.transform.position = line[0] + Vector3.forward * -0.05f + Vector3.up * 0.12f;
                    panelGo.SetActive(true);
                }
            }
            else if (panelGo != null && o == null)
            {
                panelGo.SetActive(false);
            }
            Debug.Log("[Inspector] " + (o == null ? "—" : text.Replace("\n", " | ")));
        }

        void SetStatus(string s)
        {
            if (statusText != null) statusText.text = s;
        }
    }
}
