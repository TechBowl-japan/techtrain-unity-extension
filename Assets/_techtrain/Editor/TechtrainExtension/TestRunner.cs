using System;
using System.Threading.Tasks;
using TechtrainExtension.Manifests.Models;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace TechtrainExtension
{

    internal class TestRunner
    {
        private TestRunnerApi testRunner;

        internal List<TestResult> results;
        internal int order = 0;

        private const string TestResultKey = "dev.techtrain.TechtrainExtension.TestResult";
        private const string TestOrderKey = "dev.techtrain.TechtrainExtension.TestOrder";

        internal TestRunner()
        {
            testRunner = ScriptableObject.CreateInstance<TestRunnerApi>();
            RestoreTestResuts();



            var testCallback = new TestCallback();
            testCallback.OnRunFinishedEvent += (result) =>
            {
                if (result == null)
                {
                    return;
                }
                var parsed = ParseTestResult(result);
                results = parsed;
                PlayerPrefs.SetString(TestResultKey, JsonConvert.SerializeObject(parsed.ToArray()));
                Debug.Log(PlayerPrefs.GetString(TestResultKey));

            };
            testRunner.RegisterCallbacks(testCallback);
        }

        private void RestoreTestResuts()
        {
            var testResults = PlayerPrefs.GetString(TestResultKey);
            if (!string.IsNullOrEmpty(testResults))
            {
                try
                {
                    results = JsonConvert.DeserializeObject<List<TestResult>>(testResults);
                }
                catch (Exception e)
                {
                    PlayerPrefs.DeleteKey(TestResultKey);
                    PlayerPrefs.DeleteKey(TestOrderKey);
                    results = new List<TestResult>();
                }
            }
            else
            {
                results = new List<TestResult>();
            }
        }

        static List<TestResult> ParseTestResult(ITestResultAdaptor result, string path = "", List<TestResult> parsed = null)
        {
            if (parsed == null)
            {
                parsed = new List<TestResult>();
            }
            if (result.Children == null || result.Children.Count()< 1)
            {
                parsed.Add(new TestResult()
                {
                    path = result.FullName,
                    isPassed = result.TestStatus == TestStatus.Passed,
                    errorMessage = result.Message
                });
                return parsed;
            }
            foreach (var child in result.Children)
            {
                ParseTestResult(child, $"{path}/{result.Name}", parsed);
            }
            return parsed;
        }

        public void RunTest(StationTest test, int _order)
        {
            order = _order;
            PlayerPrefs.SetInt(TestOrderKey, order);
            var tcs = new TaskCompletionSource<TestResult>();
            var filter = CreateFilter(test);
            var executionSettings = new ExecutionSettings(filter);

            testRunner.Execute(executionSettings);

        }
        private Filter CreateFilter(StationTest test)
        {
            if (test.command.StartsWith("category:"))
            {
                return new Filter()
                {
                    testMode = TestMode.PlayMode,
                    categoryNames = new[] { test.command.Substring(9) }
                };
            }
            if (test.command.StartsWith("group:"))
            {
                return new Filter()
                {
                    testMode = TestMode.PlayMode,
                    groupNames = new[] { test.command.Substring(6) }
                };
            }
            if (test.command.StartsWith("test:"))
            {
                return new Filter()
                {
                    testMode = TestMode.PlayMode,
                    testNames = new[] { test.command.Substring(5) }
                };
            }
            return new Filter()
            {
                testMode = TestMode.PlayMode,
                testNames = new[] { test.command }
            };
        }
    }

    class TestCallback : ICallbacks
    {
        public event Action<ITestAdaptor>? OnRunStartedEvent;
        public event Action<ITestResultAdaptor>? OnRunFinishedEvent;
        public event Action<ITestAdaptor>? OnTestStartedEvent;
        public event Action<ITestResultAdaptor>? OnTestFinishedEvent;

        public void RunStarted(ITestAdaptor testsToRun)
        {
            OnRunStartedEvent?.Invoke(testsToRun);
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            OnRunFinishedEvent?.Invoke(result);
        }

        public void TestStarted(ITestAdaptor test)
        {
            OnTestStartedEvent?.Invoke(test);
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            OnTestFinishedEvent?.Invoke(result);
        }
    }


    public class TestResult
    {
        public string path;
        public bool isPassed;
        public string? errorMessage;
    }
}