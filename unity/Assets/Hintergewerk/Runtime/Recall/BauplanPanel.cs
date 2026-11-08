// Hintergewerk — Bauplan in der Hand (D39, ADR-0005): Live-Top-Down des Raums auf einem
// Panel an der linken Hand. Der Plan ist OUTPUT (wächst beim Zeichnen mit) — gezeichnet
// wird weiter an der Wand. Wiederverwendet die Store-/Anker-Interfaces, kein neuer Pfad.
// Ehrlichkeit: „nicht verortbare" Objekte erscheinen NICHT im Plan (keine Pose → keine
// Erfindung), werden aber in der Statuszeile geführt — wie im Recall-Overlay.
using System.Collections.Generic;
using UnityEngine;
using Hintergewerk.Core;
using Hintergewerk.Data;
using Hintergewerk.Localization;

namespace Hintergewerk.Recall
{
    public class BauplanPanel : MonoBehaviour
    {
        [SerializeField] MonoBehaviour anchorProviderBehaviour; // OvrAnchorProvider
        [SerializeField] Transform followTarget;                // linke Hand (Anchor)
        [SerializeField] string roomId = "raum_1";
        [SerializeField] float panelWidthM = 0.22f;
        [SerializeField] float panelHeightM = 0.16f;
        [SerializeField] float pollSeconds = 0.5f;
        [SerializeField] Color paperColor = new Color(0.94f, 0.92f, 0.87f);

        IAnchorProvider anchors;
        HouseDataStore store;
        Transform paper;
        TextMesh status;
        readonly List<LineRenderer> lines = new();
        PlanTransform plan;
        float nextPoll;
        int lastSignature = int.MinValue;

        void Start()
        {
            anchors = anchorProviderBehaviour as IAnchorProvider;
            if (anchors == null) { Debug.LogError("[Bauplan] Anchor-Adapter fehlt oder implementiert das Interface nicht."); enabled = false; return; }
            store = new HouseDataStore();
            BuildPanel();
            anchors.LoadAll(_ => { /* Posen folgen asynchron — Poll rebuildet */ });
        }

        void BuildPanel()
        {
            // Papier (Quad, Vorderseite +z nach Drehung unten)
            paper = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            paper.name = "Bauplan_Papier";
            paper.SetParent(transform, false);
            paper.localPosition = Vector3.zero;
            paper.localRotation = Quaternion.Euler(0f, 180f, 0f); // Unity-Quad zeigt −z → zur Kopfseite drehen
            paper.localScale = new Vector3(panelWidthM, panelHeightM, 1f);
            var mr = paper.GetComponent<MeshRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            var paperMat = new Material(shader != null ? shader : Shader.Find("Sprites/Default")) { color = paperColor };
            if (paperMat.HasProperty("_Cull")) paperMat.SetFloat("_Cull", 0f); // von beiden Seiten sichtbar
            mr.material = paperMat;

            // Statuszeile (TextMesh, kein TMP — wie im Rest der App). Kopfseite = lokal +z
            // (LookRotation zeigt zum Kopf), deshalb Text/Linien bei +z — sonst gespiegelt.
            var statusGo = new GameObject("Bauplan_Status");
            statusGo.transform.SetParent(transform, false);
            statusGo.transform.localPosition = new Vector3(0f, -panelHeightM * 0.5f + 0.012f, 0.002f);
            statusGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // TextMesh zeigt nach −z → zur Kopfseite (+z) drehen (Beweisfoto m9b)
            status = statusGo.AddComponent<TextMesh>();
            status.fontSize = 100;
            status.characterSize = 0.0055f;
            status.anchor = TextAnchor.MiddleCenter;
            status.alignment = TextAlignment.Center;
            status.color = new Color(0.15f, 0.15f, 0.15f);
            status.text = "";
        }

        void Update()
        {
            FollowLeftHand();

            if (Time.time >= nextPoll)
            {
                nextPoll = Time.time + pollSeconds;
                var sig = StoreSignature();
                if (sig != lastSignature)
                {
                    lastSignature = sig;
                    Rebuild();
                }
            }
        }

        void FollowLeftHand()
        {
            if (followTarget == null) return;
            var target = followTarget.position + Vector3.down * 0.05f;
            transform.position = Vector3.Lerp(transform.position, target, 0.35f);
            var head = Camera.main != null ? Camera.main.transform.position : target + Vector3.back;
            var toHead = head - target;
            toHead.y = 0f;
            var rot = toHead.sqrMagnitude > 1e-4f
                ? Quaternion.LookRotation(toHead.normalized, Vector3.up)
                : Quaternion.identity;
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 0.35f);
        }

        int StoreSignature()
        {
            int h = 17;
            foreach (var o in string.IsNullOrEmpty(roomId) ? store.Data.objects : store.ObjectsInRoom(roomId))
            {
                h = h * 31 + o.id.GetHashCode();
                h = h * 31 + o.geometry.Count;
                h = h * 31 + (int)o.type;
            }
            return h;
        }

        void Rebuild()
        {
            foreach (var l in lines) if (l != null) Destroy(l.gameObject);
            lines.Clear();

            // roomId leer = alle Räume (M4 „zweiter Raum"): der Bauplan zeigt das ganze Haus.
            var objs = string.IsNullOrEmpty(roomId) ? store.Data.objects : store.ObjectsInRoom(roomId);
            var worldPts = new List<Vector3>();
            int located = 0;
            var locatedObjs = new List<TrackedObject>();
            foreach (var o in objs)
            {
                if (o.orphaned || !anchors.TryGetPose(o.anchor_id, out var pose)) continue; // ehrlich: keine Pose → kein Plan-Eintrag
                located++;
                locatedObjs.Add(o);
                for (int i = 0; i < o.geometry.Count; i++)
                {
                    var p = o.geometry.Point(i);
                    worldPts.Add(pose.position + pose.rotation * new Vector3((float)p[0], (float)p[1], (float)p[2]));
                }
            }

            plan = PlanProjection.Fit(worldPts, new Vector2(panelWidthM * 0.9f, panelHeightM * 0.82f));

            foreach (var o in locatedObjs)
            {
                anchors.TryGetPose(o.anchor_id, out var pose);
                var line = new GameObject("Plan_" + o.id).AddComponent<LineRenderer>();
                line.transform.SetParent(transform, false);
                line.useWorldSpace = false;
                line.positionCount = o.geometry.Count;
                for (int i = 0; i < o.geometry.Count; i++)
                {
                    var p = o.geometry.Point(i);
                    var world = pose.position + pose.rotation * new Vector3((float)p[0], (float)p[1], (float)p[2]);
                    var l2 = plan.ToLocal(world);
                    line.SetPosition(i, new Vector3(l2.x - panelWidthM * 0.5f, l2.y - panelHeightM * 0.5f, 0.002f));
                }
                var width = Mathf.Clamp(plan.Scale * (float)o.safety_corridor_cm / 100f, 0.003f, 0.018f);
                line.startWidth = width;
                line.endWidth = width;
                var c = RecallPalette.TypeColor(o.type);
                if (o.confidence != Confidence.Measured) c.a = 0.65f; // Geschätztes sichtbar schwächer (Konfidenz-Regel)
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                line.material = new Material(shader != null ? shader : Shader.Find("Sprites/Default")) { color = c };
                lines.Add(line);
            }

            int notLocatable = objs.Count - located;
            status.text = Strings.Format("recall.loaded", objs.Count) +
                          (notLocatable > 0 ? " · " + Strings.Format("recall.located_status", located, notLocatable) : "");
        }
    }
}
