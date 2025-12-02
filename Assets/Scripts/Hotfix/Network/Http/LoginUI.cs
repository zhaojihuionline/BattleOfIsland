using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;

public class LoginUI : MonoBehaviour
{
    [Header("UI 引用")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField emailInput; // 注册用
    public Button loginButton;
    public Button registerButton;
    public Text statusText;

    void Start()
    {
        loginButton.onClick.AddListener(async () => await OnLoginClicked());
        registerButton.onClick.AddListener(async () => await OnRegisterClicked());
    }

    async UniTask OnLoginClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            statusText.text = "请输入用户名和密码";
            return;
        }

        statusText.text = "登录中...";
        loginButton.interactable = false;

        var result = await HTTPManager.Instance.LoginUser(username, password);

        if (result.code == 200)
        {
            statusText.text = "登录成功！";
            // 登录成功，可以跳转到主菜单或开始游戏
            Debug.Log($"Token: {result.token}");

            // 在这里启动你的WebSocket连接
            // await WebSocketManager.Instance.ConnectWithToken(result.token);
        }
        else
        {
            statusText.text = $"登录失败: {result.message}";
            loginButton.interactable = true;
        }
    }

    async UniTask OnRegisterClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        string email = emailInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            statusText.text = "请输入用户名和密码";
            return;
        }

        statusText.text = "注册中...";
        registerButton.interactable = false;

        var result = await HTTPManager.Instance.RegisterUser(username, password, email);

        if (result.code == 200)
        {
            statusText.text = "注册成功！请登录";
            // 注册成功后可以自动登录或清空表单
            passwordInput.text = "";
        }
        else
        {
            statusText.text = $"注册失败: {result.message}";
        }

        registerButton.interactable = true;
    }
}