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
                root.Add(new Label("����Railway�̂��ׂĂ�Station���N���A���܂����I�����l�ł����B"));
                return;
            }
            if (!railwayManager.IsAlreadyChallenging())
            {
                root.Add(new Label("����Railway�ɒ��킷��ꍇ�̓u���E�U�ォ�璧��{�^���������Ă�������"));
                root.Add(new Button(() => { this.Reload(); }) { text = "�ēǂݍ���" });
                return;
            }
            var currentStation = railwayManager.GetCurrentStation();
            if (currentStation == null)
            {
                root.Add(new Label("Station���̎擾�Ɏ��s���܂����B���Ԃ������čēx�������A�^�c�܂ł��₢���킹��������"));
                root.Add(new Button(() => { this.Reload(); }) { text = "�ēǂݍ���" });
                return;
            }
            root.Add(new Label($"���풆��Station: {currentStation.title}"));
            if (currentStation.confirmation_method != Api.Models.v3.RailwayStationConfirmationMethod.unit_test)
            {
                root.Add(new Label("����Station�͎����e�X�g�ł͂Ȃ�����Unity��ŃN���A���肪�s���܂���B�u���E�U�ォ�画����s���Ă�������"));
                return;
            }
            if (!railwayManager.IsStationPermitted(currentStation))
            {
                root.Add(new Label("�����ɒ��킷��ɂ́A�L���v�����ւ̓o�^���K�v�ł��B"));
                return;
            }

            root.Add(new Label("You can challenge this station."));
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
