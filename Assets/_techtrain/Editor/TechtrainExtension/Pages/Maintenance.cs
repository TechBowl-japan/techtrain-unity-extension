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
            return new Label("���݃����e�i���X���ł��B��Q���������Ă���ꍇ�� X (@TechBowl1) �ɂď󋵂�z�M�����Ă��������܂��B");
        }
    }
}