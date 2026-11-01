// Hintergewerk — Nahtstellen (Seams) zwischen SDK-freiem Kern und Meta XR SDK.
// Der komplette Kern (Data/Capture/Recall/Export) kennt kein OVR*-API. Die dünnen SDK-Adapter
// liegen in Assets/Hintergewerk/Adapters/ (Assembly-CSharp, außerhalb dieser asmdef):
//   Adapters/OvrAnchorProvider   → OVRSpatialAnchor (erzeugen, speichern, laden)
//   Adapters/OvrHandSource       → Meta XR Hands (Pinch, Fingerspitzen)
//   Adapters/SceneWallProvider   → Scene API (Wandebenen für SnapEngine)
// Solange die Adapter fehlen, lassen sich alle Kern-Module mit Fake-Implementierungen
// im Editor/XR Simulator testen.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hintergewerk.Core
{
    /// <summary>Ebene im Raum (Wand/Decke/Boden) aus Scene API oder manueller Kalibrierung.</summary>
    public struct WallPlane
    {
        public Vector3 Point;   // Punkt auf der Ebene
        public Vector3 Normal;  // Normale, zeigt in den Raum
        public string Label;    // z. B. "wall", "ceiling" (Scene-API-Klassifikation)
    }

    /// <summary>Ein Hand-Snapshot, pro Frame abgefragt.</summary>
    public struct HandFrame
    {
        public bool Tracked;
        public Vector3 IndexTip;   // Zeigefingerspitze (Zeichnen)
        public Vector3 PalmCenter; // Handfläche (Menü-Geste)
        public bool Pinching;      // Pinch (Daumen+Zeigefinger) aktiv
        public float PinchStrength;
        public Vector3 PalmNormal; // für PalmFacingBody (Menü-Geste)
    }

    public interface IHandSource
    {
        HandFrame GetHand(bool left);
        /// <summary>Handfläche zeigt zum Körper → Menü öffnen (Grammatik §1).</summary>
        bool PalmFacingBody(bool left);
    }

    public interface IAnchorProvider
    {
        /// <summary>Anker erzeugen. Liefert anchor_id (HouseDataStore.RegisterAnchor danach aufrufen).</summary>
        string CreateAnchor(Pose pose, string roomId);
        bool TryGetPose(string anchorId, out Pose pose);
        /// <summary>Alle Anker persistent speichern (SDK-seitig, z. B. OVRSpatialAnchor.Save).</summary>
        void PersistAll();
        /// <summary>Anker asynchron laden (App-Start). onDone(false) = Fallback-Weg einschlagen.</summary>
        void LoadAll(Action<bool> onDone);

        /// <summary>Bekannte (geladene oder erzeugte) Anker-Ids — für Demo, Store-Abgleich und Debug.</summary>
        IEnumerable<string> KnownAnchorIds { get; }
    }

    public interface IWallProvider
    {
        List<WallPlane> GetWalls();
        /// <summary>Wand unter einem Weltpunkt suchen (Raycast auf Scene-Ebenen). maxDist in Metern.</summary>
        bool TryFindPlane(Vector3 point, Vector3 direction, float maxDist, out WallPlane plane);
    }
}
