using System;
using HutongGames.PlayMaker;
using UnityEngine;

namespace QFramework.Game
{
	public partial class SkillController : ViewController
	{
		protected SkillPacket packetData;
		//可能控制的动画组件
		protected Animator ctrlAnimator;

		// 当前播放的动画名称
		protected string currentAnimName = "";
		// 动画播放状态跟踪
		protected bool isAnimationPlaying = false;

		#region 调试相关字段
		[SerializeField]
		protected bool enableDebug = false;

		#endregion

		void Awake()
		{
			InitializeStates();
			if (enableDebug)
			{
				Debug.Log($"[SkillController] 技能控制器初始化完成，初始状态: {SkillStates.None}");
			}
		}

		protected virtual void InitializeStates()
		{
			FSM.AddState(SkillStates.None, new NoneState(FSM, this));
			FSM.AddState(SkillStates.Start_WindUp, new Start_WindUpState(FSM, this));
			FSM.AddState(SkillStates.Do_WindUp, new Do_WindUpState(FSM, this));
			FSM.AddState(SkillStates.End_WindUp, new End_WindUpState(FSM, this));
			FSM.AddState(SkillStates.Start_Charge, new Start_ChargeState(FSM, this));
			FSM.AddState(SkillStates.Do_Charge, new Do_ChargeState(FSM, this));
			FSM.AddState(SkillStates.End_Charge, new End_ChargeState(FSM, this));
			FSM.AddState(SkillStates.Start_Cast, new Start_CastState(FSM, this));
			FSM.AddState(SkillStates.Do_Cast, new Do_CastState(FSM, this));
			FSM.AddState(SkillStates.End_Cast, new End_CastState(FSM, this));
			FSM.AddState(SkillStates.Start_Recovery, new Start_RecoveryState(FSM, this));
			FSM.AddState(SkillStates.Do_Recovery, new Do_RecoveryState(FSM, this));
			FSM.AddState(SkillStates.End_Recovery, new End_RecoveryState(FSM, this));
		}

		/// <summary>
		/// 或许需要外部注入
		/// </summary>
		/// <param name="pdata"></param>
		public void SetSkillPacket(SkillPacket pdata)
		{
			packetData = pdata;
			ctrlAnimator = packetData != null ? packetData.caster?.GetComponentInChildren<Animator>() : null;
			FSM.StartState(SkillStates.None);
			if (enableDebug)
			{
				Debug.Log($"[SkillController] 设置技能数据包: {pdata._data?.Name ?? "Unknown"}");
			}
		}
		#region  外部调用虚函数

		public virtual void TriggerDamage()
		{
			//触发伤害的逻辑 

			//根据伤害范围 确定伤害目标  目标列表根据是不是敌人判断
		}

		public virtual void SetLockMethod(int i)
		{
			if (packetData._data.LockMethod == 1)
			{
				//如果是追踪的话  那就生成后一直追踪目标对象  目标点没有追踪一说
			}
		}


		#endregion

		#region 动画控制方法
		/// <summary>
		/// 播放动画，如果当前已经在播放相同的动画且未结束，则不做任何操作
		/// </summary>
		/// <param name="animName">动画名称</param>
		/// <param name="layer">动画层级</param>
		/// <returns>是否成功播放了动画</returns>
		protected bool PlayAnimationIfNeeded(string animName, bool replay = false, int layer = 0)
		{
			if (ctrlAnimator == null)
			{
				if (enableDebug)
				{
					Debug.LogWarning($"[SkillController] 无法播放动画: Animator为空");
				}
				return false;
			}

			// 如果当前正在播放相同的动画且未结束，则不重复播放
			if (currentAnimName == animName && IsAnimationPlaying(animName, layer) && !replay)
			{
				if (enableDebug)
				{
					Debug.Log($"[SkillController] 跳过动画播放: {animName} (已在播放中)");
				}
				return false;
			}

			// 播放新动画
			ctrlAnimator.Play(animName, layer);
			currentAnimName = animName;
			isAnimationPlaying = true;

			if (enableDebug)
			{
				Debug.Log($"[SkillController] 播放动画: {animName}");
			}

			return true;
		}

		/// <summary>
		/// 检查指定动画是否正在播放
		/// </summary>
		/// <param name="animName">动画名称</param>
		/// <param name="layer">动画层级</param>
		/// <returns>是否正在播放</returns>
		protected bool IsAnimationPlaying(string animName, int layer = 0)
		{
			if (ctrlAnimator == null) return false;

			AnimatorStateInfo stateInfo = ctrlAnimator.GetCurrentAnimatorStateInfo(layer);
			return stateInfo.IsName(animName) && stateInfo.normalizedTime < 1.0f;
		}

		/// <summary>
		/// 获取当前状态的动画名称
		/// </summary>
		/// <param name="state">技能状态</param>
		/// <returns>动画名称</returns>
		protected string GetAnimationNameForState(SkillStates state)
		{
			if (packetData == null || packetData._data == null || packetData._data.ReleaseTime == null)
				return "";

			int index = GetReleaseTimeIndex(state);
			if (index >= 0 && index < packetData._data.ReleaseTime.Count)
			{
				return packetData._data.ReleaseTime[index].Animator;
			}

			return "";
		}

		/// <summary>
		/// 获取状态对应的ReleaseTime索引
		/// </summary>
		/// <param name="state">技能状态</param>
		/// <returns>索引值</returns>
		private int GetReleaseTimeIndex(SkillStates state)
		{
			return state switch
			{
				SkillStates.Do_WindUp => 0,
				SkillStates.Do_Charge => 1,
				SkillStates.Do_Cast => 2,
				SkillStates.Do_Recovery => 3,
				_ => -1
			};
		}

		/// <summary>
		/// 获取状态对应的持续时间
		/// </summary>
		/// <param name="state">技能状态</param>
		/// <returns>持续时间</returns>
		protected float GetDurationForState(SkillStates state)
		{
			if (packetData == null || packetData._data == null || packetData._data.ReleaseTime == null)
				return 0f;

			int index = GetReleaseTimeIndex(state);
			if (index >= 0 && index < packetData._data.ReleaseTime.Count)
			{
				float duration = packetData._data.ReleaseTime[index].Time;
				if (enableDebug)
				{
					Debug.Log($"[SkillController] 状态 {state} 持续时间: {duration:F2}s");
				}
				return duration / 100f;
			}

			return 0f;
		}
		#endregion

		#region 状态机相关
		public FSM<SkillStates> FSM = new FSM<SkillStates>();

		/// <summary>
		/// 技能生命周期 状态机 
		/// </summary>
		public enum SkillStates
		{
			None,               // 测试状态
			Start_WindUp,        // 前摇时
			Do_WindUp,       // 前摇中
			End_WindUp,          // 前摇结束
			Start_Charge,        // 蓄力时
			Do_Charge,       // 蓄力中
			End_Charge,     // 蓄力完成
			Start_Cast,          // 释放时
			Do_Cast,         // 释放中
			End_Cast,       // 释放完成
			Start_Recovery,      // 后摇时
			Do_Recovery,     // 后摇中
			End_Recovery         // 后摇结束
		}

		// ActionKit延时工具方法
		protected void DelayChangeState(float delay, SkillStates nextState)
		{
			if (delay > 0)
			{
				if (enableDebug)
				{
					Debug.Log($"[SkillController] 设置延时切换: {delay:F2}s 后切换到 {nextState}");
				}

				ActionKit.Delay(delay, () =>
				{
					if (FSM.CurrentStateId != nextState)
					{
						if (enableDebug)
						{
							Debug.Log($"[SkillController] 执行延时切换: {FSM.CurrentStateId} -> {nextState}");
						}
						FSM.ChangeState(nextState);
					}
				}).Start(this);
			}
			else
			{
				//没有持续时间  直接切换状态 
				if (enableDebug)
				{
					Debug.Log($"[SkillController] 立即切换状态: {FSM.CurrentStateId} -> {nextState}");
				}
				FSM.ChangeState(nextState);
			}
		}

		#endregion

		#region 可重写的虚方法 - 供子类继承实现具体逻辑

		//查找目标
		//造成伤害

		protected virtual void OnNoneEnter() { FSM.ChangeState(SkillStates.Start_WindUp); }
		protected virtual void OnNoneExit() { }

		protected virtual void OnStart_WindUp()
		{
			if (packetData.caster != null)
			{
				packetData.caster.GetComponent<ICanSkill>().isRelease = true;
			}
			// 自动进入前摇中状态
			FSM.ChangeState(SkillStates.Do_WindUp);
		}
		protected virtual void OnStart_WindUpExit() { }

		protected virtual void OnDo_WindUp()
		{
			float duration = GetDurationForState(SkillStates.Do_WindUp);

			// 播放前摇动画（如果当前没有在播放相同的动画）
			string animName = GetAnimationNameForState(SkillStates.Do_WindUp);
			if (!string.IsNullOrEmpty(animName))
			{
				PlayAnimationIfNeeded(animName);
			}

			DelayChangeState(duration, SkillStates.End_WindUp);
		}
		protected virtual void OnDo_WindUpExit() { }

		protected virtual void OnEnd_WindUp()
		{ // 自动进入蓄力开始状态
			FSM.ChangeState(SkillStates.Start_Charge);
		}
		protected virtual void OnEnd_WindUpExit() { }

		protected virtual void OnStart_Charge()
		{// 自动进入蓄力中状态
			FSM.ChangeState(SkillStates.Do_Charge);
		}
		protected virtual void OnStart_ChargeExit() { }

		protected virtual void OnDo_Charge()
		{
			float duration = GetDurationForState(SkillStates.Do_Charge);

			// 播放蓄力动画（如果当前没有在播放相同的动画）
			string animName = GetAnimationNameForState(SkillStates.Do_Charge);
			if (!string.IsNullOrEmpty(animName))
			{
				PlayAnimationIfNeeded(animName);
			}

			// 根据packetData._data.ReleaseTime[1]延时进入蓄力完成状态
			DelayChangeState(duration, SkillStates.End_Charge);
		}
		protected virtual void OnDo_ChargeExit() { }

		protected virtual void OnEnd_Charge()
		{   // 自动进入释放开始状态
			FSM.ChangeState(SkillStates.Start_Cast);
		}
		protected virtual void OnEnd_ChargeExit() { }

		protected virtual void OnStart_Cast()
		{ // 自动进入释放中状态
			FSM.ChangeState(SkillStates.Do_Cast);
		}
		protected virtual void OnStart_CastExit() { }

		protected virtual void OnDo_Cast()
		{
			float duration = GetDurationForState(SkillStates.Do_Cast);

			// 播放释放动画（如果当前没有在播放相同的动画）
			string animName = GetAnimationNameForState(SkillStates.Do_Cast);
			if (!string.IsNullOrEmpty(animName))
			{
				PlayAnimationIfNeeded(animName, true);
			}

			// 根据packetData._data.ReleaseTime[2]延时进入释放完成状态
			DelayChangeState(duration, SkillStates.End_Cast);
		}
		protected virtual void OnDo_CastExit() { }

		protected virtual void OnEnd_Cast()
		{   // 自动进入后摇开始状态
			FSM.ChangeState(SkillStates.Start_Recovery);
		}
		protected virtual void OnEnd_CastExit() { }

		protected virtual void OnStart_Recovery()
		{// 自动进入后摇中状态
			FSM.ChangeState(SkillStates.Do_Recovery);
		}
		protected virtual void OnStart_RecoveryExit() { }

		protected virtual void OnDo_Recovery()
		{
			float duration = GetDurationForState(SkillStates.Do_Recovery);

			// 播放后摇动画（如果当前没有在播放相同的动画）
			string animName = GetAnimationNameForState(SkillStates.Do_Recovery);
			if (!string.IsNullOrEmpty(animName))
			{
				PlayAnimationIfNeeded(animName);
			}

			// 根据packetData._data.ReleaseTime[3]延时进入后摇结束状态
			DelayChangeState(duration, SkillStates.End_Recovery);
		}
		protected virtual void OnDo_RecoveryExit() { }

        protected bool KeepAlive { get; set; } = false;
        protected virtual void OnEnd_Recovery()
		{
			if (enableDebug)
			{
				Debug.Log($"[SkillController] 技能到此释放结束");
			}
			if (packetData.caster != null)
			{
				packetData.caster.GetComponent<ICanSkill>().isRelease = false;
			}

			if(!KeepAlive)
			Destroy(gameObject);
		}
		protected virtual void OnEnd_RecoveryExit() { }
		#endregion

		#region 基础状态类
		public abstract class SkillStateBase : AbstractState<SkillStates, SkillController>
		{
			protected SkillStateBase(FSM<SkillStates> fsm, SkillController target) : base(fsm, target) { }
		}

		// 瞬时状态基类
		public abstract class InstantSkillState : SkillStateBase
		{
			protected InstantSkillState(FSM<SkillStates> fsm, SkillController target) : base(fsm, target) { }
		}

		// 持续状态基类
		public abstract class DurationSkillState : SkillStateBase
		{
			protected DurationSkillState(FSM<SkillStates> fsm, SkillController target) : base(fsm, target) { }
		}
		#endregion

		public class NoneState : InstantSkillState
		{
			public NoneState(FSM<SkillStates> fsm, SkillController target) : base(fsm, target) { }

			protected override bool OnCondition() => mFSM.CurrentStateId != SkillStates.None;

			protected override void OnEnter()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] >>> 进入状态: {SkillStates.None}");
				}
				mTarget.OnNoneEnter();
			}

			protected override void OnExit()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] <<< 离开状态: {SkillStates.None}");
				}
				mTarget.OnNoneExit();
			}
		}

		public class Start_WindUpState : InstantSkillState
		{
			public Start_WindUpState(FSM<SkillStates> fsm, SkillController target) : base(fsm, target) { }

			protected override bool OnCondition() => mFSM.CurrentStateId != SkillStates.Start_WindUp;

			protected override void OnEnter()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] >>> 进入状态: {SkillStates.Start_WindUp}");
				}
				mTarget.OnStart_WindUp();

			}

			protected override void OnExit()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] <<< 离开状态: {SkillStates.Start_WindUp}");
				}
				mTarget.OnStart_WindUpExit();
			}
		}

		public class Do_WindUpState : DurationSkillState
		{
			public Do_WindUpState(FSM<SkillStates> fsm, SkillController target) : base(fsm, target) { }

			protected override bool OnCondition() => mFSM.CurrentStateId != SkillStates.Do_WindUp;

			protected override void OnEnter()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] >>> 进入状态: {SkillStates.Do_WindUp}");
				}
				mTarget.OnDo_WindUp();
			}

			protected override void OnExit()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] <<< 离开状态: {SkillStates.Do_WindUp}");
				}
				mTarget.OnDo_WindUpExit();
			}
		}

		public class End_WindUpState : InstantSkillState
		{
			public End_WindUpState(FSM<SkillStates> fsm, SkillController target) : base(fsm, target) { }

			protected override bool OnCondition() => mFSM.CurrentStateId != SkillStates.End_WindUp;

			protected override void OnEnter()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] >>> 进入状态: {SkillStates.End_WindUp}");
				}
				mTarget.OnEnd_WindUp();
			}

			protected override void OnExit()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] <<< 离开状态: {SkillStates.End_WindUp}");
				}
				mTarget.OnEnd_WindUpExit();
			}
		}

		public class Start_ChargeState : InstantSkillState
		{
			public Start_ChargeState(FSM<SkillStates> fsm, SkillController target) : base(fsm, target) { }

			protected override bool OnCondition() => mFSM.CurrentStateId != SkillStates.Start_Charge;

			protected override void OnEnter()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] >>> 进入状态: {SkillStates.Start_Charge}");
				}
				mTarget.OnStart_Charge();

			}

			protected override void OnExit()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] <<< 离开状态: {SkillStates.Start_Charge}");
				}
				mTarget.OnStart_ChargeExit();
			}
		}

		public class Do_ChargeState : DurationSkillState
		{
			public Do_ChargeState(FSM<SkillStates> fsm, SkillController target) : base(fsm, target) { }

			protected override bool OnCondition() => mFSM.CurrentStateId != SkillStates.Do_Charge;

			protected override void OnEnter()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] >>> 进入状态: {SkillStates.Do_Charge}");
				}

				mTarget.OnDo_Charge();

			}

			protected override void OnExit()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] <<< 离开状态: {SkillStates.Do_Charge}");
				}
				mTarget.OnDo_ChargeExit();
			}
		}

		public class End_ChargeState : InstantSkillState
		{
			public End_ChargeState(FSM<SkillStates> fsm, SkillController target) : base(fsm, target) { }

			protected override bool OnCondition() => mFSM.CurrentStateId != SkillStates.End_Charge;

			protected override void OnEnter()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] >>> 进入状态: {SkillStates.End_Charge}");
				}
				mTarget.OnEnd_Charge();

			}

			protected override void OnExit()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] <<< 离开状态: {SkillStates.End_Charge}");
				}
				mTarget.OnEnd_ChargeExit();
			}
		}

		public class Start_CastState : InstantSkillState
		{
			public Start_CastState(FSM<SkillStates> fsm, SkillController target) : base(fsm, target) { }

			protected override bool OnCondition() => mFSM.CurrentStateId != SkillStates.Start_Cast;

			protected override void OnEnter()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] >>> 进入状态: {SkillStates.Start_Cast}");
				}
				mTarget.OnStart_Cast();

			}

			protected override void OnExit()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] <<< 离开状态: {SkillStates.Start_Cast}");
				}
				mTarget.OnStart_CastExit();
			}
		}

		public class Do_CastState : DurationSkillState
		{
			public Do_CastState(FSM<SkillStates> fsm, SkillController target) : base(fsm, target) { }

			protected override bool OnCondition() => mFSM.CurrentStateId != SkillStates.Do_Cast;

			protected override void OnEnter()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] >>> 进入状态: {SkillStates.Do_Cast}");
				}

				mTarget.OnDo_Cast();

			}

			protected override void OnExit()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] <<< 离开状态: {SkillStates.Do_Cast}");
				}
				mTarget.OnDo_CastExit();
			}
		}

		public class End_CastState : InstantSkillState
		{
			public End_CastState(FSM<SkillStates> fsm, SkillController target) : base(fsm, target) { }

			protected override bool OnCondition() => mFSM.CurrentStateId != SkillStates.End_Cast;

			protected override void OnEnter()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] >>> 进入状态: {SkillStates.End_Cast}");
				}
				mTarget.OnEnd_Cast();

			}

			protected override void OnExit()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] <<< 离开状态: {SkillStates.End_Cast}");
				}
				mTarget.OnEnd_CastExit();
			}
		}

		public class Start_RecoveryState : InstantSkillState
		{
			public Start_RecoveryState(FSM<SkillStates> fsm, SkillController target) : base(fsm, target) { }

			protected override bool OnCondition() => mFSM.CurrentStateId != SkillStates.Start_Recovery;

			protected override void OnEnter()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] >>> 进入状态: {SkillStates.Start_Recovery}");
				}
				mTarget.OnStart_Recovery();

			}

			protected override void OnExit()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] <<< 离开状态: {SkillStates.Start_Recovery}");
				}
				mTarget.OnStart_RecoveryExit();
			}
		}

		public class Do_RecoveryState : DurationSkillState
		{
			public Do_RecoveryState(FSM<SkillStates> fsm, SkillController target) : base(fsm, target) { }

			protected override bool OnCondition() => mFSM.CurrentStateId != SkillStates.Do_Recovery;

			protected override void OnEnter()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] >>> 进入状态: {SkillStates.Do_Recovery}");
				}

				mTarget.OnDo_Recovery();

			}

			protected override void OnExit()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] <<< 离开状态: {SkillStates.Do_Recovery}");
				}
				mTarget.OnDo_RecoveryExit();
			}
		}

		public class End_RecoveryState : InstantSkillState
		{
			public End_RecoveryState(FSM<SkillStates> fsm, SkillController target) : base(fsm, target) { }

			protected override bool OnCondition() => mFSM.CurrentStateId != SkillStates.End_Recovery;

			protected override void OnEnter()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] >>> 进入状态: {SkillStates.End_Recovery}");
				}
				mTarget.OnEnd_Recovery();
			}

			protected override void OnExit()
			{
				if (mTarget.enableDebug)
				{
					Debug.Log($"[SkillController] <<< 离开状态: {SkillStates.End_Recovery}");
				}
				mTarget.OnEnd_RecoveryExit();
			}
		}
	}
}