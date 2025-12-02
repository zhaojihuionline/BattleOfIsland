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

    }

    public override void Init(Effect _effect, BuffEntity _buffEntity, Transform _target)
    {

    }

    public override void OnExit()
    {

    }

    public override void Update()
    {

    }
}
