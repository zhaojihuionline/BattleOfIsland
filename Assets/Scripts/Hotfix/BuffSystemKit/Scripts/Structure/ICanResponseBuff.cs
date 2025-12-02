using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 是否可以响应Buff
/// </summary>
public interface ICanResponseBuff
{
    public bool IsEnemy { get; set; }
    /// <summary>
    /// 伤害减免中......
    /// </summary>
    public bool Defense_PercentReduction_all { get; set; }
    /// <summary>
    /// Buff执行器
    /// </summary>
    public BuffRunner buffRunner { get; set; }
    /// <summary>
    /// 血量更新
    /// </summary>
    /// <param name="chanedValue"></param>
    void OnUpgradeBlood(float chanedValue) { }
    /// <summary>
    /// 攻击力更新
    /// </summary>
    /// <param name="changeValue"></param>
    void OnUpgradeAttack(float changeValue) { }
    /// <summary>
    /// 攻击速度更新
    /// </summary>
    /// <param name="changeValue"></param>
    void OnUpgradeAttackSpeed(float changeValue) { }
    /// <summary>
    /// 移动速度更新
    /// </summary>
    /// <param name="changeValue"></param>
    void OnUpgradeSpeed(float changeValue) { }
    /// <summary>
    /// 经验值更新
    /// </summary>
    /// <param name="changeValue"></param>
    void OnUpgradeExp(float changeValue) { }
    /// <summary>
    /// 恢复速度
    /// </summary>
    void SetDefaultSpeed() { }

    void SetDefense_PercentReduction_all(int chanveValue) { }

    void SetDefenseDown_Percent(int chanveValue) { }
}
