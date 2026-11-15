// Hintergewerk — Editor-Tool: Bone-Bindings der Hand-Skeletons nachziehen.
// Root-Cause 07.11. (HANDOFF-CLAUDE.md §306): OVRCustomHandPrefab liefert UNGBUNDEN aus
// (_customBones_V2 = nulls); der „Auto Map Bones"-Fix wurde damals nur in 01_Smoketest
// gespeichert. Szene 02 (Start-Szene am Gerät!) trägt nur Positions-Overrides → falsch
// ausgerichtete Hände (15.11. am Gerät gemeldet). Dieses Tool wiederholt denselben Fix
// gegen-beliebig für weitere Szenen. Menü: Hintergewerk → Hand-Bones fixen.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Hintergewerk.EditorTools
{
    public static class HandBonesFix
    {
        const string Szene02 = "Assets/Hintergewerk/Scenes/02_AnkerTest.unity";

        [MenuItem("Hintergewerk/Hand-Bones fixen (Szene 02)")]
        public static void FixSzene02()
        {
            FixSzene(Szene02);
        }

        static void FixSzene(string path)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int count = 0;
            foreach (var skeleton in Object.FindObjectsByType<OVRCustomSkeleton>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                // v207: retargetingType ist internal → öffentliche Variante ohne den Parameter
                skeleton.TryAutoMapBonesByName();
                EditorUtility.SetDirty(skeleton);
                count++;
                int mapped = 0;
                foreach (var b in skeleton.CustomBones) if (b != null) mapped++;
                Debug.Log($"[HandBonesFix] {skeleton.gameObject.name}: {mapped}/{skeleton.CustomBones.Count} Bones gebunden");
            }
            if (count == 0)
            {
                Debug.LogWarning($"[HandBonesFix] Kein OVRCustomSkeleton in {path} — nichts zu tun.");
                return;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[HandBonesFix] {path} gespeichert ({count} Skeletons gemappt).");
        }
    }
}
