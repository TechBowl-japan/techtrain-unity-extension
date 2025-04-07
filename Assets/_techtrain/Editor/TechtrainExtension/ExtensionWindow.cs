#nullable enable
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Threading.Tasks;
using System.Linq;
using TechtrainExtension.Api.Models.v3;


namespace TechtrainExtension
{
    public class ExtensionWindow : EditorWindow
    {
        [SerializeField] private StyleSheet? styleSheet;
        internal Config.ConfigManager? configManager;
        internal Api.Client? apiClient;
        internal RailwayManager? railwayManager;
        internal TestRunner? testRunner;

        private VisualElement? root;

        public void CreateGUI()
        {
            configManager = new Config.ConfigManager();
            apiClient = new Api.Client(configManager);
            testRunner = new TestRunner();

            root = new VisualElement();
            root.styleSheets.Add(styleSheet);
            root.AddToClassList("root");
            rootVisualElement.Clear();
            rootVisualElement.Add(root);
            _ = InitializePage();
        }

        private async Task InitializePage()
        {
            if (apiClient == null || root == null || configManager == null || testRunner == null)
            {
                return;
            }
            var isMaintenance = await IsMaintenance();
            if (isMaintenance)
            {
                var maintenance = new Pages.Maintenance(this);
                root.Add(maintenance.root);
                return;
            }
            Response<UsersMeResponse>? user = null;
            try
            {

                user = await apiClient.PostUsersMe();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }

            if (user == null || user.data == null)
            {

                var login = new Pages.Login(this);
                root.Add(login.root);
                return;
            }

            var label = new Label("Railway情報を読み込んでいます...");
            root.Add(label);

            try
            {
                railwayManager = new RailwayManager(apiClient, user.data.is_paid);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                root.Clear();
                root.Add(new Label("Railway情報の取得に失敗しました。時間をおいて再度試すか、運営までお問い合わせください"));
                root.Add(new Button(() => { this.Reload(); }) { text = "再読み込み" });
                return;
            }
            await railwayManager.Initialize();
            if (railwayManager.IsClearAllStations())
            {
                root.Clear();
                root.Add(new Label("このRailwayのすべてのStationをクリアしました！お疲れ様でした。"));
                return;
            }
            if (!railwayManager.IsAlreadyChallenging())
            {
                root.Clear();
                root.Add(new Label("このRailwayに挑戦する場合はブラウザ上から挑戦ボタンを押してください"));
                root.Add(new Button(() => { this.Reload(); }) { text = "再読み込み" });
                return;
            }
            label.text = "Station情報を読み込んでいます...";
            var currentStation = railwayManager.GetCurrentStation();
            if (currentStation == null)
            {
                root.Clear();
                root.Add(new Label("Station情報の取得に失敗しました。時間をおいて再度試すか、運営までお問い合わせください"));
                root.Add(new Button(() => { this.Reload(); }) { text = "再読み込み" });
                return;
            }
            root.Clear();
            root.Add(new Label($"挑戦中のStation: {currentStation.title}"));
            if (currentStation.confirmation_method != Api.Models.v3.RailwayStationConfirmationMethod.unit_test)
            {
                root.Add(new Label("このStationは自動テストではないためUnity上でクリア判定が行えません。ブラウザ上から判定を行ってください"));
                return;
            }
            if (!railwayManager.IsStationPermitted(currentStation))
            {
                root.Add(new Label("続きに挑戦するには、有料プランへの登録が必要です。"));
                return;
            }
            var manifestStation = railwayManager.GetCurrentStationManifest();
            if (manifestStation == null)
            {
                root.Add(new Label("Station情報の取得に失敗しました。時間をおいて再度試すか、運営までお問い合わせください"));
                root.Add(new Button(() => { this.Reload(); }) { text = "再読み込み" });
                return;
            }
            if (manifestStation.tests.Count() > 1 || !manifestStation.tests.All((test) => test.type == "unity"))
            {
                root.Add(new Label("このStationは自動テストではないためUnity上でクリア判定が行えません。ブラウザ上から判定を行ってください"));
                return;
            }
            if (testRunner.isRunning)
            {
                await testRunner.WaitForTestResult();
            }
            await railwayManager.ReportTestResult(currentStation.order, testRunner);
            if (testRunner.IsTestSucessful(currentStation.order))
            {
                testRunner.ClearTestResults();
                this.Reload();
                return;
            }
            var tests = new Pages.Tests(this, manifestStation, testRunner, currentStation.order);
            root.Add(tests.root);
        }

        internal void Reload()
        {
            if (root == null || configManager == null)
            {
                return;
            }
            root.Clear();
            configManager.Reload();
            apiClient = new Api.Client(configManager);

            _ = InitializePage();
        }

        [MenuItem("Tools/Techtrain")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<ExtensionWindow>();
            wnd.titleContent = new GUIContent("Techtrain");
        }

        private async Task<bool> IsMaintenance()
        {
            var response = await apiClient.GetIsMaintenance();
            return (response?.data.is_maintenance ?? true);
        }
    }
}
