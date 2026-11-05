// Hintergewerk — Projekt-Setup in einem Rutsch (docs/06 §3): Android-Buildtarget,
// IL2CPP/ARM64, Bundle-ID, minSdk, OpenXR-Loader für Android + Meta Quest Feature.
// Ausführen per unity command script eval: Hintergewerk.EditorTools.ProjectSetup.Apply()
// (oder Editor-Menü Hintergewerk → Projekt-Setup Android+OpenXR). Idempotent.
//
// API-Hinweise: XR Management 4.x — Settings leben als EditorBuildSettings-Config-Objekt
// (Asset: XRGeneralSettingsPerBuildTarget). Öffentliche Erzeugung nur über die
// Instanz-Methoden CreateDefaultSettingsForBuildTarget / CreateDefaultManagerSettingsForBuildTarget.
// Das OpenXR-Feature heißt UnityEngine.XR.OpenXR.Features.MetaQuestSupport.MetaQuestFeature.
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.XR.Management;

namespace Hintergewerk.EditorTools
{
    public static class ProjectSetup
    {
        const string BundleId = "de.bautoolz.hintergewerk";

        [MenuItem("Hintergewerk/Projekt-Setup Android+OpenXR")]
        public static void Apply()
        {
            // ---- 1) Build-Target Android ----
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                {
                    Debug.LogError("[ProjectSetup] Wechsel auf Android fehlgeschlagen.");
                    return;
                }
                Debug.Log("[ProjectSetup] Build-Target: Android");
            }

            // ---- 2) Player-Einstellungen ----
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BundleId);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32; // Quest-Vorgabe
            Debug.Log("[ProjectSetup] IL2CPP · ARM64 · " + BundleId + " · minSdk 32");

            // ---- 3) XR Plug-in Management: Settings-Asset sicherstellen ----
            var perTarget = FindOrCreatePerTargetAsset();
            if (perTarget == null) { Debug.LogError("[ProjectSetup] XRGeneralSettingsPerBuildTarget nicht erstellbar."); return; }

            if (!perTarget.HasSettingsForBuildTarget(BuildTargetGroup.Android))
                perTarget.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Android);
            var general = perTarget.SettingsForBuildTarget(BuildTargetGroup.Android);
            if (general == null) { Debug.LogError("[ProjectSetup] XRGeneralSettings (Android) null."); return; }

            var manager = general.Manager;
            if (manager == null)
            {
                perTarget.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
                manager = perTarget.ManagerSettingsForBuildTarget(BuildTargetGroup.Android);
                Debug.Log("[ProjectSetup] XRManagerSettings (Android) neu angelegt");
            }
            if (manager == null) { Debug.LogError("[ProjectSetup] XRManagerSettings (Android) null."); return; }

            var loader = AssetDatabase.LoadAssetAtPath<XRLoader>("Assets/XR/Loaders/OpenXRLoader.asset");
            if (loader == null)
            {
                Debug.LogError("[ProjectSetup] Assets/XR/Loaders/OpenXRLoader.asset nicht gefunden.");
                return;
            }
            if (!manager.activeLoaders.Contains(loader))
            {
                if (!manager.TryAddLoader(loader)) { Debug.LogError("[ProjectSetup] TryAddLoader fehlgeschlagen."); return; }
                Debug.Log("[ProjectSetup] OpenXRLoader für Android hinzugefügt");
            }

            // ---- 4) OpenXR: Meta Quest Feature (Android-Kontext nach Target-Switch) ----
            string questState = "n/a";
            var oxr = UnityEngine.XR.OpenXR.OpenXRSettings.ActiveBuildTargetInstance;
            if (oxr != null)
            {
                var quest = oxr.GetFeature<UnityEngine.XR.OpenXR.Features.MetaQuestSupport.MetaQuestFeature>();
                if (quest != null)
                {
                    if (!quest.enabled) { quest.enabled = true; EditorUtility.SetDirty(quest); }
                    questState = quest.enabled.ToString();
                }
                EditorUtility.SetDirty(oxr);
            }
            else
            {
                Debug.LogWarning("[ProjectSetup] OpenXRSettings.ActiveBuildTargetInstance ist null — Feature-Setup übersprungen.");
            }

            EditorUtility.SetDirty(general);
            EditorUtility.SetDirty(manager);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ProjectSetup] FERTIG · OpenXR aktiv={manager.activeLoaders.Contains(loader)} · QuestFeature={questState}");
        }

        /// <summary>Per-Target-Settings-Asset finden (EditorBuildSettings-Config) oder erzeugen.</summary>
        static XRGeneralSettingsPerBuildTarget FindOrCreatePerTargetAsset()
        {
            var assets = AssetDatabase.FindAssets("t:XRGeneralSettingsPerBuildTarget");
            if (assets.Length > 0)
                return AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(AssetDatabase.GUIDToAssetPath(assets[0]));

            // Kein Asset da (frischer Klon): exakt wie XRManagements interne Erzeugung vorgehen.
            var created = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            const string folder = "Assets/XR";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "XR");
            AssetDatabase.CreateAsset(created, folder + "/XRGeneralSettingsPerBuildTarget.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("[ProjectSetup] XRGeneralSettingsPerBuildTarget-Asset neu erzeugt");
            return created;
        }
    }
}
#endif
