using UnityEngine;

namespace QFramework.Game
{
    public partial class skill_10004 : SkillController
    {
        [Header("弹体设置")]
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private float projectileRange = 200f;
        [SerializeField] private int projectileCount = 3;
        [SerializeField] private float spreadAngle = 60f;

        void OnEnable()
        {
            Debug.Log($"skill_10004.OnEnable");
        }

        protected override void OnStart_Cast()
        {
            Debug.Log("skill_1000401 技能施法开始");

            foreach (var buffId in packetData._data.Effect)
            {
                 packetData.TargetData = this.SendCommand(new QuerySkillTargets(packetData.caster, buffId));
                this.SendCommand<AddSingleBuffToTargetCommand>(new AddSingleBuffToTargetCommand(packetData.TargetData, buffId, null));
            }

            base.OnStart_Cast();
        }

    }
}