using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这是一个实体
/// </summary>
public interface IEntity { }

/// <summary>
/// 不管是角色实体还是建筑  只要是能放技能  而且可以收到伤害的  都要继承这个接口
/// </summary>
public interface IRoleEntity : IBuildEntity, ICanSendEvent
{

}
public interface IBuildEntity : ICanSkill, ICanHurt, IHaveHP
{
    /// <summary>
    /// 是不是敌方单位
    /// </summary>
    /// <returns></returns>
    bool isEnemy { get; set; }
    Transform myTransform { get; }
    Animator GetAnimator();
}

public interface ICanMove : IEntity
{
    bool isMoveing { get; set; }
    float movespeed { get; set; }
    Transform myTransform { get; }
    void Move(Transform target);
    void Move(Vector3 target);
    void StopMove();
}

public interface ICanSkill : IEntity
{
    /// <summary>
    /// 是否正在释放中
    /// </summary>
    /// <returns></returns>
    bool isRelease { get; set; }
    /// <summary>
    /// 普攻数据包
    /// </summary>
    /// <returns></returns>
    SkillPacket nomalAttackPacket { get; set; }
    /// <summary>
    /// 技能列表
    /// 服务器读取 或者 本地加载
    /// </summary>
    /// <returns></returns>
    List<int> skills { get; set; }
    //默认初始化方法
    /// <summary>
    /// 
    /// </summary>
    /// <param name="skills">传入技能id列表</param>
    void InitCanSkill(List<int> skillsParam, List<int> skillEnable = null);
}

public interface IHaveHP : IEntity
{
    int currentHP { get; set; }
    int HPMAX { get; set; }
    bool IsAlive { get; set; }
    float HealthPercent { get; }
    //默认初始化方法
    //void InitHaveHp(int hpMax);
}
public interface ICanHurt : IEntity
{
    void BeHurt(int demage);
}