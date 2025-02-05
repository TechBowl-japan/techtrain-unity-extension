#nullable enable

using UnityEngine.UIElements;
using UnityEngine;

namespace TechtrainExtension.Page
{
    public class Login
    {
        private ExtensionWindow window;
        internal VisualElement root;

        private TextField emailField;
        private TextField passwordField;
        private Button loginButton;

        private VisualElement errorArea;

        public Login(ExtensionWindow self)
        {
            window = self;
            emailField = CreateEmailField();
            passwordField = CreatePasswordField();
            loginButton = CreateLoginButton();
            errorArea = CreateErrorArea();

            root = Create();
            root.Add(emailField);
            root.Add(passwordField);
            root.Add(loginButton);
            root.Add(errorArea);
        }

        private VisualElement Create()
        {
            var root = new VisualElement();
            root.AddToClassList("login");
            return root;
        }

        private TextField CreateEmailField()
        {
            return new TextField("Email");
        }

        private TextField CreatePasswordField()
        {
            return new TextField("Password")
            {
                isPasswordField = true
            };
        }

        private Button CreateLoginButton()
        {
            return new Button(async() =>
            {
                SetInputEnabled(false);
                var task = await window.apiClient.PostLogin(emailField.value, passwordField.value);
                if (task?.data?.api_token != null)
                {
                    Debug.Log($"Login success token: {task.data?.api_token}");
                }
                else if(task != null)
                {
                    ShowError($"ログインに失敗しました\n{task.message}");
                }
                else
                {
                    ShowError("ログインに失敗しました");
                }
            })
            {
                text = "Login"
            };
        }

        private VisualElement CreateErrorArea()
        {
            return new VisualElement();
        }

        private void ShowError(string message)
        {
            errorArea.Clear();
            errorArea.Add(new HelpBox()
            {
                text = message,
                messageType = HelpBoxMessageType.Error
            }
            );
        }

        private void SetInputEnabled(bool enabled)
        {
            emailField.SetEnabled(enabled);
            passwordField.SetEnabled(enabled);
            loginButton.SetEnabled(enabled);
        }
    }
}
