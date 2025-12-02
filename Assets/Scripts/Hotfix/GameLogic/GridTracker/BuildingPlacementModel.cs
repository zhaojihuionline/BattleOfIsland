using QFramework;
using UnityEngine;

/// <summary>
/// 建筑放置数据模型 - 存储建筑放置相关的状态数据
/// </summary>
public class BuildingPlacementModel : AbstractModel
{
    // 当前选择的建筑预制体（现在主要用于记录选择，实际实例由Controller管理）
    public GameObject CurrentBuildingPrefab { get; set; }

    // 是否正在放置建筑的状态标志
    public bool IsPlacingBuilding { get; set; }

    public Vector3 buildingOffset;               // 建筑放置的位置偏移量

    // 当前位置是否可以放置建筑（由碰撞检测决定）
    public bool CanPlaceAtCurrentPosition { get; set; }


    protected override void OnInit()
    {
        // 初始化默认状态：未选择建筑，未在放置状态
        CurrentBuildingPrefab = null;
        IsPlacingBuilding = false;
    }
}
