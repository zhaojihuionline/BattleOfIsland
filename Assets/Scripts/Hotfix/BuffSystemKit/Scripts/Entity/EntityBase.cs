using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 支持buff系统的实体
/// </summary>
//public abstract class EntityCanBuff : EntityBase, ICanResponseBuff, ICanResponseEffect
//{
//    /// <summary>
//    /// buff执行器
//    /// </summary>
//    public abstract BuffRunner buffRunner { get; set; }
//    /// <summary>
//    /// 初始化buff执行器
//    /// </summary>
//    protected abstract void InitBuffRunner();
//    /// <summary>
//    /// 血量更新
//    /// </summary>
//    /// <param name="chanedValue"></param>
//    public abstract void OnUpgradeBlood(float chanedValue);
//    /// <summary>
//    /// 攻击力更新
//    /// </summary>
//    /// <param name="changeValue"></param>
//    public abstract void OnUpgradeAttack(float changeValue);
//    /// <summary>
//    /// 移动速度更新
//    /// </summary>
//    /// <param name="changeValue"></param>
//    public abstract void OnUpgradeSpeed(float changeValue);
//    /// <summary>
//    /// 经验值更新
//    /// </summary>
//    /// <param name="changeValue"></param>
//    public abstract void OnUpgradeExp(float changeValue);
//    /// <summary>
//    /// 移动速度恢复
//    /// </summary>
//    public abstract void SetDefaultSpeed();
//}
/// <summary>
/// 普通实体基类
/// </summary>
//public abstract class EntityBase : MonoBehaviour, IController
//{
//    public abstract bool IsEnemy { get; set; }
//    public abstract void Init();
//    public abstract void Hurt(Transform hitTarget, float v);
//    public IArchitecture GetArchitecture()
//    {
//        return GameApp.Interface;
//    }
//}