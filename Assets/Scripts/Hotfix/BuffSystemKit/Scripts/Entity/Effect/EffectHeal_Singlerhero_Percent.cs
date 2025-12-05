using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 恢复血量的效果器,对应的buff id为20274 释放条件，血量低于30%
/// </summary>
public class EffectHeal_Singlerhero_Percent : EffectEntity
{
    public override cfg.AttributeType attributeType => cfg.AttributeType.Heal_Singlerhero_Percent;

    public override void Execute()
    {
        int v = Mathf.FloorToInt(target.GetComponent<EntityController>().HPMAX * effect.effectNode.Param[1] / 100.0f);
        target.GetComponent<EntityController>().AddBlood(v);
        IsFinished = true;
        buffEntity.RemoveEffect(this);
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
