using cfg;
using PitayaClient.Protocol;
using QFramework;
using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

/// <summary>
/// 
/// </summary>
public interface ISkillSystem : ISystem
{

}
/// <summary>
/// 技能系统  
/// 负责索敌以及技能释放 
/// 对象池生成技能 回收技能
/// 释放后全部交由对应的SkillController去执行具体逻辑
/// </summary>
public class SkillSystem : AbstractSystem, ISkillSystem,ICanSendCommand
{
    public Dictionary<int, GameObject> resourcesMap = new Dictionary<int, GameObject>();
    protected override void OnInit()
    {
        ResKit.Init();
    }

    public void ReleaseSkill(SkillTable _data, GameObject caster, TargetData target, Vector3 point)
    {
        SkillPacket Packet = new SkillPacket();
        Packet.SetTable(_data);
        Packet.SetCaster(caster);
        Packet.SetTarget(target, point);
        //测试伤害
        DamageDate damageDate = new DamageDate();
        damageDate.Init();
        damageDate.SetDamage(HurtType.PHYSICAL_DAMAGE, 0);
        Packet.SetDamsge(damageDate);
        ReleaseSkill(Packet);
    }
    /// <summary>
    /// 创建技能
    /// </summary>
    public void ReleaseSkill(SkillPacket Packet)
    {
        Debug.Log($"释放技能 {Packet._data.Name} 对目标 {Packet.target} 位置 {Packet.targetPoint}");

        switch (Packet._data.SkillType)
        {
            case SkillType.PASSIVE_SKILL:
                {
                    foreach (var buffId in Packet._data.Effect)
                    {
                        //筛选出技能施法目标
                        Packet.TargetData = this.SendCommand(new QuerySkillTargets(Packet.caster, buffId));
                        this.SendCommand<AddSingleBuffToTargetCommand>(new AddSingleBuffToTargetCommand(Packet.TargetData, buffId, null));
                    }
                    SkillController skillController = DisplaySkill(Packet);
                    skillController.SetSkillPacket(Packet);
                }
                break;
            case SkillType.AURA_SKILL:
                {
                    foreach (var buffId in Packet._data.Effect)
                    {
                        //筛选出技能施法目标
                        Packet.TargetData = this.SendCommand(new QuerySkillTargets(Packet.caster, buffId));
                        this.SendCommand<AddSingleBuffToTargetCommand>(new AddSingleBuffToTargetCommand(Packet.TargetData, buffId, null));
                    }
                    SkillController skillController = DisplaySkill(Packet);
                    skillController.SetSkillPacket(Packet);
                }
                break;
            case SkillType.NORMAL_ATTACK:
            case SkillType.ACTIVE_SKILL:
                {
                    SkillController skillController = DisplaySkill(Packet);
                    skillController.SetSkillPacket(Packet);
                    Packet.CanRelease = false;//进入冷却了
                }
                break;
        }
    }

    SkillController DisplaySkill(SkillPacket Packet)
    {
        GameObject skill = null;
        if (resourcesMap.ContainsKey(Packet._data.Id))
        {
            skill = resourcesMap[Packet._data.Id];
        }
        else
        {
            ResLoader loader = ResLoader.Allocate();
            skill = loader.LoadSync<GameObject>(Packet._data.Prefab);
            loader.Recycle2Cache();

            resourcesMap.Add(Packet._data.Id, skill);
        }
        Vector3 point = Vector3.zero;
        if (Packet.target == null)
        {
            point = Packet.targetPoint;
        }
        else
        {
            if (Packet.caster != null)
            {
                point = Packet.caster.transform.Find("point").position;
            }
        }
        GameObject res = Object.Instantiate(skill, point, Quaternion.identity);
        SkillController skillController = res.GetComponent<QFramework.Game.SkillController>();

        switch(Packet._data.SkillType)
        {
            case SkillType.PASSIVE_SKILL:
            case SkillType.AURA_SKILL:
                res.transform.parent = Packet.caster.transform;
                res.transform.localPosition = Vector3.zero;
                break;
            case SkillType.NORMAL_ATTACK:
            case SkillType.ACTIVE_SKILL:

                break;
        }

        return skillController;
    }

    /// <summary>
    /// 创建法术牌 因为需要先确定位置  所以 所有法术牌都由统一的SkillController去生成投射物？
    /// </summary>
    public void ReleaseSpell(SkillPacket Packet)
    {
        Debug.Log($"释放法术牌 {Packet._data.Name} 对目标 {Packet.target} 位置 {Packet.targetPoint}");

        GameObject skill = null;
        if (resourcesMap.ContainsKey(Packet._data.Id))
        {
            skill = resourcesMap[Packet._data.Id];
        }
        else
        {
            ResLoader loader = ResLoader.Allocate();
            skill = loader.LoadSync<GameObject>(Packet._data.Prefab);
            loader.Recycle2Cache();

            resourcesMap.Add(Packet._data.Id, skill);
        }
        GameObject res = Object.Instantiate(skill, Packet.targetPoint, Quaternion.identity);
        res.GetComponent<QFramework.Game.SkillController>().SetSkillPacket(Packet);
    }

    /// <summary>
    /// 对一个列表进行筛选 根据tag
    /// </summary>
    /// <param name="objects"></param>
    /// <param name="tags"></param>
    /// <returns></returns>
    public List<GameObject> FindObjectsByTagsOptimized(List<GameObject> objects, List<string> tags)
    {
        //如果没有tags 直接返回所有  不检测
        if (tags == null || tags.Count == 0)
        {
            return objects;
        }
        foreach (var tag in tags)
        {
            var foundObjects = new List<GameObject>();
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].CompareTag(tag))
                {
                    foundObjects.Add(objects[i]);
                }
            }
            if (foundObjects.Count > 0)
            {
                return foundObjects;
            }
        }
        return null;
    }
    /// <summary>
    /// 对一个列表进行筛选 根据距离
    /// </summary>
    /// <param name="objects"></param>
    /// <param name="center"></param>
    /// <param name="maxDistance"></param>
    /// <returns></returns>
    public List<GameObject> FindObjectsByDistance(List<GameObject> objects, Vector3 center, float maxDistance = 9999)
    {
        //如果判断距离是0的话 直接返回全部 不进行距离判断  大概率是自己位置直接找个目标 方向之类的释放
        if (maxDistance <= 0f)
        {
            return objects;
        }
        var foundObjects = new List<GameObject>();

        foreach (var obj in objects)
        {
            if (obj == null) continue;

            //float distance = Vector3.Distance(obj.transform.position, center);
            float distance = Vector2.Distance(new Vector2(obj.transform.position.x, obj.transform.position.z), new Vector2(center.x, center.z));
            //Debug.Log($"FindObjectsByDistance {obj.name} {distance} {maxDistance} {distance <= maxDistance}");
            if (distance <= maxDistance)
            {
                foundObjects.Add(obj);
            }
        }

        return foundObjects.Count > 0 ? foundObjects : null;
    }

    /// <summary>
    /// 清理所有无效目标
    /// </summary>
    public void CleanInvalidTargets(List<GameObject> targetList)
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

    /// <summary>
    ///根据索敌偏好 选取目标
    /// </summary>
    public void FindTargetWithPreference()
    {

    }
}
