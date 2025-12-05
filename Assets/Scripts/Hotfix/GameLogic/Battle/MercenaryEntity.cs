using DG.Tweening;
using Pathfinding;
using PitayaGame.GameSvr;
using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using QFramework.UI;

public class MercenaryEntity : UnitViewController, ICanHurt, ICanResponseBuff
{
    public bool canMove { get; set; }
    public bool canAttack { get; set; }
    public float attackSpeed { get; set; }
    float curAttackTimer;

    public AIPath aIPath;
    public AIDestinationSetter aIDestinationSetter;

    public MercenaryData mercenaryData { get; set; }
    public MercenaryData runTimemercenaryData { get; set; }
    public BuffRunner buffRunner { get; set; }
    public bool IsEnemy { get; set; }
    bool ICanResponseBuff.Defense_PercentReduction_all { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public void Start()
    {
        aIPath = GetComponent<AIPath>();
        aIDestinationSetter = GetComponent<AIDestinationSetter>();
        canMove = true;
        canAttack = false;
        curAttackTimer = 0;
        FindClosedTarget();
        InitBuffRunner();
    }


    public void Update()
    {
        if (canMove)
        {
            if (aIDestinationSetter.target != null)
            {
                float dis = Vector3.Distance(transform.position, aIDestinationSetter.target.position);
                if (dis <= 1.0f)
                {
                    aIPath.enabled = false;
                    transform.LookAt(aIDestinationSetter.target);
                    Debug.Log("移动结束，准备攻击");
                    canMove = false;
                    canAttack = true;
                }
            }
        }
        else
        {
            if (canAttack)
            {
                curAttackTimer += Time.deltaTime;
                if (curAttackTimer >= attackSpeed)
                {
                    curAttackTimer = 0;
                    AttackHandler();
                }
            }
        }

        // 更新buff
        if (buffRunner != null)
        {
            buffRunner.UpdateBuffs();
        }
    }
    public IEnumerator StartAttackB(float delayTime, UnityAction callBack)
    {
        GetComponentInChildren<Animator>().CrossFade(AnimationClipNames.Attack, 0.1f, 0, 0);
        yield return new WaitForSeconds(delayTime);
        callBack?.Invoke();
    }
    void AttackHandler()
    {
        if (aIDestinationSetter.target != null)
        {
            //GameObject arrow = Instantiate(arrowPrefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
            //arrow.GetComponent<ArrowEntity>().Run(transform, aIDestinationSetter.target);
            GetComponentInChildren<Animator>().CrossFade(AnimationClipNames.Attack, 0.1f, 0, 0);
            Debug.Log("实际攻击");
            var targetEntity = aIDestinationSetter.target.GetComponent<BuildEntity>();
            if (targetEntity != null)
            {
                targetEntity.BeHurt((int)runTimemercenaryData.attackDamage);
            }
        }
        else
        {
            FindClosedTarget();
            canMove = true;
            canAttack = false;
        }
    }

    public Transform closestTarget = null;
    /// <summary>
    /// 查找最近的目标建筑或单位
    /// </summary>
    void FindClosedTarget()
    {
        float minDistance = float.MaxValue;// 全图索敌/建筑物
        foreach (var obj in BattleManagerView.Instance.battleInPanel.battleModel.player_allEntitys)// MapManager.instance.objectsToPlace
        {
            if (obj && obj.GetComponent<BuildEntity>())
            {
                float distance = Vector3.Distance(transform.position, obj.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTarget = obj.transform;
                }
            }
        }
        aIDestinationSetter.target = closestTarget;
        if (aIPath.enabled == false)
        {
            aIPath.enabled = true;
            GetComponentInChildren<Animator>().CrossFade(AnimationClipNames.Walk, 0.1f, 0, 0);
        }
    }

    #region 继承buff系统的实体要重写的抽象方法
    protected void InitBuffRunner()
    {
        // 初始化buff执行器
        buffRunner = new BuffRunner();
        buffRunner.Init();
    }
    #region 可在PlayMaker中调用的方法
    public void AddBuff(int buffId)
    {
        buffRunner.GiveBuff(transform, buffId);
    }

    public void ExecuteBuffs()
    {
        buffRunner.ExecuteBuffs();
    }
    #endregion
    public void OnUpgradeBlood(float chanedValue)
    {
        Debug.Log("改变了血量");
        runTimemercenaryData.blood += chanedValue;
        ChangeModelColorFX();
    }

    void ChangeModelColorFX()
    {
        transform.DOScale(Vector3.one * 1.2f, 0.1f).OnComplete(() =>
        {
            transform.DOScale(Vector3.one, 0.1f);
        });
    }
    public void OnUpgradeAttack(float changeValue)
    {
        //Debug.Log("改变了OnUpgradeAttack");
        
        runTimemercenaryData.attackDamage += changeValue;
        Debug.Log("当前英雄攻击力：" + runTimemercenaryData.attackDamage);
    }

    public void OnUpgradeSpeed(float changeValue)
    {
        Debug.Log("改变了OnUpgradeSpeed");
        runTimemercenaryData.moveSpeed += changeValue;
    }

    public void OnUpgradeExp(float changeValue)
    {
        Debug.Log("改变了OnUpgradeExp");
    }

    public void SetDefaultSpeed()
    {

    }

    public void Init()
    {
        attackSpeed = runTimemercenaryData.attackSpeed;
    }

    public void BeHurt(int v)
    {
        runTimemercenaryData.blood -= v;
        if (runTimemercenaryData.blood <= 0)
        {
            runTimemercenaryData.blood = 0;
            Debug.Log("佣兵死亡");
        }
        ChangeModelColorFX();
    }

    public void OnAttackTypeChange(int damageRange)
    {
        throw new System.NotImplementedException();
    }

    public void OnAttributeChange(AttributeChangeData attributeChangeData)
    {
        throw new System.NotImplementedException();
    }
    #endregion
}
