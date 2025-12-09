using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 移速降低-百分比的Buff效果器
/// </summary>
public class EffectSpeedDown_Percent : EffectEntity
{
    public override cfg.AttributeType attributeType => cfg.AttributeType.SpeedDown_Percent;

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
