using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowEntity : MonoBehaviour
{
    private void Awake()
    {
    }
    /// <summary>
    /// 设置箭飞行到目标
    /// </summary>
    /// <param name="toTarget">要飞到的目标</param>
    public void Run(Transform castTarget,Transform toTarget)
    {
        //BezierMovement bezierMovement = gameObject.AddComponent<BezierMovement>();
        //bezierMovement.selfObject = gameObject;
        //bezierMovement.targetObject = toTarget.gameObject;
        //bezierMovement.SetDamage(castTarget.GetComponent<HeroEntity>().runTimeheroData.attackDamage);
        //bezierMovement.SetBezier();
        //bezierMovement.SetCanFly();
    }
}
