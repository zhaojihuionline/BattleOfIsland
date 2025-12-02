using QFramework;
using QFramework.Game;
using System.Collections.Generic;
using UnityEngine;

public class GlobalBuffRunnerSystem : AbstractSystem
{
    public Dictionary<GameObject, BuffRunner> buffRunners;
    protected override void OnInit()
    {
        buffRunners = new Dictionary<GameObject, BuffRunner>();
    }

    /// <summary>
    /// 新实体创建时注册到全局buff系统中
    /// </summary>
    /// <param name="entity"></param>
    public void RegisterNewEntity(GameObject entity)
    {
        if (!buffRunners.ContainsKey(entity))
        {
            buffRunners.Add(entity, entity.GetComponent<EntityController>().buffRunner);
        }
    }

    /// <summary>
    /// 每次有一个新的实体加入战斗时让新实体知道游戏中已有的正在执行的buff
    /// </summary>
    /// <param name="newEntity"></param>
    public void NotifyExistingBuffsToNewEntity(GameObject newEntity)
    {
        foreach (var kvp in buffRunners)// 遍历已有的实体和它们的buff执行器
        {
            if(kvp.Key == null) continue;
            EntityController existingEntity = kvp.Key.GetComponent<EntityController>();
            BuffRunner existingBuffRunner = kvp.Value;
            if (existingEntity != newEntity && existingBuffRunner.buffEntities.Count > 0 && existingBuffRunner.buffEntities != null)// 排除自己
            {
                foreach (var buffEntity in existingBuffRunner.buffEntities)// 遍历已有实体的buff实体
                {
                    if(buffEntity != null)
                    {
                        newEntity.GetComponent<EntityController>().buffRunner.GiveBuff(newEntity.transform, buffEntity.buff.id);
                        // 如果是立即执行buff，需要立即执行
                        newEntity.GetComponent<EntityController>().buffRunner.ExecuteBuffs();
                    }
                }
            }
        }
    }

    protected override void OnDeinit()
    {
        buffRunners.Clear();
    }
}