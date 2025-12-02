using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可移动行为基类
/// </summary>
public class BaseMovement : MonoBehaviour
{
    public GameObject selfObject;
    public GameObject targetObject;
    /// <summary>
    /// 移动速度
    /// </summary>
    public float Speed { get; set; }
    /// <summary>
    /// 飞行结束后造成的伤害值
    /// </summary>
    public float CastDamage { get; set; }
    /// <summary>
    /// 是否可以移动/飞行 
    /// </summary>
    public bool CanMove { get; set; }
    public virtual void Update()
    {

    }

    public virtual void SetCanFly()
    {

    }

    public virtual void SetDamage(float _damage)
    {
        CastDamage = _damage;
    }
}
