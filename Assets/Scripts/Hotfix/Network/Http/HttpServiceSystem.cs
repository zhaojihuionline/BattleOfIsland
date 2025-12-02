// Systems/HttpServiceSystem.cs
//#define NETWORK_108
using QFramework;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System;
using System.Text;

public interface IHttpServiceSystem : ISystem
{
    UniTask<RegisterResponse> Register(string username, string password, string email = "");
    UniTask<LoginResponse> Login(string username, string password);
}

public class HttpServiceSystem : AbstractSystem, IHttpServiceSystem
{
    private string BaseUrl
    {
        get
        {
            var config = NetworkConfigProvider.Config;
            if (config == null)
            {
                Debug.LogError("NetworkConfig asset missing. Please create Resources/NetworkConfig.asset");
                return "http://127.0.0.1:8888/api/v1";
            }

            return config.httpBaseUrl;
        }
    }

    protected override void OnInit()
    {
        Debug.Log("HttpServiceSystem 初始化完成");
    }

    public async UniTask<RegisterResponse> Register(string username, string password, string email = "")
    {
        string url = $"{BaseUrl}/auth/register";

        string jsonData = JsonUtility.ToJson(new RegisterRequest
        {
            username = username,
            password = password,
            email = email
        });

        Debug.Log($"注册请求: {jsonData}");

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
                    return JsonUtility.FromJson<RegisterResponse>(jsonResponse);
                }
                else
                {
                    Debug.LogError($"注册请求失败: {request.error}");
                    return new RegisterResponse { code = (int)request.responseCode, message = $"网络错误: {request.error}" };
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"注册异常: {ex.Message}");
            return new RegisterResponse { code = 500, message = $"异常: {ex.Message}" };
        }
    }

    public async UniTask<LoginResponse> Login(string username, string password)
    {
        string url = $"{BaseUrl}/auth/login";

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
                    return JsonUtility.FromJson<LoginResponse>(jsonResponse);
                }
                else
                {
                    Debug.LogError($"登录请求失败: {request.error}");
                    return new LoginResponse { code = (int)request.responseCode, message = $"网络错误: {request.error}" };
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"登录异常: {ex.Message}");
            return new LoginResponse { code = 500, message = $"异常: {ex.Message}" };
        }
    }
}