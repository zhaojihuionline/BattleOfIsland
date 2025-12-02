using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using QFramework;

public class BattleInModel : AbstractModel
{
    /// <summary>
    /// 战斗时的玩家所有单位数据列表
    /// </summary>
    public List<GameObject> player_allEntitys;
    /// <summary>
    /// 对手玩家的所有单位数据列表
    /// </summary>
    public List<GameObject> opponent_allEntitys;
    /// <summary>
    /// 建筑物摧毁率
    /// </summary>
    public BindableProperty<float> DestructionRate { get; } = new BindableProperty<float>();
    /// <summary>
    /// 总建筑物实体数量
    /// </summary>
    public BindableProperty<int> TotalEntityCount { get; } = new BindableProperty<int>();

    /// <summary>
    /// 移除的建筑物实体数量
    /// </summary>
    public BindableProperty<int> DestroyedEntityCount { get; } = new BindableProperty<int>();

    public GameObject WillDeathEntity;
    public Transform WhoHitme;
    public Player locaPlayer;
    protected override void OnInit()
    {
        player_allEntitys = new List<GameObject>();
        opponent_allEntitys = new List<GameObject>();
        // 设置初始值（不触发事件）
        DestructionRate.SetValueWithoutEvent(0);
        TotalEntityCount.SetValueWithoutEvent(0);
        DestroyedEntityCount.SetValueWithoutEvent(0);

        locaPlayer = PlayerManager.Instance.LocalPlayer;
    }
}
public class BattleInPanelApp : Architecture<BattleInPanelApp>
{
    protected override void Init()
    {
        
    }
}
