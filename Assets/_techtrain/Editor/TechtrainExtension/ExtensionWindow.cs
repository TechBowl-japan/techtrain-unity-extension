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
            var login = new Page.Login(this);
            root.Add(login.root);
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
