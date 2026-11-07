// Hintergewerk — blendet Hand-Meshes aus, solange kein Tracking anliegt.
// Fix für die "dritte Hand" (eingefrorenes Bind-Pose-Mesh am Stage-Ursprung,
// sobald die Hand verloren geht und der Hand-Anchor auf Null fällt).
using UnityEngine;

namespace Hintergewerk.Adapters
{
    public class HandVisibilityDriver : MonoBehaviour
    {
        void Update()
        {
            foreach (var h in FindObjectsByType<OVRHand>(FindObjectsInactive.Exclude))
            {
                bool visible = h.IsDataValid && h.IsTracked;
                foreach (var r in h.GetComponentsInChildren<SkinnedMeshRenderer>())
                    r.enabled = visible;
            }
        }
    }
}
