// Hintergewerk — temporärer Diagnose-MB für den M0-Smoketest (logcat-Debugging).
// Loggt alle 2 s den Hand-Tracking-Zustand: IsTracked/IsDataValid/PoseSourceInferred,
// Skeleton-Bone-Anzahl und Handwurzel-Pose. Nach Verifikation ENTFERNEN.
// Gehört in Adapters/ (Assembly-CSharp), weil Invariante 3: kein OVR* in Runtime/.
using UnityEngine;

namespace Hintergewerk.Adapters
{
    public class HandDebugProbe : MonoBehaviour
    {
        float next;

        void Update()
        {
            if (Time.unscaledTime < next) return;
            next = Time.unscaledTime + 2f;

            var sb = new System.Text.StringBuilder("HGW-PROBE");
            foreach (var h in FindObjectsByType<OVRHand>(FindObjectsInactive.Exclude))
            {
                sb.Append(" [").Append(name(h)).Append(" tracked=").Append(h.IsTracked)
                  .Append(" dataValid=").Append(h.IsDataValid)
                  .Append(" highConf=").Append(h.IsDataHighConfidence)
                  .Append(" inferred=").Append(h.PoseSourceInferred)
                  .Append(" pinch=").Append(h.GetFingerIsPinching(OVRHand.HandFinger.Index))
                  .Append(" tip=").Append(h.PointerPose != null && h.PointerPose.position != Vector3.zero
                      ? h.PointerPose.position.ToString("F2") : "-");
                var skel = h.GetComponent<OVRCustomSkeleton>();
                if (skel != null && skel.Bones != null && skel.Bones.Count > 0)
                {
                    sb.Append(" bones=").Append(skel.Bones.Count);
                    foreach (var b in skel.Bones)
                        if (b.Id.ToString().Contains("WristRoot"))
                            sb.Append(" wrist=").Append(b.Transform.position.ToString("F2"));
                }
                else sb.Append(" bones=0");
                sb.Append("]");
            }
            Debug.Log(sb.ToString());
        }

        static string name(OVRHand h) => h.transform.root.name.Contains("Left") || h.name.Contains("l_") || h.transform.parent.name.Contains("Left")
            ? "L" : "R";
    }
}
