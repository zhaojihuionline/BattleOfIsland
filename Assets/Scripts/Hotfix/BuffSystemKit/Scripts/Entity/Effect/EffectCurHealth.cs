using cfg;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectCurHealth : EffectEntity
{
    public override AttributeType attributeType => cfg.AttributeType.CurHealth;

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
        if (effect.buffTable.Time != -1)
        {
            buffEntity.RemoveEffect(this);
        }
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
    float timer = 0;
    public override void Update()
    {
        if (effect.buffTable.Time == -1)
        {
            timer += Time.deltaTime;
            if (timer >= effect.buffTable.TickCd)
            {
                timer = 0;
                AttributeChangeData attributeChangeData = new AttributeChangeData()
                {
                    attributeType = attributeType,
                    value = effect.effectNode.Param[1],
                    baseCalculateType = effect.buffTable.BaseCalType,
                };
                target.GetComponent<ICanResponseBuff>().OnAttributeChange(attributeChangeData);
            }
        }
    }
}
