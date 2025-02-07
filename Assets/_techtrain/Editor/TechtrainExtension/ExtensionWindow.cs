using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Threading.Tasks;


namespace TechtrainExtension
{
    public class ExtensionWindow : EditorWindow
    {
        [SerializeField] private StyleSheet styleSheet;
        internal Config.ConfigManager configManager;
        internal Api.Client apiClient;
        internal Manifests.Manager manifestsManager;
        private VisualElement root;

        public void CreateGUI()
        {
            configManager = new Config.ConfigManager();
            apiClient = new Api.Client(configManager.Config);
            manifestsManager = new Manifests.Manager();

            root = new VisualElement();
            root.styleSheets.Add(styleSheet);
            root.AddToClassList("root");
            rootVisualElement.Clear();
            rootVisualElement.Add(root);
            _ = InitializePage();
        }

        private async Task InitializePage()
        {
            var manifestRailway = manifestsManager.GetRailway();
            var railway = await apiClient.GetRailway(manifestRailway.railwayId);
            Debug.Log(railway);
        }

        internal void Reload()
        {
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
