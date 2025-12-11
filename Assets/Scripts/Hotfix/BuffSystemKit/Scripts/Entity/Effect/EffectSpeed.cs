using cfg;
using QFramework;
using QFramework.Game;
using UnityEngine;

/// <summary>
/// 改变移速的效果器
/// </summary>
public class EffectSpeed : EffectEntity
{
    public override cfg.AttributeType attributeType => cfg.AttributeType.Speed;
    float _curDuration = 0;
    bool canProcessReduceSpeed = false;
    public override void Init(Effect _effect, BuffEntity _buffEntity, Transform _target)
    {
        this.buffEntity = _buffEntity;
        this.effect = _effect;
        this.target = _target;
        this.eDuration = buffEntity.bDuration;
        this.eName = _buffEntity.bName + " 效果器";
        Debug.Log(eName + ": 效果器");
        defauleAttributeValue = _effect.attributeValue;
    }
    public override void Execute()
    {
        //// 生成一个增加移速的特效
        //ResLoader loader = ResLoader.Allocate();
        //GameObject newFX = loader.LoadSync<GameObject>(buffEntity.buff_fxName);// 加移速的特效
        //GameObject newFXEntity = Object.Instantiate(newFX, target.position, Quaternion.identity, target);
        //newFXEntity.transform.localScale = Vector3.one * 3.0f;

        //// 具体增加移速的逻辑
        ////target.GetComponent<EntityController>()?.OnUpgradeSpeed(effect.effectNode.Param[1]);
        //float ms = target.GetComponent<EntityController>().MoveSpeed;
        //float _addValue = ms * (effect.effectNode.Param[1] / 100f);
        //ms += _addValue;
        //target.GetComponent<EntityController>().aiPath.maxSpeed = ms;

        //IsFinished = true;
        //buffEntity.RemoveEffect(this);
        AttributeChangeData attributeChangeData = new AttributeChangeData()
        {
            attributeType = attributeType,
            value = effect.effectNode.Param[1],
            baseCalculateType = effect.buffTable.BaseCalType,
        };
        target.GetComponent<ICanResponseBuff>().OnAttributeChange(attributeChangeData);
        IsFinished = true;
        Debug.Log("确实执行了改变移速的效果器");

        //持续时间（0是瞬发，-1是永久）
        if (effect.buffTable.Time == 0)
        {
            RemoveBuff(attributeChangeData);
        }
        else if(effect.buffTable.Time > 0)
        {
            ActionKit.Delay(effect.buffTable.Time, () =>
            {
                RemoveBuff(attributeChangeData);
            }).Start(buffEntity);
        }
        
        //buffEntity.RemoveEffect(this);
    }
    public override void Update()
    {

    }
    public override void OnExit()
    {
        //target.GetComponent<ICanResponseBuff>()?.SetDefaultSpeed();
    }

    void RemoveBuff(AttributeChangeData attributeChangeData)
    {
        attributeChangeData.isReverse = true;
        target.GetComponent<ICanResponseBuff>().OnAttributeChange(attributeChangeData);
        buffEntity.RemoveEffect(this);
    }
}