using System;
using System.Collections;
using System.Collections.Generic;
using cfg;
using UnityEngine;

/// <summary>
/// 伤害包  暂时无需后端  简单版  无其它功能
/// </summary>
public class DamageDate
{
    /// <summary>
    /// 一个稍微特殊的伤害结构  可以把伤害根据不同类型分别装进map
    /// </summary>
    public Dictionary<HurtType, int> damageMap;

    public void Init()
    {
        damageMap = new Dictionary<HurtType, int>();
    }

    public int GetAllDamage()
    {
        int damage = 0;
        foreach (var item in damageMap)
        {
            damage += item.Value;
        }
        return damage;
    }

    public void SetDamage(HurtType type, int damage)
    {
        damageMap[type] = damage;
    }
}
