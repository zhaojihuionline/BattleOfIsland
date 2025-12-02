//using HutongGames.PlayMaker;
//using System.Collections.Generic;
//using UnityEngine;

//// 1. 暴露HeroEntity的canMove属性
//[ActionCategory("HeroEntity")]
//[HutongGames.PlayMaker.Tooltip("设置Hero是否可移动")]
//public class SetHeroCanMove : FsmStateAction
//{
//    [RequiredField]
//    [CheckForComponent(typeof(HeroEntity))]
//    public FsmOwnerDefault gameObject;

//    public FsmBool canMove;

//    public override void Reset()
//    {
//        gameObject = null;
//        canMove = false;
//    }

//    public override void OnEnter()
//    {
//        DoSetCanMove();
//        Finish();
//    }

//    void DoSetCanMove()
//    {
//        var go = Fsm.GetOwnerDefaultTarget(gameObject);
//        if (go == null) return;
//        var hero = go.GetComponent<HeroEntity>();
//        if (hero != null)
//        {
//            hero.canMove = canMove.Value;
//        }
//    }
//}

//// 2. 暴露HeroEntity的canAttack属性
//[ActionCategory("HeroEntity")]
//[HutongGames.PlayMaker.Tooltip("设置Hero是否可攻击")]
//public class SetHeroCanAttack : FsmStateAction
//{
//    [RequiredField]
//    [CheckForComponent(typeof(HeroEntity))]
//    public FsmOwnerDefault gameObject;

//    public FsmBool canAttack;

//    public override void Reset()
//    {
//        gameObject = null;
//        canAttack = false;
//    }

//    public override void OnEnter()
//    {
//        DoSetCanAttack();
//        Finish();
//    }

//    void DoSetCanAttack()
//    {
//        var go = Fsm.GetOwnerDefaultTarget(gameObject);
//        if (go == null) return;
//        var hero = go.GetComponent<HeroEntity>();
//        if (hero != null)
//        {
//            hero.canAttack = canAttack.Value;
//        }
//    }
//}

//// 3. 调用HeroEntity的FindClosedTarget方法
//[ActionCategory("HeroEntity")]
//[HutongGames.PlayMaker.Tooltip("执行寻找最近目标")]
//public class HeroFindClosedTarget : FsmStateAction
//{
//    [RequiredField]
//    [CheckForComponent(typeof(HeroEntity))]
//    public FsmOwnerDefault gameObject;

//    public override void Reset()
//    {
//        gameObject = null;
//    }

//    public override void OnEnter()
//    {
//        var go = Fsm.GetOwnerDefaultTarget(gameObject);
//        if (go == null) { Finish(); return; }
//        var hero = go.GetComponent<HeroEntity>();
//        if (hero != null)
//        {
//            var method = typeof(HeroEntity).GetMethod("FindClosedTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//            if (method != null)
//            {
//                method.Invoke(hero, null);
//            }
//        }
//        Finish();
//    }
//}

//// 4. 调用HeroEntity的AttackHandler方法
//[ActionCategory("HeroEntity")]
//[HutongGames.PlayMaker.Tooltip("执行攻击方法")]
//public class HeroAttackHandler : FsmStateAction
//{
//    [RequiredField]
//    [CheckForComponent(typeof(HeroEntity))]
//    public FsmOwnerDefault gameObject;

//    public override void Reset()
//    {
//        gameObject = null;
//    }

//    public override void OnEnter()
//    {
//        var go = Fsm.GetOwnerDefaultTarget(gameObject);
//        if (go == null) { Finish(); return; }
//        var hero = go.GetComponent<HeroEntity>();
//        if (hero != null)
//        {
//            var method = typeof(HeroEntity).GetMethod("AttackHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//            if (method != null)
//            {
//                method.Invoke(hero, null);
//            }
//        }
//        Finish();
//    }
//}



//[ActionCategory("HeroEntity")]
//[HutongGames.PlayMaker.Tooltip("英雄的buff执行器")]
//public class HeroBuffRunner : FsmStateAction
//{
//    [RequiredField]
//    [CheckForComponent(typeof(HeroEntity))]
//    public FsmOwnerDefault gameObject;
//    public override void Reset()
//    {
//        gameObject = null;
//    }
//    public override void OnEnter()
//    {
//        var go = Fsm.GetOwnerDefaultTarget(gameObject);
//        if (go == null) { Finish(); return; }
//        var hero = go.GetComponent<HeroEntity>();
//        if (hero != null)
//        {
//            var method = typeof(HeroEntity).GetMethod("InitBuffRunner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//            if (method != null)
//            {
//                method.Invoke(hero, null);
//            }
//        }
//        Finish();
//    }
//}


//[ActionCategory("HeroEntity")]
//public class SetFindEntityType:FsmStateAction
//{
//    [RequiredField]
//    [CheckForComponent(typeof(HeroEntity))]
//    public FsmOwnerDefault gameObject;
    
//    public cfg.Enum_BuildingType buildingType;
//    public override void Reset()
//    {
//        gameObject = null;
//        buildingType = cfg.Enum_BuildingType.defBuilding;
//    }
//    public override void OnEnter()
//    {
//        var go = Fsm.GetOwnerDefaultTarget(gameObject);
//        if (go == null) { Finish(); return; }
//        var hero = go.GetComponent<HeroEntity>();
//        if (hero != null)
//        {
//            hero.searchBuildingType = buildingType;
//        }
//        Finish();
//    }
//}