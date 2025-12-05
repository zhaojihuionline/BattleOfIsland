using cfg;
using QFramework;
using QFramework.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.YamlDotNet.Serialization.NodeTypeResolvers;
using UnityEngine;
using static UnityEditor.ShaderKeywordFilter.FilterAttribute;

/// <summary>
/// 
/// </summary>
public class SkillSystemCommand : AbstractCommand
{
    protected override void OnExecute()
    {
    }
}

public class ReleaseSkillCommand : AbstractCommand
{
    SkillPacket Packet;
    SkillSystem system;
    //这个id是 id+lv  id在对应的英雄表格中 技能列表
    public ReleaseSkillCommand(SkillPacket Packet)
    {
        this.Packet = Packet;
    }
    public ReleaseSkillCommand(SkillTable _data, GameObject caster, TargetData target, Vector3 point)
    {
        Packet = new SkillPacket();

        Init(_data, caster, target, point);
    }

    public ReleaseSkillCommand(int id, int lv, GameObject caster, TargetData target, Vector3 point)
    {
        Packet = new SkillPacket();
        Init(CfgMgr.GetSkillTableS(id * 100 + lv), caster, target, point);
    }

    public void Init(SkillTable _data, GameObject caster, TargetData target, Vector3 point)
    {
        Packet.SetTable(_data);
        Packet.SetCaster(caster);
        Packet.SetTarget(target, point);
        //测试伤害
        DamageDate damageDate = new DamageDate();
        damageDate.Init();
        damageDate.SetDamage(HurtType.PHYSICAL_DAMAGE, 0);
        Packet.SetDamsge(damageDate);
    }
    protected override void OnExecute()
    {
        switch (Packet._data.SkillType)
        {
            case SkillType.PASSIVE_SKILL:
                if(Packet._data.Effect.Count > 0)
                {
                    this.SendCommand<AddSingleBuffToTargetCommand>(new AddSingleBuffToTargetCommand(Packet.target.transform, Packet._data.Effect[0], null));
                }
                break;
            case SkillType.NORMAL_ATTACK:
            case SkillType.ACTIVE_SKILL:
                system = this.GetSystem<SkillSystem>();
                system.ReleaseSkill(Packet);
                break;
        }
    }
}

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
    ///// <summary>
    ///// 查找目标的命令  参数需要  传入目标列表  标签列表  范围  偏好规则  查找者
    ///// </summary>
    ///// <param name="targetList">目标列表</param>
    ///// <param name="listMask">列表Mask</param>
    ///// <param name="range">检测范围</param>
    ///// <param name="preference">规则偏好</param>
    ///// <param name="gameObject">目标  没有目标传位置坐标</param>
    //public FindTargetCommand(List<GameObject> targetList, List<string> listMask, float range, string preference, GameObject gameObject)
    //{
    //    this.targetList = targetList;
    //    this.gameObject = gameObject;
    //    if (this.gameObject != null)
    //    {
    //        centerPoint = this.gameObject.transform.position;
    //    }
    //}
    //public FindTargetCommand(List<GameObject> targetList, List<string> listMask, float range, string preference, Vector3 centerPoint)
    //{
    //    this.targetList = targetList;
    //    this.centerPoint = centerPoint;
    //    gameObject = null;
    //}

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

//public class FindAOETargetsCommand : AbstractCommand<List<GameObject>>
//{
//    EntityController entityController;
//    float radius;
//    List<GameObject> enemys;
//    SkillTable skillTable;
//    public FindAOETargetsCommand(EntityController entityController, float radius, List<GameObject> enemys,SkillTable skillTable)
//    {
//        this.entityController = entityController;
//        this.radius = radius;
//        this.enemys = enemys;
//        this.skillTable = skillTable;
//    }

//    List<GameObject> list = new List<GameObject>();
//    protected override List<GameObject> OnExecute()
//    {
//        list.Clear();
//        //范围小于等于0  且有查找者的时候  就是自身为目标   
//        if (radius <= 0)
//        {
//            list.Add(entityController.gameObject);
//            return list;
//        }
//        SkillSystem system = this.GetSystem<SkillSystem>();
//        system.CleanInvalidTargets(enemys);
//        //索敌规则  看配置 全部敌方列表 tag layer + 规则 最近等等 
//        //根据tag筛选目标  筛选返回结果  这是可以进行选择的目标列表
//        List<GameObject> targetGList = system.FindObjectsByTagsOptimized(enemys, skillTable.TagMask);
//        if (targetGList == null || targetGList.Count == 0)
//        {
//            // Debug.LogError($"{this.gameObject.name}: 没有符合标签的目标");
//            return null;
//        }
//        //根据索敌距离再次进行筛选 就是范围内可以选择的列表  可能没有施法者
//        targetGList = system.FindObjectsByDistance(targetGList, entityController.transform.position, radius / 100f);

//        if (targetGList == null || targetGList.Count == 0)
//        {
//            // Debug.LogError($"{this.gameObject.name}: 没有符合距离的目标");
//            return null;
//        }
//        ResLoader loader = ResLoader.Allocate();
//        //这就是读取的索敌规则 用它去查看有没有目标就可以  是否每帧执行看情况
//        TargetingPreference targetingPreference = loader.LoadSync<TargetingPreference>(skillTable.Preference);

//        List<GameObject> targets = targetingPreference.SelectTargets(new IData.Data()
//        {
//            data = new Dictionary<string, IData.IVariable>()
//                {
//                    {
//                        "targetList",
//                        new IData.Variable<List<GameObject>>(targetGList)
//                    },
//                    {
//                        "self",
//                        new IData.Variable<GameObject>(entityController.gameObject)
//                    }
//                }
//        });
//        loader.Recycle2Cache();
//        return targets;
//    }
//}