// Events/AuthEvents.cs
using QFramework;

// 登录成功事件
public struct LoginSuccessEvent
{
    public string AccountId;
    public string Username;
    public string Token;
}

// 登录失败事件
public struct LoginFailedEvent
{
    public int ErrorCode;
    public string ErrorMessage;
}

// 注册成功事件
public struct RegisterSuccessEvent
{
    public string AccountId;
    public string Message;
}

// 注册失败事件
public struct RegisterFailedEvent
{
    public int ErrorCode;
    public string ErrorMessage;
}

// 登出事件
public struct LogoutEvent { }