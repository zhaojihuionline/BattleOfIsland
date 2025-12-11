using cfg;
using HutongGames.PlayMaker;
using PitayaClient.Protocol;
using QFramework;
using QFramework.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.YamlDotNet.Serialization.NodeTypeResolvers;
using UnityEngine;
using static UnityEditor.ShaderKeywordFilter.FilterAttribute;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// 
/// </summary>
public class SkillSystemCommand : AbstractCommand
{
    protected override void OnExecute()
    {
    }
}

//public class ReleaseSkillCommand : AbstractCommand
//{
//    SkillPacket Packet;
//    SkillSystem system;
//    //这个id是 id+lv  id在对应的英雄表格中 技能列表
//    public ReleaseSkillCommand(SkillPacket Packet)
//    {
//        this.Packet = Packet;
//    }
//    public ReleaseSkillCommand(SkillTable _data, GameObject caster, TargetData target, Vector3 point)
//    {
//        Packet = new SkillPacket();

//        Init(_data, caster, target, point);
//    }

//    //public ReleaseSkillCommand(int id, int lv, GameObject caster, TargetData target, Vector3 point)
//    //{
//    //    Packet = new SkillPacket();
//    //    Init(CfgMgr.GetSkillTableS(id * 100 + lv), caster, target, point);
//    //}

//    public void Init(SkillTable _data, GameObject caster, TargetData target, Vector3 point)
//    {
//        Packet.SetTable(_data);
//        Packet.SetCaster(caster);
//        Packet.SetTarget(target, point);
//        //测试伤害
//        DamageDate damageDate = new DamageDate();
//        damageDate.Init();
//        damageDate.SetDamage(HurtType.PHYSICAL_DAMAGE, 0);
//        Packet.SetDamsge(damageDate);
//    }
//    protected override void OnExecute()
//    {
//        Debug.Log($"释放技能 AddBuff skillId:{Packet._data.Id}");
//        switch (Packet._data.SkillType)
//        {
//            case SkillType.PASSIVE_SKILL:

//                foreach(var buffId in Packet._data.Effect)
//                {
//                    //筛选出技能施法目标
//                    Packet.TargetData = this.SendCommand(new QuerySkillTargets(Packet.caster, buffId));
//                    this.SendCommand<AddSingleBuffToTargetCommand>(new AddSingleBuffToTargetCommand(Packet.TargetData, buffId, null));
//                }
//                //this.SendCommand<AddSingleBuffToTargetCommand>(new AddSingleBuffToTargetCommand(Packet.target.transform, Packet._data.Effect[0], null));
//                break;
//            case SkillType.AURA_SKILL:
//                foreach (var buffId in Packet._data.Effect)
//                {
//                    //筛选出技能施法目标
//                    Packet.TargetData = this.SendCommand(new QuerySkillTargets(Packet.caster, buffId));
//                    this.SendCommand<AddSingleBuffToTargetCommand>(new AddSingleBuffToTargetCommand(Packet.TargetData, buffId, null));
//                }
//                break;
//            case SkillType.NORMAL_ATTACK:
//            case SkillType.ACTIVE_SKILL:
//                system = this.GetSystem<SkillSystem>();
//                system.ReleaseSkill(Packet);
//                break;
//        }
//    }
//}

public class ReleaseSpellCommand : AbstractCommand
{
    SkillPacket Packet;
    SkillSystem system;
    public ReleaseSpellCommand(SkillTable _data, GameObject caster, TargetData targetData, Vector3 point)
    {
        Packet = new SkillPacket();
        Init(_data, caster, targetData, point);
    }


    public void Init(SkillTable _data, GameObject caster, TargetData targetData, Vector3 point)
    {
        Packet.SetTable(_data);
        Packet.SetCaster(caster);
        Packet.SetTarget(targetData, point);
        //测试伤害
        DamageDate damageDate = new DamageDate();
        damageDate.Init();
        damageDate.SetDamage(HurtType.PHYSICAL_DAMAGE, 0);
        Packet.SetDamsge(damageDate);
    }
    protected override void OnExecute()
    {
        system = this.GetSystem<SkillSystem>();
        system.ReleaseSpell(Packet);
    }
}
/// <summary>
/// 清理该列表中的无效数据
/// </summary>
public class CleanInvalidTargetsCommand : AbstractCommand
{
    List<GameObject> targetList;
    public CleanInvalidTargetsCommand(List<GameObject> targetList)
    {
        this.targetList = targetList;
    }
    protected override void OnExecute()
    {
        CleanInvalidTargets();
    }
    /// <summary>
    /// 清理所有无效目标
    /// </summary>
    public void CleanInvalidTargets()
    {
        if (targetList == null) return;

        int originalCount = targetList.Count;
        targetList.RemoveAll(obj => obj == null);
        int removedCount = originalCount - targetList.Count;

        if (removedCount > 0)
        {
            Debug.Log($"清理了 {removedCount} 个无效目标");
        }
    }
}
// /// <summary>
// /// 根据攻击偏好选择目标
// /// </summary>
// public class FindTargetWithPreferenceCommand : AbstractCommand<GameObject>
// {
//     protected override GameObject OnExecute()
//     {
//     }
// }

public struct TargetData
{
    public static TargetData New()
    {
        return new TargetData { Target = null, Targets = new List<GameObject>() };
    }

    public GameObject Target;
    public List<GameObject> Targets;
}
public class FindTargetCommand : AbstractCommand<TargetData>
{
    List<GameObject> targetList;
    EntityController entityController;
    Vector3 centerPoint;
    SkillTable skillTable;
    float searchRange = -1;

    public FindTargetCommand(List<GameObject> targetList, SkillTable skillTable, EntityController entityController, float searchRange = -1)
    {
        this.targetList = targetList;
        this.skillTable = skillTable;
        this.entityController = entityController;
        this.searchRange = searchRange;
        if (this.entityController != null)
        {
            centerPoint = this.entityController.transform.position;
        }
    }

    protected override TargetData OnExecute()
    {
        //范围小于等于0  且有查找者的时候  就是自身为目标   
        if (skillTable.CastRanage <= 0 && entityController != null)
        {
            return new TargetData() { Target = entityController.gameObject };
        }
        SkillSystem system = this.GetSystem<SkillSystem>();
        system.CleanInvalidTargets(targetList);
        //索敌规则  看配置 全部敌方列表 tag layer + 规则 最近等等 
        //根据tag筛选目标  筛选返回结果  这是可以进行选择的目标列表
        List<GameObject> targetGList = system.FindObjectsByTagsOptimized(targetList, skillTable.TagMask);
        if (targetGList == null || targetGList.Count == 0)
        {
           // Debug.LogError($"{this.gameObject.name}: 没有符合标签的目标");
            return default;
        }
        //根据索敌距离再次进行筛选 就是范围内可以选择的列表  可能没有施法者
        targetGList = system.FindObjectsByDistance(targetGList, centerPoint, searchRange <= 0 ? (skillTable.CastRanage / 100f) : searchRange);

        if (targetGList == null || targetGList.Count == 0)
        {
            // Debug.LogError($"{this.gameObject.name}: 没有符合距离的目标");
            return default;
        }
        ResLoader loader = ResLoader.Allocate();
        //这就是读取的索敌规则 用它去查看有没有目标就可以  是否每帧执行看情况
        TargetingPreference targetingPreference = loader.LoadSync<TargetingPreference>(skillTable.Preference);

        IData.Data data = new IData.Data()
        {
            data = new Dictionary<string, IData.IVariable>()
                {
                    {
                        "targetList",
                        new IData.Variable<List<GameObject>>(targetGList)
                    },
                    {
                        "self",
                        new IData.Variable<GameObject>(entityController.gameObject)
                    }
                }
        };

        GameObject target = targetingPreference.SelectTarget(data);

        List<GameObject> targets = null;
        float damageRange = entityController.UnitData.GetDamageRange();
        if (damageRange > 0)
        {
            targets = system.FindObjectsByDistance(targetList, target.transform.position, damageRange);
        }

        loader.Recycle2Cache();
        return new TargetData { Target = target,Targets = targets };
    }
}

public class QuerySkillTargets : AbstractCommand<TargetData>
{
    GameObject caster;
    BuffTable buffTable;
    public QuerySkillTargets(GameObject caster, int buffId)
    {
        this.caster = caster;
        this.buffTable = CfgMgr.Instance.Tables.TbBuff.Get(buffId);
    }
    protected override TargetData OnExecute()
    {
        TargetData targetData = TargetData.New();
        List<GameObject> entitys = null;
        var battleInModel = this.GetModel<BattleInModel>();
        switch (buffTable.FirstGoal)
        {
            case ECampType.NONE:

                break;
            case ECampType.All:

                break;
            case ECampType.Self:
                targetData.Target = caster;
                break;
            case ECampType.Friend:
                entitys = battleInModel.player_allEntitys;
                break;
            case ECampType.Enemy:
                entitys = battleInModel.opponent_allEntitys;
                break;
        }

        if (entitys == null)
            return targetData;

        foreach (var entity in entitys)
        {
            AddEntity(entity, ref targetData);
        }
        
        return targetData;
    }

    void AddEntity(GameObject entity,ref TargetData targetData)
    {
        EntityController entityController = entity.GetComponent<EntityController>();
        foreach (ETargetType value in Enum.GetValues(typeof(ETargetType)))
        {
            if (value == ETargetType.NONE) continue;
            if (buffTable.NextGoal.HasFlag(value))
            {
                targetData.Targets.Add(entity);
            }
        }
    }
}