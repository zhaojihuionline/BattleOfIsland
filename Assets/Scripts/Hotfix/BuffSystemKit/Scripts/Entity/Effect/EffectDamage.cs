using PitayaClient.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectDamage : EffectEntity
{
    public override cfg.AttributeType attributeType => cfg.AttributeType.Damage;

    public override void Execute()
    {
        //SkillKit.BeHurt(new TargetData { Target = target.gameObject }, effect.effectNode.Param[1]);
        AttributeChangeData attributeChangeData = new AttributeChangeData()
        {
            attributeType = attributeType,
            value = effect.effectNode.Param[1],
            baseCalculateType = effect.buffTable.BaseCalType,
        };
        target.GetComponent<ICanResponseBuff>().OnAttributeChange(attributeChangeData);
        IsFinished = true;
        buffEntity.RemoveEffect(this);
    }

    public override void Init(Effect _effect, BuffEntity _buffEntity, Transform _target)
    {
        this.buffEntity = _buffEntity;
        this.effect = _effect;
        this.target = _target;
        this.eDuration = buffEntity.bDuration;
        this.eName = _buffEntity.bName + " Ð§¹ûÆ÷";
        defauleAttributeValue = _effect.attributeValue;
    }

    public override void OnExit()
    {

    }

    public override void Update()
    {

    }
}
