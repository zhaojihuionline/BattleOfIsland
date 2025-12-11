using PitayaClient.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectExtraDamage : EffectEntity
{
    public override cfg.AttributeType attributeType => cfg.AttributeType.ExtraDamage;

    public override void Execute()
    {
        AttributeChangeData attributeChangeData = new AttributeChangeData()
        {
            attributeType = attributeType,
            value = effect.effectNode.Param[1],
            baseCalculateType = effect.buffTable.BaseCalType,
        };
        target.GetComponent<ICanResponseBuff>().OnAttributeChange(attributeChangeData);

        //target.GetComponent<ICanResponseBuff>().OnUpgradeAttack(effect.effectNode.Param[0]);
        IsFinished = true;
        Debug.Log("确实执行了修改额外攻击力效果器");
        //buffEntity.RemoveEffect(this);
        //SkillKit.BeHurt(new TargetData { Target = target.gameObject }, effect.effectNode.Param[1]);
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
