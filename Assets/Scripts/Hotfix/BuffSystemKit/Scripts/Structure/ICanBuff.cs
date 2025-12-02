using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// buff系统抽象接口
/// </summary>
public interface ICanBuff: UpgradeAttributes, IRecoverSpeed
{
    public BuffRunner buffRunner { get; set; }
}

/// <summary>
/// 要更新的属性接口
/// </summary>
public interface UpgradeAttributes
{
    /// <summary>
    /// 血量更新
    /// </summary>
    /// <param name="chanedValue"></param>
    void OnUpgradeBlood(float chanedValue);
    /// <summary>
    /// 攻击力更新
    /// </summary>
    /// <param name="changeValue"></param>
    void OnUpgradeAttack(float changeValue);
    /// <summary>
    /// 移动速度更新
    /// </summary>
    /// <param name="changeValue"></param>
    void OnUpgradeSpeed(float changeValue);
    /// <summary>
    /// 经验值更新
    /// </summary>
    /// <param name="changeValue"></param>
    void OnUpgradeExp(float changeValue);
}
