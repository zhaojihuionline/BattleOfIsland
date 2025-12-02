using QFramework;
using UnityEngine;

public class GridTrackerController : MonoBehaviour, IController
{
    [Header("必要引用")]
    public Camera mainCamera;        // 主摄像机引用
    public GridLayout gridLayout;    // 网格布局组件引用

    private void Start()
    {
        InitializeSystemComponents();    // 初始化系统组件
        SetupEventSubscription();        // 设置事件订阅
    }

    private void Update()
    {
        // 调试快捷键：Z键启用网格追踪，X键禁用
        if (Input.GetKeyDown(KeyCode.Z))
        {
            this.GetSystem<IGridTrackerSystem>().GridTrackerStateChange(true);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            this.GetSystem<IGridTrackerSystem>().GridTrackerStateChange(false);
        }
    }

    /// <summary>
    /// 初始化网格追踪系统所需的组件
    /// </summary>
    private void InitializeSystemComponents()
    {
        // 如果未手动赋值，自动获取组件
        gridLayout = gridLayout ?? GetComponent<GridLayout>();
        mainCamera = mainCamera ?? Camera.main;

        // 将组件设置到网格追踪系统中
        this.GetSystem<IGridTrackerSystem>().SetComponent(gridLayout, mainCamera);
    }

    /// <summary>
    /// 设置事件订阅
    /// </summary>
    private void SetupEventSubscription()
    {
        // 订阅网格数据更新事件，收到事件后更新模型数据
        this.RegisterEvent<GridTrackerDataUpdatedEvent>(e =>
        {
            this.SendCommand(new UpdateGridTrackerDataCommand(e.Data));
        }).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    public IArchitecture GetArchitecture() => GridTrackerApp.Interface;

#if UNITY_EDITOR
    /// <summary>
    /// 在Scene视图中绘制调试信息（仅在编辑器中生效）
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return; // 只在运行模式下绘制

        var gridModel = this.GetModel<GridTrackerModel>();
        var buildingModel = this.GetModel<BuildingPlacementModel>();
        var system = this.GetSystem<IGridTrackerSystem>();
        var gridLayout = system.GetGridLayout();
        // Debug.Log("!!!!!!!!!!!!!!!!!");
        if (gridLayout == null) return;

        // 根据可放置状态选择颜色：可放置=绿色，不可放置=红色
        Gizmos.color = buildingModel.CanPlaceAtCurrentPosition ? Color.green : Color.red;

        // 计算当前网格单元的中心位置
        Vector3 cellCenter = gridLayout.CellToWorld(gridModel.CurrentGridCoord) + gridLayout.cellSize / 2f;

        // 绘制网格单元边框线框
        Gizmos.DrawWireCube(cellCenter, gridLayout.cellSize);

        // 在网格上方显示调试信息标签
        UnityEditor.Handles.Label(cellCenter + Vector3.up * 0.5f,
            $"Grid: {gridModel.CurrentGridCoord}\nCanPlace: {buildingModel.CanPlaceAtCurrentPosition}");

        // 绘制从摄像机到目标点的连线（黄色）
        if (mainCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(mainCamera.transform.position, gridModel.CurrentWorldPosition);
        }
    }
#endif
}
