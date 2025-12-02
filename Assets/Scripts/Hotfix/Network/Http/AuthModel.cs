// Models/AuthModel.cs
using QFramework;
using UnityEngine;

public interface IAuthModel : IModel
{
    BindableProperty<string> AuthToken { get; }
    BindableProperty<string> UserId { get; }
    BindableProperty<string> Username { get; }
    BindableProperty<bool> IsLoggedIn { get; }
}

public class AuthModel : AbstractModel, IAuthModel
{
    public BindableProperty<string> AuthToken { get; } = new BindableProperty<string>();
    public BindableProperty<string> UserId { get; } = new BindableProperty<string>();
    public BindableProperty<string> Username { get; } = new BindableProperty<string>();
    public BindableProperty<bool> IsLoggedIn { get; } = new BindableProperty<bool>();

    protected override void OnInit()
    {
        // Model只负责数据恢复，不包含业务逻辑
        AuthToken.Value = PlayerPrefs.GetString("auth_token", string.Empty);
        UserId.Value = PlayerPrefs.GetString("user_id", string.Empty);
        Username.Value = PlayerPrefs.GetString("username", string.Empty);
        IsLoggedIn.Value = !string.IsNullOrEmpty(AuthToken.Value);
    }

}