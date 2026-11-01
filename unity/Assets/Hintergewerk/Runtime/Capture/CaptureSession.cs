// Hintergewerk — Capture-Flow als explizite Zustandsmaschine (docs/03 §3).
// Rein (kein SDK, kaum Unity): Der DrawController (morgen, mit Händen) füttert sie
// mit Snap-Ergebnissen; Confirm liefert das fertige TrackedObject für den Store.
//
// Phasen: Idle → Typwahl → Zeichnen → Tiefe → Bestaetigen → Idle
// Jede Methode ist eine geschützte Transition (false = illegaler Übergang).
using System;
using System.Collections.Generic;
using UnityEngine;
using Hintergewerk.Data;

namespace Hintergewerk.Capture
{
    public enum CapturePhase { Idle, Typwahl, Zeichnen, Tiefe, Bestaetigen }

    public class CaptureSession
    {
        public CapturePhase Phase { get; private set; } = CapturePhase.Idle;
        public ObjectType? Type { get; private set; }
        public double DepthCm { get; private set; }
        public Confidence Confidence { get; private set; } = Confidence.Estimated;
        public IReadOnlyList<double[]> Punkte => punkte;
        readonly List<double[]> punkte = new();

        public event Action<CapturePhase> PhaseChanged;

        void Go(CapturePhase p) { Phase = p; PhaseChanged?.Invoke(p); }

        public bool BeginTypwahl()
        {
            if (Phase != CapturePhase.Idle) return false;
            Go(CapturePhase.Typwahl);
            return true;
        }

        public bool SelectType(ObjectType t)
        {
            if (Phase != CapturePhase.Typwahl) return false;
            Type = t;
            DepthCm = HouseDataStore.DefaultDepthCm(t); // Typ-Preset als Startwert
            punkte.Clear();
            Go(CapturePhase.Zeichnen);
            return true;
        }

        public bool AddPoint(double x, double y, double z)
        {
            if (Phase != CapturePhase.Zeichnen) return false;
            punkte.Add(new[] { x, y, z }); // anker-lokal (ADR 0002)
            return true;
        }

        public bool UndoLastPoint()
        {
            if (Phase != CapturePhase.Zeichnen || punkte.Count == 0) return false;
            punkte.RemoveAt(punkte.Count - 1);
            return true;
        }

        public bool EndDraw()
        {
            if (Phase != CapturePhase.Zeichnen || punkte.Count < 2) return false;
            Go(CapturePhase.Tiefe);
            return true;
        }

        public bool SetDepth(double cm)
        {
            if (Phase != CapturePhase.Tiefe) return false;
            DepthCm = Mathf.Clamp((float)cm, 0f, 30f);
            return true;
        }

        public bool SetConfidence(Confidence c)
        {
            if (Phase != CapturePhase.Tiefe && Phase != CapturePhase.Bestaetigen) return false;
            Confidence = c;
            return true;
        }

        public bool BeginBestaetigen()
        {
            if (Phase != CapturePhase.Tiefe) return false;
            Go(CapturePhase.Bestaetigen);
            return true;
        }

        /// <summary>Schließt den Flow ab und liefert das fertige Objekt (Store: AddTracked).</summary>
        public TrackedObject Confirm(string anchorId, string roomId)
        {
            if (Phase != CapturePhase.Bestaetigen) return null;
            var t = Type ?? ObjectType.Power;
            var o = new TrackedObject
            {
                id = "obj_" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N").Substring(0, 4),
                type = t,
                room_id = roomId,
                anchor_id = anchorId,
                confidence = Confidence,
                safety_corridor_cm = HouseDataStore.DefaultCorridorCm(t)
            };
            o.geometry.depth_cm = DepthCm;
            o.geometry.diameter_cm = HouseDataStore.DefaultDiameterCm(t);
            o.geometry.points_lokal.AddRange(punkte);
            o.meta.created_at = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            Reset();
            return o;
        }

        public bool Cancel()
        {
            if (Phase == CapturePhase.Idle) return false;
            Reset();
            return true;
        }

        void Reset()
        {
            Type = null;
            DepthCm = 0;
            Confidence = Confidence.Estimated;
            punkte.Clear();
            Go(CapturePhase.Idle);
        }
    }
}
