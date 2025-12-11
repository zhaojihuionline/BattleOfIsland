using DG.Tweening;
using QFramework;
using QFramework.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
/// <summary>
/// 巴顿英雄的被动技能10015 神之庇佑,功能：每秒恢复x点血
/// </summary>
public class skill_10015 : SkillController
{
    protected override void OnStart_Cast()
    {
        Debug.Log($"效果：巴顿{packetData._data.Description}");
        base.OnStart_Cast();

        // 每秒恢复x点血
        // 每秒执行一次施加buff

        //IDisposable disposable = Observable.Interval(TimeSpan.FromSeconds(1))
        //.Subscribe(_ =>
        //{
        //    var targetData = TargetData.New();
        //    targetData.Target = packetData.caster;
        //    for (int i = 0; i < packetData._data.Effect.Count; i++)
        //    {
        //        this.SendCommand(new AddSingleBuffToTargetCommand(targetData, packetData._data.Effect[i], null));
        //    }
        //});
    }
}
