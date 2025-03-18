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
        private VisualElement root;

        public void CreateGUI()
        {
            configManager = new Config.ConfigManager();
            apiClient = new Api.Client(configManager);
            root = new VisualElement();
            root.styleSheets.Add(styleSheet);
            root.AddToClassList("root");
            rootVisualElement.Clear();
            rootVisualElement.Add(root);
            _ = InitializePage();
        }

        private async Task InitializePage()
        {
            var isMaintenance = await IsMaintenance();
            if (isMaintenance)
            {
                var maintenance = new Pages.Maintenance(this);
                root.Add(maintenance.root);
                return;
            }
        }

        internal void Reload()
        {
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
