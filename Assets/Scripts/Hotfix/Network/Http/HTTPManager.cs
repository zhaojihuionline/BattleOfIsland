using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System;
using System.Text;
using QFramework;

// 修复后的响应类
[System.Serializable]
public class LoginRequest
{
    /// <summary>
    /// 
    /// </summary>
    public string password;
    /// <summary>
    /// 
    /// </summary>
    public string username;
}
[System.Serializable]
public class LoginResponse
{
    /// <summary>
    /// 
    /// </summary>
    public string account_id;
    /// <summary>
    /// 
    /// </summary>
    public int code;
    /// <summary>
    /// 
    /// </summary>
    public string expires_at;
    /// <summary>
    /// 
    /// </summary>
    public string message;
    /// <summary>
    /// 
    /// </summary>
    public string token;
    /// <summary>
    /// 
    /// </summary>
    public string username;
}
[System.Serializable]
public class RegisterRequest
{
    /// <summary>
    /// 
    /// </summary>
    public string email;
    /// <summary>
    /// 
    /// </summary>
    public string password;
    /// <summary>
    /// 
    /// </summary>
    public string phone;
    /// <summary>
    /// 
    /// </summary>
    public string username;
}
[System.Serializable]
public class RegisterResponse
{
    /// <summary>
    /// 
    /// </summary>
    public string account_id;
    /// <summary>
    /// 
    /// </summary>
    public int code;
    /// 
    /// </summary>
    public string message;
}
// 用户信息获取相关
[System.Serializable]
public class CommonResp
{
    public int code;
    public string msg;
}


public class HTTPManager : MonoSingleton<HTTPManager>
{
    [SerializeField] private NetworkConfigSO configAsset;

    public string AuthToken { get; private set; }
    public string UserId { get; private set; }

    private NetworkConfigSO ResolveConfig()
    {
        if (configAsset != null)
        {
            return configAsset;
        }

        configAsset = NetworkConfigProvider.Config;
        return configAsset;
    }

    private string GetBaseUrl()
    {
        var config = ResolveConfig();
        if (config == null)
        {
            Debug.LogError("NetworkConfig asset missing. Please place NetworkConfig.asset under Resources.");
            return "http://127.0.0.1:8888/api/v1";
        }

        return config.httpBaseUrl;
    }

    public async UniTask<RegisterResponse> RegisterUser(string username, string password, string email = "")
    {
        string url = $"{GetBaseUrl()}/auth/register";

        string jsonData = JsonUtility.ToJson(new RegisterRequest
        {
            username = username,
            password = password,
            email = email
        });
        Debug.Log("注册请求: " + jsonData);

        try
        {
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");

                await request.SendWebRequest().ToUniTask();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log("注册返回: " + jsonResponse);

                    RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(jsonResponse);
                    return response;
                }
                else
                {
                    Debug.LogError($"注册请求失败: {request.error}");
                    Debug.LogError($"URL: {url}");
                    Debug.LogError($"状态码: {request.responseCode}");
                    return new RegisterResponse { message = $"网络错误: {request.error}" };
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"注册异常: {ex.Message}");
            return new RegisterResponse { message = $"异常: {ex.Message}" };
        }
    }

    public async UniTask<LoginResponse> LoginUser(string username, string password)
    {
        string url = $"{GetBaseUrl()}/auth/login";

        string jsonData = JsonUtility.ToJson(new LoginRequest
        {
            username = username,
            password = password
        });
        Debug.Log("登录请求: " + jsonData);

        try
        {
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");

                await request.SendWebRequest().ToUniTask();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log("登录返回: " + jsonResponse);

                    LoginResponse response = JsonUtility.FromJson<LoginResponse>(jsonResponse);

                    // 根据code判断是否成功
                    if (response.code == 200) // 假设200表示成功
                    {
                        AuthToken = response.token;
                        UserId = response.account_id;
                        Debug.Log($"登录成功: {response.message}");
                        OnLoginSuccess?.Invoke(response);
                    }
                    else
                    {
                        Debug.LogWarning($"登录失败: {response.message} (代码: {response.code})");
                    }

                    return response;
                }
                else
                {
                    Debug.LogError($"登录请求失败: {request.error}");
                    Debug.LogError($"URL: {url}");
                    Debug.LogError($"状态码: {request.responseCode}");
                    return new LoginResponse { message = $"网络错误: {request.error}" };
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"登录异常: {ex.Message}");
            return new LoginResponse { message = $"异常: {ex.Message}" };
        }
    }

    public event Action<LoginResponse> OnLoginSuccess;

    public void Logout()
    {
        AuthToken = null;
        UserId = null;
        Debug.Log("用户已登出");
    }

    public bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(AuthToken);
    }
}