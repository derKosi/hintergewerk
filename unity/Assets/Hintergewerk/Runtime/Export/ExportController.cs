// Hintergewerk — Export-Controller (M3): im Recall-Raum per Geste den Raum als
// PDF-Grundriss + JSON exportieren (persistentDataPath, Abholung via adb — docs/07 §E).
// Geste: linke Hand Pinch (steigende Flanke) — rechte Hand bleibt frei fürs Erkunden.
// Kein SDK: nur Interfaces + ExportRunner/MapRenderer/PdfEmitter (getestete Kern-Klassen).
using UnityEngine;
using Hintergewerk.Core;
using Hintergewerk.Data;
using Hintergewerk.Localization;

namespace Hintergewerk.Export
{
    public class ExportController : MonoBehaviour
    {
        [SerializeField] MonoBehaviour anchorProviderBehaviour; // OvrAnchorProvider
        [SerializeField] MonoBehaviour handSourceBehaviour;     // OvrHandSource
        [SerializeField] string roomId = "raum_1";
        [SerializeField] TextMesh statusText;
        [SerializeField] float cooldownS = 3f;

        IAnchorProvider anchors;
        IHandSource hands;
        HouseDataStore store;
        bool pinchWasDown;
        float readyAt;
        string lastStatus;

        void Start()
        {
            anchors = anchorProviderBehaviour as IAnchorProvider;
            hands = handSourceBehaviour as IHandSource;
            if (anchors == null || hands == null)
            {
                Debug.LogError("[ExportController] Adapter fehlen oder implementieren die Interfaces nicht.");
                enabled = false;
                return;
            }
            store = new HouseDataStore(); // lädt persistentDataPath (Auto-Save-Datei des Capture-Runs)
            SetStatus(Strings.Get("export.hint"));
        }

        void Update()
        {
            var left = hands.GetHand(left: true);
            bool pinch = left.Tracked && left.Pinching;
            bool rising = pinch && !pinchWasDown;
            pinchWasDown = pinch;

            if (rising && Time.time >= readyAt)
            {
                readyAt = Time.time + cooldownS;
                SetStatus(Strings.Get("export.started"));
                var json = ExportRunner.ExportJson(store, out _); // JSON bleibt 1:1 das ganze Haus (D32)

                // roomId leer = alle Räume (M4): je Raum ein PDF-Grundriss, JSON bleibt global.
                string pdfList;
                if (string.IsNullOrEmpty(roomId))
                {
                    var names = new System.Collections.Generic.List<string>();
                    foreach (var room in store.Data.rooms)
                        names.Add(System.IO.Path.GetFileName(ExportRunner.ExportRoomPdf(store, room.id, anchors, out _)));
                    pdfList = string.Join(" + ", names);
                }
                else
                {
                    pdfList = System.IO.Path.GetFileName(ExportRunner.ExportRoomPdf(store, roomId, anchors, out _));
                }
                SetStatus(Strings.Format("export.done", pdfList)
                    + " + " + System.IO.Path.GetFileName(json));
            }
        }

        void SetStatus(string s)
        {
            if (s == lastStatus) return;
            lastStatus = s;
            if (statusText != null) statusText.text = s;
            Debug.Log("[ExportController] " + s);
        }
    }
}
