// Hintergewerk — platziert Smoketest-Objekte beim Start relativ zur Kopfpose,
// damit die Stage-Orientierung des Geräts keine Rolle spielt (M0).
// Tisch vor dem User, Würfel + Stab liegen darauf, Text schwebt höher.
// Läuft in Awake → vor ThrowCube.Start (Heimpositionen).
using UnityEngine;

namespace Hintergewerk.Adapters
{
    public class SmoketestPlacer : MonoBehaviour
    {
        [SerializeField] Transform table;
        [SerializeField] Transform cube;
        [SerializeField] Transform stick;
        [SerializeField] Transform statusText;
        [SerializeField] float distance = 0.55f;
        [SerializeField] float textLift = 0.35f;
        [SerializeField] float tableDrop = 0.45f;   // Tisch unter Augenhöhe

        void Awake()
        {
            var eye = GameObject.Find("CenterEyeAnchor");
            if (eye == null)
            {
                Debug.LogError("[SmoketestPlacer] CenterEyeAnchor nicht gefunden.");
                return;
            }

            var f = eye.transform.forward; f.y = 0f;
            if (f.sqrMagnitude < 1e-6f) f = Vector3.forward;
            f.Normalize();
            var eyePos = eye.transform.position;
            var look = Quaternion.LookRotation(f, Vector3.up);

            if (table != null)
                table.SetPositionAndRotation(eyePos + f * distance - Vector3.up * tableDrop, look);

            float tableTop = table != null
                ? table.position.y + table.lossyScale.y * 0.5f
                : eyePos.y - tableDrop;

            // TextMesh ist von der -Z-Seite lesbar → fwd muss VON der Kamera WEG zeigen
            // (gleiche Richtung wie der Blick), sonst sieht man ihn spiegelverkehrt.
            if (statusText != null)
                statusText.SetPositionAndRotation(
                    eyePos + f * (distance * 0.8f) + Vector3.up * textLift,
                    look);

            if (cube != null)
                cube.SetPositionAndRotation(
                    new Vector3(table != null ? table.position.x : eyePos.x + f.x, tableTop + 0.08f,
                                table != null ? table.position.z : eyePos.z + f.z) + f * 0.1f,
                    look);

            if (stick != null)
                stick.SetPositionAndRotation(
                    new Vector3(table != null ? table.position.x : eyePos.x + f.x, tableTop + 0.03f,
                                table != null ? table.position.z : eyePos.z + f.z) - f * 0.12f,
                    look);

            Debug.Log($"[SmoketestPlacer] table={(table != null ? table.position.ToString("F2") : "-")} " +
                      $"cube={(cube != null ? cube.position.ToString("F2") : "-")} " +
                      $"stick={(stick != null ? stick.position.ToString("F2") : "-")} " +
                      $"text={(statusText != null ? statusText.position.ToString("F2") : "-")} " +
                      $"eye={eyePos.ToString("F2")} fwd={f.ToString("F2")}");
        }
    }
}
