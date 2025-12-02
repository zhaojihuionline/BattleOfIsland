using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using cfg;
using Unity.VisualScripting.YamlDotNet.Serialization.NodeTypeResolvers;
using System.Linq;

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
    public ReleaseSkillCommand(SkillTable _data, GameObject caster, GameObject target, Vector3 point)
    {
        Packet = new SkillPacket();

        Init(_data, caster, target, point);
    }
    public ReleaseSkillCommand(int id, int lv, GameObject caster, GameObject target, Vector3 point)
    {
        Packet = new SkillPacket();
        Init(CfgMgr.GetSkillTableS(id * 100 + lv), caster, target, point);
    }
    public void Init(SkillTable _data, GameObject caster, GameObject target, Vector3 point)
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
        system = this.GetSystem<SkillSystem>();
        system.ReleaseSkill(Packet);
    }
}
public class ReleaseSpellCommand : AbstractCommand
{
    SkillPacket Packet;
    SkillSystem system;
    public ReleaseSpellCommand(SkillTable _data, GameObject caster, GameObject target, Vector3 point)
    {
        Packet = new SkillPacket();
        Init(_data, caster, target, point);
    }


    public void Init(SkillTable _data, GameObject caster, GameObject target, Vector3 point)
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

public class FindTargetCommand : AbstractCommand<GameObject>
{
    List<string> listMask;
    List<GameObject> targetList;
    float range;
    string preference;
    GameObject gameObject;
    Vector3 centerPoint;
    /// <summary>
    /// 查找目标的命令  参数需要  传入目标列表  标签列表  范围  偏好规则  查找者
    /// </summary>
    /// <param name="targetList">目标列表</param>
    /// <param name="listMask">列表Mask</param>
    /// <param name="range">检测范围</param>
    /// <param name="preference">规则偏好</param>
    /// <param name="gameObject">目标  没有目标传位置坐标</param>
    public
    FindTargetCommand(List<GameObject> targetList, List<string> listMask, float range, string preference, GameObject gameObject)
    {
        this.targetList = targetList;
        this.listMask = listMask;
        this.range = range;
        this.preference = preference;
        this.gameObject = gameObject;
        if (this.gameObject != null)
        {
            centerPoint = this.gameObject.transform.position;
        }
    }
    public FindTargetCommand(List<GameObject> targetList, List<string> listMask, float range, string preference, Vector3 centerPoint)
    {
        this.targetList = targetList;
        this.listMask = listMask;
        this.range = range;
        this.preference = preference;
        this.centerPoint = centerPoint;
        gameObject = null;
    }

    protected override GameObject OnExecute()
    {
        //范围小于等于0  且有查找者的时候  就是自身为目标   
        if (range <= 0 && gameObject != null)
        {
            return gameObject;
        }
        SkillSystem system = this.GetSystem<SkillSystem>();
        system.CleanInvalidTargets(targetList);
        //索敌规则  看配置 全部敌方列表 tag layer + 规则 最近等等 
        //根据tag筛选目标  筛选返回结果  这是可以进行选择的目标列表
        List<GameObject> targetGList = system.FindObjectsByTagsOptimized(targetList, listMask);
        if (targetGList == null || targetGList.Count == 0)
        {
           // Debug.LogError($"{this.gameObject.name}: 没有符合标签的目标");
            return null;
        }
        //根据索敌距离再次进行筛选 就是范围内可以选择的列表  可能没有施法者
        targetGList = system.FindObjectsByDistance(targetGList, centerPoint, range / 100f);

        if (targetGList == null || targetGList.Count == 0)
        {
            // Debug.LogError($"{this.gameObject.name}: 没有符合距离的目标");
            return null;
        }
        ResLoader loader = ResLoader.Allocate();
        //这就是读取的索敌规则 用它去查看有没有目标就可以  是否每帧执行看情况
        TargetingPreference targetingPreference = loader.LoadSync<TargetingPreference>(preference);

        GameObject target = targetingPreference.SelectTarget(new IData.Data()
        {
            data = new Dictionary<string, IData.IVariable>()
                {
                    {
                        "targetList",
                        new IData.Variable<List<GameObject>>(targetGList)
                    },
                    {
                        "self",
                        new IData.Variable<GameObject>(gameObject)
                    }
                }
        });
        loader.Recycle2Cache();
        return target;
    }
}
