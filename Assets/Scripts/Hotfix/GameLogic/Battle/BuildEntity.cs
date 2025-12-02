using cfg;
using DG.Tweening;
using PitayaGame.GameSvr;
using QFramework;
using System.Collections;
using System.Linq;
using UnityEngine;
using QFramework.UI;
[System.Serializable]
public class BuildData
{
    /// <summary>
    /// 建筑信息表
    /// </summary>
    public cfg.Builds data;

    /// <summary>
    /// 建筑对应的等级表
    /// </summary>
    public cfg.BuildsLevel buildsLevel;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bid">建筑的id</param>
    public void Init(int bid,int _level)
    {
        data = CfgMgr.Instance.Tables.TbBuilds.Get(bid);
        buildsLevel = CfgMgr.Instance.Tables.TbBuildsLevel.Get(bid * 100 + _level);
    } 

    public void Upgrade(int bid,int newLevel)
    {
        buildsLevel = CfgMgr.Instance.Tables.TbBuildsLevel.Get(bid * 100 + newLevel);
    }
}
public class BuildEntity : ViewController,ICanHurt,ICanResponseBuff
{
    public BuildData buildData;
    public MeshRenderer mesh;
    public GameObject destroyFXPrefab;
    public GameObject arrowPrefab;

    Transform hitTarget;// 攻击我的目标

    Transform closestTarget;

    [Header("建筑的ID，测试用")]
    public int BuildID;
    public int Level { get; set; }
    public bool IsEnemy { get; set; }
    public BuffRunner buffRunner { get; set; }
    bool ICanResponseBuff.Defense_PercentReduction_all { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public float Blood;
    public Enum_BuildingType buildingType;

    public bool canAttack;
    float curAttackTimer;
    float attackInterval = 1f;

    public bool canFindClosed;
    float findClosedTargetInterval = 0.5f;
    float curFindClosedTargetTimer;

    private void Update()
    {
        if (canFindClosed)
        {
            curFindClosedTargetTimer += Time.deltaTime;
            if (curFindClosedTargetTimer >= findClosedTargetInterval)
            {
                curFindClosedTargetTimer = 0;
                FindClosedTarget();
            }
        }
        else
        {
            if (canAttack)
            {
                curAttackTimer += Time.deltaTime;
                if (curAttackTimer >= attackInterval)
                {
                    //AttackHandler();
                    curAttackTimer = 0;
                }
            }
        }
        
    }
    void AttackHandler()
    {
        //if (buildData.data.BuildingType == cfg.EntityType.Defense) {
        //    // 发射箭矢
        //    if (closestTarget != null)
        //    {
        //        if (arrowPrefab != null)
        //        {
        //            GameObject arrow = Instantiate(arrowPrefab, transform.position + new Vector3(0, 4.0f, 0), Quaternion.identity);
        //            arrow.transform.LookAt(closestTarget);
        //            arrow.transform.DOMove(closestTarget.position + new Vector3(0, 1.0f, 0), 0.5f).OnComplete(() =>
        //            {
        //                Destroy(arrow);
        //                closestTarget.GetComponent<ICanResponseBuff>().OnUpgradeBlood(2.0f);
        //            });
        //        }
        //    }
        //    else
        //    {
        //        FindClosedTarget();
        //        canAttack = false;
        //    }
        //}
    }
    void FindClosedTarget()
    {
        float minDistance = 8.0f;

        foreach (var obj in BattleManagerView.Instance.battleInPanel.battleModel.player_allEntitys)
        {
            if (obj && obj.GetComponent<ICanResponseBuff>()!=null)
            {
                float distance = Vector3.Distance(transform.position, obj.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTarget = obj.transform;
                    canFindClosed = false;
                    canAttack = true;
                }
            }
        }
    }
    public void BeHurt(int v)
    {
        this.Blood -= v;
        //Debug.Log($"建筑受到{v}点伤害，当前血量{this.Blood}");
        Material clonedMat = mesh.material;
        clonedMat.color = Color.red;
        mesh.material = clonedMat;

        StartCoroutine(ResetColor());
        if (this.Blood <= 0)
        {
            var newFX = Instantiate(destroyFXPrefab, transform.position, Quaternion.identity);
            Destroy(newFX, 1f);
            EventCenter.Broadcast("UpgradeUI", hitTarget, gameObject);
        }
    }

    /// <summary>
    /// 升级建筑
    /// </summary>
    /// <param name="buildingData"></param>
    public async void UpgradeLevel(PitayaGame.GameSvr.BuildingData buildingData)
    {
        var newLevelRes = await GameRemoteAPI.UpgradeBuilding(buildingData.BuildId, true);
        buildData.Upgrade(BuildID, newLevelRes.NewLevel);

        Blood = buildData.buildsLevel.Health;
    }

    IEnumerator ResetColor()
    {
        yield return new WaitForSeconds(0.2f);
        Material clonedMat = mesh.material;
        clonedMat.color = Color.white;
        mesh.material = clonedMat;
    }

    public void Init()
    {
        //Debug.Log(buildData.data.BuildingType);
        IsEnemy = true;
    }

    public BuildEntity SetBuildData(PitayaGame.GameSvr.BuildingData buildingData)
    {
        buildData = new BuildData();
        canFindClosed = true;
        buildData.Init(BuildID, buildingData.Level);
        Blood = buildData.buildsLevel.Health;
        Level = buildingData.Level;
        buildingType = (Enum_BuildingType)buildingData.BuildingType;
        return this;
    }

    public BuildEntity SetBuildData(int bLevel,int bType)
    {
        buildData = new BuildData();
        canFindClosed = true;
        buildData.Init(BuildID, bLevel);
        Blood = buildData.buildsLevel.Health;
        Level = bLevel;
        buildingType = (Enum_BuildingType)bType;
        return this;
    }

    public void OnUpgradeBlood(float chanedValue)
    {

    }

    public void OnUpgradeAttack(float changeValue)
    {

    }

    public void OnUpgradeSpeed(float changeValue)
    {

    }

    public void OnUpgradeExp(float changeValue)
    {

    }

    public void SetDefaultSpeed()
    {

    }
}
