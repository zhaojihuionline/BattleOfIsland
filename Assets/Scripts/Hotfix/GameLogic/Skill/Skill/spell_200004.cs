using DG.Tweening;
using QFramework;
using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 醺酒法术牌技能
/// 持续3秒，所有友方单位移速降低20%，攻击力增加20%
/// </summary>
public partial class spell_200004 : SkillController
{
    protected override void OnDo_Cast()
    {
        base.OnDo_Cast();

        var heros = this.GetModel<BattleInModel>().player_allEntitys;
        foreach (var hero in heros)
        {
            GameObject effect = Instantiate(Effect, hero.transform.position, Quaternion.identity);
            effect.transform.SetParent(hero.transform);
            effect.name = "spell_200004";
            effect.transform.localPosition = Vector3.zero + new Vector3(0, 0.7f, 0);
            effect.transform.localScale = Vector3.one * 2.5f;
            effect.SetActive(true);

            // 延迟一段时间删除特效和还原模型大小
            DOVirtual.DelayedCall(3f, () =>
            {
                if (hero != null)
                {
                    Destroy(effect);
                    Destroy(this.gameObject);
                }
            });

            // TODO: 添加|攻击力增加20%|的buff
            // TODO: 添加|移速降低20%|的buff
            var targetData = TargetData.New();
            targetData.Target = hero;
            foreach (var effectIndex in packetData._data.Effect)
            {
                Debug.Log("醺酒法术施加后，添加了buff:" + effectIndex);
                this.SendCommand(new AddSingleBuffToTargetCommand(targetData, effectIndex, Effect));
            }
        }
    }
}
