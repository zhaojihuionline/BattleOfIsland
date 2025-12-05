using QFramework;
using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        // TODO: 添加攻速增加10%的buff
    }
}
