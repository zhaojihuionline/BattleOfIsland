using UnityEngine;
using QFramework;
using System.Collections.Generic;
using System;

namespace QFramework.Game
{
	public partial class spell_200018 : SkillController
	{
		protected override void OnStart_Charge()
		{
			base.OnStart_Charge();
		}

		private void Do()
		{
			BattleInModel model = this.GetModel<BattleInModel>();
			SkillSystem system = this.GetSystem<SkillSystem>();

			var res = this.SendCommand(new FindTargetCommand(model.opponent_allEntitys, packetData._data.TagMask, packetData._data.CastRanage, packetData._data.Preference, null));
			if (res != null)
			{
				GameObject effect = Instantiate(Effect, res.transform.position, Quaternion.identity);
				effect.SetActive(true);
                //实际触发伤害可能会稍微延迟一点
                ActionKit.Delay(0.1f, () =>
                {
                    int buffId = packetData._data.Effect[0];
					this.SendCommand(new AddSingleBuffToTargetCommand(res.transform, buffId, effect));// 添加雷霆伤害技能对应的buff
                                                                                               //res.GetComponent<ICanHurt>().BeHurt(packetData.damageDate.GetAllDamage());
                }).Start(this);
            }
		}

		protected override void OnDo_Cast()
		{
			base.OnDo_Cast();
			Do();
			//根据持续时间 每0.5s触发一次雷霆伤害
			ActionKit.Repeat()
			.Delay(0.3f)
			.Callback(() =>
			{
				Do();
			})
			.Start(this);
		}

		protected override void OnEnd_Recovery()
		{
			base.OnEnd_Recovery();
		}
	}
}
