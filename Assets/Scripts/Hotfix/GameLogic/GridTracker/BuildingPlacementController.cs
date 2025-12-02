using GAME.QF;
using QFramework;
using UniRx;
using UnityEngine;

/// <summary>
/// 建筑放置控制器 - 管理建筑预览、碰撞检测和用户输入
/// </summary>
public class BuildingPlacementController : MonoBehaviour, IController
{
    [Header("长按设置")]
    public float longPressDuration = 1.0f; // 长按时间阈值
    private float pressStartTime;
    private bool isPressing;
    private GameObject pressedBuilding; // 当前按下的建筑

    [Header("测试用建筑预制体")]
    public GameObject[] testBuildingPrefabs; // 用于测试的建筑预制体数组

    [Header("预览颜色设置")]
    public Color canPlaceColor = Color.green;     // 可放置时的预览颜色（绿色）
    public Color cannotPlaceColor = Color.red;    // 不可放置时的预览颜色（红色）

    BuildingPlacementModel bModel;
    GridTrackerModel gModel;
    IGridTrackerSystem gSystem;
    private CompositeDisposable disposables = new CompositeDisposable(); // Rx订阅管理

    // 长按检测相关
    private LayerMask buildingLayerMask;

    private void Start()
    {
        bModel = this.GetModel<BuildingPlacementModel>();
        gModel = this.GetModel<GridTrackerModel>();
        gSystem = this.GetSystem<IGridTrackerSystem>();

        // 初始化建筑层掩码
        buildingLayerMask = 1 << LayerMask.NameToLayer("Build");

        SetupEventSubscriptions(); // 设置事件监听
    }

    private void Update()
    {
        HandleBuildingSelection(); // 处理建筑选择输入
        HandlePlacementInput();    // 处理放置确认/取消输入

        // 只有当不在放置状态时才进行长按检测
        if (!bModel.IsPlacingBuilding)
        {
            HandleLongPressDetection(); // 处理建筑长按选择
        }
    }

    /// <summary>
    /// 每帧更新预览位置和验证状态
    /// </summary>
    private void OnUpdate()
    {
        if (!bModel.IsPlacingBuilding) return;

        UpdatePreviewPosition();     // 更新预览建筑的位置
    }

    /// <summary>
    /// 设置事件订阅
    /// </summary>
    private void SetupEventSubscriptions()
    {
        // 订阅建筑放置取消事件
        this.RegisterEvent<BuildingPlacementCanceledEvent>(OnBuildingPlacementCanceled)
            .UnRegisterWhenCurrentSceneUnloaded();

        // 预览模式监听
        this.RegisterEvent<StartBuildingPlacementEvent>(e =>
        {
            StartBuildingPlacement();
        }).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    /// <summary>
    /// 开始建筑放置
    /// </summary>
    public void StartBuildingPlacement()
    {
        OnUpdate(); // 立即更新一次
        disposables.Clear();

        // 每5帧更新一次预览（性能优化）
        Observable.EveryUpdate()
        .SampleFrame(4)
        .Subscribe(_ => OnUpdate())
        .AddTo(disposables);
    }

    /// <summary>
    /// 建筑放置取消事件处理
    /// </summary>
    private void OnBuildingPlacementCanceled(BuildingPlacementCanceledEvent e)
    {
        disposables.Clear(); // 清理订阅
    }

    /// <summary>
    /// 更新预览建筑位置（跟随网格）
    /// </summary>
    private void UpdatePreviewPosition()
    {
        if (bModel.CurrentBuildingPrefab == null) return;

        var gridLayout = gSystem.GetGridLayout();

        if (gridLayout == null) return;

        // 获取网格中心位置并计算最终位置
        Vector3 gridCenter = gSystem.GetGridCenterPosition(gModel.CurrentGridCoord);
        Vector3 finalPosition = gridCenter + bModel.buildingOffset;
        bModel.CurrentBuildingPrefab.transform.position = finalPosition;
    }

    /// <summary>
    /// 处理建筑选择输入（数字键1-9）
    /// </summary>
    private void HandleBuildingSelection()
    {
        for (int i = 0; i < testBuildingPrefabs.Length; i++)
        {
            // 检测数字键1-9按下，选择对应的建筑
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && testBuildingPrefabs[i] != null)
            {
                this.SendCommand<SelectBuildingCommand>(new SelectBuildingCommand(testBuildingPrefabs[i], true));
            }
        }
    }

    /// <summary>
    /// 处理放置输入
    /// </summary>
    private void HandlePlacementInput()
    {
        var buildingModel = this.GetModel<BuildingPlacementModel>();
        if (!buildingModel.IsPlacingBuilding) return; // 不在放置状态时忽略输入

        if (Input.GetMouseButtonUp(0)) // 鼠标左键点击确认放置
        {
            this.SendCommand(new ConfirmBuildingPlacementCommand());
        }
        else if (Input.GetMouseButtonDown(1)) // 鼠标右键点击取消放置
        {
            this.SendCommand(new CancelBuildingPlacementCommand());
        }
    }

    /// <summary>
    /// 处理建筑长按选择
    /// </summary>
    private void HandleLongPressDetection()
    {
        // 检查是否有触摸或鼠标按下（支持多设备）
        bool inputStarted = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

        if (inputStarted)
        {
            // 开始长按计时
            var hitBuilding = RaycastToBuilding();
            if (hitBuilding != null)
            {
                isPressing = true;
                pressedBuilding = hitBuilding;
                pressStartTime = Time.time;
                Debug.Log($"开始长按检测: {hitBuilding.name}");
            }
        }

        // 检查是否持续按下（鼠标或触摸）
        bool inputHeld = Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Stationary);

        if (inputHeld && isPressing)
        {
            // 检查长按时间
            if (Time.time - pressStartTime >= longPressDuration)
            {
                OnLongPressDetected(pressedBuilding);
                isPressing = false;
            }
        }

        // 检查输入结束（鼠标抬起或触摸结束）
        bool inputEnded = Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);

        if (inputEnded && isPressing)
        {
            // 取消长按
            Debug.Log("长按取消");
            isPressing = false;
            pressedBuilding = null;
        }
    }

    /// <summary>
    /// 射线检测建筑（支持鼠标和触摸）
    /// </summary>
    private GameObject RaycastToBuilding()
    {
        Vector2 inputPosition = GetInputPosition();
        Ray ray = Camera.main.ScreenPointToRay(inputPosition);

        // 调试信息
        int buildLayer = LayerMask.NameToLayer("Build");
        Debug.Log($"🔍 射线检测调试:");
        Debug.Log($"  - Build层级索引: {buildLayer}");
        Debug.Log($"  - LayerMask值: {buildingLayerMask}");
        Debug.Log($"  - 输入位置: {inputPosition}");

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, buildingLayerMask))
        {
            Debug.Log($"🎯 检测到建筑: '{hit.collider.gameObject.name}' (层级: {hit.collider.gameObject.layer})");
            return hit.collider.gameObject;
        }
        else
        {
            Debug.Log("❌ 未检测到建筑");
            return null;
        }
    }

    /// <summary>
    /// 获取输入位置（支持鼠标和触摸）
    /// </summary>
    private Vector2 GetInputPosition()
    {
        if (Input.touchCount > 0)
        {
            return Input.GetTouch(0).position;
        }
        else
        {
            return Input.mousePosition;
        }
    }

    /// <summary>
    /// 长按成功处理
    /// </summary>
    private void OnLongPressDetected(GameObject building)
    {
        Debug.Log($"🎯 长按选择建筑: {building.name}");
        this.SendCommand(new SelectBuildingCommand(building, false));

        // 重置状态
        isPressing = false;
        pressedBuilding = null;
    }

    // 在所有Controller中统一模式
    private void OnDestroy()
    {
        disposables?.Clear();
        disposables?.Dispose();
    }

    public IArchitecture GetArchitecture() => GridTrackerApp.Interface;
}