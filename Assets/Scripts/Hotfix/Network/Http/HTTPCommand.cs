// Commands/LoginCommand.cs
using QFramework;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LoginCommand : AbstractCommand
{
    private readonly string mUsername;
    private readonly string mPassword;

    public LoginCommand(string username, string password)
    {
        mUsername = username;
        mPassword = password;
    }

    protected override async void OnExecute()
    {
        var httpService = this.GetSystem<IHttpServiceSystem>();

        var result = await httpService.Login(mUsername, mPassword);

        if (result.code == 200 && !string.IsNullOrEmpty(result.token))
        {
            // 使用Command来保存登录数据
            this.SendCommand(new SaveLoginDataCommand(
                result.token,
                result.account_id,
                result.username
            ));

            // 发送登录成功事件
            this.SendEvent(new LoginSuccessEvent
            {
                AccountId = result.account_id,
                Username = result.username,
                Token = result.token
            });

            Debug.Log($"登录成功: {result.message}");
        }
        else
        {
            // 发送登录失败事件
            this.SendEvent(new LoginFailedEvent
            {
                ErrorCode = result.code,
                ErrorMessage = result.message
            });

            Debug.LogWarning($"登录失败: {result.message} (代码: {result.code})");
        }
    }
}
public class RegisterCommand : AbstractCommand
{
    private readonly string mUsername;
    private readonly string mPassword;
    private readonly string mEmail;

    public RegisterCommand(string username, string password, string email = "")
    {
        mUsername = username;
        mPassword = password;
        mEmail = email;
    }

    protected override async void OnExecute()
    {
        var httpService = this.GetSystem<IHttpServiceSystem>();

        var result = await httpService.Register(mUsername, mPassword, mEmail);

        if (result.code == 200) // 假设200表示成功
        {
            // 发送注册成功事件
            this.SendEvent(new RegisterSuccessEvent
            {
                AccountId = result.account_id,
                Message = result.message
            });

            Debug.Log($"注册成功: {result.message}");

            PlayerPrefs.SetString("username", mUsername); //ToDo
            PlayerPrefs.SetString("password", mPassword);
        }
        else
        {
            // 发送注册失败事件
            this.SendEvent(new RegisterFailedEvent
            {
                ErrorCode = result.code,
                ErrorMessage = result.message
            });

            Debug.LogWarning($"注册失败: {result.message} (代码: {result.code})");
        }
    }
}

public class LogoutCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        // 使用Command来清除登录数据
        this.SendCommand(new ClearLoginDataCommand());

        // 发送登出事件
        this.SendEvent<LogoutEvent>();

        Debug.Log("执行登出操作");
    }
}
/// <summary>
/// 保存登录数据的Command
/// </summary>
public class SaveLoginDataCommand : AbstractCommand
{
    private readonly string mToken;
    private readonly string mUserId;
    private readonly string mUsername;

    public SaveLoginDataCommand(string token, string userId, string username)
    {
        mToken = token;
        mUserId = userId;
        mUsername = username;
    }

    protected override void OnExecute()
    {
        var authModel = this.GetModel<IAuthModel>();

        // 更新Model数据
        authModel.AuthToken.Value = mToken;
        authModel.UserId.Value = mUserId;
        authModel.Username.Value = mUsername;
        authModel.IsLoggedIn.Value = true;

        // 保存到本地存储
        PlayerPrefs.SetString("auth_token", mToken);
        PlayerPrefs.SetString("user_id", mUserId);
        PlayerPrefs.SetString("username", mUsername);
        PlayerPrefs.Save();

        Debug.Log($"登录数据已保存: {mUsername}");
    }
}

/// <summary>
/// 清除登录数据的Command
/// </summary>
public class ClearLoginDataCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        var authModel = this.GetModel<IAuthModel>();

        // 清除Model数据
        authModel.AuthToken.Value = string.Empty;
        authModel.UserId.Value = string.Empty;
        authModel.Username.Value = string.Empty;
        authModel.IsLoggedIn.Value = false;

        // 清除本地存储
        PlayerPrefs.DeleteKey("auth_token");
        PlayerPrefs.DeleteKey("user_id");
        PlayerPrefs.DeleteKey("username");
        PlayerPrefs.Save();

        Debug.Log("登录数据已清除");
    }
}