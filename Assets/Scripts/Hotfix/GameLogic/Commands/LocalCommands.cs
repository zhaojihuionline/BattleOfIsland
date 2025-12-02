using QFramework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QFramework.UI;
using QFramework.Game;
using Cysharp.Threading.Tasks;
using PitayaClient.Network.Manager;
using PitayaGame.GameSvr;
using PitayaGame.Enums;

/// <summary>
/// �ֲ��決AStar��������Command
/// </summary>
public class BakeAStarPathCommand : AbstractCommand
{
    public Vector3 point;
    public float radius;

    public BakeAStarPathCommand(Vector3 _point,float _radius)
    {
        this.point = _point;
        this.radius = _radius;
    }
    protected override void OnExecute()
    {
        AStarBaker.Instance.DoLocalBake(this.point, this.radius);
    }
}
// ��ȡ���н����е�ָ�����ͽ���,type��ȡ
public class GetAllBuildingsOfTypeCommand : AbstractCommand<List<GameObject>>
{
    public cfg.Enum_BuildingType buildingType;
    public GetAllBuildingsOfTypeCommand(cfg.Enum_BuildingType _buildingType)
    {
        this.buildingType = _buildingType;
    }
    protected override List<GameObject> OnExecute()
    {
        if (buildingType == cfg.Enum_BuildingType.None)
        {
            return BattleManagerView.Instance.battleInPanel.battleModel.player_allEntitys;
        }
        else
        {
            // ��������Ľ����е����з�������
            var alldefBuildings = BattleManagerView.Instance.battleInPanel.battleModel.player_allEntitys
                .Where(pe => pe.GetComponent<BuildEntity>() && pe.GetComponent<BuildEntity>().buildingType == buildingType).ToList();
            return alldefBuildings;
        }
    }
}
public class RemoveAllPlayerEntitysCommand:AbstractCommand
{
    protected override void OnExecute()
    {
        if (BattleManagerView.Instance && BattleManagerView.Instance.battleInPanel != null)
        {
            foreach (var pe in BattleManagerView.Instance.battleInPanel.battleModel.player_allEntitys)
            {
                if (pe != null)
                {
                    GameObject.Destroy(pe);
                }
            }
            foreach (var pe in BattleManagerView.Instance.battleInPanel.battleModel.opponent_allEntitys)
            {
                if (pe != null)
                {
                    GameObject.Destroy(pe);
                }
            }
            this.GetModel<BattleInModel>().player_allEntitys.Clear();
            this.GetModel<BattleInModel>().opponent_allEntitys.Clear();
        }
    }
}
public class SetAllBuildingsLayerCommand : AbstractCommand
{
    public GameObject newBuiding;
    public SetAllBuildingsLayerCommand(GameObject _newBuiding)
    {
        this.newBuiding = _newBuiding;
    }
    protected override void OnExecute()
    {
        BattleManagerView.Instance.battleInPanel.battleModel.player_allEntitys.Add(newBuiding);
    }
}
/// <summary>
/// ����һ��buff��Ŀ������
/// </summary>
public class AddSingleBuffToTargetCommand : AbstractCommand
{
    public Transform target;
    public int buffid;
    public GameObject effectObj;
    public AddSingleBuffToTargetCommand(Transform _target, int _buffid,GameObject effectObj)
    {
        this.target = _target;
        this.buffid = _buffid;
        this.effectObj = effectObj;
    }
    protected override void OnExecute()
    {
        var buffRunner = target.GetComponent<EntityController>().buffRunner;
        if (buffRunner != null)
        {
            buffRunner.GiveBuff(target, buffid);
            buffRunner.ExecuteBuffs();
        }
    }
}
/// <summary>
/// ִ��Ŀ�����ϵ�����buffЧ��
/// </summary>
public class ExecuteAllBuffsCommand : AbstractCommand
{
    public Transform target;
    public ExecuteAllBuffsCommand(Transform _target)
    {
        this.target = _target;
    }
    protected override void OnExecute()
    {
        var buffRunner = target.GetComponent<BuffRunner>();
        if (buffRunner != null)
        {
            buffRunner.ExecuteBuffs();
        }
    }
}   
/// <summary>
/// ����һ��ʵ�嵽������
/// </summary>
public class AddEntityToBattleModelCommand : AbstractCommand
{
    public GameObject entity;
    public bool isEnemy;
    public AddEntityToBattleModelCommand(GameObject _entity, bool _isEnemy)
    {
        this.entity = _entity;
        this.isEnemy = _isEnemy;
    }
    protected override void OnExecute()
    {
        if (isEnemy)
        {
            BattleManagerView.Instance.battleInPanel.battleModel.player_allEntitys.Add(entity);
        }
        else
        {
            BattleManagerView.Instance.battleInPanel.battleModel.opponent_allEntitys.Add(entity);
        }
    }
}
public class RemoveEntityFromBattleModelCommand : AbstractCommand
{
    public GameObject entity;
    public bool isEnemy;
    public RemoveEntityFromBattleModelCommand(GameObject _entity, bool _isEnemy)
    {
        this.entity = _entity;
        this.isEnemy = _isEnemy;
    }
    protected override void OnExecute()
    {
        if (isEnemy)
        {
            BattleManagerView.Instance.battleInPanel.battleModel.opponent_allEntitys.Remove(entity);
        }
        else
        {
            BattleManagerView.Instance.battleInPanel.battleModel.player_allEntitys.Remove(entity);
        }
    }
}
public class EntityDeathCommand : AbstractCommand
{
    public GameObject entity;
    public Transform whoHitMe;
    private BattleInModel battleInModel;
    public EntityDeathCommand(Transform whoHitme, GameObject _entity)
    {
        this.entity = _entity;
        this.whoHitMe = whoHitme;
    }
    protected override void OnExecute()
    {
        battleInModel = this.GetModel<BattleInModel>();

        if (entity != null && (entity.transform.CompareTag("Build")|| entity.transform.CompareTag("Tower")))
        {

            battleInModel.DestroyedEntityCount.Value++;


            battleInModel.DestructionRate.Value =
                (float)battleInModel.DestroyedEntityCount.Value /
                (float)battleInModel.TotalEntityCount.Value;

            battleInModel.WillDeathEntity = entity;
            battleInModel.WhoHitme = whoHitMe;
        }
    }
}