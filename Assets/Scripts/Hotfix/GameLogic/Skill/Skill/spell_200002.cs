using DG.Tweening;
using QFramework;
using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

/// <summary>
/// 先祖教诲法术牌技能
/// </summary>
public partial class spell_200002 : SkillController
{
    protected override void OnDo_Cast()
    {
        base.OnDo_Cast();

        // 查找场上战斗力最高的英雄
        var hero = this.GetModel<BattleInModel>().player_allEntitys.Where(entity => entity.transform.CompareTag("Hero")).FirstOrDefault();

        GameObject effect = Instantiate(Effect, hero.transform.position, Quaternion.identity);
        effect.transform.SetParent(hero.transform);
        effect.name = "spell_200002";
        effect.transform.localPosition = Vector3.zero;
        effect.transform.localScale = Vector3.one * 1.75f;
        effect.SetActive(true);

        // 延迟一段时间删除特效，暂时写在这里，后续优化
        DOVirtual.DelayedCall(5f, () =>
        {
            if (hero != null)
            {
                Destroy(effect);
                Destroy(this.gameObject);
            }
        });

        // TODO: 添加buff
    }
}
