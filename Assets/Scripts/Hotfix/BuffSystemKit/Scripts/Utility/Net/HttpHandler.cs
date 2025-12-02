using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class HttpHandler : MonoBehaviour
{
    private static HttpHandler _instance;

    public static HttpHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("HttpHandler");
                _instance = obj.AddComponent<HttpHandler>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    /// <summary>
    /// 发起 GET 请求
    /// </summary>
    /// <param name="url">请求地址</param>
    /// <param name="onSuccess">成功回调（返回字符串）</param>
    /// <param name="onError">失败回调（返回错误信息）</param>
    public void Get(string url, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(GetCoroutine(url, onSuccess, onError));
    }

    private IEnumerator GetCoroutine(string url, Action<string> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(webRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[HttpService] GET 请求出错: {webRequest.error}");
                onError?.Invoke(webRequest.error);
            }
        }
    }

    /// <summary>
    /// 发起 POST 请求（支持 JSON 字符串）
    /// </summary>
    /// <param name="url">请求地址</param>
    /// <param name="jsonData">要发送的 JSON 数据</param>
    /// <param name="onSuccess">成功回调（返回字符串）</param>
    /// <param name="onError">失败回调（返回错误信息）</param>
    public void Post(string url, string jsonData, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(PostCoroutine(url, jsonData, onSuccess, onError));
    }

    private IEnumerator PostCoroutine(string url, string jsonData, Action<string> onSuccess, Action<string> onError)
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(webRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[HttpService] POST 请求出错: {webRequest.error}");
                onError?.Invoke(webRequest.error);
            }
        }
    }
}
