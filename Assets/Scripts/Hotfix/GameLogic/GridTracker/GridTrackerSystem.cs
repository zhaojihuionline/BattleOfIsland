using QFramework;
using UniRx;
using UnityEngine;

/// <summary>
/// 网格追踪系统接口定义
/// </summary>
public interface IGridTrackerSystem : ISystem
{
    void GridTrackerStateChange(bool isEnabled);        // 启用/禁用网格追踪功能
    void SetComponent(GridLayout layout, Camera camera); // 设置必要的Unity组件
    GridLayout GetGridLayout();                         // 获取网格布局组件
    Vector3 GetGridCenterPosition(Vector3Int gridCoord); // 获取网格中心位置
}

/// <summary>
/// 网格追踪系统实现 - 核心功能：将屏幕坐标转换为网格坐标
/// </summary>
public class GridTrackerSystem : AbstractSystem, IGridTrackerSystem
{
    private GridLayout gridLayout;                      // Unity的GridLayout组件
    private Camera mainCamera;                          // 主摄像机引用
    private CompositeDisposable disposables = new CompositeDisposable(); // Rx订阅管理器
    private Vector2 lastInputPosition;                  // 上一次输入位置（用于优化）

    // 射线检测的图层掩码，只检测名为"Panel"的图层
    private LayerMask placementLayerMask = 1 << LayerMask.NameToLayer("Panel");

    protected override void OnInit()
    {
        Debug.Log("网格追踪系统初始化完成");
        placementLayerMask = 1 << LayerMask.NameToLayer("Panel"); // 确保图层掩码正确设置
    }

    protected override void OnDeinit()
    {
        GridTrackerStateChange(false);  // 系统销毁时自动停止追踪
        disposables?.Dispose();         // 清理所有Rx订阅，防止内存泄漏
    }

    /// <summary>
    /// 设置系统运行所需的Unity组件
    /// </summary>
    public void SetComponent(GridLayout layout, Camera camera)
    {
        gridLayout = layout;
        mainCamera = camera;
    }

    /// <summary>
    /// 启用或禁用网格追踪功能
    /// </summary>
    public void GridTrackerStateChange(bool isEnabled)
    {
        disposables.Clear(); // 清理之前的订阅

        if (isEnabled)
        {
            // 验证必要组件是否已正确设置
            if (gridLayout == null || mainCamera == null)
            {
                Debug.LogError("启用系统错误：需要的组件未设置！");
                return;
            }

            lastInputPosition = Vector2.zero;
            UpdateGridTracking(); // 立即执行一次追踪更新

            // 使用Rx框架每5帧更新一次网格追踪（性能优化）
            Observable.EveryUpdate()
                .SampleFrame(4)  // 每5帧采样一次，减少计算频率
                .Subscribe(_ => UpdateGridTracking())
                .AddTo(disposables); // 添加到可销毁集合便于管理

            Debug.Log("网格追踪已启用");
        }
        else
        {
            Debug.Log("网格追踪已禁用");
        }
    }

    public GridLayout GetGridLayout() => gridLayout;

    /// <summary>
    /// 更新网格追踪逻辑
    /// </summary>
    private void UpdateGridTracking()
    {
        if (gridLayout == null || mainCamera == null) return;

        Vector2 currentInputPos = GetCurrentInputPosition();

        // 优化：输入位置变化很小时跳过更新（减少不必要的计算）
        if (Vector2.Distance(lastInputPosition, currentInputPos) < 0.1f) return;

        lastInputPosition = currentInputPos;
        string inputDevice = Input.touchCount > 0 ? "Touch" : "Mouse"; // 自动检测输入设备
        DetectGridCoordinate(currentInputPos, inputDevice);
    }

    /// <summary>
    /// 获取当前输入位置（同时支持触摸和鼠标）
    /// </summary>
    private Vector2 GetCurrentInputPosition()
    {
        return Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
    }

    /// <summary>
    /// 核心功能：将屏幕坐标转换为网格坐标
    /// </summary>
    private void DetectGridCoordinate(Vector2 screenPosition, string inputDevice)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Vector3 worldPosition = Vector3.zero;
        Vector3Int gridCoord = Vector3Int.zero;

        // 优先使用射线精确检测Panel图层（更准确的位置计算）
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementLayerMask))
        {
            worldPosition = hit.point;                          // 获取射线与Panel碰撞点的世界坐标
            gridCoord = gridLayout.WorldToCell(worldPosition);  // 将世界坐标转换为网格坐标
        }
        else
        {
            // 射线检测失败时使用备用方法（直接屏幕坐标转世界坐标）
            worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
            gridCoord = gridLayout.WorldToCell(worldPosition);
        }

        // 发布网格数据更新事件（canPlace参数由BuildingPlacementController计算）
        this.SendEvent(new GridTrackerDataUpdatedEvent(
            gridCoord: gridCoord,
            worldPosition: worldPosition,
            screenPosition: screenPosition,
            inputDevice: inputDevice,
            canPlace: false // 此值由建筑放置控制器通过碰撞检测计算
        ));
    }

    /// <summary>
    /// 获取指定网格坐标的中心位置（Y轴归零，适用于2.5D游戏）
    /// </summary>
    public Vector3 GetGridCenterPosition(Vector3Int gridCoord)
    {
        if (gridLayout == null) return Vector3.zero;

        // 计算网格单元的中心点：网格左下角 + 半个单元格大小
        Vector3 cellCenter = gridLayout.CellToWorld(gridCoord) + gridLayout.cellSize / 2f;
        return new Vector3(cellCenter.x, 0f, cellCenter.z); // 忽略Y轴高度，保持在地面上
    }
}
