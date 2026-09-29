using System.IO;
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
            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded)
            {
                MakeWebGLCanvasResponsive("Builds/WebGL");
            }

            Report(report);
        }

        /// <summary>
        /// Unity's default WebGL template hardcodes the canvas to a fixed 960x600 landscape box on
        /// desktop browsers (only mobile UAs get a 100%-filled canvas) - wrong for a portrait game
        /// embedded in a portrait itch.io frame, and the cause of squashed/overlapping UI there.
        /// Patches the built index.html/style.css so the canvas always fills its container.
        /// </summary>
        private static void MakeWebGLCanvasResponsive(string buildDir)
        {
            string indexPath = Path.Combine(buildDir, "index.html");
            string html = File.ReadAllText(indexPath);
            html = html.Replace("canvas.style.width = \"960px\";", "canvas.style.width = \"100%\";");
            html = html.Replace("canvas.style.height = \"600px\";", "canvas.style.height = \"100%\";");
            File.WriteAllText(indexPath, html);

            string cssPath = Path.Combine(buildDir, "TemplateData/style.css");
            string css = File.ReadAllText(cssPath);
            css = css.Replace(
                "#unity-container.unity-desktop { left: 50%; top: 50%; transform: translate(-50%, -50%) }",
                "#unity-container.unity-desktop { position: fixed; width: 100%; height: 100% }");
            css = css.Replace(
                "#unity-canvas { background: #231F20 }",
                "#unity-canvas { background: #231F20; width: 100%; height: 100% }");
            File.WriteAllText(cssPath, css);

            Debug.Log("MakeWebGLCanvasResponsive: patched index.html and style.css to fill the container on desktop.");
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
