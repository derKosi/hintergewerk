// Hintergewerk — Beweisfoto-Adapter (Ansatz D8): App-Render-Snapshot via ScreenCapture.
// Passthrough-Aufnahmen aus der App sind API-beschränkt (R3) — also der ehrliche Fallback:
// das App-Overlay (Linie + Marker) wird als Beweis gerettet. Schlägt die Aufnahme fehl
// (z. B. Vulkan/Berechtigung), kommt onDone(null) zurück → UI meldet „Foto nicht verfügbar".
// LIEGT AUSSERHALB der Runtime-asmdef (Assembly-CSharp) — ScreenCapture ist UnityEngine, kein OVR.
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Hintergewerk.Core;

namespace Hintergewerk.Adapters
{
    public class AppSnapshotEvidence : MonoBehaviour, IEvidenceCapture
    {
        const float WaitSeconds = 2f;

        public void CaptureFor(string objectId, Action<string> onDone) =>
            StartCoroutine(CaptureCoroutine(objectId, onDone));

        IEnumerator CaptureCoroutine(string objectId, Action<string> onDone)
        {
            var dir = Path.Combine(Application.persistentDataPath, "photos");
            try { Directory.CreateDirectory(dir); } catch (Exception e) { Debug.LogError($"[Evidence] Ordner: {e.Message}"); onDone?.Invoke(null); yield break; }

            var rel = $"photos/{objectId}_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
            ScreenCapture.CaptureScreenshot(rel); // mobil: relativ zu persistentDataPath

            // CaptureScreenshot schreibt asynchron (nächstes gerendertes Frame) — begrenzt warten.
            // Am Gerät: persistentDataPath. Im EDITOR schreibt Unity ins Projektverzeichnis —
            // beide Orte akzeptieren (gleiche ehrliche Semantik, nur Ablageort je Plattform).
            var absDevice = Path.Combine(Application.persistentDataPath, rel);
            var absEditor = Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, rel);
            float t = 0f;
            while (!File.Exists(absDevice) && !File.Exists(absEditor) && t < WaitSeconds)
            { t += Time.deltaTime; yield return null; }

            string found = File.Exists(absDevice) ? absDevice : (File.Exists(absEditor) ? absEditor : null);
            if (found != null)
            {
                Debug.Log($"[Evidence] Snapshot: {rel} ({new FileInfo(found).Length} Bytes)");
                onDone?.Invoke(rel);
            }
            else
            {
                Debug.LogWarning("[Evidence] Kein Snapshot innerhalb 2 s — Foto bleibt leer (ehrlich).");
                onDone?.Invoke(null);
            }
        }
    }
}
