// Hintergewerk — minimaler Pinch-Greif-Demo (M0-Spielwiese, kein Interaction SDK):
// Pinch nahe einem Objekt greift, Objekt folgt der Pinch-Pose, Release wirft
// mit der geglätteten Handgeschwindigkeit. Objekte ruhen auf dem Tisch
// (SmoketestPlacer, Awake) — Greifen/ Werfen/ Aufprall-Ton via CollisionBlip.
using System.Collections.Generic;
using UnityEngine;

namespace Hintergewerk.Adapters
{
    public class ThrowCube : MonoBehaviour
    {
        [SerializeField] Transform cube;
        [SerializeField] List<Transform> more = new();
        [SerializeField] float grabRadius = 0.14f;
        [SerializeField] float throwVelocityScale = 1.0f;
        [SerializeField] float resetBelowY = -5f;

        readonly Dictionary<Transform, Rigidbody> bodies = new();
        readonly Dictionary<Transform, Vector3> homes = new();
        Vector3 lastTip;
        Vector3 smoothVel;
        Transform held;

        void Start()
        {
            foreach (var t in AllObjects())
            {
                if (t == null) continue;
                var rb = t.GetComponent<Rigidbody>();
                if (rb == null) rb = t.gameObject.AddComponent<Rigidbody>();
                if (t.GetComponent<Collider>() == null) t.gameObject.AddComponent<BoxCollider>();
                rb.useGravity = true;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                if (t.GetComponent<CollisionBlip>() == null) t.gameObject.AddComponent<CollisionBlip>();
                bodies[t] = rb;
                homes[t] = t.position;   // placer liefert in Awake die Tisch-Endpositionen
            }
        }

        IEnumerable<Transform> AllObjects()
        {
            yield return cube;
            foreach (var m in more) yield return m;
        }

        void Update()
        {
            // Gefallene Objekte zurück auf den Tisch
            foreach (var kv in bodies)
                if (kv.Key != held && kv.Key.position.y < resetBelowY)
                {
                    kv.Value.isKinematic = false;
                    kv.Value.linearVelocity = Vector3.zero;
                    kv.Value.angularVelocity = Vector3.zero;
                    kv.Key.position = homes[kv.Key];
                }

            foreach (var h in FindObjectsByType<OVRHand>(FindObjectsInactive.Exclude))
            {
                if (!h.IsDataValid || !h.IsTracked || h.PointerPose == null) continue;
                bool pinch = h.GetFingerIsPinching(OVRHand.HandFinger.Index);
                Vector3 tip = h.PointerPose.position;

                if (held == null && pinch)
                {
                    foreach (var kv in bodies)
                    {
                        if ((tip - kv.Key.position).sqrMagnitude > grabRadius * grabRadius) continue;
                        held = kv.Key;
                        held.GetComponent<CollisionBlip>()?.Blip(1.4f);
                        kv.Value.isKinematic = true;
                        smoothVel = Vector3.zero;
                        lastTip = tip;
                        return;
                    }
                }
                else if (held != null)
                {
                    float dt = Time.unscaledDeltaTime;
                    if (dt > 0f)
                        smoothVel = Vector3.Lerp(smoothVel, (tip - lastTip) / dt, 0.35f);
                    lastTip = tip;
                    held.position = tip;

                    if (!pinch)
                    {
                        var rb = bodies[held];
                        held.GetComponent<CollisionBlip>()?.Blip(1.1f);
                        rb.isKinematic = false;
                        rb.linearVelocity = smoothVel * throwVelocityScale;
                        rb.angularVelocity = Vector3.zero;
                        held = null;
                        return;
                    }
                }
            }
        }
    }
}
