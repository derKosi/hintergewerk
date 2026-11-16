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

        // DemoDreiLeitungen: drei getrennte Quadrate auf derselben (geschätzten) Ebene
        static readonly Vector3[] A = { P1, P2, P3, P4 };
        static readonly Vector3[] B = { P1 + Vector3.right * 0.5f, P2 + Vector3.right * 0.5f, P3 + Vector3.right * 0.5f, P4 + Vector3.right * 0.5f };
        static readonly Vector3[] C = { P1 + Vector3.left * 0.6f - Vector3.up * 0.35f, P2 + Vector3.left * 0.6f - Vector3.up * 0.35f,
                                        P3 + Vector3.left * 0.6f - Vector3.up * 0.35f, P4 + Vector3.left * 0.6f - Vector3.up * 0.35f };

        /// <summary>Drag-Grammatik: Pinch geht bei t0 NIEDRIG (Punkt 1 = Ecke 0), die Spitze zieht
        /// in 3 Segmenten à 0,4 s durch die Ecken, ruht an Ecke 3, loslassen bei t0+1,8 (EndDraw);
        /// Tiefe-Pinch t0+3,0, Confirm-Pinch t0+4,0 (jeweils 0,4 s Fenster).</summary>
        static void Drag(Vector3[] ecken, float t, float t0, out Vector3 tip, out bool pinch)
        {
            float u = t - t0;
            pinch = (u >= 0f && u < 1.8f) || (u >= 3.0f && u < 3.4f) || (u >= 4.0f && u < 4.4f);
            if (u < 0f) tip = ecken[0];
            else if (u < 0.4f) tip = Vector3.Lerp(ecken[0], ecken[1], u / 0.4f);
            else if (u < 0.8f) tip = Vector3.Lerp(ecken[1], ecken[2], (u - 0.4f) / 0.4f);
            else if (u < 1.2f) tip = Vector3.Lerp(ecken[2], ecken[3], (u - 0.8f) / 0.4f);
            else tip = ecken[3];
        }

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
            else if (scenario == "RecallApproach")
            {
                // Rechte Hand nähert sich der Demo-Wasserlinie (world ≈ (−0.6..0.5, 0.9, 0.5)):
                // fern → nah → im Korridor (Kritisch). Linke Pinch ab t=8 s → Export-Trigger.
                if (T < 2f) f.IndexTip = new Vector3(0f, 1.3f, 1.2f);
                else if (T < 6f) f.IndexTip = Vector3.Lerp(new Vector3(0f, 1.3f, 1.2f), new Vector3(0f, 0.92f, 0.5f), (T - 2f) / 4f);
                else f.IndexTip = new Vector3(0f, 0.92f, 0.5f);
                f.PalmCenter = f.IndexTip + Vector3.back * 0.1f;
                f.PalmNormal = Vector3.forward;
            }
            else if (scenario == "DemoDreiLeitungen" && !left)
            {
                // Sim-Demo (16.11.): drei Objekte in EINEM Lauf, Drag-Grammatik (Pinch halten,
                // Linie ziehen, loslassen = EndDraw — docs/03 „Linie folgt der Hand"). Typwechsel
                // über echte Menü-Gesten. Zeiten: Objekt 1 Elektro (Auto-Start) 1.0–5.4 ·
                // Menü→Wasser 6.5–8.5 · Objekt 2 9.0–13.4 · Menü→Heizung 14.5–16.5 · Objekt 3 17.0–21.4.
                float t = T;
                bool pinch; Vector3 tip;
                if (t < 9.0f) Drag(A, t, 1.0f, out tip, out pinch);       // Objekt 1 + Menü-Fenster 1
                else if (t < 17.0f) Drag(B, t, 9.0f, out tip, out pinch); // Objekt 2 + Menü-Fenster 2
                else Drag(C, t, 17.0f, out tip, out pinch);               // Objekt 3
                if (t >= 6.5f && t < 8.5f && t >= 7.3f && t < 7.7f) pinch = true;            // Menü 1: → Wasser
                if (t >= 14.5f && t < 16.5f && ((t >= 15.1f && t < 15.5f) || (t >= 15.9f && t < 16.3f))) pinch = true; // Menü 2: → Heizung
                f.IndexTip = tip;
                f.Pinching = pinch;
                f.PalmCenter = tip + Vector3.back * 0.1f;
                f.PalmNormal = Vector3.forward;
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
                if (scenario == "RecallApproach") f.Pinching = T > 8f && T < 8.5f; // Export-Flanke
                // Achtung: linke Hand NIE pincht in DemoDreiLeitungen — linker Pinch + Handfläche
                // zum Körper wäre der SceneSwitcher (unbeabsichtigter Szenenwechsel).
            }
            return f;
        }

        Vector3 GetRightFrame() => GetHand(false).IndexTip;

        public bool PalmFacingBody(bool left)
        {
            if (!left) return false;
            if (scenario == "MenuCycle") return T > 1f && T < 4f;
            if (scenario == "DemoDreiLeitungen")
                // Fenster MÜSSEN exakt zur GetHand-Timeline passen (16.11.-Befund: stale Fenster
                // hier öffneten das Menü mitten im dritten Draw und verschoben alles danach)
                return (T >= 6.5f && T < 8.5f) ||    // Menü 1: rechter Pinch 7.3 → Wasser
                       (T >= 14.5f && T < 16.5f);    // Menü 2: Pinch 15.1 → Wasser, 15.9 → Heizung
            return false;
        }

        float SzenarioEnde()
        {
            switch (scenario)
            {
                case "DemoDreiLeitungen": return 25f;
                default: return 12f;
            }
        }

        void Update()
        {
            if (T > SzenarioEnde() && !logged)
            {
                logged = true;
                Debug.Log("[HGW-AUTO] Szenario '" + scenario + "' Zeitfenster abgelaufen (" + SzenarioEnde() + " s).");
            }
        }

        bool logged;
    }
}
