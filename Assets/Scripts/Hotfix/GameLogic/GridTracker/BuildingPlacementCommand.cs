using GAME.QF;
using QFramework;
using UnityEngine;

/// <summary>
/// 选择建筑命令 - 当玩家选择要放置的建筑时执行
/// </summary>
public class SelectBuildingCommand : AbstractCommand
{
    private readonly GameObject buildingPrefab; // 要放置的建筑预制体
    private readonly bool create; // 要放置的建筑预制体

    public SelectBuildingCommand(GameObject buildingPrefab, bool create)
    {
        this.buildingPrefab = buildingPrefab;
        this.create = create;
    }

    protected override void OnExecute()
    {
        var buildingModel = this.GetModel<BuildingPlacementModel>();
        // 如果已经在放置其他建筑，先取消当前放置
        if (buildingModel.CurrentBuildingPrefab != null)
        {
            GameObject.Destroy(buildingModel.CurrentBuildingPrefab);
            buildingModel.CurrentBuildingPrefab = null;
        }

        GameObject previewInstance = buildingPrefab;
        if (create)
        {
            previewInstance = GameObject.Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);
        }

        // 更新模型状态
        buildingModel.CurrentBuildingPrefab = previewInstance;
        buildingModel.IsPlacingBuilding = true;
        buildingModel.buildingOffset = CalculateBuildingOffset(previewInstance); // 计算建筑偏移量

        // 发送事件通知开始建筑放置（触发预览显示）
        this.SendEvent(new StartBuildingPlacementEvent());
        // 启用网格追踪系统开始追踪鼠标位置
        this.GetSystem<IGridTrackerSystem>().GridTrackerStateChange(true);

        Debug.Log($"已选择建筑: {buildingPrefab.name}");
    }
    /// <summary>
    /// 计算建筑偏移量（基于参考点）
    /// </summary>
    private Vector3 CalculateBuildingOffset(GameObject prefab)
    {
        var buildingModel = this.GetModel<BuildingPlacementModel>();

        // 查找建筑的方向参考点
        Transform followPoint = buildingModel.CurrentBuildingPrefab.GetComponent<BuildingEntity>().GetPointChild(0);
        if (followPoint != null)
        {
            // 计算建筑位置与参考点的偏移
            Vector3 offset = prefab.transform.position - followPoint.position;
            return new Vector3(offset.x, 0f, offset.z); // 忽略Y轴偏移
        }
        return Vector3.zero;
    }
}
/// <summary>
/// 确认建筑放置命令 - 当玩家确认放置建筑时执行
/// </summary>
public class ConfirmBuildingPlacementCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        var buildingModel = this.GetModel<BuildingPlacementModel>();
        var gridModel = this.GetModel<GridTrackerModel>();

        // 验证是否处于有效的放置状态
        if (!buildingModel.IsPlacingBuilding || buildingModel.CurrentBuildingPrefab == null)
            return;

        // 检查当前位置是否可以放置建筑
        if (!buildingModel.CanPlaceAtCurrentPosition)
        {
            Debug.LogWarning("当前位置不可放置建筑！");
            return;
        }

        // 清理放置状态，准备下一次放置
        buildingModel.IsPlacingBuilding = false;
        // 禁用网格追踪系统（放置完成）
        this.GetSystem<IGridTrackerSystem>().GridTrackerStateChange(false);
        // 发送建筑放置确认事件（用于清理预览等操作）
        this.SendEvent<BuildingPlacementConfirmedEvent>();
        buildingModel.CurrentBuildingPrefab = null;
    }
}

/// <summary>
/// 取消建筑放置命令 - 当玩家取消放置时执行
/// </summary>
public class CancelBuildingPlacementCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        var buildingModel = this.GetModel<BuildingPlacementModel>();

        if (buildingModel.IsPlacingBuilding)
        {
            // 禁用网格追踪系统
            this.GetSystem<IGridTrackerSystem>().GridTrackerStateChange(false);
            // 重置建筑放置状态
            buildingModel.IsPlacingBuilding = false;
            GameObject.Destroy(buildingModel.CurrentBuildingPrefab);
            buildingModel.CurrentBuildingPrefab = null;
            // 发送建筑放置取消事件（用于清理预览）
            this.SendEvent<BuildingPlacementCanceledEvent>();
            Debug.Log("建筑摆放已取消");
        }
    }

}
