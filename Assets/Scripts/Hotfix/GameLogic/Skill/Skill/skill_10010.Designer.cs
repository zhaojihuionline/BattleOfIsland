using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class skill_10010 : QFramework.IController
{
    public UnityEngine.GameObject Model;

    public BPathMove BPath;

    QFramework.IArchitecture QFramework.IBelongToArchitecture.GetArchitecture() => GameApp.Interface;
}