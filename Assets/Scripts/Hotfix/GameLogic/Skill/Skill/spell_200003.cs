using DG.Tweening;
using QFramework;
using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 迷你蘑菇法术技能
/// </summary>
public partial class spell_200003 : SkillController
{
    protected override void OnDo_Cast()
    {
        base.OnDo_Cast();

        var hero = this.GetModel<BattleInModel>().player_allEntitys.Where(entity => entity.transform.CompareTag("Hero")).FirstOrDefault();
        hero.transform.Find("Model").localScale = Vector3.one * 0.5f;// 实体模型缩小一半
        GameObject effect = Instantiate(Effect, hero.transform.position, Quaternion.identity);
        effect.transform.SetParent(hero.transform);
        effect.name = "spell_200003";
        effect.transform.localPosition = Vector3.zero;
        effect.transform.localScale = Vector3.one * 1.75f;
        effect.SetActive(true);

        // 延迟一段时间删除特效和还原模型大小
        
        DOVirtual.DelayedCall(5f, () =>
        {
            if (hero != null)
            {
                hero.transform.Find("Model").localScale = Vector3.one; // 还原实体模型大小
                Destroy(effect);
                Destroy(this.gameObject);
            }
        });

        // TODO: 添加攻速增加10%的buff
    }
}
