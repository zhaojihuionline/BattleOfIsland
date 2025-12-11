using cfg;
using UnityEngine;

/// <summary>
/// 修改攻击力效果器
/// </summary>
public class EffectAttack : EffectEntity
{
    public override cfg.AttributeType attributeType => cfg.AttributeType.Attack;
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
        Debug.Log("确实执行了修改攻击力效果器");
        //buffEntity.RemoveEffect(this);
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