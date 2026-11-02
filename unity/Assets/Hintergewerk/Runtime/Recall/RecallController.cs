// Hintergewerk — Recall-Controller (M3): lädt Objekte des Raums, baut Korridore,
// bewertet jede Hand-Annäherung und eskaliert den Status.
// Logik-Rumpf läuft komplett über Interfaces + getestete Klassen (CorridorFactory, WarningSystem).
using System.Collections.Generic;
using UnityEngine;
using Hintergewerk.Core;
using Hintergewerk.Data;
using Hintergewerk.Localization;

namespace Hintergewerk.Recall
{
    public class RecallController : MonoBehaviour
    {
        [SerializeField] MonoBehaviour anchorProviderBehaviour; // OvrAnchorProvider
        [SerializeField] MonoBehaviour handSourceBehaviour;     // OvrHandSource
        [SerializeField] string roomId = "raum_1";
        [SerializeField] Material corridorMaterial;             // optional (Farbe je Typ später)
        [SerializeField] TextMesh statusText;
        [SerializeField] float nearCm = 15f;
        [SerializeField] float criticalCm = 7f;

        IAnchorProvider anchors;
        IHandSource hands;
        HouseDataStore store;
        WarningSystem warning;
        readonly List<(string id, IReadOnlyList<Vector3> line)> corridors = new();
        readonly Dictionary<string, List<GameObject>> spawned = new();
        int totalObjects;

        void Start()
        {
            anchors = anchorProviderBehaviour as IAnchorProvider;
            hands = handSourceBehaviour as IHandSource;
            if (anchors == null || hands == null)
            {
                Debug.LogError("[RecallController] Adapter fehlen oder implementieren die Interfaces nicht.");
                enabled = false;
                return;
            }
            store = new HouseDataStore();
            warning = new WarningSystem { NearCm = nearCm, CriticalCm = criticalCm };
            anchors.LoadAll(ok => { RebuildCorridors(); SetStatus(Strings.Format("recall.loaded", totalObjects)); });
        }

        void RebuildCorridors()
        {
            foreach (var kv in spawned)
                foreach (var go in kv.Value)
                    if (go != null) Destroy(go);
            spawned.Clear();
            corridors.Clear();
            totalObjects = 0;

            foreach (var o in store.ObjectsInRoom(roomId))
            {
                if (o.orphaned || !anchors.TryGetPose(o.anchor_id, out var pose))
                {
                    o.orphaned = true; // Ehrlichkeitsregel: sichtbar führen, nicht wegzaubern
                    continue;
                }
                var pts = CorridorFactory.ToWorld(o, pose);
                var half = Mathf.Max(0.01f, (float)o.safety_corridor_cm / 100f);
                var go = CorridorFactory.CreateOverlay("Corridor_" + o.id, pts, half, PlaneNormalFromPoints(pts), corridorMaterial);
                if (!spawned.TryGetValue(o.id, out var list)) { list = new List<GameObject>(); spawned[o.id] = list; }
                list.Add(go);
                corridors.Add((o.id, pts));
                totalObjects++;
            }
        }

        void Update()
        {
            var hand = hands.GetHand(left: false);
            if (!hand.Tracked) return;

            var level = warning.Evaluate(hand.IndexTip, corridors, out var nearestId, out var cm);
            // TODO(Politur 11.–13.11.): Farbe des Korridors umschalten + räumlicher Audio-Ping
            if (level == WarningLevel.Critical) SetStatus(Strings.Get("recall.critical"));
            else if (level == WarningLevel.Near) SetStatus(Strings.Get("recall.near"));
            else SetStatus(Strings.Format("recall.loaded", totalObjects));
        }

        /// <summary>Ebenen-Normale aus den ersten drei Punkten; Fallback über die Laufrichtung.</summary>
        static Vector3 PlaneNormalFromPoints(IReadOnlyList<Vector3> pts)
        {
            if (pts.Count >= 3)
            {
                var n = Vector3.Cross(pts[1] - pts[0], pts[2] - pts[0]);
                if (n.sqrMagnitude > 1e-6f) return n.normalized;
            }
            var dir = pts.Count >= 2 ? pts[1] - pts[0] : Vector3.right;
            var fallback = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.9f ? Vector3.forward : Vector3.up;
            return Vector3.Cross(dir, fallback).normalized;
        }

        void SetStatus(string s)
        {
            if (statusText != null) statusText.text = s;
            Debug.Log("[RecallController] " + s);
        }
    }
}
