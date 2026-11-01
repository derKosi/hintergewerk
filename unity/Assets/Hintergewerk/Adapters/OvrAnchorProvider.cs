// Hintergewerk — SDK-Adapter: Spatial Anchors (OVRSpatialAnchor).
// LIEGT AUSSERHALB der Runtime-asmdef (Assembly-CSharp): Hier ist OVR* erlaubt.
//
// Status: Skeleton gegen Meta-Doku + offizielles Tutorial geschrieben (01.11.).
// Verifiziert: Tutorial „unity-spatial-anchors-basic-tutorial" existiert; Komponente
// OVRSpatialAnchor mit CreateSpatialAnchor ist belegt (Forum + Tutorial).
// TODO(02.11., SDK v207 vor Ort): exakte Methodennamen prüfen — neuere Versionen
// nutzen async-Varianten (CreateSpatialAnchorAsync / SaveAsync) und anchor.Uuid.
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Hintergewerk.Core;

namespace Hintergewerk.Adapters
{
    public class OvrAnchorProvider : MonoBehaviour, IAnchorProvider
    {
        // provisorische ID -> echte Anchor-UUID. Persistiert in anker_map.json,
        // damit HouseDataStore-Objekte über App-Neustarts hinweg ihren Anker finden.
        readonly Dictionary<string, string> provToUuid = new();
        readonly Dictionary<string, Pose> knownPoses = new();
        string MapPath => Path.Combine(Application.persistentDataPath, "anker_map.json");

        public IEnumerable<string> KnownAnchorIds => knownPoses.Keys;

        void Awake() => LoadMap();

        public string CreateAnchor(Pose pose, string roomId)
        {
            var provId = "anc_" + Guid.NewGuid().ToString("N").Substring(0, 12);
            var go = new GameObject("Anchor_" + provId);
            go.transform.SetPositionAndRotation(pose.position, pose.rotation);
            var anchor = go.AddComponent<OVRSpatialAnchor>();

            // TODO(02.11., v207): neuere API ist CreateSpatialAnchorAsync()
            anchor.CreateSpatialAnchor();
            StartCoroutine(SaveWhenLocalized(anchor, provId));

            provToUuid[provId] = null; // UUID folgt nach Lokalisierung/Save
            knownPoses[provId] = pose;
            return provId;
        }

        System.Collections.IEnumerator SaveWhenLocalized(OVRSpatialAnchor anchor, string provId)
        {
            float t = 0f;
            while (!anchor.Localized && t < 10f) { t += Time.deltaTime; yield return null; }
            if (!anchor.Localized) { Debug.LogError($"[AnchorProvider] {provId}: nicht lokalisiert (10 s)"); yield break; }

            // TODO(02.11., v207): neuere API ist await anchor.SaveAsync();
            if (!anchor.Save()) { Debug.LogError($"[AnchorProvider] {provId}: Save fehlgeschlagen"); yield break; }

            if (anchor.TryGetUuid(out var uuid)) // TODO(02.11.): neuere SDKs: anchor.Uuid
            {
                provToUuid[provId] = uuid.ToString();
                SaveMap();
                Debug.Log($"[AnchorProvider] {provId} gespeichert als {uuid}");
            }
        }

        public bool TryGetPose(string anchorId, out Pose pose) => knownPoses.TryGetValue(anchorId, out pose);

        public void PersistAll() { /* Save passiert pro Anker — bewusst leer. */ }

        public void LoadAll(Action<bool> onDone) => StartCoroutine(LoadAllCoroutine(onDone));

        System.Collections.IEnumerator LoadAllCoroutine(Action<bool> onDone)
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

            // TODO(02.11., v207): exakte LoadOptions/API prüfen. Skizze:
            //   var options = new OVRSpatialAnchor.LoadOptions {
            //       StorageLocation = OVRSpace.StorageLocation.Local, Uuids = uuids };
            //   var loaded = OVRSpatialAnchor.LoadUnboundSpatialAnchors(options);
            //   foreach (var unbound in loaded) { var a = unbound.Instantiate(); /* auf Localized warten */ }
            Debug.LogWarning("[AnchorProvider] LoadAll: UUID-basiertes Laden am 02.11. gegen v207 fertigstellen (docs/07, Run 1–3).");
            onDone?.Invoke(uuids.Count > 0);
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
