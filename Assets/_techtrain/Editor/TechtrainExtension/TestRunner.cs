using System;
using System.Threading.Tasks;
using TechtrainExtension.Manifests.Models;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace TechtrainExtension
{

    internal class TestRunner
    {
        private TestRunnerApi testRunner;
        internal TestRunner()
        {
            testRunner = ScriptableObject.CreateInstance<TestRunnerApi>();
        }

        public Task<TestResult> RunTest(StationTest test)
        {
            var tcs = new TaskCompletionSource<TestResult>();
            var filter = CreateFilter(test);
            var executionSettings = new ExecutionSettings(filter);


            var testCallback = new TestCallback();
            testCallback.OnTestFinishedEvent += (result) =>
            {
                Debug.Log("RunFinished");
                Debug.Log(result.TestStatus);
                Debug.Log(result.Message);
                tcs.SetResult(new TestResult()
                {
                    test = test,
                    isPassed = result.TestStatus == TestStatus.Passed,
                    errorMessage = result.Message
                });
            };
            testRunner.RegisterCallbacks(testCallback);
            Debug.Log("Execute");

            testRunner.Execute(executionSettings);
            return tcs.Task;

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
            Debug.Log("RunStarted");
            OnRunStartedEvent?.Invoke(testsToRun);
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            Debug.Log("RunFinished");
            OnRunFinishedEvent?.Invoke(result);
        }

        public void TestStarted(ITestAdaptor test)
        {
            Debug.Log("TestStarted");
            OnTestStartedEvent?.Invoke(test);
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            Debug.Log("TestFinished");
            OnTestFinishedEvent?.Invoke(result);
        }
    }


    public class TestResult
    {
        public StationTest test;
        public bool isPassed;
        public string? errorMessage;
    }
}