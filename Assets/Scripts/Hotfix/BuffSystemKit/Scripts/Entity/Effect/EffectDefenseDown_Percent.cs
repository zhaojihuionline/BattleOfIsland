using QFramework;
using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
/// <summary>
/// 降低防御力
/// </summary>
public class EffectDefenseDown_Percent : EffectEntity
{
    public override cfg.AttributeType attributeType => cfg.AttributeType.DefenseDown_Percent;

    public override void Execute()
    {
        //Debug.Log($"添加了降低防御力的效果器");
        ActionKit.Delay(5.0f, () =>
        {
            IsFinished = true;
            buffEntity.RemoveEffect(this);
        }).Start(target.GetComponent<EntityController>());
        //Observable.Timer(System.TimeSpan.FromSeconds(5.0f)).Subscribe(_ =>
        //{
        //    IsFinished = true;
        //    buffEntity.RemoveEffect(this);
        //}).AddTo(target);
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
        Object.Destroy(target.Find("spell_200001").gameObject);
    }

    public override void Update()
    {

    }
}
