using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using QFramework;

public class EffectRoot : EffectEntity
{
    public override cfg.AttributeType attributeType => cfg.AttributeType.Root;

    public override void Execute()
    {
        // 定身效果暂时这么实现，后期再优化
        target.GetComponent<AIPath>().maxSpeed = 0;
        
        // 持续时间结束后解除定身
        Observable.Timer(System.TimeSpan.FromSeconds(effect.effectNode.Param[1])).Subscribe(_ =>
        {
            IsFinished = true;
            target.GetComponent<AIPath>().maxSpeed = 6f;
            buffEntity.RemoveEffect(this);
        }).AddTo(target);
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
