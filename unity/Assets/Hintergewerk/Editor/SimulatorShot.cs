// Hintergewerk — Simulator-Foto-Reporter: manueller Kamera-Render → PNG.
// Warum manuell? XR rendert nicht in den Game-View (D33): ScreenCapture liefert im
// XR-Simulator-Lauf kein Bild. Wir rendern die Center-Eye-Kamera selbst in eine
// RenderTexture und lesen die Pixel. Editor-Tool, kein Runtime-Code.
// Aufruf (im Play-Modus): script eval 'Hintergewerk.EditorTools.SimulatorShot.Capture("name");'
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hintergewerk.EditorTools
{
    public static class SimulatorShot
    {
        static string Dir()
        {
            var repo = Directory.GetParent(Application.dataPath).Parent.FullName; // Repo-Root (unity/ liegt darunter)
            var dir = Path.Combine(repo, "TestShots");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }

        public static void Capture(string fileName) => CaptureAt(fileName, null, null);

        /// <summary>Foto mit expliziter Kameraposition/Blickziel — der XR-Simulator übersteuert
        /// die Kopf-Pose laufend, deshalb setzen wir sie NUR für den einen manuellen Render.</summary>
        public static void CaptureAt(string fileName, float? px, float? py, float? pz, float? lx, float? ly, float? lz)
        {
            Vector3? pos = px.HasValue ? new Vector3(px.Value, py ?? 0f, pz ?? 0f) : (Vector3?)null;
            Vector3? look = lx.HasValue ? new Vector3(lx.Value, ly ?? 0f, lz ?? 0f) : (Vector3?)null;
            CaptureAt(fileName, pos, look);
        }

        public static void CaptureAt(string fileName, Vector3? pos, Vector3? look)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                // OVR-Rig: CenterEyeAnchor trägt meist den MainCamera-Tag — sonst gezielt suchen.
                var anchor = GameObject.Find("CenterEyeAnchor");
                cam = anchor != null ? anchor.GetComponent<Camera>() : null;
            }
            if (cam == null) { Debug.LogError("[SimulatorShot] Keine Kamera (Camera.main/CenterEyeAnchor)."); return; }

            var prevPos = cam.transform.position;
            var prevRot = cam.transform.rotation;
            if (pos.HasValue) cam.transform.position = pos.Value;
            if (look.HasValue) cam.transform.LookAt(look.Value);

            const int w = 1280, h = 960;
            var rt = new RenderTexture(w, h, 24);
            var prevTarget = cam.targetTexture;
            var prevActive = RenderTexture.active;
            cam.targetTexture = rt;
            RenderTexture.active = rt;
            cam.Render();

            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();

            cam.targetTexture = prevTarget;
            RenderTexture.active = prevActive;
            cam.transform.SetPositionAndRotation(prevPos, prevRot); // XR-Pose frei geben

            var path = Path.Combine(Dir(), fileName.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase) ? fileName : fileName + ".png");
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(rt);
            Debug.Log("[SimulatorShot] Geschrieben: " + path + " (cam=" + cam.name + " pos=" + cam.transform.position + ")");
        }
    }
}
#endif
