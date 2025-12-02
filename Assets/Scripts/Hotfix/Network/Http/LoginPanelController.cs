// Controllers/LoginPanelController.cs
using QFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginPanelController : MonoBehaviour, IController
{
    [Header("UI References")]
    public TMP_InputField UsernameInput;
    public TMP_InputField PasswordInput;
    public Button LoginButton;
    public Button RegisterButton;
    public Text StatusText;

    public NetworkDemo networkDemo;

    public IArchitecture GetArchitecture()
    {
        return HttpServiceApp.Interface;
    }

    private void Start()
    {
        // 注册事件监听
        this.RegisterEvent<LoginSuccessEvent>(OnLoginSuccess).UnRegisterWhenGameObjectDestroyed(gameObject);
        this.RegisterEvent<LoginFailedEvent>(OnLoginFailed).UnRegisterWhenGameObjectDestroyed(gameObject);
        this.RegisterEvent<RegisterSuccessEvent>(OnRegisterSuccess).UnRegisterWhenGameObjectDestroyed(gameObject);
        this.RegisterEvent<RegisterFailedEvent>(OnRegisterFailed).UnRegisterWhenGameObjectDestroyed(gameObject);

        // 绑定按钮事件
        LoginButton.onClick.AddListener(OnLoginClick);
        RegisterButton.onClick.AddListener(OnRegisterClick);

        // 检查是否已登录
        var authModel = this.GetModel<IAuthModel>();
        if (authModel.IsLoggedIn.Value)
        {
            StatusText.text = $"已登录: {authModel.Username.Value}";
        }

        // networkDemo.OnConnectClick();
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

    private void OnLoginSuccess(LoginSuccessEvent e)
    {
        StatusText.text = $"登录成功! 欢迎 {e.Username}";
        LoginButton.interactable = true;
        _ = networkDemo.OnLoginClick(e.Token);
        // 登录成功后的逻辑，比如跳转场景等
        Debug.Log($"Token: {e.Token}, AccountId: {e.AccountId}");
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
}