using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using cfg;

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
public class SkillSystem : AbstractSystem, ISkillSystem
{
    public Dictionary<int, GameObject> resourcesMap = new Dictionary<int, GameObject>();
    protected override void OnInit()
    {
        ResKit.Init();
    }

    /// <summary>
    /// 创建技能
    /// </summary>
    public void ReleaseSkill(SkillPacket Packet)
    {
        Debug.Log($"释放技能 {Packet._data.Name} 对目标 {Packet.target} 位置 {Packet.targetPoint}");

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
        res.GetComponent<QFramework.Game.SkillController>().SetSkillPacket(Packet);
        Packet.CanRelease = false;//进入冷却了
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
    public List<GameObject> FindObjectsByDistance(List<GameObject> objects, Vector3 center, float maxDistance)
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
