using UnityEngine;

/// <summary>
/// 网格追踪数据更新事件 - 当鼠标/触摸位置在网格上移动时触发
/// </summary>
public class GridTrackerDataUpdatedEvent
{
    public GridTrackerData Data { get; } // 包含所有追踪数据的结构体

    // 构造函数：直接传入数据对象
    public GridTrackerDataUpdatedEvent(GridTrackerData data) => Data = data;

    // 构造函数：传入各个数据字段创建新对象
    public GridTrackerDataUpdatedEvent(Vector3Int gridCoord, Vector3 worldPosition,
        Vector2 screenPosition, string inputDevice, bool canPlace = false)
    {
        Data = new GridTrackerData
        {
            gridCoord = gridCoord,       // 网格坐标（整数）
            worldPosition = worldPosition, // 世界空间坐标
            screenPosition = screenPosition, // 屏幕空间坐标
            inputDevice = inputDevice,   // 输入设备类型（Mouse/Touch）
        };
    }
}

/// <summary>
/// 网格追踪数据结构 - 包含鼠标/触摸在网格上的所有相关信息
/// </summary>
public struct GridTrackerData
{
    public Vector3Int gridCoord;         // 当前所在的网格坐标（如[1,0,2]）
    public Vector3 worldPosition;        // 对应的世界空间坐标
    public Vector2 screenPosition;       // 原始的屏幕空间坐标
    public string inputDevice;           // 输入设备："Mouse"或"Touch"
}
