using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Buff
{
    /// <summary>
    /// buff id
    /// </summary>
    public int id;
    /// <summary>
    /// buff名称
    /// </summary>
    public string name;
    /// <summary>
    /// buff类型
    /// </summary>
    public bool isdebuff;
    /// <summary>
    /// buff效果类型
    /// </summary>
    public BuffKind buffKind;
    /// <summary>
    /// buff图标
    /// </summary>
    public Sprite selectedSprite;
    /// <summary>
    /// buff介绍
    /// </summary>
    public string des;
    /// <summary>
    /// 此buff的效果集合
    /// </summary>
    public List<Effect> effects;
    /// <summary>
    /// buff持续时间
    /// </summary>
    public float bDuration;
    /// <summary>
    /// 是否使用过
    /// </summary>
    public bool isUsed;
    /// <summary>
    /// 可以使用的次数
    /// </summary>
    public int UsedCount;
    /// <summary>
    /// 是否可叠加
    /// </summary>
    public bool isOverlay;
    /// <summary>
    /// 是否是常驻buff
    /// </summary>
    public bool isResident;

    public string buff_fxName;// buff特效名称

    public Buff(int id, string name, bool isdebuff, BuffKind buffKind, Sprite selectedSprite, string des, List<Effect> effects, float bDuration, bool isUsed, bool isOverlay, string buff_fxName)
    {
        this.id = id;
        this.name = name;
        this.isdebuff = isdebuff;
        this.buffKind = buffKind;
        this.selectedSprite = selectedSprite;
        this.des = des;
        this.effects = effects;
        this.bDuration = bDuration;
        this.isUsed = isUsed;
        this.isOverlay = isOverlay;
        this.buff_fxName = buff_fxName;
    }
}

[System.Serializable]
public class BuffProtocol {
    public int id;
    public string name;
    public int buffType;
    public int buffKind;
    public string des;
    public string effects;
    public float bDuration;
    public int isOverlay;
}
