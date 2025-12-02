using QAssetBundle;
using UnityEngine;
using UnityEngine.UI;

namespace QFramework.UI
{
    public class LoginPanelData : UIPanelData
    {

    }
    public partial class LoginPanel : UIPanel, IController
    {
        [Header("UI References")]
        public InputField UsernameInput;
        public InputField PasswordInput;
        public Button LoginButton;
        public Button RegisterButton;

        public Button RegisterMultipleUserButton;
        public Text StatusText;

        public IArchitecture GetArchitecture()
        {
            return HttpServiceApp.Interface;
        }
        private void OnLoginClick()
        {
            string username = UsernameInput.text;
            string password = PasswordInput.text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                StatusText.text = "请输入用户名和密码";
                return;
            }

            StatusText.text = "登录中...";
            LoginButton.interactable = false;

            // 执行登录Command
            this.SendCommand(new LoginCommand(username, password));
        }

        private void OnRegisterClick()
        {
            string username = UsernameInput.text;
            string password = PasswordInput.text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                StatusText.text = "请输入用户名和密码";
                return;
            }

            StatusText.text = "注册中...";
            RegisterButton.interactable = false;



            // 执行注册Command
            this.SendCommand(new RegisterCommand(username, password));
        }

        private async void OnLoginSuccess(LoginSuccessEvent e)
        {
            StatusText.text = $"登录成功! 欢迎 {e.Username}";
            LoginButton.interactable = true;
            await NetworkDemo.Instance.OnLoginClick(e.Token);
            Debug.Log($"Token: {e.Token}, AccountId: {e.AccountId}");
            // 登录成功后的逻辑，比如跳转场景等
            ResLoader loader = ResLoader.Allocate();
            //loader.LoadSceneSync(Scenes_ab.Game);
            UIKit.ClosePanel<LoginPanel>();
            UIKit.OpenPanel(QAssetBundle.Prefabs_uipanel_ab.HomeMainPanel);
        }

        private void OnLoginFailed(LoginFailedEvent e)
        {
            StatusText.text = $"登录失败: {e.ErrorMessage}";
            LoginButton.interactable = true;
        }

        private void OnRegisterSuccess(RegisterSuccessEvent e)
        {
            StatusText.text = $"注册成功! {e.Message}";
            RegisterButton.interactable = true;

            // 注册成功后可以自动填充登录表单
            PasswordInput.text = "";
        }

        private void OnRegisterFailed(RegisterFailedEvent e)
        {
            StatusText.text = $"注册失败: {e.ErrorMessage}";
            RegisterButton.interactable = true;
        }
        protected override void OnInit(IUIData uiData = null)
        {
            mData = uiData as LoginPanelData ?? new LoginPanelData();
            // please add init code here
        }

        protected override void OnOpen(IUIData uiData = null)
        {
            // 注册事件监听
            this.RegisterEvent<LoginSuccessEvent>(OnLoginSuccess).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<LoginFailedEvent>(OnLoginFailed).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<RegisterSuccessEvent>(OnRegisterSuccess).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<RegisterFailedEvent>(OnRegisterFailed).UnRegisterWhenGameObjectDestroyed(gameObject);

            // 绑定按钮事件
            LoginButton.onClick.AddListener(OnLoginClick);

            RegisterButton.onClick.AddListener(OnRegisterClick);

            // 批量注册多个假人用户
            //RegisterMultipleUserButton.onClick.AddListener(() => {
                
            //    RegisterMultipleUserButton.interactable = false;
            //    for (int i = 0; i < 5; i++)
            //    {
            //        this.SendCommand(new RegisterCommand("Computer_" + i, "111111"));
            //    }
            //});

            // 检查是否已登录
            var authModel = this.GetModel<IAuthModel>();
            if (authModel.IsLoggedIn.Value)
            {
                StatusText.text = $"已登录: {authModel.Username.Value}";
                UsernameInput.text = authModel.Username.Value;
            }
            UsernameInput.text = PlayerPrefs.GetString("username", "");
            PasswordInput.text = PlayerPrefs.GetString("password", "");
        }

        protected override void OnShow()
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnClose()
        {
            // NetworkDemo.Instance.OnLogoutClick(); //ToDo
            // this.SendCommand(new LogoutCommand());
        }
    }
}
