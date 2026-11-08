// Hintergewerk — Demo-Seeder für Szene 04 (Recall): legt — NUR wenn im Inspector aktiviert —
// drei Demo-Objekte im leeren Raum an, damit Korridor/Warnung/Export ohne Gerätetest
// sichtbar sind (Editor/XR-Simulator). Am Gerät bleibt er AUS: echte Daten gehen vor.
// Ehrlichkeitsregel: Demo-Objekte sind als solche markiert (author "Demo", note) —
// keine erfundenen Messwerte; das fehlende Anker-Demoobjekt zeigt bewusst „nicht verortbar".
using UnityEngine;
using Hintergewerk.Capture;
using Hintergewerk.Core;
using Hintergewerk.Data;

namespace Hintergewerk.Recall
{
    // Vor allen anderen Starts seeden — sonst laden Recall/Export noch den leeren Store
    // (07.11.: RebuildCorridors und Export liefen ins Leere, bis die Ausführungsreihenfolge stand).
    [DefaultExecutionOrder(-100)]
    public class RecallDemoSeeder : MonoBehaviour
    {
        [SerializeField] MonoBehaviour anchorProviderBehaviour; // OvrAnchorProvider
        [SerializeField] string roomId = "raum_1";
        [SerializeField] float wallZ = 0.5f;    // Welt-z der Demo-Wand (Szene 04 baut das Quad dorthin)
        [SerializeField] float anchorZ = -0.3f; // Anker hinter dem Kopf, damit lokal ≠ Welt sichtbar ist
        [SerializeField] float room2X = 1.9f;   // Welt-x der zweiten Demo-Wand (M4: zweiter Raum)
        [SerializeField] bool seedRoom2 = true; // M4-Demo; bei reinem M3-Test abschaltbar

        void Start()
        {
            var anchors = anchorProviderBehaviour as IAnchorProvider;
            if (anchors == null) { Debug.LogError("[DemoSeeder] Anchor-Adapter fehlt."); return; }

            var store = new HouseDataStore();
            if (store.ObjectsInRoom(roomId).Count > 0)
            {
                Debug.Log("[DemoSeeder] Raum bereits gefüllt — kein Seed (echte Daten gehen vor).");
                return;
            }

            var anchorPose = new Pose(new Vector3(0f, 1.2f, anchorZ), Quaternion.identity);
            var anchorId = anchors.CreateAnchor(anchorPose, roomId);
            store.EnsureRoom(roomId, "Demo-Wand (Szene 04)", 0);
            store.RegisterAnchor(anchorId, roomId);

            // Punkte als WELTkoordinaten ausdrücken (Wand bei z=wallZ), dann ADR-0002-konform
            // anker-lokal speichern (gleiche Mathematik wie der CaptureDriver, getestet).
            var wasserA = new Vector3(-0.6f, 0.9f, wallZ);
            var wasserB = new Vector3(0.5f, 0.9f, wallZ);
            Seed(store, roomId, ObjectType.Water, anchorId, anchorPose, 6.0,
                "Demo: Wasserleitung (Spritzwasser Wand)", wasserA, wasserB);

            var elektroA = new Vector3(0.2f, 1.5f, wallZ);
            var elektroB = new Vector3(0.2f, 0.7f, wallZ);
            Seed(store, roomId, ObjectType.Power, anchorId, anchorPose, 4.0,
                "Demo: Elektroleitung (Unterputz)", elektroA, elektroB);

            // Bewusst verwaist: kein passender Anker → „nicht verortbar" (Ehrlichkeit sichtbar testen)
            var o = store.AddObject(ObjectType.Heating, roomId, "anc_demo_fehlt", 5.0, Confidence.Estimated);
            var hA = CaptureDriver.WorldToLocal(anchorPose, new Vector3(-0.4f, 1.2f, wallZ));
            var hB = CaptureDriver.WorldToLocal(anchorPose, new Vector3(0.3f, 1.2f, wallZ));
            o.geometry.AddPoint(hA.x, hA.y, hA.z);
            o.geometry.AddPoint(hB.x, hB.y, hB.z);
            o.meta.author = "Demo";
            o.meta.note = "Demo: Heizungsrohr — Anker absichtlich fehlend";
            store.Save();

            Debug.Log($"[DemoSeeder] 3 Demo-Objekte geseedet (Anker {anchorId}).");

            if (seedRoom2) SeedRoom2(store, anchors);
        }

        /// <summary>Zweiter Raum (M4): eigene Wand-Quad, eigener Anker, eigene Objekte —
        /// inkl. eines verwaisten (Ehrlichkeitsregel sichtbar über Räume hinweg).</summary>
        void SeedRoom2(HouseDataStore store, IAnchorProvider anchors)
        {
            const string r2 = "raum_2";

            var wall2 = GameObject.CreatePrimitive(PrimitiveType.Quad);
            wall2.name = "DemoWall_Raum2";
            wall2.transform.SetPositionAndRotation(new Vector3(room2X, 1.1f, wallZ), Quaternion.identity);
            wall2.transform.localScale = new Vector3(1.6f, 2.2f, 1f);

            var anchorPose2 = new Pose(new Vector3(room2X, 1.2f, anchorZ), Quaternion.identity);
            var anchorId2 = anchors.CreateAnchor(anchorPose2, r2);
            store.EnsureRoom(r2, "Demo-Flur (Szene 04, Raum 2)", 0);
            store.RegisterAnchor(anchorId2, r2);

            Seed(store, r2, ObjectType.Power, anchorId2, anchorPose2, 4.0,
                "Demo: Elektroleitung Flur (Unterputz)",
                new Vector3(room2X, 1.4f, wallZ), new Vector3(room2X, 0.8f, wallZ));
            Seed(store, r2, ObjectType.Stud, anchorId2, anchorPose2, 8.0,
                "Demo: Ständerwerk Flur (BeTexung)",
                new Vector3(room2X - 0.5f, 1.0f, wallZ), new Vector3(room2X + 0.3f, 1.0f, wallZ));

            Debug.Log($"[DemoSeeder] Raum 2 geseedet: 2 Objekte (Anker {anchorId2}).");
        }

        static void Seed(HouseDataStore store, string roomId, ObjectType t, string anchorId, Pose anchorPose,
                         double depth, string note, Vector3 worldA, Vector3 worldB)
        {
            var o = store.AddObject(t, roomId, anchorId, depth, Confidence.Estimated);
            var a = CaptureDriver.WorldToLocal(anchorPose, worldA);
            var b = CaptureDriver.WorldToLocal(anchorPose, worldB);
            o.geometry.AddPoint(a.x, a.y, a.z);
            o.geometry.AddPoint(b.x, b.y, b.z);
            o.meta.author = "Demo";
            o.meta.note = note;
            store.Save();
        }
    }
}
