using UnityEngine;
using QFramework;

namespace QFramework.Game
{
	public partial class spell_200001 : SkillController
	{
		void Start()
		{
			// Code Here
		}
        protected override void OnDo_Cast()
        {
            base.OnDo_Cast();
            // 使用索敌规则，找所有敌方箭塔
            // 临时使用查找所有敌方建筑标签为Tower的物体
            this.GetModel<BattleInModel>().opponent_allEntitys.ForEach(entity =>
            {
                if (entity.transform.CompareTag("Tower"))
                {
                    GameObject effect = Instantiate(Effect, entity.transform.position, Quaternion.identity);
                    effect.transform.SetParent(entity.transform);
                    effect.name = "spell_200001";
                    effect.transform.localPosition = Vector3.zero;
                    effect.transform.localScale = Vector3.one * 1.75f;
                    effect.SetActive(true);
                    // 给每个箭塔添加buff
                    TargetData targetData = new TargetData() { Target = entity };
                    this.SendCommand<AddSingleBuffToTargetCommand>(new AddSingleBuffToTargetCommand(targetData, packetData._data.Effect[0], effect));//这里的：packetData._data.Effect[0]就是BuffId是20294
                }
            });
        }
	}
}
