using QFramework;
using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 英雄 巴顿 的盾之领袖被动技能
/// </summary>
public class skill_10016 : SkillController
{
    protected override void OnDo_Cast()
    {
        base.OnDo_Cast();
        Debug.Log("盾之领袖：为友方盾兵增加x点防御力");
        // 查找我方所有盾兵
        //var hero = this.GetModel<BattleInModel>().player_allEntitys.Where(entity => entity.GetComponent<EntityController>().UnitData.).FirstOrDefault();
        
    }
}
