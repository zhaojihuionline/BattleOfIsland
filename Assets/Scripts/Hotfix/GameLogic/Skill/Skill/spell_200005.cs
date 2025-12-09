using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework.Game;
using QFramework;
using DG.Tweening;
using System.Linq;

/// <summary>
/// 复苏术法术牌技能
/// </summary>
public partial class spell_200005 : SkillController
{
    protected override void OnDo_Cast()
    {
        base.OnDo_Cast();

        var heros = this.GetModel<BattleInModel>().player_allEntitys;
        // 查找heros中，血量最低的一名英雄
        var lowestHealthHero = heros.OrderBy(hero => hero.GetComponent<EntityController>().currentHP).FirstOrDefault();

        GameObject effect = Instantiate(Effect, lowestHealthHero.transform.position, Quaternion.identity);
        effect.transform.SetParent(lowestHealthHero.transform);
        effect.name = "spell_200005";
        effect.transform.localPosition = Vector3.zero;
        effect.transform.localScale = Vector3.one * 2.5f;
        effect.SetActive(true);

        // 延迟一段时间删除特效和还原模型大小
        DOVirtual.DelayedCall(1f, () =>
        {
            if (lowestHealthHero != null)
            {
                Destroy(effect);
                Destroy(this.gameObject);
            }
        });

        var targetData = TargetData.New();
        targetData.Target = lowestHealthHero;
        foreach (var effectIndex in packetData._data.Effect)
        {
            Debug.Log("复苏术法术施加后，添加了buff:" + effectIndex);
            this.SendCommand(new AddSingleBuffToTargetCommand(targetData, effectIndex, Effect));
        }
    }
}
