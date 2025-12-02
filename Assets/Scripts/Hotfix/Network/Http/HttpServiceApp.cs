using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;


/// <summary>
/// 
/// </summary>
public class HttpServiceApp : Architecture<HttpServiceApp>
{
    protected override void Init()
    {
        // 注册Model
        this.RegisterModel<IAuthModel>(new AuthModel());

        // 注册System
        this.RegisterSystem<IHttpServiceSystem>(new HttpServiceSystem());
    }
}
