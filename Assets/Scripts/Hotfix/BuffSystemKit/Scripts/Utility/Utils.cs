using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class Utils
{
    /// <summary>
    /// 简易get请求
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static IEnumerator GetRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // 发送请求并等待返回
            yield return webRequest.SendWebRequest();

            // 检查结果
            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("请求出错: " + webRequest.error);
            }
            else
            {
                // 获取返回字符串
                string responseText = webRequest.downloadHandler.text;
                Debug.Log("服务器返回: " + responseText);
            }
        }
    }
    /// <summary>
    /// 解析数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static List<(int id, int value)> ParseString(string input)
    {
        List<(int id, int value)> parsedList = new List<(int, int)>();

        string pattern = @"\{(-?\d+),(-?\d+)\}";
        Regex regex = new Regex(pattern);
        MatchCollection matches = regex.Matches(input);

        foreach (Match match in matches)
        {
            int id = int.Parse(match.Groups[1].Value);
            int value = int.Parse(match.Groups[2].Value);
            parsedList.Add((id, value));
        }

        return parsedList;
    }

    public static int GetBuffIDByEnum(BuffIndex buffIndex)
    {
        switch (buffIndex)
        {
            case BuffIndex.buff10001:
                return 10001;
            case BuffIndex.buff10002:
                return 10002;
            case BuffIndex.buff10003:
                return 10003;
            case BuffIndex.buff10004:
                return 10004;
            case BuffIndex.buff10005:
                return 10005;
            case BuffIndex.buff10006:
                return 10006;
            default:
                return -1;
        }
    }
}
