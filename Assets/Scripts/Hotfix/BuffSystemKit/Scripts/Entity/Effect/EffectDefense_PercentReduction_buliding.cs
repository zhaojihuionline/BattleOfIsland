using DG.Tweening;
using QFramework;
using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectDefense_PercentReduction_buliding : EffectEntity
{
    public override cfg.AttributeType attributeType => cfg.AttributeType.Defense_PercentReduction_buliding;

    public override void Execute()
    {
        target.transform.DOScale(Vector3.one * 1.2f, 0.2f);
        Debug.Log("主动技能的特效并施加buff");
        ActionKit.Delay(5.0f, () =>
        {
            target.transform.DOScale(Vector3.one, 0.2f);
            IsFinished = true;
            buffEntity.RemoveEffect(this);
        }).Start(target.GetComponent<EntityController>());
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
        if (target.Find("FX_Badun_Skill0"))
        {
            Object.Destroy(target.Find("FX_Badun_Skill0").gameObject);
        }
    }

    public override void Update()
    {

    }
}
