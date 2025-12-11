using cfg;
using PitayaClient.Protocol;
using QFramework.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Pathfinding.Drawing.DrawingData;

public class SkillKit
{

    public static void BeHurt(SkillPacket skillPacket, int damage)
    {
        skillPacket.target.GetComponent<ICanHurt>().BeHurt(damage);
        if (skillPacket.targets != null)
        {
            foreach (var target in skillPacket.targets)
            {
                target.GetComponent<ICanHurt>().BeHurt(damage);
            }
        }
    }

    public static void BeHurt(TargetData targetData, int damage)
    {
        targetData.Target.GetComponent<ICanHurt>().BeHurt(damage);
        if (targetData.Targets != null)
        {
            foreach (var target in targetData.Targets)
            {
                target.GetComponent<ICanHurt>().BeHurt(damage);
            }
        }
    }

    public static void CheckAuraSkill(GameObject caster, int buffId)
    {
        //筛选出技能施法目标
        TargetData targetData = GameApp.Interface.SendCommand(new QuerySkillTargets(caster, buffId));
        GameApp.Interface.SendCommand<AddSingleBuffToTargetCommand>(new AddSingleBuffToTargetCommand(targetData, buffId, null));
    }

    //public static void AddSingleBuffToTarget(TargetData targetData, int _buffid, GameObject effectObj)
    //{
    //    AddBuff(targetData.Target, _buffid);
    //    foreach (var target in targetData.Targets)
    //    {
    //        if (targetData.Target == target)
    //            continue;
    //        AddBuff(target, _buffid);
    //    }
    //}

    //static void AddBuff(GameObject target,int buffId)
    //{
    //    if (!target) return;
    //    var buffRunner = target.GetComponent<EntityController>().buffRunner;
    //    if (buffRunner.HasBuff(buffId)) return;
    //    Debug.Log($"AddBuff target{target} {buffId}");
    //    if (buffRunner != null)
    //    {
    //        buffRunner.GiveBuff(target.transform, buffId);
    //        buffRunner.ExecuteBuff(buffId);
    //    }
    //}

    //public static TargetData QuerySkillTargets(GameObject caster, int buffId) 
    //{
    //    BattleInModel battleInModel = GameApp.Interface.GetModel<BattleInModel>();
    //    BuffTable buffTable = CfgMgr.Instance.Tables.TbBuff.Get(buffId);
    //    TargetData targetData = TargetData.New();
    //    List<GameObject> entitys = null;
    //    switch (buffTable.FirstGoal)
    //    {
    //        case ECampType.NONE:

    //            break;
    //        case ECampType.All:

    //            break;
    //        case ECampType.Self:
    //            targetData.Target = caster;
    //            break;
    //        case ECampType.Friend:
    //            entitys = battleInModel.player_allEntitys;
    //            break;
    //        case ECampType.Enemy:
    //            entitys = battleInModel.opponent_allEntitys;
    //            break;
    //    }

    //    if (entitys == null)
    //        return targetData;

    //    foreach (var entity in entitys)
    //    {
    //        //AddEntity
    //        AddEntity(entity, ref targetData,buffTable);
    //    }

    //    return targetData;
    //}

    //static void AddEntity(GameObject entity, ref TargetData targetData,BuffTable buffTable)
    //{
    //    EntityController entityController = entity.GetComponent<EntityController>();
    //    foreach (ETargetType value in Enum.GetValues(typeof(ETargetType)))
    //    {
    //        if (value == ETargetType.NONE) continue;
    //        if (buffTable.NextGoal.HasFlag(value))
    //        {
    //            switch(value)
    //            {
    //                case ETargetType.Target:

    //                    break;
    //                default:
    //                    targetData.Targets.Add(entity);
    //                    break;
    //            }
    //            break;
    //        }
    //    }
    //}

    public static float ProcessCalculate(cfg.EBaseCalculateType baseCalculateType, float attribute, int value, string attributeName,string HeroName,string entityName)
    {
        float preAttribute = attribute;
        float fValue = value / 100f;
        switch (baseCalculateType)
        {
            case EBaseCalculateType.Add:
                attribute += fValue;
                Debug.Log($"属性加成 属性：{attributeName} {baseCalculateType}属性增加{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{HeroName} GoName:{entityName}");
                break;
            case EBaseCalculateType.Subtract:
                attribute -= fValue;
                Debug.Log($"属性加成 属性：{attributeName} {baseCalculateType}属性减少{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{HeroName} GoName:{entityName}");
                break;
            case EBaseCalculateType.Multiply:
                attribute *= (1 + (float)fValue);
                Debug.Log($"属性加成 属性：{attributeName} {baseCalculateType}属性加成{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{HeroName} GoName:{entityName}");
                break;
            case EBaseCalculateType.Divide:
                attribute *= (1 - (float)fValue);
                Debug.Log($"属性加成 属性：{attributeName} {baseCalculateType}属性减成{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{HeroName} GoName:{entityName}");
                break;
        }
        return attribute;
    }
    //反向计算
    public static float ProcessCalculateReverse(cfg.EBaseCalculateType baseCalculateType, float attribute, int value, string attributeName, string HeroName, string entityName)
    {
        float preAttribute = attribute;
        float fValue = value / 100f;
        switch (baseCalculateType)
        {
            case EBaseCalculateType.Add:
                attribute -= fValue;
                Debug.Log($"属性移除 属性：{attributeName} {baseCalculateType}属性增加{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{HeroName} GoName:{entityName}");
                break;
            case EBaseCalculateType.Subtract:
                attribute += fValue;
                Debug.Log($"属性移除 属性：{attributeName} {baseCalculateType}属性减少{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{HeroName} GoName:{entityName}");
                break;
            case EBaseCalculateType.Multiply:
                attribute *= (1 - (float)fValue);
                Debug.Log($"属性移除 属性：{attributeName} {baseCalculateType}属性加成{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{HeroName} GoName:{entityName}");
                break;
            case EBaseCalculateType.Divide:
                attribute *= (1 + (float)fValue);
                Debug.Log($"属性移除 属性：{attributeName} {baseCalculateType}属性减成{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{HeroName} GoName:{entityName}");
                break;
        }
        return attribute;
    }
}
