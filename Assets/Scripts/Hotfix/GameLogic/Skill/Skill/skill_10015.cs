using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 巴顿英雄的被动技能10015 神之庇佑
/// </summary>
public class skill_10015 : SkillController
{
    protected override void OnStart_Cast()
    {
        Debug.Log($"效果：巴顿{packetData._data.Description}");
        base.OnStart_Cast();
    }
}
