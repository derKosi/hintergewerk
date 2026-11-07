// Hintergewerk — Recall-Controller (M3): lädt Objekte des Raums, baut Korridore in
// TYP-FARBEN, bewertet jede Hand-Annäherung und eskaliert sichtbar (Farbe + Puls)
// und hörbar (Audio-Ping, 2D). Status nur bei Änderung setzen (kein Log-Spam mehr).
// Ehrlichkeit: Objekte ohne SDK-verortbaren Anker zählen als „nicht verortbar" und
// werden im Status geführt — nie weggefiltert, nie geraten (Invarianten §1).
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
        [SerializeField] Material corridorMaterial;             // Template — je Korridor wird eine Instanz eingefärbt
        [SerializeField] TextMesh statusText;
        [SerializeField] float nearCm = 15f;
        [SerializeField] float criticalCm = 7f;
        [SerializeField] float pingIntervalNear = 0.8f;         // Audio-Ping-Abstand (s)
        [SerializeField] float pingIntervalCritical = 0.25f;

        IAnchorProvider anchors;
        IHandSource hands;
        HouseDataStore store;
        WarningSystem warning;
        readonly List<(string id, IReadOnlyList<Vector3> line)> corridors = new();
        readonly Dictionary<string, (MeshRenderer renderer, ObjectType type)> corridorRenderers = new();
        int totalObjects;
        int notLocatable;
        string lastStatus;
        WarningLevel lastLevel = WarningLevel.None;
        float nextPingTime;

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
            anchors.LoadAll(ok => { RebuildCorridors(); SetStatus(StatusLine()); });
        }

        void RebuildCorridors()
        {
            foreach (var kv in corridorRenderers)
                if (kv.Value.renderer != null) Destroy(kv.Value.renderer.gameObject);
            corridorRenderers.Clear();
            corridors.Clear();
            totalObjects = 0;
            notLocatable = 0;

            foreach (var o in store.ObjectsInRoom(roomId))
            {
                if (o.orphaned || !anchors.TryGetPose(o.anchor_id, out var pose))
                {
                    notLocatable++; // Ehrlichkeitsregel: sichtbar führen, nicht wegzaubern
                    continue;
                }
                var pts = CorridorFactory.ToWorld(o, pose);
                var half = Mathf.Max(0.01f, (float)o.safety_corridor_cm / 100f);
                var go = CorridorFactory.CreateOverlay("Corridor_" + o.id, pts, half, PlaneNormalFromPoints(pts), CorridorMaterialFor(o));
                corridors.Add((o.id, pts));
                corridorRenderers[o.id] = (go.GetComponent<MeshRenderer>(), o.type);
                totalObjects++;
            }
            Debug.Log($"[RecallController] {totalObjects} Korridore, {notLocatable} nicht verortbar (Raum {roomId})");
        }

        /// <summary>Material-Instanz in Typ-Farbe (Template aus der Szene; Fallback: unlit).</summary>
        Material CorridorMaterialFor(TrackedObject o)
        {
            Material m;
            if (corridorMaterial != null) m = new Material(corridorMaterial);
            else
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                m = new Material(shader != null ? shader : Shader.Find("Sprites/Default"));
            }
            var c = RecallPalette.TypeColor(o.type);
            m.color = c;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c * 0.6f);
            return m;
        }

        void Update()
        {
            var hand = hands.GetHand(left: false);
            if (!hand.Tracked)
            {
                if (lastLevel != WarningLevel.None) { lastLevel = WarningLevel.None; ResetColors(); SetStatus(StatusLine()); }
                return;
            }

            var level = warning.Evaluate(hand.IndexTip, corridors, out var nearestId, out var cm);
            if (level != lastLevel || level != WarningLevel.None)
                ApplyEscalation(level, nearestId);

            if (level != lastLevel)
            {
                SetStatus(level switch
                {
                    WarningLevel.Critical => Strings.Get("recall.critical"),
                    WarningLevel.Near => Strings.Get("recall.near"),
                    _ => StatusLine()
                });
            }

            // Audio-Ping: Abstand je nach Stufe (2D — ausreichend für M3; FoV-aware später)
            if (level != WarningLevel.None && Time.time >= nextPingTime)
            {
                WarningAudio.Ping(level);
                nextPingTime = Time.time + (level == WarningLevel.Critical ? pingIntervalCritical : pingIntervalNear);
            }
            lastLevel = level;
        }

        void ApplyEscalation(WarningLevel level, string nearestId)
        {
            foreach (var kv in corridorRenderers)
            {
                if (kv.Value.renderer == null) continue;
                var isNearest = kv.Key == nearestId;
                var baseC = RecallPalette.TypeColor(kv.Value.type);
                var c = level switch
                {
                    WarningLevel.Critical when isNearest => RecallPalette.CriticalColor(kv.Value.type),
                    WarningLevel.Near when isNearest => RecallPalette.NearColor(kv.Value.type),
                    _ => baseC
                };
                var pulse = isNearest ? RecallPalette.Pulse(Time.time, level) : 0f;
                c = Color.Lerp(c, Color.white, pulse * 0.25f);
                kv.Value.renderer.material.color = c;
                if (kv.Value.renderer.material.HasProperty("_BaseColor"))
                    kv.Value.renderer.material.SetColor("_BaseColor", c);
            }
        }

        void ResetColors() => ApplyEscalation(WarningLevel.None, null);

        string StatusLine()
        {
            var loaded = Strings.Format("recall.loaded", totalObjects);
            return notLocatable > 0 ? loaded + " · " + Strings.Format("recall.located_status", totalObjects, notLocatable) : loaded;
        }

        void SetStatus(string s)
        {
            if (s == lastStatus) return; // kein Log-Spam (Fix 07.11.: vorher jede Frame)
            lastStatus = s;
            if (statusText != null) statusText.text = s;
            Debug.Log("[RecallController] " + s);
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
    }
}
