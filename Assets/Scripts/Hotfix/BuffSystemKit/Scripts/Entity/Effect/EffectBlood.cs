using UnityEngine;

/// <summary>
/// 修改血量效果器
/// </summary>
public class EffectBlood : EffectEntity
{
    float _curBurnDuration = 0;
    bool canProcessBurn = false;

    public override cfg.AttributeType attributeType => cfg.AttributeType.Blood;
    public override void Init(Effect _effect, BuffEntity _buffEntity, Transform _target)
    {
        this.buffEntity = _buffEntity;
        this.effect = _effect;
        this.target = _target;
        this.eDuration = buffEntity.bDuration;
        this.eName = _buffEntity.bName + " 效果器";
        defauleAttributeValue = _effect.attributeValue;
    }
    public override void Execute()
    {
        // 瞬时改变血量（具体数值）
        target.GetComponent<ICanResponseBuff>()?.OnUpgradeBlood(effect.effectNode.Param[1]);
        IsFinished = true;
        buffEntity.RemoveEffect(this);
    }

    public override void Update()
    {

    }
    public override void OnExit()
    {

    }
}