#nullable enable

using System.Threading.Tasks;
using TechtrainExtension.Manifests.Models;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.TestTools.TestRunner;
using UnityEditor;

namespace TechtrainExtension.Pages
{
    internal class Tests
    {
        private ExtensionWindow window;
        private TestRunner testRunner;

        private Station? manifestStation;
        private int order = 0;

        internal VisualElement root;

        private Button runButton;
        private VisualElement resultArea;
        private Button showTestRunnerButton;

        internal Tests(ExtensionWindow self, Station _manifestStation, TestRunner _testRunner, int currentOrder)
        {
            window = self;
            manifestStation = _manifestStation;

            testRunner = _testRunner;

            order = currentOrder;

            root = Create();
            runButton = CreateRunButton();
            resultArea = CreateResultArea();
            showTestRunnerButton = CreateShowTestRunnerButton();

            root.Add(runButton);
            root.Add(resultArea);
            root.Add(showTestRunnerButton);
        }
        private VisualElement Create()
        {
            var root = new VisualElement();
            root.AddToClassList("test-runner");
            return root;
        }

        private Button CreateRunButton()
        {
            var runButton = new Button(() => RunTests())
            {
                text = "できた！"
            };
            return runButton;
        }

        private VisualElement CreateResultArea()
        {
            var resultArea = new VisualElement();
            resultArea.AddToClassList("result-area");

            if (testRunner.results == null || testRunner.results.Count == 0 || testRunner.order != order)
            {
                resultArea.Add(new Label("テストを実行してください"));
                return resultArea;
            }

            foreach (var result in testRunner.results)
            {
                var resultElement = new HelpBox()
                {
                    messageType = HelpBoxMessageType.None
                };
                resultElement.AddToClassList("result");
                var icon = new Image()
                {
                    image = result.isPassed ? EditorGUIUtility.IconContent("TestPassed").image : EditorGUIUtility.IconContent("TestFailed").image,
                    style = { width = 32, height = 32 }
                };
                resultElement.Add(icon);
                var container = new VisualElement();
                var summary = result.isPassed ? "Passed" : "Failed";
                var testLabel = new Label($"{result.path} {summary}");
                testLabel.AddToClassList("test-label");
                container.Add(testLabel);
                if (!result.isPassed)
                {
                    var errorMessageLabel = new Label(result.errorMessage);
                    errorMessageLabel.AddToClassList("error-message");
                    container.Add(errorMessageLabel);
                }
                resultElement.Add(container);
                resultArea.Add(resultElement);
            }
            return resultArea;
        }

        private Button CreateShowTestRunnerButton()
        {
            return new Button(() => ShowTestRunner())
            {
                text = "テストランナーを開く"
            };
        }

        private void ShowTestRunner()
        {
            var wnd = TestRunnerWindow.GetWindow<TestRunnerWindow>();
            wnd.titleContent = new GUIContent("TestRunner");
        }

        private void RunTests()
        {
            if (manifestStation == null || manifestStation.tests == null || manifestStation.tests.Count != 1)
            {
                return;
            }
            testRunner.RunTest(manifestStation.tests[0], order);
        }
    }
}