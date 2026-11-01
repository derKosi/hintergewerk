// Hintergewerk — Demo-Controller für Szene 02_AnkerTest (docs/11, Meilenstein M1).
// Nutzt NUR die Interfaces (Runtime bleibt SDK-frei): Die Adapter-MonoBehaviours
// werden im Inspector zugewiesen und hier auf die Interfaces gecastet.
// Demo-Code — fliegt nach bestandenem M1 wieder raus.
using System;
using UnityEngine;
using Hintergewerk.Core;
using Hintergewerk.Data;

namespace Hintergewerk.Demo
{
    public class AnchorTestController : MonoBehaviour
    {
        [SerializeField] MonoBehaviour anchorProviderBehaviour; // implementiert IAnchorProvider
        [SerializeField] MonoBehaviour handSourceBehaviour;     // implementiert IHandSource
        [SerializeField] string roomId = "raum_test";
        [SerializeField] float offsetMeters = 0.5f;

        IAnchorProvider provider;
        IHandSource hands;
        HouseDataStore store;
        TextMesh status;
        bool pinchWasDown;
        int placedCount;

        void Start()
        {
            provider = anchorProviderBehaviour as IAnchorProvider;
            hands = handSourceBehaviour as IHandSource;
            if (provider == null || hands == null)
            {
                Debug.LogError("[AnkerTest] Adapter fehlen oder implementieren die Interfaces nicht.");
                enabled = false;
                return;
            }

            store = new HouseDataStore();
            status = CreateStatusText();
            SetStatus("Lade Anker…");

            provider.LoadAll(ok =>
            {
                placedCount = 0;
                foreach (var id in provider.KnownAnchorIds)
                    if (provider.TryGetPose(id, out var pose)) { SpawnCubes(id, pose); placedCount++; }
                SetStatus(ok ? $"Geladen: {placedCount} Anker" : "Laden fehlgeschlagen — Fallback (docs/07)");
            });
        }

        void Update()
        {
            var hand = hands.GetHand(left: false);
            bool pinching = hand.Tracked && hand.Pinching;
            if (pinching && !pinchWasDown) PlaceAnchor(hand.IndexTip); // Pinch-Steigende Flanke
            pinchWasDown = pinching;
        }

        void PlaceAnchor(Vector3 tip)
        {
            var pose = new Pose(tip, Quaternion.identity);
            string id = provider.CreateAnchor(pose, roomId);
            if (string.IsNullOrEmpty(id)) { SetStatus("Anker-Erzeugung fehlgeschlagen"); return; }

            store.RegisterAnchor(id, roomId);
            SpawnCubes(id, pose);
            SetStatus($"Anker gesetzt: {id.Substring(0, Math.Min(8, id.Length))}… (gesamt {store.Data.anchors.Count})");
        }

        void SpawnCubes(string id, Pose pose)
        {
            var rot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rot.name = "AnkerCube_" + id;
            rot.transform.position = pose.position;
            rot.transform.localScale = Vector3.one * 0.06f;

            var white = GameObject.CreatePrimitive(PrimitiveType.Cube);
            white.name = "OffsetCube_" + id;
            white.transform.position = pose.position + pose.rotation * new Vector3(offsetMeters, 0f, 0f);
            white.transform.localScale = Vector3.one * 0.06f;
        }

        TextMesh CreateStatusText()
        {
            var go = new GameObject("Status");
            go.transform.position = new Vector3(0f, 1.8f, 0.6f);
            var tm = go.AddComponent<TextMesh>();
            tm.characterSize = 0.02f;
            tm.fontSize = 64;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.text = "…";
            return tm;
        }

        void SetStatus(string s)
        {
            if (status != null) status.text = s;
            Debug.Log("[AnkerTest] " + s);
        }
    }
}
