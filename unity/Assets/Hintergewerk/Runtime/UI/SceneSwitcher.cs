// Hintergewerk — Szenenwechsel am Gerät ohne adb: linke Handfläche zum Körper halten
// (≥ holdSeconds) + linker Pinch → nächste Szene der Build-Liste.
// Kollisionsfrei mit der Capture-Grammatik: Die linke Pinch-Flanke wird nirgends anders
// genutzt (Tiefe = linke HandPOSITION, Menü-Cycle = rechter Pinch, Export = linker Pinch
// OHNE Handfläche). Log statt StatusText — der Status gehört den Szenen-Controllern.
using UnityEngine;
using UnityEngine.SceneManagement;
using Hintergewerk.Core;

namespace Hintergewerk.UI
{
    public class SceneSwitcher : MonoBehaviour
    {
        [SerializeField] MonoBehaviour handSourceBehaviour; // OvrHandSource
        [SerializeField] float holdSeconds = 0.4f;

        IHandSource hands;
        float palmTimer;
        bool pinchWasDown;

        void Start()
        {
            hands = handSourceBehaviour as IHandSource;
            if (hands == null)
            {
                Debug.LogError("[SceneSwitcher] Hand-Adapter fehlt oder implementiert IHandSource nicht.");
                enabled = false;
            }
        }

        void Update()
        {
            bool palm = hands.PalmFacingBody(left: true);
            palmTimer = palm ? palmTimer + Time.deltaTime : 0f;

            var left = hands.GetHand(left: true);
            bool pinch = left.Tracked && left.Pinching;
            if (palm && palmTimer >= holdSeconds && pinch && !pinchWasDown) Switch();
            pinchWasDown = pinch;
        }

        void Switch()
        {
            int count = SceneManager.sceneCountInBuildSettings;
            if (count <= 1) return;
            int idx = SceneManager.GetActiveScene().buildIndex;
            int next = (idx + 1) % count;
            Debug.Log($"[SceneSwitcher] Szene {idx} → {next} ({SceneName(next)})");
            SceneManager.LoadScene(next);
        }

        static string SceneName(int buildIndex)
        {
            var p = SceneUtility.GetScenePathByBuildIndex(buildIndex);
            return System.IO.Path.GetFileNameWithoutExtension(p);
        }
    }
}
