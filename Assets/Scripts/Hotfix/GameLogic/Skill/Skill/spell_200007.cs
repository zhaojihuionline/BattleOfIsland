using DG.Tweening;
using QFramework;
using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 流沙阵法术牌技能,特效暂时用瘴气代替
/// </summary>

public partial class spell_200007 : SkillController
{
    protected override void OnDo_Cast()
    {
        base.OnDo_Cast();

        this.GetModel<BattleInModel>().opponent_allEntitys.ForEach(entity =>
        {
            if (entity.transform.CompareTag("Tower"))
            {
                GameObject effect = Instantiate(Effect, entity.transform.position, Quaternion.identity);
                effect.transform.SetParent(entity.transform);
                effect.name = "spell_200007";
                effect.transform.localPosition = Vector3.zero;
                effect.transform.localScale = Vector3.one * 0.75f;
                effect.SetActive(true);
                // 给每个箭塔添加buff
                TargetData targetData = new TargetData() { Target = entity };
                this.SendCommand<AddSingleBuffToTargetCommand>(new AddSingleBuffToTargetCommand(targetData, packetData._data.Effect[0], effect));//这里的：packetData._data.Effect[0]就是BuffId是20294
            }
        });

        DOVirtual.DelayedCall(5.0f, () =>
        {
            Destroy(this.gameObject);
        });
    }
}
