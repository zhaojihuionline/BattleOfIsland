//using SimpleJSON;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SimpleJSON;
/// <summary>
/// 数据加载器
/// </summary>
/// <typeparam name="T">数据模型类</typeparam>
public class DataLoader<T>
{
    //public T configDatas { get; set; }
    //public cfg.Tables tables;
    public void Init(bool useRemote = false)
    {
        //if (useRemote)
        //{
        //    HttpHandler.Instance.Get("https://127.0.0.1:8080/Datas/game1/GameCfg.json",
        //        (successRes) =>
        //        {

        //        },
        //        (errorRes) =>
        //        {

        //        }
        //    );
        //}
        //else
        //{
        //    tables = new cfg.Tables(file =>
        //    {
        //        var textAsset = Resources.Load<TextAsset>("LuabanGenerateDatas/json/" + file);
        //        if (textAsset == null)
        //            return null;
        //        return Newtonsoft.Json.Linq.JArray.Parse(textAsset.text);
        //    });
        //}
    }
}
