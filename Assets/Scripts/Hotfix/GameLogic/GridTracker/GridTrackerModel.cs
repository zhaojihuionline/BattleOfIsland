using QFramework;
using UnityEngine;

/// <summary>
/// 网格追踪数据模型 - 存储当前网格追踪的状态数据
/// </summary>
public class GridTrackerModel : AbstractModel
{
    // 当前鼠标/触摸所在的网格坐标
    public Vector3Int CurrentGridCoord { get; set; }

    // 当前鼠标/触摸对应的世界空间坐标
    public Vector3 CurrentWorldPosition { get; set; }

    // 当前鼠标/触摸的屏幕空间坐标
    public Vector2 CurrentScreenPosition { get; set; }

    // 当前使用的输入设备类型
    public string InputDevice { get; set; }

    protected override void OnInit()
    {
        // 初始化所有属性为默认值
        CurrentGridCoord = Vector3Int.zero;          // 网格坐标初始为(0,0,0)
        CurrentWorldPosition = Vector3.zero;         // 世界坐标初始为原点
        CurrentScreenPosition = Vector2.zero;        // 屏幕坐标初始为左下角
        InputDevice = "Mouse";                       // 默认输入设备为鼠标
    }
}
