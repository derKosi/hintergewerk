// Hintergewerk — In-Memory-Implementierung von IAnchorProvider (Editor-Iteration ohne Gerät).
// Der statische Shared-Store simuliert Geräte-Persistenz: Eine NEUE Instanz ist ein
// „App-Neustart" — LoadAll() holt die Anker aus dem geteilten Speicher zurück.
// Für Tests: ResetSharedStore() leert alles.
using System;
using System.Collections.Generic;
using UnityEngine;
using Hintergewerk.Core;

namespace Hintergewerk.Mocks
{
    public class InMemoryAnchorProvider : IAnchorProvider
    {
        static readonly Dictionary<string, Pose> Shared = new();
        readonly Dictionary<string, Pose> local = new();
        int counter;

        public bool FailLoadAll; // simuliert fehlgeschlagenes Laden (Fallback-Pfad testen)

        public string CreateAnchor(Pose pose, string roomId)
        {
            var id = "anc_mock_" + (++counter).ToString("D3");
            Shared[id] = pose;
            local[id] = pose;
            return id;
        }

        public bool TryGetPose(string anchorId, out Pose pose) =>
            local.TryGetValue(anchorId, out pose) || Shared.TryGetValue(anchorId, out pose);

        public void PersistAll() { /* In-Memory: nichts zu tun */ }

        public void LoadAll(Action<bool> onDone)
        {
            if (FailLoadAll) { onDone?.Invoke(false); return; }
            foreach (var kv in Shared) local[kv.Key] = kv.Value; // „Neustart": geteilter Speicher wieder da
            onDone?.Invoke(true);
        }

        public IEnumerable<string> KnownAnchorIds => local.Keys;

        public int SharedCount => Shared.Count;

        public static void ResetSharedStore() => Shared.Clear();
    }
}
