using DG.Tweening;
using QFramework;
using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 巴顿主动技能  举盾并放大  并施加了一个20004的buff，也就是对应的EffectDefense_PercentReduction_buliding效果器
/// </summary>
public class skill_10005 : SkillController
{
    protected override void OnDo_Cast()
    {
        base.OnDo_Cast();
    }
    protected override void OnStart_Cast()
    {
        Debug.Log("skill_1000501 技能施法开始");
        Debug.Log("效果：巴顿举起盾牌并放大");

        GameObject _effect = transform.Find("Effect").gameObject;
        GameObject newFXEntity = Object.Instantiate(_effect, packetData.caster.transform.position, Quaternion.identity, packetData.caster.transform);
        newFXEntity.gameObject.name = "FX_Badun_Skill0";
        newFXEntity.transform.localPosition = Vector3.zero;
        newFXEntity.transform.localScale = Vector3.one * 2.5f;
        newFXEntity.SetActive(true);

        newFXEntity.transform.Find("FX_0").gameObject.SetActive(false);
        newFXEntity.transform.Find("FX_1").gameObject.SetActive(false);

        newFXEntity.transform.Find("FX_0").gameObject.SetActive(true);
        this.Delay(0.25f, () => {
            newFXEntity.transform.Find("FX_1").gameObject.SetActive(true);
        });

        if (packetData.caster.transform.CompareTag("Hero"))
        {
            //if (packetData.caster.transform.name.Contains("Hero_Batun"))
            //{

            //}
            this.SendCommand(new AddSingleBuffToTargetCommand(packetData.caster.transform, packetData._data.Effect[0], newFXEntity));
        }
        base.OnStart_Cast();
    }
}