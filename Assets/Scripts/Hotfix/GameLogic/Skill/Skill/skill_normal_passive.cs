using UnityEngine;
using QFramework;

namespace QFramework.Game
{
	//基础 直接触发 被动技能
	public partial class skill_normal_passive : SkillController
	{
		protected override void OnDo_Cast()
		{
			base.OnDo_Cast();
			//或许可能有释放动画  应该循环触发所有的  现只有一个  目标选择逻辑待补全
			//直接触发一个Buff
			this.SendCommand<AddSingleBuffToTargetCommand>(new AddSingleBuffToTargetCommand(packetData.TargetData, packetData._data.Effect[0], null));
		}
	}
}
