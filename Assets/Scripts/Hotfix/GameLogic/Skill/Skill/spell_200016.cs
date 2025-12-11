using DG.Tweening;
using QFramework;
using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

/// <summary>
/// 暴风雪法术牌技能
/// </summary>
public partial class spell_200016 : SkillController
{
    protected override void OnDo_Cast()
    {
        base.OnDo_Cast();
        // 砸落特效
        GameObject effect = Instantiate(Effect, new Vector3(0, -5, 8), Quaternion.identity);
        effect.name = "spell_200016";
        effect.transform.localScale = Vector3.one;
        effect.SetActive(true);

        DOVirtual.DelayedCall(1f, () =>
        {
            // 添加buff
            this.GetModel<BattleInModel>().opponent_allEntitys.ForEach(entity =>
            {
                if (entity.transform.CompareTag("Tower"))
                {
                    // 给每个建筑添加buff
                    TargetData targetData = new TargetData() { Target = entity };
                    this.SendCommand<AddSingleBuffToTargetCommand>(new AddSingleBuffToTargetCommand(targetData, packetData._data.Effect[0], effect));//这里的：packetData._data.Effect[0]就是BuffId是20294
                }
            });
        });

        DOVirtual.DelayedCall(5.0f, () =>
        {
            Destroy(effect);
            Destroy(this.gameObject);
        });
    }
}