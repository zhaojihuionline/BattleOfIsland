using DG.Tweening;
using UnityEngine;

namespace QFramework.Game
{
    //基础普通射击 改预制体特效就可以
    public partial class skill_normal_shoot : SkillController
    {
        protected override void OnStart_WindUp()
        {
            base.OnStart_WindUp();
            Model.SetActive(false);
        }
        protected override void OnStart_Cast()
        {
            base.OnStart_Cast();
            Model.SetActive(true);
            packetData._canRelease = false;

            if (BPath == null || packetData.target == null) return;
            // 设置事件回调
            BPath.OnMoveComplete += OnMoveComplete;

            Debug.Log("skill_9000201 技能施法开始");

            BPath.DOTweenMovePath(packetData.target.transform);

        }

        void OnMoveComplete()
        {
            Debug.Log("移动完成!");
            packetData.target.GetComponent<ICanHurt>().BeHurt(10);
            packetData.caster.GetComponent<EntityController>().isRelease = false;// 暂时放这
            Cleanup();
        }

        void Cleanup()
        {
            if (BPath != null)
            {
                BPath.OnMoveComplete -= OnMoveComplete;
            }
            Destroy(gameObject);
        }

        protected override void OnEnd_Recovery()
        {
            if (enableDebug)
            {
                Debug.Log($"[SkillController] 技能到此释放结束");
            }
            //packetData.caster.GetComponent<EntityController>().isRelease = false;
        }

        void OnDestroy()
        {
            if (BPath != null)
            {
                BPath.OnMoveComplete -= OnMoveComplete;
            }
        }
    }
}