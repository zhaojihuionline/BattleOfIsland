using DG.Tweening;
using QFramework;
using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 巴顿主动技能  举盾并放大  并施加了一个20004的buff，也就是对应的EffectDefense_PercentReduction_buliding效果器
/// </summary>
public class skill_10006 : SkillController
{
    [SerializeField] GameObject Model;
    [SerializeField] LayerMask layerMask;
    protected override void OnDo_Cast()
    {
        base.OnDo_Cast();
    }
    protected override void OnStart_WindUp()
    {
        base.OnStart_WindUp();
        Model.SetActive(false);
    }
    protected override void OnStart_Cast()
    {
        base.OnStart_Cast();
        Debug.Log("开始释放skill10006");
        var enemys = SectorDetection.GetUnitsInSector<EntityController>(packetData.caster.transform,5,180, layerMask);
        SkillKit.BeHurt(packetData, 10);
        Model.SetActive(true);
    }
}