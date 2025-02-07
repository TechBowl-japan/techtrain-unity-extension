#nullable enable
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Threading.Tasks;


namespace TechtrainExtension
{
    public class ExtensionWindow : EditorWindow
    {
        [SerializeField] private StyleSheet styleSheet;
        internal Config.ConfigManager? configManager;
        internal Api.Client? apiClient;
        internal RailwayManager? railwayManager;
        private VisualElement? root;

        public void CreateGUI()
        {
            configManager = new Config.ConfigManager();
            apiClient = new Api.Client(configManager.Config);

            root = new VisualElement();
            root.styleSheets.Add(styleSheet);
            root.AddToClassList("root");
            rootVisualElement.Clear();
            rootVisualElement.Add(root);
            _ = InitializePage();
        }

        private async Task InitializePage()
        {
            if (apiClient == null || root == null)
            {
                return;
            }
            railwayManager = new RailwayManager(apiClient, false);
            await railwayManager.Initialize();
            if (railwayManager.IsClearAllStations())
            {
                root.Add(new Label("このRailwayのすべてのStationをクリアしました！お疲れ様でした。"));
                return;
            }
            if (!railwayManager.IsAlreadyChallenging())
            {
                root.Add(new Label("このRailwayに挑戦する場合はブラウザ上から挑戦ボタンを押してください"));
                root.Add(new Button(() => { this.Reload(); }) { text = "再読み込み" });
                return;
            }
            var currentStation = railwayManager.GetCurrentStation();
            if (currentStation == null)
            {
                root.Add(new Label("Station情報の取得に失敗しました。時間をおいて再度試すか、運営までお問い合わせください"));
                root.Add(new Button(() => { this.Reload(); }) { text = "再読み込み" });
                return;
            }
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
            var tests = new Pages.Tests(this, railwayManager);
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
            apiClient = new Api.Client(configManager.Config);

            _ = InitializePage();
        }

        [MenuItem("Tools/Techtrain")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<ExtensionWindow>();
            wnd.titleContent = new GUIContent("Techtrain");
        }

    }

}
