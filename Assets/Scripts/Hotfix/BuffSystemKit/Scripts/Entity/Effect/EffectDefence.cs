using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EffectDefence : EffectEntity
{
    public override cfg.AttributeType attributeType => cfg.AttributeType.Defense;
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
        //buffEntity.RemoveEffect(this);
    }

    void OnEnable()
    {
        Debug.Log($"EffectDefence.OnEnable");
    }

    //public enum ETargetType
    //{
    //    NONE = 0,
    //    /// <summary>
    //    /// ¹­±øÓ¢ÐÛ
    //    /// </summary>
    //    ArcheHero = 1,
    //    /// <summary>
    //    /// ¹­±ø¹ÍÓ¶±ø
    //    /// </summary>
    //    Arche = 2,
    //    /// <summary>
    //    /// ¶Ü±øÓ¢ÐÛ
    //    /// </summary>
    //    ShieldHero = 3,
    //    /// <summary>
    //    /// ¶Ü±ø¹ÍÓ¶±ø
    //    /// </summary>
    //    ShieldSoldier = 4,
    //    /// <summary>
    //    /// Æï±øÓ¢ÐÛ
    //    /// </summary>
    //    CavalryHero = 5,
    //    /// <summary>
    //    /// Æï±ø¹ÍÓ¶±ø
    //    /// </summary>
    //    Cavalry = 6,
    //    /// <summary>
    //    /// ´ó±¾Óª£¨ºËÐÄ½¨Öþ£©
    //    /// </summary>
    //    KeyBuilding = 7,
    //    /// <summary>
    //    /// Éú²ú½¨Öþ
    //    /// </summary>
    //    ProductionBuilding = 8,
    //    /// <summary>
    //    /// ´æ´¢½¨Öþ
    //    /// </summary>
    //    StorageBuilding = 9,
    //    /// <summary>
    //    /// ¹¦ÄÜÐÍ½¨Öþ
    //    /// </summary>
    //    FunctionalBuilding = 10,
    //    /// <summary>
    //    /// ³ÇÇ½
    //    /// </summary>
    //    Wall = 11,
    //    /// <summary>
    //    /// ·ÀÓùÐÍ½¨Öþ£¨¼ýËþ£©
    //    /// </summary>
    //    Tower = 12,
    //}

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

    public override void Update()
    {

    }
}
