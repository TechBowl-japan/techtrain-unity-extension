#nullable enable

using System.Threading.Tasks;
using TechtrainExtension.Manifests.Models;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TechtrainExtension.Pages
{
    public class Tests
    {
        private ExtensionWindow window;
        private RailwayManager railwayManager;
        private TestRunner testRunner;

        private Station? manifestStation;

        internal VisualElement root;

        private Button runButton;
        private VisualElement resultArea;

        public Tests(ExtensionWindow self, RailwayManager _railwayManager)
        {
            window = self;
            railwayManager = _railwayManager;
            manifestStation = railwayManager.GetCurrentStationManifest();
            testRunner = new TestRunner();


            root = Create();
            runButton = CreateRunButton();
            resultArea = CreateResultArea();

            root.Add(runButton);
            root.Add(resultArea);
            root.Add(new Label("Tests"));
        }
        private VisualElement Create()
        {
            var root = new VisualElement();
            root.AddToClassList("test-runner");
            return root;
        }

        private Button CreateRunButton()
        {
            var runButton = new Button(() => _ = RunTests())
            {
                text = "Run Tests"
            };
            return runButton;
        }

        private VisualElement CreateResultArea()
        {
            var resultArea = new VisualElement();
            resultArea.AddToClassList("result-area");
            return resultArea;
        }

        private async Task<bool> RunTests()
        {
            if (manifestStation == null)
            {
                return false;
            }
            var results = new List<TestResult>();
            foreach (var test in manifestStation.tests)
            {
                results.Add(await testRunner.RunTest(test));
            }
            return results.All(result => result.isPassed);
        }

    }
}