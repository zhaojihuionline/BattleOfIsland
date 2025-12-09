using UnityEngine;

namespace QFramework.Game
{
    public partial class skill_10003 : SkillController
    {
        [Header("弹体设置")]
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private float projectileRange = 200f;
        [SerializeField] private int projectileCount = 3;
        [SerializeField] private float spreadAngle = 60f;

        void OnEnable()
        {
            Debug.Log($"skill_10003.OnEnable");
        }

        protected override void OnStart_Cast()
        {
            Debug.Log("skill_1000301 技能施法开始");

            // 获取目标方向（纯水平方向）
            Vector3 targetDirection = GetTargetDirection();

            // 生成多个方向的弹体
            GenerateProjectiles(targetDirection);

            base.OnStart_Cast();
        }

        private Vector3 GetTargetDirection()
        {
            Vector3 direction = Vector3.forward;

            if (packetData.targetPoint != Vector3.zero)
            {
                Vector3 toTarget = packetData.targetPoint - transform.position;
                direction = new Vector3(toTarget.x, 0, toTarget.z).normalized;
                Debug.Log($"使用目标点方向: {direction}");
            }
            else if (packetData.target != null)
            {
                Vector3 toTarget = packetData.target.transform.position - transform.position;
                direction = new Vector3(toTarget.x, 0, toTarget.z).normalized;
                Debug.Log($"使用目标对象方向: {direction}");
            }
            else
            {
                direction = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
                Debug.Log($"使用施法者朝向: {direction}");
            }

            return direction;
        }

        private void GenerateProjectiles(Vector3 baseDirection)
        {
            ResLoader loader = ResLoader.Allocate();

            // 确保基础方向是纯水平方向
            baseDirection = new Vector3(baseDirection.x, 0, baseDirection.z).normalized;

            float angleStep = projectileCount > 1 ? spreadAngle / (projectileCount - 1) : 0f;
            float startAngle = -spreadAngle / 2f;

            for (int i = 0; i < projectileCount; i++)
            {
                float currentAngle = startAngle + i * angleStep;
                GenerateProjectile(baseDirection, currentAngle, loader);
            }

            loader.Recycle2Cache();
        }

        private void GenerateProjectile(Vector3 baseDirection, float angle, ResLoader loader)
        {
            // 计算旋转后的方向 - 只在水平面上旋转
            Vector3 direction = Quaternion.Euler(0, angle, 0) * baseDirection;

            // 计算弹体的旋转（朝向目标方向）
            Quaternion rotation = Quaternion.LookRotation(direction);

            // 在自身位置生成，并设置正确的旋转
            skill_10003_item item = Instantiate(skill_10003_item, transform.position, rotation).GetComponent<skill_10003_item>();

            // 初始化弹体
            item.Init(packetData, direction, projectileSpeed, projectileRange);

            Debug.Log($"生成弹体，角度：{angle}°，方向：{direction}");
        }
    }
}