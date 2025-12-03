using cfg;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QFramework.Game
{
	public partial class EntityController : ViewController, IRoleEntity
	{
		// 动画配置字典，可以配置特殊动画名称
		private Dictionary<EntityState, string> animationMap = new Dictionary<EntityState, string>()
		{
			{ EntityState.Idle, "idle" },
			{ EntityState.MoveToTarget, "walk" },
			{ EntityState.Attacking, "attack" },
			{ EntityState.Die, "death" }
		};
		public bool isEnemy { get; set; }
		public float readonlyMoveSpeed = 10f;
		public float MoveSpeed = 10f;
		public float rotateSpeed = 60f;
		public List<GameObject> targetList = new List<GameObject>();
		Animator animator;
		#region   血量受伤相关
		public int currentHP { get; set; }
		public int HPMAX { get; set; }
		public bool IsAlive { get; set; }
		public Transform myTransform => transform;
		public float HealthPercent => currentHP / HPMAX * 100f;

		#endregion


		#region   技能相关
		public bool isRelease { get; set; }
		public Dictionary<int, SkillPacket> skillPacketDict { get; set; }
		public SkillPacket nomalAttackPacket { get; set; }
		public List<int> skills { get; set; }
		public bool IsEnemy { get; set; }
		public BuffRunner buffRunner { get; set; }

		//public EntityAttributeModel entityAttribute { get; set; }
        public bool Defense_PercentReduction_all { get; set; }
		public int Defense_PercentReductionValue = 0;
		#endregion


		#region 状态钩子（子类可重写）
		// 将状态机的进入/退出/更新行为委托到这些虚方法，子类可重写实现自定义逻辑
		protected virtual void OnIdleEnter()
		{
			PlayStateAnimation(EntityState.Idle);
		}
		protected virtual void OnIdleExit() { }
		protected virtual void OnIdleUpdate() { }

		protected virtual void OnMoveToTargetEnter()
		{
			PlayStateAnimation(EntityState.MoveToTarget);
		}
		protected virtual void OnMoveToTargetExit()
		{
			StopMove();
		}
		protected virtual void OnMoveToTargetUpdate() { }

		protected virtual void OnAttackingEnter()
		{
			// 保持原有默认行为（原来在状态中播放Idle）
			PlayStateAnimation(EntityState.Idle);
		}
		protected virtual void OnAttackingExit() { }
		protected virtual void OnAttackingUpdate() { }

		protected virtual void OnDieEnter()
		{
			this.SendCommand(new RemoveEntityFromBattleModelCommand(this.gameObject, this.IsEnemy));
			this.SendCommand(new EntityDeathCommand(null, this.gameObject));

			if (this.transform.CompareTag("Build") || this.transform.CompareTag("Tower"))
			{
				// 生成摧毁特效，暂时写在这里
				ResLoader loader = ResLoader.Allocate();
				GameObject destroyFXPrefab = loader.LoadSync<GameObject>("FX_build_cuihui");
				var newFX = Instantiate(destroyFXPrefab, this.transform.position + new Vector3(0, 1, 0), Quaternion.identity);
				Destroy(newFX, 1f);
				loader.Recycle2Cache();
			}
			Destroy(gameObject);
		}
		protected virtual void OnDieExit() { }
		protected virtual void OnDieUpdate() { }
		#endregion

		void Awake()
		{
			animator = GetComponentInChildren<Animator>();
		}

		void Start()
		{
			FSM.AddState(EntityState.Idle, new IdleState(FSM, this));
			FSM.AddState(EntityState.MoveToTarget, new MoveToTargetState(FSM, this));
			FSM.AddState(EntityState.Attacking, new AttackingState(FSM, this));
			FSM.AddState(EntityState.Die, new DieState(FSM, this)); // 修正为DieState

			FSM.StartState(EntityState.Idle);
		}

		public void InitCanSkill(List<int> skillsParam,List<int> skillEnable = null)
		{
            skills = skillsParam;
			isRelease = false;
			skillPacketDict = new Dictionary<int, SkillPacket>();

			//直接根据当前技能列表生成
			for (int i = 0; i < skills.Count; i++)
			//foreach (var skillID in skills)
			{
				int skillID = skills[i];
				SkillTable table = CfgMgr.GetSkillTableS(skillID);
				SkillPacket skillPacket = new SkillPacket(table, this.GetModel<BattleInModel>(), skillEnable != null ? skillEnable[i] : 0);
				skillPacketDict.Add(skillID, skillPacket);

				skillPacketDict = skillPacketDict
					.OrderBy(kv => GetSortOrder(kv.Value._data.SkillType))
					.ThenBy(kv => kv.Key)  // 如果枚举值相同，再按key排序
					.ToDictionary(kv => kv.Key, kv => kv.Value);

				//如果是有冷却时间的  现在先用普攻和主动技能进行判断
				if (table.SkillType == SkillType.NORMAL_ATTACK || table.SkillType == SkillType.ACTIVE_SKILL)
				{
					if (table.SkillType == SkillType.NORMAL_ATTACK)
					{
						nomalAttackPacket = skillPacket;
						skillPacket.CanRelease = true;
						//普通攻击没有开场冷却
					}
					else
					{
						skillPacket.CanRelease = false;
						
					}

					this.DelayFrame(UnityEngine.Random.Range(0, 5), () =>
					{
						//开启冷却时间检测
						// 单个序列处理所有逻辑
						ActionKit.Repeat()
							.Condition(() => enabled)
							.DelayFrame(5) // 每5帧检测一次
							.Condition(() =>  !skillPacket.CanRelease) // 条件检测  如果未达到可以释放的冷却时间
							.Callback(() =>
							{
								Debug.Log("进入冷却  " + skillPacket._data.Name);
							})
							.Delay(skillPacket._data.CollTime / 100f) // 延迟冷却时间
							.Callback(() =>
							{
								Debug.Log($"检测到冷却结束，可以释放技能  Name:{skillPacket._data.Name} CanRelease:{skillPacket.CanRelease}");
								skillPacket.CanRelease = true; // 设置为冷却结束
							})
							.Start(this);
					});
				}
				else if (table.SkillType == SkillType.PASSIVE_SKILL)
				{
					//被动技能直接触发  ToDo  目标还是要根据选择规则和偏好去选择 目前之哟一个对自己生效的被动  先写死
					this.SendCommand(new ReleaseSkillCommand(table, gameObject, gameObject, gameObject.transform.position));
				}
			}
		}

		public void InitHaveHp(int hpMax)
		{
			//根据等级获取对应表数据
			currentHP = HPMAX = hpMax;
			IsAlive = currentHP >= HPMAX;
			//Debug.Log(entityAttribute);
            //entityAttribute.SetAttribute("Health", currentHP);// 设置生命值属性

            if (bloodController != null)
			{
				Debug.Log($"更新了血条显示currentHP:{currentHP}HPMAX:{HPMAX}hpMax{hpMax}");
				float percent = (float)currentHP / HPMAX;

				bloodController.Init(percent);
				bloodController.gameObject.SetActive(false);// 初始时是否显示血条
            }
        }

		/// <summary>
		/// 生成实体后的初始化方法
		/// </summary>
		/// <param name="id"></param>
		/// <param name="lv"></param>
		public void Init(bool isEnemy = false)
		{
			// //测试列表  初始化  后边迁移到BattleModel
			// var tg = GameObject.Find("EnemyGroup");
			// foreach (Transform item in tg.transform)
			// {
			// 	targetList.Add(item.gameObject);
			// }

			//根据是不是敌人 确定当前使用的目标列表
			BattleInModel model = this.GetModel<BattleInModel>();
            targetList = isEnemy ? model.player_allEntitys : model.opponent_allEntitys;

			//entityAttribute = this.GetModel<EntityAttributeModel>();


			buffRunner = new BuffRunner();
			buffRunner.Init();
        }

        public void SetMoveSpeed(float spd)
		{
			aiPath.maxSpeed = spd;
			readonlyMoveSpeed = spd;
			MoveSpeed = spd;
            //entityAttribute.SetAttribute("MoveSpeed", (int)spd);// 设置移速属性
        }

		private void Update()
		{
			if (bloodController != null)
			{
				// 保持血条与相机平行
				Vector3 cameraForward = Camera.main.transform.forward;
				bloodController.transform.rotation = Quaternion.LookRotation(cameraForward);
			}
		}

		private int GetSortOrder(SkillType type)
		{
			return type switch
			{
				SkillType.ACTIVE_SKILL => 0,
				SkillType.NORMAL_ATTACK => 1,
				_ => 2
			};
		}

		public void BeHurt(int damage)
		{
			if (currentHP <= 0 && IsAlive)
			{
				IsAlive = false;
				FSM.ChangeState(EntityState.Die);
				// Destroy(gameObject);
				return;
			}

			// 处理伤害减免,根据Defense_PercentReductionValue值计算伤害减免后，并计算出最终damage值
			if (transform.CompareTag("Hero") && Defense_PercentReduction_all)
			{
				damage = Mathf.RoundToInt(damage * (1 - Defense_PercentReductionValue / 100f));
			}

			bloodController.gameObject.SetActive(true);

            currentHP -= damage;

			// 更新受伤特效
			Transform mat = transform.Find("Model/Tower_B/Tower_B_model");
			if (mat != null)
			{
				Material oldm = mat.GetComponent<MeshRenderer>().material;

				ResLoader loader = ResLoader.Allocate();
				Material newMat = loader.LoadSync<Material>("BuildingHurtMat");
				mat.GetComponent<MeshRenderer>().material = newMat;
				this.Delay(0.2f, () =>
				{
					mat.GetComponent<MeshRenderer>().material = oldm;
					loader.Recycle2Cache();
				});

				//if (this.Blood <= 0)
				//{
				//    var newFX = Instantiate(destroyFXPrefab, transform.position, Quaternion.identity);
				//    Destroy(newFX, 1f);
				//    EventCenter.Broadcast("UpgradeUI", hitTarget, gameObject);
				//}
			}

			if (bloodController != null)
			{
				float percent = (float)currentHP / HPMAX;
				//Debug.Log($"currentHP:{currentHP} HPMAX:{HPMAX}percent:{percent}");
				bloodController.UpdateBlood(percent);
			}
			//Debug.Log(gameObject.name + "   收到伤害  " + damage + "   当前血量   " + currentHP);
		}

		#region 处理buff相关逻辑

		public void SetDefenseDown_Percent(int chanveValue)
		{
			if (transform.CompareTag("Tower"))
			{
				//  降低防御力  后续添加
				Debug.Log($"降低了{chanveValue / 100f}%的防御力");
			}
		}

		#endregion

		public void Move(Transform target)
		{
			if (aiDestination != null)
			{
				Debug.Log($"{transform.name}准备移动");
				//通过A* 开始向目标移动 如果目标消失  进入待机状态 重新索敌
				aiDestination.target = target;
				aiPath.isStopped = false;
				Debug.Log("当前角色正在移动...");

				// 移动时添加一个被动技能buff：3秒内减免 n% 所有伤害
				if (transform.CompareTag("Hero"))
				{
					if (transform.name.Contains("Hero_Shield"))
					{
                        buffRunner.GiveBuff(transform, 20024);
                        buffRunner.ExecuteBuff(20024);
					}
				}
			}
		}
		public void Move(Vector3 target)
		{
			//通过A* 开始向目标移动 如果目标消失  进入待机状态 重新索敌
			if (aiPath != null)
			{
				aiPath.destination = target;
				aiPath.isStopped = false;
			}
		}
		public void StopMove()
		{
			if (aiPath != null)
			{
				aiDestination.target = null;
				aiPath.isStopped = true;
				Debug.Log("当前角色已经停止移动...");
				if (transform.CompareTag("Hero") && transform.name.Contains("Hero_Shield"))
				{
					buffRunner.RemoveBuffEntity(20024);
					if (transform.Find("FX_haojiao"))
					{
						Destroy(transform.Find("FX_haojiao").gameObject);
					}
				}
				Defense_PercentReduction_all = false;
			}
		}
		public bool ReleaseSkill()
		{
			if (isRelease)
			{
				return false;
			}
			//判断冷却时间 然后 判断范围内有没有目标 有的话就准备释放技能  释放规则加载资源即可  条件通过就可以释放技能了 
			//若是主动技能 或者普攻  判断冷却时间 
			foreach (var packet in skillPacketDict)
			{
				SkillTable table = packet.Value._data;
				if (table.SkillType != SkillType.NORMAL_ATTACK && table.SkillType != SkillType.ACTIVE_SKILL)
				{
					//Debug.LogError($"技能 ID:{table.Name} 无法主动释放");
					//非主动技能 可能需要走另外的 被动技能或者光环的逻辑  暂无
					continue;
				}
				if (!packet.Value.CanRelease)
				{
					//Debug.LogError($"技能 ID:{table.Name} 处于冷却中 不能释放");
					continue;
				}

				GameObject target = null;
				//TODO
				if (table.Preference == "HealthBelowPercentStrategy")
				{
					List<GameObject> targetListNow = this.GetModel<BattleInModel>().player_allEntitys;
					target = this.SendCommand(new FindTargetCommand(targetListNow, table.TagMask, table.CastRanage, table.Preference, gameObject));
				}
				else
				{
					target = this.SendCommand(new FindTargetCommand(targetList, table.TagMask, table.CastRanage, table.Preference, gameObject));
				}

				//如果有目标  那么就可以释放技能了

				if (target == null)
				{
					//Debug.LogError("没有目标 不能释放技能");
					continue;
				}
				//释放技能之前需要先转身
				packet.Value.CanRelease = false;
				if (rotateSpeed > 0 && target != gameObject)
				{
					transform.DOLookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z), rotateSpeed).SetEase(Ease.Linear)
								.SetSpeedBased()
								.OnComplete(() =>
								{
									//检查一下 目标可能已经没有了
									if (target == null)
									{
										return;
									}
									Debug.Log($"准备释放技能 ID:{packet.Value._data.Id}");
									this.SendCommand(new ReleaseSkillCommand(table, gameObject, target, target.transform.position));
								});
				}
				else
				{
					Debug.Log($"准备释放技能 ID:{packet.Value._data.Id}");
					this.SendCommand(new ReleaseSkillCommand(table, gameObject, target, target.transform.position));
				}
				return true;
			}
			return false;
		}

		#region 动画相关
		// 播放状态对应的动画
		private void PlayStateAnimation(EntityState state, float transitionTime = 0.05f)
		{
			if (animator != null && animationMap.ContainsKey(state))
			{
				string animationName = animationMap[state];
				if (HasAnimation(animationName))
				{
					animator.CrossFade(animationName, transitionTime, 0, 0);
				}
				else
				{
					// 如果找不到对应动画，尝试使用枚举名
					string fallbackName = state.ToString();
					if (HasAnimation(fallbackName))
					{
						animator.CrossFade(fallbackName, transitionTime, 0, 0);
					}
					else
					{
						Debug.LogWarning($"Animation not found: {animationName} or {fallbackName}");
					}
				}
			}
		}
		// 检查动画是否存在
		private bool HasAnimation(string stateName)
		{
			if (animator == null || animator.runtimeAnimatorController == null)
				return false;

			var controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
			if (controller == null) return false;

			// 遍历所有层
			foreach (var layer in controller.layers)
			{
				// 遍历层中的所有状态
				foreach (var state in layer.stateMachine.states)
				{
					if (state.state.name == stateName)
						return true;
				}
			}
			return false;
		}

		// 设置自定义动画名称
		public void SetAnimationName(EntityState state, string animationName)
		{
			if (animationMap.ContainsKey(state))
			{
				animationMap[state] = animationName;
			}
			else
			{
				animationMap.Add(state, animationName);
			}
		}

		public Animator GetAnimator()
		{
			return animator;
		}

		#endregion

		#region 状态机相关
		public FSM<EntityState> FSM = new FSM<EntityState>();

		public enum EntityState
		{
			Idle,
			MoveToTarget,
			Attacking,
			Die
		}


		[Serializable]
		public class IdleState : AbstractState<EntityState, EntityController>
		{
			private QFramework.IActionController _idleController;

			public IdleState(FSM<EntityState> fsm, EntityController target) : base(fsm, target)
			{
				// Action moved to OnEnter
			}
			protected override bool OnCondition()
			{
				return mFSM.CurrentStateId != EntityState.Idle;
			}
			protected override void OnEnter()
			{
				mTarget.OnIdleEnter();
                // 启动周期性检测（每5帧）
                _idleController = ActionKit.Repeat()
					.Condition(() => !mTarget.isRelease && mFSM.CurrentStateId == EntityState.Idle)
					.DelayFrame(5)
					.Callback(() =>
					{
                        mTarget.SendCommand<CleanInvalidTargetsCommand>(new CleanInvalidTargetsCommand(mTarget.targetList));
						if (mTarget.targetList.Count > 0)
						{
                            if (mTarget.nomalAttackPacket != null)// 临时加的防止空引用，如果普攻包为空，说明初始化没做好
                            {
                                var target = mTarget.SendCommand(new FindTargetCommand(mTarget.targetList, mTarget.nomalAttackPacket._data.TagMask, 99999, mTarget.nomalAttackPacket._data.Preference, mTarget.gameObject));
                                if (target == null)
                                {
                                    Debug.Log($"{mTarget.name}IdleState中找不到目标");
                                    return;
                                }

                                Debug.Log($"{mTarget.name}: 准备移动到: " + target.name);
                                mTarget.Move(target.transform);
                                mFSM.ChangeState(EntityState.MoveToTarget);
                            }
                            //try
                            //{
                            //	if (mTarget.nomalAttackPacket != null)// 临时加的防止空引用，如果普攻包为空，说明初始化没做好
                            //                         {
                            //                             var target = mTarget.SendCommand(new FindTargetCommand(mTarget.targetList, mTarget.nomalAttackPacket._data.TagMask, 99999, mTarget.nomalAttackPacket._data.Preference, mTarget.gameObject));
                            //                             if (target == null)
                            //                             {
                            //                                 Debug.Log($"{mTarget.name}IdleState中找不到目标");
                            //                                 return;
                            //                             }

                            //                             Debug.Log($"{mTarget.name}: 准备移动到: " + target.name);
                            //                             mTarget.Move(target.transform);
                            //                             mFSM.ChangeState(EntityState.MoveToTarget);
                            //	}
                            //	else
                            //	{
                            //		Debug.LogWarning($"{mTarget.name}的普攻包为空，无法进行索敌");
                            //	}
                            //}
                            //catch (System.Exception e)
                            //{
                            //	Debug.Log($"mTarget.name{mTarget.name}错误原因: {e.Message} 当前名字：{mTarget.gameObject.name}");
                            //                     }
                        }
					})
					.Start(mTarget);
			}
			protected override void OnExit()
			{
				mTarget.OnIdleExit();
				if (_idleController != null)
				{
					_idleController.Deinit();
					_idleController = null;
				}
			}
		}

        [Serializable]
        public class MoveToTargetState : AbstractState<EntityState, EntityController>
		{
			private QFramework.IActionController _moveController;

			public MoveToTargetState(FSM<EntityState> fsm, EntityController target) : base(fsm, target)
			{

			}
			protected override bool OnCondition()
			{
				return mFSM.CurrentStateId != EntityState.MoveToTarget;
			}
			protected override void OnEnter()
			{
				mTarget.OnMoveToTargetEnter();

				_moveController = ActionKit.Repeat()
					.Condition(() => !mTarget.isRelease && mFSM.CurrentStateId == EntityState.MoveToTarget)
					.DelayFrame(5)
					.Callback(() =>
					{
						if (mTarget.aiDestination != null)
						{
							if (mTarget.aiDestination.target == null)
							{
								mFSM.ChangeState(EntityState.Idle);
							}
						}

						//可能需要优化  每次都创建命令对象  有点浪费
						var obj = mTarget.SendCommand(new FindTargetCommand(mTarget.targetList, mTarget.nomalAttackPacket._data.TagMask, mTarget.nomalAttackPacket._data.CastRanage, mTarget.nomalAttackPacket._data.Preference, mTarget.gameObject));
						if (obj != null)
						{
							Debug.Log("aaaaaaaaaaaaaaaaaaaaa");
							// 找到目标，进入攻击状态
							mFSM.ChangeState(EntityState.Attacking);
						}
						//else
						//{
						//	Debug.Log($"{mTarget.transform.name}没找到目标啊");
						//}
					})
					.Start(mTarget);
			}
			protected override void OnExit()
			{
				mTarget.OnMoveToTargetExit();
				if (_moveController != null)
				{
					_moveController.Deinit();
					_moveController = null;
				}
			}
		}
        [Serializable]
        public class AttackingState : AbstractState<EntityState, EntityController>
		{
			private QFramework.IActionController _attackingFindController;
			private QFramework.IActionController _attackingReleaseController;

			public AttackingState(FSM<EntityState> fsm, EntityController target) : base(fsm, target)
			{
				// Actions moved to OnEnter
			}
			protected override bool OnCondition()
			{
				return mFSM.CurrentStateId != EntityState.Attacking;
			}
			protected override void OnEnter()
			{
				mTarget.OnAttackingEnter();

				_attackingFindController = ActionKit.Repeat()
					.Condition(() => !mTarget.isRelease && mFSM.CurrentStateId == EntityState.Attacking)
					.DelayFrame(5)
					.Callback(() =>
					{
						//可能需要优化  每次都创建命令对象  有点浪费
						var obj = mTarget.SendCommand(new FindTargetCommand(mTarget.targetList, mTarget.nomalAttackPacket._data.TagMask, mTarget.nomalAttackPacket._data.CastRanage, mTarget.nomalAttackPacket._data.Preference, mTarget.gameObject));
						if (obj == null)
						{
							// 没有目标了 进入待机 重新索敌
							mFSM.ChangeState(EntityState.Idle);
						}
					})
					.Start(mTarget);

				_attackingReleaseController = ActionKit.Repeat()
					.Condition(() => !mTarget.isRelease && mFSM.CurrentStateId == EntityState.Attacking)
					.DelayFrame(5)
					.Callback(() =>
					{
						//检测当前是否有技能可以释放的
						if (mTarget.ReleaseSkill())
						{

						}
					})
					.Start(mTarget);
			}
			protected override void OnUpdate()
			{
				mTarget.OnAttackingUpdate();

			}
			protected override void OnExit()
			{
				mTarget.OnAttackingExit();
				if (_attackingFindController != null)
				{
					_attackingFindController.Deinit();
					_attackingFindController = null;
				}
				if (_attackingReleaseController != null)
				{
					_attackingReleaseController.Deinit();
					_attackingReleaseController = null;
				}

			}
		}
        [Serializable]
        public class DieState : AbstractState<EntityState, EntityController>
		{
			public DieState(FSM<EntityState> fsm, EntityController target) : base(fsm, target)
			{
			}

			protected override bool OnCondition()
			{
				return true;
			}

			protected override void OnEnter()
			{
				mTarget.OnDieEnter();
			}

			protected override void OnExit()
			{
				mTarget.OnDieExit();
			}
		}
	}
	#endregion
}