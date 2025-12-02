using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
public class GameManager : MonoSingleton<GameManager>, IController
{
    public IArchitecture GetArchitecture()
    {
        throw new System.NotImplementedException();
    }

    void Start()
    {
        Debug.Log("QQQQQQQQQQQQQQQQQQ");

        UIKit.OpenPanel(QAssetBundle.Prefabs_uipanel_ab.LoginPanel);
    }

}
