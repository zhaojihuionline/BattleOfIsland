using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectAttackSpeed : EffectEntity
{
    public override cfg.AttributeType attributeType => cfg.AttributeType.Attack_SpeedUp_Percent;

    public override void Execute()
    {
        target.GetComponent<ICanResponseBuff>().OnUpgradeAttackSpeed(effect.effectNode.Param[1]);
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
