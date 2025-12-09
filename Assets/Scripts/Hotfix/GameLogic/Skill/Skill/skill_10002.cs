using UnityEngine;

namespace QFramework.Game
{
    public partial class skill_10002 : SkillController
    {
        [SerializeField] GameObject effect;
        protected override void OnStart_WindUp()
        {
            base.OnStart_WindUp();
            Model.SetActive(false);
        }
        protected override void OnStart_Cast()
        {
            base.OnStart_Cast();
            Model.SetActive(true);
            packetData.CanRelease = false;

            if (BPath == null || packetData.target == null) return;
            // 设置事件回调
            BPath.OnMoveComplete += OnMoveComplete;

            Debug.Log("skill_1000201 技能施法开始");

            BPath.DOTweenMovePath(packetData.target.transform,false);

        }

        void OnMoveComplete()
        {
            Debug.Log("移动完成! skill_1000201");
            SkillKit.BeHurt(packetData, 10);
            packetData.caster.GetComponent<EntityController>().isRelease = false;// 暂时放这

            FX_Skill_Oliver.transform.position = packetData.target.transform.position;
            FX_Skill_Oliver.SetActive(true);
            ActionKit.Delay(2, () =>
            {
                Cleanup();
            }).Start(this);
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
            Debug.Log($"skill_10002.OnDestroy");
            if (BPath != null)
            {
                BPath.OnMoveComplete -= OnMoveComplete;
            }
        }

        private void OnEnable()
        {
            Debug.Log($"skill_10002.OnEnable");
        }

        private void OnDisable()
        {
            Debug.Log($"skill_10002.OnDisable");
        }


    }
}