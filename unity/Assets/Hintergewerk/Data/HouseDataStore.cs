// Hintergewerk — Persistenz-Speicher (CRUD + Auto-Save).
// Nutzt nur UnityEngine (Pfad/Logging), kein Meta SDK. Atomares Schreiben über .tmp;
// korrupte Dateien werden NIEMALS überschrieben, sondern als .corrupt-<ts> gesichert.
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hintergewerk.Data
{
    public class HouseDataStore
    {
        public const string DefaultAuthor = "Mathias";

        public HouseData Data { get; private set; } = new HouseData();

        readonly string path;
        public string Path => path;

        public HouseDataStore() : this(null) { }

        /// <param name="explicitPath">Für Tests/Editor; null = persistentDataPath.</param>
        public HouseDataStore(string explicitPath)
        {
            path = explicitPath ?? DefaultPath();
            Load();
        }

        public static string DefaultPath() =>
            System.IO.Path.Combine(Application.persistentDataPath, "hintergewerk.json");

        // ---------------- Laden / Speichern ----------------

        void Load()
        {
            if (!File.Exists(path)) return;
            try
            {
                Data = SimpleJson.Parse(File.ReadAllText(path));
                RebuildOrphans();
                Debug.Log($"[Store] {Data.rooms.Count} Räume, {Data.anchors.Count} Anker, {Data.objects.Count} Objekte geladen: {path}");
            }
            catch (Exception e)
            {
                var backup = path + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                try { File.Copy(path, backup, true); } catch { /* best effort */ }
                Debug.LogError($"[Store] JSON unlesbar ({e.Message}). Alte Datei gesichert als {backup}. Starte mit leerem Haus — NICHT gelöscht.");
                Data = new HouseData();
            }
        }

        /// <summary>Atomar schreiben. Wird nach jeder Mutation aufgerufen (Auto-Save, kein Save-Button).</summary>
        public void Save()
        {
            var tmp = path + ".tmp";
            File.WriteAllText(tmp, SimpleJson.Write(Data));
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }

        // ---------------- Räume ----------------

        public RoomData EnsureRoom(string roomId, string label, int floor)
        {
            var r = Data.rooms.Find(x => x.id == roomId);
            if (r == null)
            {
                r = new RoomData(roomId, label, floor);
                Data.rooms.Add(r);
                Save();
            }
            return r;
        }

        // ---------------- Anker ----------------

        public void RegisterAnchor(string anchorId, string roomId, string confidence = "high")
        {
            if (Data.anchors.Exists(a => a.id == anchorId)) return;
            Data.anchors.Add(new AnchorData(anchorId, roomId, confidence, Now()));
            RebuildOrphans();
            Save();
        }

        public bool HasAnchor(string anchorId) =>
            Data.anchors.Exists(a => a.id == anchorId);

        // ---------------- Objekte ----------------

        public TrackedObject AddObject(ObjectType type, string roomId, string anchorId,
                                        double depthCm, Confidence confidence = Confidence.Estimated)
        {
            var o = new TrackedObject
            {
                id = "obj_" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N").Substring(0, 4),
                type = type,
                room_id = roomId,
                anchor_id = anchorId,
                confidence = confidence,
                safety_corridor_cm = DefaultCorridorCm(type),
                meta = new ObjectMeta { created_at = Now(), author = DefaultAuthor, note = "" }
            };
            o.geometry.depth_cm = depthCm;
            o.geometry.diameter_cm = DefaultDiameterCm(type);
            Data.objects.Add(o);
            Save();
            return o;
        }

        public void RemoveObject(string objectId)
        {
            var o = Data.objects.Find(x => x.id == objectId);
            if (o == null) return;
            Data.objects.Remove(o);
            Save();
        }

        public List<TrackedObject> ObjectsInRoom(string roomId) =>
            Data.objects.FindAll(o => o.room_id == roomId);

        /// <summary>Objekte, deren Anker fehlt → „nicht verortbar" (Ehrlichkeitsregel).</summary>
        public List<TrackedObject> OrphanedObjects() =>
            Data.objects.FindAll(o => o.orphaned);

        // ---------------- Interna ----------------

        void RebuildOrphans()
        {
            var ids = new HashSet<string>();
            foreach (var a in Data.anchors) ids.Add(a.id);
            foreach (var o in Data.objects) o.orphaned = !ids.Contains(o.anchor_id);
        }

        static string Now() => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        public static double DefaultCorridorCm(ObjectType t) => t switch
        {
            ObjectType.Power => 5.0,
            ObjectType.Water => 6.0,
            ObjectType.Heating => 6.0,
            ObjectType.Stud => 3.0,
            _ => 10.0 // NoDrillZone als Fläche: großzügiger Korridor
        };

        public static double DefaultDiameterCm(ObjectType t) => t switch
        {
            ObjectType.Power => 2.5,
            ObjectType.Water => 4.0,
            ObjectType.Heating => 3.5,
            ObjectType.Stud => 6.0,
            _ => 0.0
        };
    }
}
