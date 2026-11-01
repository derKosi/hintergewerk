// Hintergewerk — SDK-Adapter: Hände (OVRHand + OVRSkeleton).
// LIEGT AUSSERHALB der Runtime-asmdef (Assembly-CSharp): Hier ist OVR* erlaubt.
//
// Verifiziert (01.11.): Offizielles Meta-Tutorial „unity-tutorial-basic-hand-training"
// zeigt GetFingerIsPinching(OVRHand.HandFinger.Index); OVRHand liefert Pinch-Strength
// und PointerPose (Klassenreferenz). TODO(02.11., v207): BoneId-Details (Hand_Index3
// vs. Hand_IndexTip) und Palm-Normal-Ausrichtung links/rechts am Gerät tunen.
using UnityEngine;
using Hintergewerk.Core;

namespace Hintergewerk.Adapters
{
    public class OvrHandSource : MonoBehaviour, IHandSource
    {
        [SerializeField] OVRHand leftHand;
        [SerializeField] OVRHand rightHand;
        [SerializeField] OVRSkeleton leftSkeleton;
        [SerializeField] OVRSkeleton rightSkeleton;

        public HandFrame GetHand(bool left)
        {
            var hand = left ? leftHand : rightHand;
            var skeleton = left ? leftSkeleton : rightSkeleton;
            var f = new HandFrame();

            if (hand == null) return f;
            f.Tracked = hand.IsTracked; // TODO(02.11.): Property-Name gegen v207 prüfen
            if (!f.Tracked) return f;

            // Offiziell bestätigt (Meta-Tutorial + Klassenreferenz):
            f.Pinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            f.PinchStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);

            // PointerPose als IndexTip-Fallback (Position des Daumen-Zeigefinger-Strahls)
            f.IndexTip = hand.PointerPose != null ? hand.PointerPose.position : hand.transform.position;

            if (skeleton != null && skeleton.Bones != null)
            {
                if (TryBone(skeleton, OVRPlugin.BoneId.Hand_Wrist, out var wrist) &
                    TryBone(skeleton, OVRPlugin.BoneId.Hand_Index1, out var iMcp) &
                    TryBone(skeleton, OVRPlugin.BoneId.Hand_Pinky1, out var pMcp))
                {
                    f.PalmCenter = (wrist + iMcp + pMcp) / 3f;
                    var n = Vector3.Cross(iMcp - wrist, pMcp - wrist).normalized;
                    f.PalmNormal = left ? -n : n; // TODO(02.11.): Ausrichtung links/rechts am Gerät tunen
                }
                if (TryBone(skeleton, OVRPlugin.BoneId.Hand_Index3, out var indexTip)) // TODO: Hand_IndexTip?
                    f.IndexTip = indexTip;
            }
            return f;
        }

        public bool PalmFacingBody(bool left)
        {
            var f = GetHand(left);
            if (!f.Tracked) return false;
            var cam = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            // Palm-Normale zeigt in Richtung Kopf => Handfläche zum Körper
            return Vector3.Dot(f.PalmNormal, f.PalmCenter - cam) > 0f;
        }

        static bool TryBone(OVRSkeleton skeleton, OVRPlugin.BoneId id, out Vector3 pos)
        {
            foreach (var b in skeleton.Bones)
                if (b.Id == id) { pos = b.Transform.position; return true; }
            pos = Vector3.zero;
            return false;
        }
    }
}
