using QFramework;
using UnityEngine;

/// <summary>
/// 更新网格追踪数据命令 - 用于更新网格追踪模型中的数据
/// </summary>
public class UpdateGridTrackerDataCommand : AbstractCommand
{
    private readonly GridTrackerData data; // 要更新的网格追踪数据

    public UpdateGridTrackerDataCommand(GridTrackerData data)
    {
        this.data = data;
    }

    protected override void OnExecute()
    {
        // 获取网格追踪模型并更新所有数据
        var model = this.GetModel<GridTrackerModel>();
        model.CurrentGridCoord = data.gridCoord;                // 更新网格坐标
        model.CurrentWorldPosition = data.worldPosition;        // 更新世界坐标
        model.CurrentScreenPosition = data.screenPosition;      // 更新屏幕坐标
        model.InputDevice = data.inputDevice;                   // 更新输入设备类型
    }
}
