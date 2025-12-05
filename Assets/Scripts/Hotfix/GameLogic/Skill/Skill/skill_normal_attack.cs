using UnityEngine;
using QFramework;

namespace QFramework.Game
{
	/// <summary>
	/// 最基础的啥都没有的普通攻击  根据时间直接触发伤害就好了
	/// </summary>
	public partial class skill_normal_attack : SkillController
	{
		protected override void OnDo_Cast()
		{
			base.OnDo_Cast();
			if (packetData.target != null)
			{
				SkillKit.BeHurt(packetData, 10);
                packetData.caster.GetComponent<EntityController>().isRelease = false;// 暂时放这
            }
		}

	}
}
