using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework.Game;
/// <summary>
/// 恢复某个英雄血量x点  对应的buffid 20515
/// </summary>
public class EffectHeal_Singlerhero : EffectEntity
{
    public override cfg.AttributeType attributeType => cfg.AttributeType.Heal_Singlerhero;

    public override void Execute()
    {
        AttributeChangeData attributeChangeData = new AttributeChangeData()
        {
            attributeType = attributeType,
            value = effect.effectNode.Param[1],
            baseCalculateType = effect.buffTable.BaseCalType,
        };
        target.GetComponent<ICanResponseBuff>().OnAttributeChange(attributeChangeData);
        IsFinished = true;
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

    }

    public override void Update()
    {

    }
}
