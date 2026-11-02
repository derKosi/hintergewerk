// Hintergewerk — Export-Runner: Raum → Grundriss-PDF im persistentDataPath.
// Nur Interfaces + getestete Klassen; der Pfad geht zurück in die UI („Export fertig: {0}").
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Hintergewerk.Core;
using Hintergewerk.Data;

namespace Hintergewerk.Export
{
    public static class ExportRunner
    {
        public static string ExportRoomPdf(HouseDataStore store, string roomId, IAnchorProvider anchors, out int scaleRatio)
        {
            var room = store.Data.rooms.Find(r => r.id == roomId);
            var mapped = new List<MappedObject>();

            foreach (var o in store.ObjectsInRoom(roomId))
            {
                if (o.orphaned) continue; // nicht verortbar → nicht im Plan erfinden
                if (!anchors.TryGetPose(o.anchor_id, out var pose)) continue;

                var m = new MappedObject
                {
                    Type = o.type,
                    DepthCm = (float)o.geometry.depth_cm,
                    Confidence = o.confidence,
                    CorridorCm = (float)o.safety_corridor_cm,
                    Id = o.id
                };
                m.WorldPoints.AddRange(CorridorFactory.ToWorld(o, pose));
                mapped.Add(m);
            }

            var doc = MapRenderer.BuildDoc(room?.label ?? roomId, HouseDataStore.DefaultAuthor, null, mapped, out var scale, out scaleRatio);
            var bytes = PdfEmitter.Emit(doc);

            var path = Path.Combine(Application.persistentDataPath,
                $"hintergewerk_{roomId}_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf");
            File.WriteAllBytes(path, bytes);
            Debug.Log($"[Export] {mapped.Count} Objekte, {bytes.Length} Bytes, 1:{scaleRatio} → {path}");
            return path;
        }
    }
}
