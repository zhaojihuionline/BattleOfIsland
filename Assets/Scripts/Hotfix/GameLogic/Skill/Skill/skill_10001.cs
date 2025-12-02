using UnityEngine;
using QFramework;

namespace QFramework.Game
{
	/// <summary>
	/// 按条件选择目标 单体回血
	/// </summary>
	public partial class skill_10001 : SkillController
	{
		void Start()
		{
			// Code Here
		}

		protected override void OnDo_Cast()
		{
			base.OnDo_Cast();
			Transform res = packetData.target.transform;
			if (res != null)
			{
				GameObject effect = Instantiate(Effect, res.position, Quaternion.identity);
				effect.SetActive(true);
			}
			IHaveHP ch = packetData.target.GetComponent<IHaveHP>();
			ch.currentHP += ch.HPMAX * packetData._data.Effect[0];
		}

	}
}
