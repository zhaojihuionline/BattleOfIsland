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

    //public enum ETargetType
    //{
    //    NONE = 0,
    //    /// <summary>
    //    /// 弓兵英雄
    //    /// </summary>
    //    ArcheHero = 1,
    //    /// <summary>
    //    /// 弓兵雇佣兵
    //    /// </summary>
    //    Arche = 2,
    //    /// <summary>
    //    /// 盾兵英雄
    //    /// </summary>
    //    ShieldHero = 3,
    //    /// <summary>
    //    /// 盾兵雇佣兵
    //    /// </summary>
    //    ShieldSoldier = 4,
    //    /// <summary>
    //    /// 骑兵英雄
    //    /// </summary>
    //    CavalryHero = 5,
    //    /// <summary>
    //    /// 骑兵雇佣兵
    //    /// </summary>
    //    Cavalry = 6,
    //    /// <summary>
    //    /// 大本营（核心建筑）
    //    /// </summary>
    //    KeyBuilding = 7,
    //    /// <summary>
    //    /// 生产建筑
    //    /// </summary>
    //    ProductionBuilding = 8,
    //    /// <summary>
    //    /// 存储建筑
    //    /// </summary>
    //    StorageBuilding = 9,
    //    /// <summary>
    //    /// 功能型建筑
    //    /// </summary>
    //    FunctionalBuilding = 10,
    //    /// <summary>
    //    /// 城墙
    //    /// </summary>
    //    Wall = 11,
    //    /// <summary>
    //    /// 防御型建筑（箭塔）
    //    /// </summary>
    //    Tower = 12,
    //}

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