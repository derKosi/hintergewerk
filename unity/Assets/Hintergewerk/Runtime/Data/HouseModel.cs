// Hintergewerk — Datenmodell (schema_version 1)
// Reines C#, keine Unity-/SDK-Abhängigkeit: Damit ist das Modell überall testbar und überlebt Stack-Wechsel.
// Koordinaten-Konvention (02-architektur.md §2): Geometrie ist ANKER-LOKAL.
//   Punkt_world = AnchorPose * Punkt_lokal  — Anker-Drift/Re-Lokalisierung überleben so.
using System;
using System.Collections.Generic;

namespace Hintergewerk.Data
{
    public enum ObjectType { Power, Water, Heating, Stud, NoDrillZone }
    public enum Confidence { Estimated, Measured, Scanner }

    public static class Schema
    {
        public const int Version = 1;

        public static string ToKey(this ObjectType t) => t switch
        {
            ObjectType.Power => "power",
            ObjectType.Water => "water",
            ObjectType.Heating => "heating",
            ObjectType.Stud => "stud",
            _ => "no_drill_zone"
        };

        public static ObjectType ObjectTypeFromKey(string k) => k switch
        {
            "power" => ObjectType.Power,
            "water" => ObjectType.Water,
            "heating" => ObjectType.Heating,
            "stud" => ObjectType.Stud,
            _ => ObjectType.NoDrillZone
        };

        public static string ToKey(this Confidence c) => c switch
        {
            Confidence.Measured => "measured",
            Confidence.Scanner => "scanner",
            _ => "estimated"
        };

        public static Confidence ConfidenceFromKey(string k) => k switch
        {
            "measured" => Confidence.Measured,
            "scanner" => Confidence.Scanner,
            _ => Confidence.Estimated
        };

        // Menschliche Labels (Produktsprache Deutsch, PDF-Legende)
        public static string Label(this ObjectType t) => t switch
        {
            ObjectType.Power => "Elektro",
            ObjectType.Water => "Wasser",
            ObjectType.Heating => "Heizung",
            ObjectType.Stud => "Ständerwerk/Balken",
            _ => "Nicht-bohren-Zone"
        };
    }

    [Serializable]
    public class HouseData
    {
        public int schema_version = Schema.Version;
        public List<RoomData> rooms = new List<RoomData>();
        public List<AnchorData> anchors = new List<AnchorData>();
        public List<TrackedObject> objects = new List<TrackedObject>();
    }

    [Serializable]
    public class RoomData
    {
        public string id;
        public string label;
        public int floor;

        public RoomData() { }
        public RoomData(string id, string label, int floor) { this.id = id; this.label = label; this.floor = floor; }
    }

    [Serializable]
    public class AnchorData
    {
        public string id;          // z. B. UUID der OVRSpatialAnchor
        public string room_id;
        public string confidence;  // high | medium | low  (Wie stabil ist das Merkmal?)
        public string created_at;  // ISO 8601 UTC

        public AnchorData() { }
        public AnchorData(string id, string roomId, string confidence, string createdAt)
        { this.id = id; room_id = roomId; this.confidence = confidence; created_at = createdAt; }
    }

    [Serializable]
    public class GeometryData
    {
        // Polylinie als [x,y,z] in Metern, anker-lokal. Doppelte Präzision: Export soll Jahre überleben.
        public List<double[]> points_lokal = new List<double[]>();
        public double depth_cm = 4.0;
        public double diameter_cm = 2.5;

        public int Count => points_lokal.Count;

        public void AddPoint(double x, double y, double z) =>
            points_lokal.Add(new[] { x, y, z });

        public void AddPoint(float x, float y, float z) =>
            points_lokal.Add(new[] { (double)x, (double)y, (double)z });

        public double[] Point(int i) => points_lokal[i];
    }

    [Serializable]
    public class EvidenceData
    {
        public List<string> photos = new List<string>(); // relative Pfade unter persistentDataPath
        public string voice_note;                        // null = keine Notiz (bleibt null im JSON)
    }

    [Serializable]
    public class ObjectMeta
    {
        public string created_at; // ISO 8601 UTC
        public string author;     // Monteur
        public string note;
    }

    [Serializable]
    public class TrackedObject
    {
        public string id;
        public ObjectType type;
        public string room_id;
        public string anchor_id;
        public GeometryData geometry = new GeometryData();
        public Confidence confidence = Confidence.Estimated;
        public double safety_corridor_cm = 5.0;
        public EvidenceData evidence = new EvidenceData();
        public ObjectMeta meta = new ObjectMeta();

        // Laufzeit-Flag, NICHT serialisiert: Anker fehlt → „nicht verortbar" (Ehrlichkeitsregel)
        [NonSerialized] public bool orphaned;
    }
}
