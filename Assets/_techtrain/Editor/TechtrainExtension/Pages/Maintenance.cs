#nullable enable

using UnityEngine.UIElements;
using UnityEngine;

namespace TechtrainExtension.Pages
{
    public class Maintenance
    {
        private ExtensionWindow window;
        internal VisualElement root;
        private Label messageLabel;
        public Maintenance(ExtensionWindow self)
        {
            window = self;
            messageLabel = CreateMessageLabel();
            root = Create();
            root.Add(messageLabel);
        }
        private VisualElement Create()
        {
            var root = new VisualElement();
            root.AddToClassList("maintenance");
            return root;
        }
        private Label CreateMessageLabel()
        {
            return new Label("現在メンテナンス中です。障害が発生している場合は X (@TechBowl1) にて状況を配信させていただきます。");
        }
    }
}