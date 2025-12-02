using QFramework;
using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 减免受到的伤害
/// </summary>
public class EffectDefense_PercentReduction_all : EffectEntity
{
    public override cfg.AttributeType attributeType => cfg.AttributeType.Defense_PercentReduction_all;

    public override void Execute()
    {
        ResLoader loader = ResLoader.Allocate();
        GameObject newFX = loader.LoadSync<GameObject>("FX_Skill_haojiaochongfeng");// 号角冲锋的特效
        GameObject newFXEntity = Object.Instantiate(newFX, target.position, Quaternion.identity, target);
        newFXEntity.gameObject.name = "FX_haojiao";
        newFXEntity.transform.localPosition = Vector3.zero;
        newFXEntity.transform.localScale = Vector3.one * 3.0f;

        target.GetComponent<EntityController>().Defense_PercentReduction_all = true;
        target.GetComponent<EntityController>().Defense_PercentReductionValue = effect.effectNode.Param[1];
    }

    public override void Init(Effect _effect, BuffEntity _buffEntity, Transform _target)
    {
        this.buffEntity = _buffEntity;
        this.effect = _effect;
        this.target = _target;
        this.eDuration = buffEntity.bDuration;
        this.eName = _buffEntity.bName + " 效果器";
        defauleAttributeValue = _effect.attributeValue;
    }

    public override void OnExit()
    {
        if (target.Find("FX_haojiao"))
        {
           Object.Destroy(target.Find("FX_haojiao").gameObject);
        }
    }

    public override void Update()
    {

    }
}
