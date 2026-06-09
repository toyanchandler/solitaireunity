using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace _Game.Scripts.Project.SolitaireModule.Editor
{
    public static class SolitaireWebGLBuildUtility
    {
        private const string OutputPath = "Builds/WebGL/Solitaire";

        public static void BuildAndExit()
        {
            try
            {
                BuildReport report = Build();
                BuildSummary summary = report.summary;
                Debug.Log($"[SolitaireWebGLBuildUtility] result={summary.result}, totalSize={summary.totalSize}, totalTime={summary.totalTime}, output={OutputPath}");
                EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static BuildReport Build()
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
                throw new InvalidOperationException("No enabled scenes are configured in EditorBuildSettings.");

            Directory.CreateDirectory(OutputPath);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded)
                ApplyResponsiveWebGLShell(OutputPath);

            return report;
        }

        private static void ApplyResponsiveWebGLShell(string outputPath)
        {
            string indexPath = Path.Combine(outputPath, "index.html");
            string stylePath = Path.Combine(outputPath, "TemplateData", "style.css");

            if (File.Exists(indexPath))
            {
                string html = File.ReadAllText(indexPath);
                html = html.Replace(
                    "<canvas id=\"unity-canvas\" width=960 height=600 tabindex=\"-1\"></canvas>",
                    "<canvas id=\"unity-canvas\" tabindex=\"-1\"></canvas>");
                html = html.Replace(
                    "        // Desktop style: Render the game canvas in a window that can be maximized to fullscreen:\n        canvas.style.width = \"960px\";\n        canvas.style.height = \"600px\";",
                    "        // Desktop style: fill the iframe supplied by the validation host.\n        canvas.style.width = \"100%\";\n        canvas.style.height = \"100%\";");
                html = html.Replace(
                    "      canvas.style.background = \"url('\" + buildUrl + \"/{{{ BACKGROUND_FILENAME }}}') center / cover\";",
                    "      canvas.style.background = \"url('\" + buildUrl + \"/{{{ BACKGROUND_FILENAME }}}') center / cover\";");
                html = html.Replace(
                    "      document.querySelector(\"#unity-loading-bar\").style.display = \"block\";",
                    "      function forceUnityResize() {\n        canvas.style.width = \"100%\";\n        canvas.style.height = \"100%\";\n        window.dispatchEvent(new Event(\"resize\"));\n      }\n\n      window.__forceUnityResize = forceUnityResize;\n\n      if (window.ResizeObserver) {\n        new ResizeObserver(forceUnityResize).observe(document.documentElement);\n      }\n\n      document.querySelector(\"#unity-loading-bar\").style.display = \"block\";");
                html = html.Replace(
                    "                document.querySelector(\"#unity-loading-bar\").style.display = \"none\";",
                    "                document.querySelector(\"#unity-loading-bar\").style.display = \"none\";\n                forceUnityResize();");
                File.WriteAllText(indexPath, html);
            }

            if (File.Exists(stylePath))
            {
                string css = File.ReadAllText(stylePath);
                css = css.Replace("body { padding: 0; margin: 0 }", "html, body { width: 100%; height: 100%; padding: 0; margin: 0; overflow: hidden; background: #05070a }");
                css = css.Replace("#unity-container { position: absolute }", "#unity-container { position: fixed; inset: 0; width: 100%; height: 100% }");
                css = css.Replace("#unity-container.unity-desktop { left: 50%; top: 50%; transform: translate(-50%, -50%) }", "#unity-container.unity-desktop { left: 0; top: 0; transform: none }");
                css = css.Replace("#unity-canvas { background: #FFFFFF }", "#unity-canvas { display: block; width: 100%; height: 100%; background: #FFFFFF }");
                css = css.Replace("#unity-footer { position: relative }", "#unity-footer { display: none }");
                File.WriteAllText(stylePath, css);
            }
        }
    }
}
