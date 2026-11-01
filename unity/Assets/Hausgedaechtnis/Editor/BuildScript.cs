// Hausgedächtnis — Batch-Build für SSH/Remote (Windows-Buildserver oder Mac-Terminal).
// Aufruf: Unity.exe -batchmode -quit -projectPath <pfad-zu-unity-ordner>
//         -executeMethod Hausgedaechtnis.EditorTools.BuildScript.BuildAndroid -logFile build.log
// Kein MenuItem: dieses Skript ist ausschließlich für den Batch-Modus gedacht
// (EditorApplication.Exit würde sonst einen offenen Editor schließen!).
#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hausgedächtnis.EditorTools
{
    public static class BuildScript
    {
        const string ApkName = "hausgedaechtnis.apk";

        public static void BuildAndroid()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var outDir = Path.Combine(projectRoot, "Builds");
            Directory.CreateDirectory(outDir);
            var apk = Path.Combine(outDir, ApkName);

            // Szenen: erst Build Settings, sonst alle unter Assets/Hausgedaechtnis (deterministisch sortiert)
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                scenes = AssetDatabase.FindAssets("t:SceneAsset", new[] { "Assets/Hausgedaechtnis" })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .OrderBy(p => p)
                    .ToArray();
                if (scenes.Length == 0)
                {
                    Debug.LogError("[Build] Keine Szenen gefunden — erst Szenen anlegen/in Build Settings aufnehmen.");
                    EditorApplication.Exit(2);
                    return;
                }
                Debug.LogWarning("[Build] Build Settings leer — baue alle Szenen unter Assets/Hausgedaechtnis: "
                                 + string.Join(", ", scenes));
            }

            // Build-Ziel sicherstellen (falls der Clone frisch ist)
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                var switched = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                if (!switched)
                {
                    Debug.LogError("[Build] Wechsel auf Android fehlgeschlagen (Module installiert?).");
                    EditorApplication.Exit(3);
                    return;
                }
            }

            // Keystore: bewusst DEBUG-Signatur (Standard) — reicht fuer den Competition-Release-Channel.
            // Falls jemals ein Custom-Keystore gesetzt wird: Datei liegt NICHT im Repo (.gitignore).
            PlayerSettings.Android.useCustomKeystore = false;

            var report = BuildPipeline.BuildPlayer(scenes, apk, BuildTarget.Android, BuildOptions.None);
            var summary = report.summary;
            Debug.Log($"[Build] Ergebnis: {summary.result} · Groesse: {summary.totalSize} Bytes · APK: {apk}");

            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
#endif
