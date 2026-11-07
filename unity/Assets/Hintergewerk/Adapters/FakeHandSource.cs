// Hintergewerk — FakeHandSource: injiziert skriptete Hand-Frames in IHandSource,
// damit der Capture-Flow voll autonom im Editor-Play-Mode getestet werden kann
// (Architektur-Annahme aus Interfaces.cs: "Fake-Implementierungen im Editor/Sim").
// Szenarien per serialized string; Frame = Funktion von Time.timeSinceLevelLoad.
using UnityEngine;
using Hintergewerk.Core;

namespace Hintergewerk.Adapters
{
    public class FakeHandSource : MonoBehaviour, IHandSource
    {
        [SerializeField] string scenario = "DrawSquare";

        static readonly Vector3 P1 = new(0.00f, 1.40f, 0.50f);
        static readonly Vector3 P2 = new(0.30f, 1.40f, 0.50f);
        static readonly Vector3 P3 = new(0.30f, 1.10f, 0.50f);
        static readonly Vector3 P4 = new(0.00f, 1.10f, 0.50f);

        float T => Time.timeSinceLevelLoad;

        public HandFrame GetHand(bool left)
        {
            var f = new HandFrame { Tracked = true, PinchStrength = 0f };

            if (scenario == "DrawSquare" && !left)
            {
                float t = T;
                // Punkt-Grammatik: steigende Pinch-Flanke = ein Punkt. 4 Taps an 4 Ecken,
                // Release 3.8 s → EndDraw (Punkte ≥ 2) → Tiefe; Pinch 6.0 → Bestaetigen;
                // Pinch 7.0 → Confirm → Store.
                bool pinch = (t >= 1.0f && t < 1.4f) || (t >= 1.8f && t < 2.2f)
                           || (t >= 2.6f && t < 3.0f) || (t >= 3.4f && t < 3.8f)
                           || (t >= 6.0f && t < 6.6f) || (t >= 7.0f && t < 7.6f);
                if (t < 1.4f) f.IndexTip = P1;
                else if (t < 1.8f) f.IndexTip = Vector3.Lerp(P1, P2, (t - 1.4f) / 0.4f);
                else if (t < 2.2f) f.IndexTip = P2;
                else if (t < 2.6f) f.IndexTip = Vector3.Lerp(P2, P3, (t - 2.2f) / 0.4f);
                else if (t < 3.0f) f.IndexTip = P3;
                else if (t < 3.4f) f.IndexTip = Vector3.Lerp(P3, P4, (t - 3.0f) / 0.4f);
                else f.IndexTip = P4;
                f.Pinching = pinch;
                f.PalmCenter = f.IndexTip + Vector3.back * 0.1f;
                f.PalmNormal = Vector3.forward;
            }
            else if (scenario == "ConfirmDepth")
            {
                // Rechte Hand ruht, linke Hand nähert sich → Tiefe ändert sich; Pinch bestätigt
                f.IndexTip = P4;
                f.PalmCenter = P4 + Vector3.back * 0.1f;
                f.PalmNormal = Vector3.forward;
                if (T > 1f) f.Pinching = T < 1.6f;
            }
            else // "Idle"
            {
                f.IndexTip = P1;
                f.PalmCenter = P1 + Vector3.back * 0.1f;
                f.PalmNormal = Vector3.forward;
            }

            // Linke Hand: verfolgt die Rechte mit Offset (für DepthFromHandDistance)
            if (left)
            {
                var r = GetRightFrame();
                f.IndexTip = r + Vector3.left * 0.15f - Vector3.up * (0.05f + Mathf.PingPong(T * 0.03f, 0.1f));
                f.PalmCenter = f.IndexTip + Vector3.back * 0.1f;
                f.PalmNormal = Vector3.forward;
            }
            return f;
        }

        Vector3 GetRightFrame() => GetHand(false).IndexTip;

        public bool PalmFacingBody(bool left) => scenario == "MenuCycle" && T > 1f && T < 4f;

        void Update()
        {
            if (T > 12f && !logged)
            {
                logged = true;
                Debug.Log("[HGW-AUTO] Szenario '" + scenario + "' Zeitfenster abgelaufen (12 s).");
            }
        }

        bool logged;
    }
}
