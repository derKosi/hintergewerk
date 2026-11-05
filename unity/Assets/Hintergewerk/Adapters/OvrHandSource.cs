// Hintergewerk — SDK-Adapter: Hände (OVRHand + OVRSkeleton, SDK 207.0.1).
// LIEGT AUSSERHALB der Runtime-asmdef (Assembly-CSharp): Hier ist OVR* erlaubt.
// Verifiziert 06.11. gegen den SDK-Quellcode:
//   OVRHand.IsTracked · GetFingerIsPinching(HandFinger) · GetFingerPinchStrength(HandFinger)
//   PointerPose (Transform, kann null sein) · OVRSkeleton.Bones : IList<OVRBone>
//   OVRBone.Id : OVRSkeleton.BoneId — Hand_WristRoot / Hand_IndexTip / Hand_Pinky1 existieren.
// Palm-Normal-Ausrichtung links/rechts am Gerät tunen (TODO am Gerät).
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
            f.Tracked = hand.IsTracked;
            if (!f.Tracked) return f;

            f.Pinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            f.PinchStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);

            // PointerPose als IndexTip-Fallback (Position des Daumen-Zeigefinger-Strahls)
            f.IndexTip = hand.PointerPose != null ? hand.PointerPose.position : hand.transform.position;

            if (skeleton != null && skeleton.Bones != null)
            {
                if (TryBone(skeleton, OVRSkeleton.BoneId.Hand_WristRoot, out var wrist) &
                    TryBone(skeleton, OVRSkeleton.BoneId.Hand_Index1, out var iMcp) &
                    TryBone(skeleton, OVRSkeleton.BoneId.Hand_Pinky1, out var pMcp))
                {
                    f.PalmCenter = (wrist + iMcp + pMcp) / 3f;
                    var n = Vector3.Cross(iMcp - wrist, pMcp - wrist).normalized;
                    f.PalmNormal = left ? -n : n; // TODO: Ausrichtung links/rechts am Gerät tunen
                }
                if (TryBone(skeleton, OVRSkeleton.BoneId.Hand_IndexTip, out var indexTip))
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

        static bool TryBone(OVRSkeleton skeleton, OVRSkeleton.BoneId id, out Vector3 pos)
        {
            foreach (var b in skeleton.Bones)
                if (b.Id == id) { pos = b.Transform.position; return true; }
            pos = Vector3.zero;
            return false;
        }
    }
}
