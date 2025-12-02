using UnityEngine;
using DG.Tweening;

namespace QFramework.Game
{
	public partial class skill_10003_item : ViewController
	{
		SkillPacket packet;
		private Tween moveTween;

		public void Init(SkillPacket packet, Vector3 direction, float speed, float range)
		{
			this.packet = packet;
			Move(direction, speed, range);
		}

		public void Move(Vector3 direction, float speed, float range)
		{
			// 计算移动时间：时间 = 距离 / 速度
			float duration = range / speed;

			// 按速度移动
			Vector3 targetPosition = transform.position + direction * range;

			moveTween = transform.DOMove(targetPosition, duration)
				.SetEase(Ease.Linear)
				.OnComplete(() =>
				{
					Debug.Log("弹体移动完成");
					// 使用ActionKit延迟5帧销毁
					this.Delay(0.5f, DestroyGameObject);
				});
		}

		private void DestroyGameObject()
		{
			moveTween?.Kill();
			Destroy(gameObject);
		}

		public void OnTriggerEnter(Collider other)
		{
			//具体碰撞策略  需要根据tag和layer进行判断 这个会进行配置 跟索敌层级应该是一致的
			Debug.Log("skill_1000301_item 碰撞到 " + other.gameObject.name);
			if (packet.target != null)
			{
				packet.target.GetComponent<IRoleEntity>().BeHurt(100);

			}
		}

		void OnDestroy()
		{
			moveTween?.Kill();
		}
	}
}