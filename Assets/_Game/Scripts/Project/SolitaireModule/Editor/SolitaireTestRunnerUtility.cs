using System;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using ApiTestMode = UnityEditor.TestTools.TestRunner.Api.TestMode;

namespace _Game.Scripts.Project.SolitaireModule.Editor
{
    public static class SolitaireTestRunnerUtility
    {
        private const double TimeoutSeconds = 300;

        public static void RunEditModeAndExit()
        {
            RunAndExit(ApiTestMode.EditMode, "/tmp/baseproject-test-results/api-editmode.xml");
        }

        public static void RunPlayModeAndExit()
        {
            RunAndExit(ApiTestMode.PlayMode, "/tmp/baseproject-test-results/api-playmode.xml");
        }

        private static void RunAndExit(ApiTestMode mode, string resultPath)
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            var callback = new ExitOnFinishedCallback(resultPath);
            api.RegisterCallbacks(callback);

            var settings = new ExecutionSettings(new Filter { testMode = mode });

            if (mode == ApiTestMode.EditMode)
                settings.runSynchronously = true;

            double startTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += CheckTimeout;
            api.Execute(settings);

            if (mode == ApiTestMode.EditMode && callback.HasFinished)
                return;

            void CheckTimeout()
            {
                if (EditorApplication.timeSinceStartup - startTime < TimeoutSeconds || callback.HasFinished)
                    return;

                Debug.LogError($"[SolitaireTestRunnerUtility] {mode} test run timed out after {TimeoutSeconds} seconds.");
                EditorApplication.update -= CheckTimeout;
                EditorApplication.Exit(1);
            }
        }

        private sealed class ExitOnFinishedCallback : ICallbacks
        {
            private readonly string _resultPath;

            public bool HasFinished { get; private set; }

            public ExitOnFinishedCallback(string resultPath)
            {
                _resultPath = resultPath;
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
                Debug.Log($"[SolitaireTestRunnerUtility] Started {testsToRun.FullName}");
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                HasFinished = true;
                TestRunnerApi.SaveResultToFile(result, _resultPath);
                Debug.Log($"[SolitaireTestRunnerUtility] Finished {result.FullName}: state={result.ResultState}, pass={result.PassCount}, fail={result.FailCount}, skip={result.SkipCount}, result={_resultPath}");
                EditorApplication.Exit(result.FailCount == 0 ? 0 : 1);
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
            }
        }
    }
}
