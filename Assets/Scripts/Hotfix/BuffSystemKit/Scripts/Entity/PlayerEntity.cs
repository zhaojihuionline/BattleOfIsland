using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// buff系统使用测试代码，实际使用请让你的实体角色继承EntityCanBuff，并重写虚方法
/// </summary>
//public class PlayerEntity : EntityCanBuff
//{
//    public float hp;
//    public float attack;
//    public float speed;
//    public float exp;

//    public override BuffRunner buffRunner { get; set; }
//    public override bool IsEnemy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

//    public override void Init()
//    {
//        // 模拟此对象数据模型
//        hp = 100;
//        attack = 10;
//        speed = 10;
//        exp = 0;

//        InitBuffRunner();
//    }
//    /// <summary>
//    /// 初始化buff Runner
//    /// </summary>
//    protected override void InitBuffRunner()
//    {
//        // 初始化buff执行器
//        buffRunner = new BuffRunner();
//        buffRunner.Init();
//    }
//    public override void OnUpgradeBlood(float chanedValue)
//    {
//        hp += chanedValue;
//    }
//    public override void OnUpgradeAttack(float changeValue)
//    {
//        attack += changeValue;
//    }
//    public override void OnUpgradeSpeed(float changeValue)
//    {
//        speed += changeValue;
//    }

//    public override void OnUpgradeExp(float changeValue)
//    {
//        exp += changeValue;
//    }

//    public override void SetDefaultSpeed()
//    {
//        speed = 10;
//    }

//    public override void Hurt(Transform hitTarget, float v)
//    {

//    }
//}
