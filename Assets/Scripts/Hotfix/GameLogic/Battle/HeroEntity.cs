//using DG.Tweening;
//using Pathfinding;
//using PitayaGame.GameSvr;
//using QFramework;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.Events;

//namespace QFramework.Game
//{
//    public partial class HeroEntity : ViewController,ICanHurt, ICanResponseBuff
//    {
//        public bool canMove { get; set; }
//        public bool canAttack { get; set; }
//        public float attackRange { get; set; }
//        float curAttackTimer;

//        public GameObject arrowPrefab;

//        public AIPath aIPath;
//        public AIDestinationSetter aIDestinationSetter;

//        public cfg.Enum_BuildingType searchBuildingType;

//        public HeroData heroData { get; set; }
//        public HeroData runTimeheroData { get; set; }
//        public BuffRunner buffRunner { get; set; }
//        public bool IsEnemy 
//        {
//            get;
//            set;
//        }

//        public void Init()
//        {
//            aIPath = GetComponent<AIPath>();
//            aIDestinationSetter = GetComponent<AIDestinationSetter>();
//            canMove = true;
//            canAttack = false;
//            attackRange = runTimeheroData.attackSpeed;
//            curAttackTimer = 0;
//            FindClosedTarget();
//            InitBuffRunner();
//            EventCenter.AddListener<float>("UpdateDestructionRate", OnUpdateDestructionRate);
//            EventCenter.AddListener("ADD_BUFF", () => {
//                this.SendCommand(new AddSingleBuffToTargetCommand(transform, 10001));

//                buffRunner.ExecuteBuffs();
//            });
//        }

//        void OnUpdateDestructionRate(float rate)
//        {
//            if (IsEnemy == false)
//            {
//                if (rate >= 0.2f)
//                {
//                    this.SendCommand(new AddSingleBuffToTargetCommand(transform, 10001));
//                }
//            }
//        }

//        public void Update()
//        {
//            if (canMove)
//            {
//                if (aIDestinationSetter.target != null)
//                {
//                    float dis = Vector3.Distance(transform.position, aIDestinationSetter.target.position);
//                    if (dis <= 4.0f)
//                    {
//                        aIPath.enabled = false;
//                        transform.LookAt(aIDestinationSetter.target);
//                        Debug.Log("移动结束，准备攻击");
//                        curAttackTimer = attackRange;
//                        canMove = false;
//                        canAttack = true;
//                    }
//                    else
//                    {
//                        FindClosedTarget();
//                    }
//                }
//                else
//                {
//                    FindClosedTarget();
//                }
//            }
//            else
//            {
//                if (canAttack)
//                {
//                    curAttackTimer += Time.deltaTime;
//                    if (curAttackTimer >= attackRange)
//                    {
//                        curAttackTimer = 0;
//                        AttackHandler();
//                    }
//                }
//            }

//            // 更新buff
//            if (buffRunner != null)
//            {
//                buffRunner.UpdateBuffs();
//            }
//        }
//        public IEnumerator StartAttackB(float delayTime, UnityAction callBack)
//        {
//            GetComponentInChildren<Animator>().CrossFade(AnimationClipNames.Attack, 0.1f, 0, 0);
//            yield return new WaitForSeconds(delayTime);
//            callBack?.Invoke();
//        }
//        void AttackHandler()
//        {
//            StartCoroutine(StartAttackB(0.53f, () => {
//                // 发射箭矢
//                if (aIDestinationSetter.target != null)
//                {
//                    GameObject arrow = Instantiate(arrowPrefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
//                    arrow.GetComponent<ArrowEntity>().Run(transform, aIDestinationSetter.target);
//                    transform.LookAt(aIDestinationSetter.target);
//                    if (IsEnemy)
//                    {
//                        float dis = Vector3.Distance(transform.position, aIDestinationSetter.target.position);
//                        if (dis > 4)
//                        {
//                            FindClosedTarget();
//                            canMove = true;
//                            canAttack = false;
//                        }
//                    }
//                }
//                else
//                {
//                    FindClosedTarget();
//                    canMove = true;
//                    canAttack = false;
//                }
//            }));
//        }

//        public Transform closestTarget = null;
//        /// <summary>
//        /// 查找最近的目标建筑或单位
//        /// </summary>
//        void FindClosedTarget()
//        {
//            if (IsEnemy == false)
//            {
//                float minDistance = float.MaxValue;// 全图索敌/建筑物

//                //// 查找最近的建筑中的所有防御建筑
//                //var alldefBuildings = BattleManager.Instance.battleInPanel.battleModel.player_allEntitys
//                //    .Where(pe => pe.GetComponent<BuildEntity>() && pe.GetComponent<BuildEntity>().buildingType == searchBuildingType).ToList();
//                //Debug.Log(searchBuildingType);
//                var alldefBuildings = BattleManagerView.Instance.battleInPanel.GetAllPlayerEntitys(searchBuildingType);

//                foreach (var obj in alldefBuildings)// MapManager.instance.objectsToPlace
//                {
//                    if (obj && obj.GetComponent<BuildEntity>())
//                    {
//                        float distance = Vector3.Distance(transform.position, obj.transform.position);
//                        if (distance < minDistance)
//                        {
//                            minDistance = distance;
//                            closestTarget = obj.transform;
//                        }
//                    }
//                }
//                aIDestinationSetter.target = closestTarget;
//                if (aIPath.enabled == false)
//                {
//                    aIPath.enabled = true;
//                    GetComponentInChildren<Animator>().CrossFade(AnimationClipNames.Walk, 0.1f, 0, 0);
//                }
//            }
//            else
//            {
//                float minDistance = float.MaxValue;
//                var _target = BattleManagerView.Instance.battleInPanel.battleModel.player_allEntitys
//                    .Where(pe => pe.GetComponent<ICanResponseBuff>().IsEnemy == false && pe.GetComponent<ICanResponseBuff>() != null)
//                    .FirstOrDefault();

//                if (_target == null)
//                {
//                    //_target = BattleManager.Instance.battleInPanel.battleModel.player_allEntitys
//                    //        .Where(pe => pe.GetComponent<BuildEntity>() && pe.GetComponent<BuildEntity>().IsEnemy == false)
//                    //    .FirstOrDefault();

//                    Debug.Log($"{(IsEnemy == true ? "红色" : "蓝色")}没有找到对手英雄目标");
//                    GetComponentInChildren<Animator>().CrossFade(AnimationClipNames.Idle, 0.1f, 0, 0);
//                    aIPath.enabled = false;
//                    return;
//                }
//                Debug.Log("_target:" + _target.name);
//                float distance = Vector3.Distance(transform.position, _target.transform.position);
//                if (distance < minDistance)
//                {
//                    minDistance = distance;
//                    closestTarget = _target.transform;
//                    aIDestinationSetter.target = closestTarget;
//                    if (aIPath.enabled == false)
//                    {
//                        aIPath.enabled = true;
//                        GetComponentInChildren<Animator>().CrossFade(AnimationClipNames.Walk, 0.1f, 0, 0);
//                    }
//                }
//            }
//        }

//        #region 继承buff系统的实体要重写的抽象方法
//        protected void InitBuffRunner()
//        {
//            // 初始化buff执行器
//            buffRunner = new BuffRunner();
//            buffRunner.Init();
//        }
//        #region 可在PlayMaker中调用的方法
//        public void AddBuff(int buffId)
//        {
//            buffRunner.GiveBuff(transform, buffId);
//        }

//        public void ExecuteBuffs()
//        {
//            buffRunner.ExecuteBuffs();
//        }
//        #endregion
//        public void OnUpgradeBlood(float chanedValue)
//        {
//            Debug.Log("改变了血量");
//            runTimeheroData.blood += chanedValue;
//            ChangeModelColorFX();
//        }

//        void ChangeModelColorFX()
//        {
//            transform.DOScale(Vector3.one * 1.2f, 0.1f).OnComplete(() =>
//            {
//                transform.DOScale(Vector3.one, 0.1f);
//            });
//        }
//        public void OnUpgradeAttack(float changeValue)
//        {
//            //Debug.Log("改变了OnUpgradeAttack");
//            runTimeheroData.attackDamage += changeValue;
//            Debug.Log("当前英雄攻击力：" + runTimeheroData.attackDamage);
//        }

//        public void OnUpgradeSpeed(float changeValue)
//        {
//            Debug.Log("改变了OnUpgradeSpeed");
//            runTimeheroData.moveSpeed += changeValue;
//        }

//        public void OnUpgradeExp(float changeValue)
//        {
//            Debug.Log("改变了OnUpgradeExp");
//        }

//        public void SetDefaultSpeed()
//        {
//            runTimeheroData.moveSpeed = heroData.moveSpeed;
//        }

//        //public override void Init()
//        //{

//        //}

//        public void BeHurt(int v)
//        {
//            runTimeheroData.blood -= v;
//            if (runTimeheroData.blood <= 0)
//            {
//                runTimeheroData.blood = 0;
//                Debug.Log("英雄死亡");
//                BattleManagerView.Instance.battleInPanel.battleModel.player_allEntitys.Remove(this.gameObject);
//                Destroy(this.gameObject);
//            }
//            ChangeModelColorFX();
//        }
//        #endregion
//    }
//}

