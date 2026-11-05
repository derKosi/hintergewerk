// Hintergewerk — skriptgesteuerte Hände (Editor-Iteration ohne Gerät).
// Rechte Hand: Frame-Queue, die Tick-für-Tick abgearbeitet wird (letzter Frame wiederholt sich).
// Linke Hand: ein konfigurierbarer Frame (für die Zwei-Hand-Tiefe).
using System.Collections.Generic;
using UnityEngine;
using Hintergewerk.Core;

namespace Hintergewerk.Mocks
{
    public class ScriptedHandSource : IHandSource
    {
        readonly Queue<HandFrame> rightFrames = new();
        HandFrame lastRight = new HandFrame();
        HandFrame leftFrame = new HandFrame();

        public ScriptedHandSource() { }
        public ScriptedHandSource(IEnumerable<HandFrame> rightFrames)
        {
            foreach (var f in rightFrames) this.rightFrames.Enqueue(f);
        }

        /// <summary>Linker Hand-Frame (für Tiefe: zwei Hand-Spitzen = Abstand).</summary>
        public HandFrame LeftFrame
        {
            get => leftFrame;
            set => leftFrame = value;
        }

        public HandFrame GetHand(bool left) => left ? leftFrame : NextRight();

        /// <summary>Mock: Menü-Geste nicht skriptiert — immer false (Grammatik §1).</summary>
        public bool PalmFacingBody(bool left) => false;

        HandFrame NextRight()
        {
            if (rightFrames.Count > 0) lastRight = rightFrames.Dequeue();
            return lastRight;
        }

        public void Enqueue(HandFrame f) => rightFrames.Enqueue(f);
        public void Clear() => rightFrames.Clear();

        public static HandFrame Frame(Vector3 tip, bool pinching, bool tracked = true) => new HandFrame
        {
            Tracked = tracked,
            IndexTip = tip,
            PalmCenter = tip + Vector3.down * 0.08f,
            Pinching = pinching,
            PinchStrength = pinching ? 1f : 0f
        };
    }
}
