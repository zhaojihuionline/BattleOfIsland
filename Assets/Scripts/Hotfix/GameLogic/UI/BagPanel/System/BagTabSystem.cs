using QFramework;

namespace QFramework.UI
{
    /// <summary>
    /// 背包Tab系统接口
    /// </summary>
    public interface IBagTabSystem : ISystem
{
    /// <summary>
    /// 切换Tab
    /// </summary>
    void SwitchTab(int tabIndex);
    
    /// <summary>
    /// 当前选中的Tab索引
    /// </summary>
    int CurrentTabIndex { get; }
}

/// <summary>
/// 背包Tab系统实现
/// </summary>
public class BagTabSystem : AbstractSystem, IBagTabSystem
{
    public int CurrentTabIndex { get; private set; } = 0;
    
    protected override void OnInit()
    {
        // 初始化逻辑
    }
    
    public void SwitchTab(int tabIndex)
    {
        if (CurrentTabIndex == tabIndex) return;
        
        int oldIndex = CurrentTabIndex;
        CurrentTabIndex = tabIndex;
        
        // 发送事件通知UI更新（System层只发送Event，Command由Controller层发送）
        this.SendEvent(new BagTabChangedEvent 
        { 
            OldIndex = oldIndex, 
            NewIndex = tabIndex 
        });
    }
}
}
