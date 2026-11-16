// Hintergewerk — SDK-Adapter: Spatial Anchors (OVRSpatialAnchor, SDK 207.0.1).
// LIEGT AUSSERHALB der Runtime-asmdef (Assembly-CSharp): Hier ist OVR* erlaubt.
// Verifiziert 06.11. gegen den SDK-Quellcode (PackageCache com.meta.xr.sdk.core 207.0.1):
//   - Erzeugung: AddComponent<OVRSpatialAnchor>() → Start() erzeugt selbst → Created/Localized
//   - Speichern: anchor.SaveAnchorAsync() → OVRTask<OVRResult<OVRAnchor.SaveResult>> (.Success)
//   - Laden: OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids, list) → UnboundAnchor.BindTo(component)
//   - UUID: anchor.Uuid (Guid) — das alte TryGetUuid existiert nicht mehr
// provId → UUID bleibt in anker_map.json, damit HouseDataStore-Objekte ihre Anker finden.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Hintergewerk.Core;

namespace Hintergewerk.Adapters
{
    public class OvrAnchorProvider : MonoBehaviour, IAnchorProvider
    {
        const float LocalizeTimeoutS = 10f;

        // provisorische ID -> echte Anchor-UUID. Persistiert in anker_map.json,
        // damit HouseDataStore-Objekte über App-Neustarts hinweg ihren Anker finden.
        readonly Dictionary<string, string> provToUuid = new();
        readonly Dictionary<string, Pose> knownPoses = new();
        readonly HashSet<Guid> gebundeneUuids = new();
        string MapPath => Path.Combine(Application.persistentDataPath, "anker_map.json");

        public IEnumerable<string> KnownAnchorIds => knownPoses.Keys;

        void Awake() => LoadMap();

        public string CreateAnchor(Pose pose, string roomId)
        {
            var provId = "anc_" + Guid.NewGuid().ToString("N").Substring(0, 12);
            var go = new GameObject("Anchor_" + provId);
            go.transform.SetPositionAndRotation(pose.position, pose.rotation);
            var anchor = go.AddComponent<OVRSpatialAnchor>(); // Start() erzeugt + lokalisiert selbst
            StartCoroutine(SaveWhenLocalized(anchor, provId));

            provToUuid[provId] = null; // UUID folgt nach Lokalisierung/Save
            knownPoses[provId] = pose;
            return provId;
        }

        IEnumerator SaveWhenLocalized(OVRSpatialAnchor anchor, string provId)
        {
            float t = 0f;
            while (!anchor.Localized && t < LocalizeTimeoutS) { t += Time.deltaTime; yield return null; }
            if (!anchor.Localized)
            {
                Debug.LogError($"[AnchorProvider] {provId}: nicht lokalisiert ({LocalizeTimeoutS:0} s)");
                yield break;
            }

            var save = anchor.SaveAnchorAsync();
            while (!save.IsCompleted) yield return null;
            if (!save.TryGetResult(out var result) || !result.Success)
            {
                Debug.LogError($"[AnchorProvider] {provId}: Save fehlgeschlagen ({(result.Status)})");
                yield break;
            }

            provToUuid[provId] = anchor.Uuid.ToString();
            SaveMap();
            Debug.Log($"[AnchorProvider] {provId} gespeichert als {anchor.Uuid}");
        }

        public bool TryGetPose(string anchorId, out Pose pose) => knownPoses.TryGetValue(anchorId, out pose);

        public void PersistAll() { /* Save passiert pro Anker — bewusst leer. */ }

        public void LoadAll(Action<bool> onDone) => StartCoroutine(LoadAllCoroutine(onDone));

        IEnumerator LoadAllCoroutine(Action<bool> onDone)
        {
            var uuids = new List<Guid>();
            foreach (var kv in provToUuid)
                if (Guid.TryParse(kv.Value, out var g)) uuids.Add(g);

            if (uuids.Count == 0)
            {
                Debug.Log("[AnchorProvider] Keine gespeicherten Anker in der Map — nichts zu laden.");
                onDone?.Invoke(true);
                yield break;
            }

            var unbound = new List<OVRSpatialAnchor.UnboundAnchor>();
            var load = OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids, unbound);
            while (!load.IsCompleted) yield return null;

            if (!load.TryGetResult(out var result) || !result.Success)
            {
                Debug.LogError($"[AnchorProvider] Load fehlgeschlagen ({(result.Status)}) — Fallback (docs/07).");
                onDone?.Invoke(false);
                yield break;
            }

            int loaded = 0;
            foreach (var u in result.Value)
            {
                // Fix 16.11. (Simulator-Befund): DrawController UND BauplanPanel rufen LoadAll —
                // die zweite Bindung warf InvalidOperationException („already bound") und dublte
                // die Anker-GOs. UUID-Dedupe: wer schon gebunden ist, wird übersprungen.
                if (!gebundeneUuids.Add(u.Uuid)) continue;
                var go = new GameObject("Anchor_" + u.Uuid.ToString("N").Substring(0, 12));
                var component = go.AddComponent<OVRSpatialAnchor>();
                u.BindTo(component);
                StartCoroutine(TrackAfterLocalized(component, u.Uuid));
                loaded++;
            }
            Debug.Log($"[AnchorProvider] {loaded}/{uuids.Count} Anker gebunden ({gebundeneUuids.Count} gesamt) — Posen folgen beim Lokalisieren.");
            onDone?.Invoke(loaded > 0);
        }

        IEnumerator TrackAfterLocalized(OVRSpatialAnchor anchor, Guid uuid)
        {
            float t = 0f;
            while (!anchor.Localized && t < LocalizeTimeoutS) { t += Time.deltaTime; yield return null; }
            if (!anchor.Localized)
            {
                Debug.LogWarning($"[AnchorProvider] {uuid}: geladen, aber nicht lokalisiert ({LocalizeTimeoutS:0} s)");
                yield break;
            }
            // OVRSpatialAnchor sync't das GameObject-Transform in Update() — Pose von dort.
            string provId = null;
            foreach (var kv in provToUuid)
                if (kv.Value == uuid.ToString()) { provId = kv.Key; break; }
            if (provId != null)
                knownPoses[provId] = new Pose(anchor.transform.position, anchor.transform.rotation);
            Debug.Log($"[AnchorProvider] {uuid} lokalisiert ({anchor.transform.position})");
        }

        void LoadMap()
        {
            if (!File.Exists(MapPath)) return;
            foreach (var line in File.ReadAllLines(MapPath))
            {
                int i = line.IndexOf('=');
                if (i > 0) provToUuid[line[..i]] = line[(i + 1)..];
            }
            Debug.Log($"[AnchorProvider] Map geladen: {provToUuid.Count} Einträge");
        }

        void SaveMap() =>
            File.WriteAllLines(MapPath, Array.ConvertAll(provToUuid.ToArray(), kv => kv.Key + "=" + kv.Value));
    }
}
