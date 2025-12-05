using cfg;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Effect
{
    /// <summary>
    /// 此效果要改变的属性id。例如：id为0改变的是对象的血量
    /// </summary>
    public int attributeID;
    /// <summary>
    /// 此效果具体要改变的属性的值
    /// </summary>
    public int attributeValue;
    public cfg.EffectNode effectNode;
    public BuffTable buffTable;
    /// <summary>
    /// 此效果的持续时间，正常情况下直接取所依附的buff持续时间
    /// </summary>
    public float attributeDuration;

    public Effect(int attributeID, int attributeValue)
    {
        this.attributeID = attributeID;
        this.attributeValue = attributeValue;
    }

    public Effect(BuffTable buffTable,int i)
    {
        this.buffTable = buffTable;
        effectNode = buffTable.BuffEffect[i];
        this.attributeID = (int)effectNode.Type;
        this.effectNode = effectNode;
        this.attributeValue = effectNode.Param[1];
    }
}
