using Cysharp.Threading.Tasks;
using PitayaClient.Network.Manager;
using PitayaGame.LoginSvr; // 添加登录相关的命名空间
using QFramework;
using System;
using UnityEngine;

/// <summary>
/// 网络演示脚本 - 简单的连接、握手、断开、重连、登录演示
/// </summary>
public class NetworkDemo : MonoSingleton<NetworkDemo>
{
    [Header("服务器配置")]
    [SerializeField] private NetworkConfigSO configAsset;

    [Header("登录配置")]
    [SerializeField] private string testToken = "your_test_jwt_token_here"; // 测试用的 JWT Token
    [SerializeField] private string deviceId = "test_device_001";
    [SerializeField] private string platform = "pc";

    private bool _isConnecting = false;
    private bool _isLoggingIn = false;
    private string _logContent = "";
    private string _currentUserId = "";

    string token;
    private NetworkConfigSO ResolveConfig()
    {
        if (configAsset != null)
        {
            return configAsset;
        }

        configAsset = NetworkConfigProvider.Config;
        return configAsset;
    }

    private void Start()
    {
        OnConnect();
        // 订阅网络事件
        NetworkManager.Instance.OnConnected += OnConnected;
        NetworkManager.Instance.OnDisconnected += OnDisconnected;
        NetworkManager.Instance.OnError += OnError;

        AddLog("🚀 网络演示程序已启动");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // 取消订阅事件
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnConnected -= OnConnected;
            NetworkManager.Instance.OnDisconnected -= OnDisconnected;
            NetworkManager.Instance.OnError -= OnError;
        }
    }

    #region 连接服务器
    public async void OnConnect()
    {
        if (_isConnecting) return;

        var config = ResolveConfig();
        if (config == null)
        {
            AddLog("❌ 未找到网络配置，请创建 Resources/NetworkConfig.asset");
            return;
        }

        _isConnecting = true;
        AddLog($"🔗 开始连接到服务器: {config.serverEndpoint}");

        try
        {
            bool success = await NetworkManager.Instance.ConnectAsync(config.serverEndpoint, config.serverPath);
            if (success)
            {
                AddLog("✅ 连接成功！握手流程已完成");
            }
            else
            {
                AddLog("❌ 连接失败");
            }
        }
        catch (Exception ex)
        {
            AddLog($"❌ 连接异常: {ex.Message}");
        }
        finally
        {
            _isConnecting = false;
        }
    }
    public async UniTask OnLoginClick(string _token)
    {
        if (!NetworkManager.Instance.IsConnected)
        {
            AddLog("请先连接到服务器");
            return;
        }

        if (_isLoggingIn) return;

        _isLoggingIn = true;

        try
        {
            // 获取 Token
            token = _token;

            if (string.IsNullOrEmpty(token))
            {
                AddLog("token 不能为空");
                return;
            }

            AddLog($"开始登录，Token: {token.Substring(0, Math.Min(10, token.Length))}...");

            // 创建登录请求
            var loginRequest = new PitayaGame.LoginSvr.LoginRequest
            {
                Token = token,
                DeviceId = deviceId,
                Platform = platform
            };


            // 发送登录请求
            var response = await NetworkManager.Instance.RequestAsync<PitayaGame.LoginSvr.LoginResponse>(
                "loginsvr.login.login",
                loginRequest,
                10f
            );

            // 处理登录响应
            if (response.Resp != null && response.Resp.Code == 0) // 假设 0 表示成功
            {
                _currentUserId = response.UserId;
                AddLog($"登录成功！用户ID: {_currentUserId}");
                AddLog($"服务器时间: {response.ServerTime}");
                AddLog($"登录Token: {response.Token}");
            }
            else
            {
                string errorMsg = response.Resp?.Message ?? "未知错误";
                AddLog($"登录失败: {errorMsg} (代码: {response.Resp?.Code})");
            }
        }
        catch (TimeoutException)
        {
            AddLog("登录请求超时");
        }
        catch (Exception ex)
        {
            AddLog($"登录异常: {ex.Message}");
        }
        finally
        {
            _isLoggingIn = false;
        }
    }

    public async void OnLogoutClick()
    {
        if (string.IsNullOrEmpty(_currentUserId))
        {
            AddLog("❌ 当前未登录");
            return;
        }

        try
        {
            AddLog("🚪 开始登出...");

            var logoutRequest = new LogoutRequest
            {
                UserId = _currentUserId
            };

            var response = await NetworkManager.Instance.RequestAsync<LogoutResponse>(
                "loginsvr.auth.logout",
                logoutRequest
            );

            if (response.Resp != null && response.Resp.Code == 0)
            {
                AddLog("✅ 登出成功");
                _currentUserId = "";
            }
            else
            {
                string errorMsg = response.Resp?.Message ?? "未知错误";
                AddLog($"❌ 登出失败: {errorMsg}");
            }
        }
        catch (Exception ex)
        {
            AddLog($"❌ 登出异常: {ex.Message}");
        }
        finally
        {
        }
    }
    #endregion

    #region 网络事件处理
    private void OnConnected()
    {
        AddLog("📡 网络连接已建立");
    }

    private void OnDisconnected(string reason)
    {
        AddLog($"📡 网络连接断开: {reason}");
        _currentUserId = ""; // 连接断开时清空用户ID
    }

    private void OnError(Exception ex)
    {
        AddLog($"⚠️ 网络错误: {ex.Message}");
    }
    #endregion

    #region UI更新

    private void AddLog(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        _logContent = $"[{timestamp}] {message}\n{_logContent}";

        // 限制日志长度
        if (_logContent.Length > 2000)
        {
            _logContent = _logContent.Substring(0, 2000);
        }

        Log.log(_logContent);
    }
    #endregion
}