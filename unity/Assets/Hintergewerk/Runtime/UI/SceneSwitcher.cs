// Hintergewerk — Szenenwechsel am Gerät ohne adb: RECHTE Handfläche zum Körper halten
// (≥ holdSeconds) + rechter Pinch → nächste Szene der Build-Liste.
// Gerät-Befund 15.11.: DIE LINKE Handfläche kollidiert am Gerät mit dem Typ-Menü
// (DrawController öffnet es bei linker Handfläche und frisst den Pinch) — D34s
// „kollisionsfrei" hielt am Gerät nicht. Die RECHTE Handfläche nutzt sonst niemand
// (Tiefe = linke HandPOSITION, Menü = linke Handfläche, Export = linker Pinch, Zeichnen
// = rechter Pinch OHNE Handfläche). HUD-Label zeigt die aktuelle Szene + die Geste.
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
        TextMesh hud;

        void Start()
        {
            hands = handSourceBehaviour as IHandSource;
            if (hands == null)
            {
                Debug.LogError("[SceneSwitcher] Hand-Adapter fehlt oder implementiert IHandSource nicht.");
                enabled = false;
                return;
            }
            CreateHud();
        }

        void Update()
        {
            bool palm = hands.PalmFacingBody(left: false);
            palmTimer = palm ? palmTimer + Time.deltaTime : 0f;

            var right = hands.GetHand(left: false);
            bool pinch = right.Tracked && right.Pinching;
            if (palm && palmTimer >= holdSeconds && pinch && !pinchWasDown) Switch();
            pinchWasDown = pinch;
        }

        /// <summary>HUD am Kopf (Kind der Center-Eye-Kamera): welche Szene bin ich in,
        /// wie wechsle ich? TextMesh zeigt seine Vorderseite Richtung −z → Y um 180° drehen.</summary>
        void CreateHud()
        {
            var cam = Camera.main;
            if (cam == null) return;
            var go = new GameObject("SceneSwitcherHud");
            go.transform.SetParent(cam.transform, false);
            go.transform.localPosition = new Vector3(0f, -0.12f, 0.6f);
            // TextMesh-Vorderseite zeigt −z = Richtung Kamera → KEINE Drehung nötig
            // (Gedächtnis-Notiz: 180° nur, wenn +z zur Kamera zeigt)
            var tm = go.AddComponent<TextMesh>();
            tm.fontSize = 48;
            tm.characterSize = 0.0025f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(1f, 1f, 1f, 0.75f);
            var idx = SceneManager.GetActiveScene().buildIndex;
            tm.text = "Szene: " + SceneName(idx) + "   ·   RECHTE Handfläche + Pinch = weiter";
            hud = tm;
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
