using UnityEngine;
using QFramework;
using System.Collections.Generic;
using System;
using System.Linq;

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

			// 从model.opponent_allEntitys中，随机取一个目标
			var targetObj = model.opponent_allEntitys[UnityEngine.Random.Range(0, model.opponent_allEntitys.Count)];
			if (targetObj != null)
			{
                var target = targetObj.GetComponent<EntityController>();

                if (target != null)
                {
                    var res = this.SendCommand(new FindTargetCommand(model.opponent_allEntitys, packetData._data, target));

                    if (res.Target != null)
                    {
                        GameObject effect = Instantiate(Effect, res.Target.transform.position, Quaternion.identity);
                        effect.SetActive(true);
                        //实际触发伤害可能会稍微延迟一点
                        ActionKit.Delay(0.1f, () =>
                        {
                            int buffId = packetData._data.Effect[0];
                            this.SendCommand(new AddSingleBuffToTargetCommand(new TargetData { Target = res.Target }, buffId, effect));// 添加雷霆伤害技能对应的buff
                                                                                                                                       //res.GetComponent<ICanHurt>().BeHurt(packetData.damageDate.GetAllDamage());
                        }).Start(this);
                    }
                }
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
