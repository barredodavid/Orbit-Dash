using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OrbitDash.EditorTools
{
    /// <summary>
    /// Command-line build entry points, for CI or a quick local build without opening the Editor UI:
    /// Unity.exe -batchmode -quit -projectPath . -executeMethod OrbitDash.EditorTools.BuildScript.BuildWebGL
    /// </summary>
    public static class BuildScript
    {
        private static readonly string[] Scenes = { "Assets/_Project/Scenes/Game.unity" };

        public static void BuildWebGL()
        {
            var options = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = "Builds/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            };
            Report(BuildPipeline.BuildPlayer(options));
        }

        public static void BuildAndroid()
        {
            var options = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = "Builds/Android/OrbitDash.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None,
            };
            Report(BuildPipeline.BuildPlayer(options));
        }

        private static void Report(BuildReport report)
        {
            BuildSummary summary = report.summary;
            Debug.Log($"Build result: {summary.result}, size: {summary.totalSize} bytes, " +
                      $"errors: {summary.totalErrors}, warnings: {summary.totalWarnings}");

            if (summary.result != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
        }
    }
}
