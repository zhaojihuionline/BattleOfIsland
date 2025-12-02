using QFramework;

/// <summary>
/// 网格追踪应用程序架构 - 全局架构配置，注册所有模型和系统
/// </summary>
public class GridTrackerApp : Architecture<GridTrackerApp>
{
    protected override void Init()
    {
        // 注册模型和系统到QFramework架构中
        this.RegisterModel<GridTrackerModel>(new GridTrackerModel());          // 注册网格追踪数据模型
        this.RegisterSystem<IGridTrackerSystem>(new GridTrackerSystem());      // 注册网格追踪系统
        this.RegisterModel<BuildingPlacementModel>(new BuildingPlacementModel()); // 注册建筑放置模型
    }
}