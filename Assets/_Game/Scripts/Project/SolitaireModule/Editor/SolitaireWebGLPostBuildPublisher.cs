using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _Game.Scripts.Project.SolitaireModule.Editor
{
    public static class SolitaireWebGLPostBuildPublisher
    {
        private const string HostDirectory = "WebGLHost";
        private const string PublishCommand = "postbuild:publish";
        private const string DisableEnvironmentVariable = "SOLITAIRE_WEBGL_AUTO_PUBLISH";

        [PostProcessBuild(1000)]
        public static void PublishAfterWebGLBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.WebGL)
                return;

            if (IsDisabled())
            {
                Debug.Log("[SolitaireWebGLPostBuildPublisher] Auto publish skipped because SOLITAIRE_WEBGL_AUTO_PUBLISH=0.");
                return;
            }

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
            {
                Debug.LogWarning("[SolitaireWebGLPostBuildPublisher] Could not resolve project root.");
                return;
            }

            string hostPath = Path.Combine(projectRoot, HostDirectory);
            if (!Directory.Exists(hostPath))
            {
                Debug.LogWarning($"[SolitaireWebGLPostBuildPublisher] Host folder not found: {hostPath}");
                return;
            }

            string sourcePath = Path.GetFullPath(pathToBuiltProject);
            string relativeSource = Path.GetRelativePath(projectRoot, sourcePath);

            string command = $"npm run {PublishCommand} -- --source {ShellQuote(relativeSource)}";
            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/zsh",
                Arguments = $"-lc {ShellQuote(command)}",
                WorkingDirectory = hostPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            startInfo.Environment["BUILD_VERSION"] = $"webgl-{DateTime.UtcNow:yyyyMMddHHmmss}";

            RunPublish(startInfo);
        }

        private static bool IsDisabled()
        {
            string value = Environment.GetEnvironmentVariable(DisableEnvironmentVariable);
            return value == "0" || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
        }

        private static string ShellQuote(string value)
        {
            return $"'{value.Replace("'", "'\\''")}'";
        }

        private static void RunPublish(ProcessStartInfo startInfo)
        {
            using Process process = Process.Start(startInfo);
            if (process == null)
            {
                Debug.LogError("[SolitaireWebGLPostBuildPublisher] Failed to start npm publish process.");
                return;
            }

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(output))
                Debug.Log($"[SolitaireWebGLPostBuildPublisher]\n{output}");

            if (process.ExitCode != 0)
            {
                Debug.LogError($"[SolitaireWebGLPostBuildPublisher] Publish failed with exit code {process.ExitCode}.\n{error}");
                return;
            }

            if (!string.IsNullOrWhiteSpace(error))
                Debug.LogWarning($"[SolitaireWebGLPostBuildPublisher]\n{error}");
        }
    }
}
