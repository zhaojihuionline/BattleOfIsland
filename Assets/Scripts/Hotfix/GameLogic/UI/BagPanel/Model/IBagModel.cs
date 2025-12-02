using System.Collections.Generic;
using QFramework;

namespace QFramework.UI
{
    /// <summary>
    /// 背包数据模型接口
    /// </summary>
    public interface IBagModel : IModel
    {
        /// <summary>
        /// 获取指定 Tab 的物品列表
        /// </summary>
        List<BagItemData> GetItemsByTab(int tabIndex);
        
        /// <summary>
        /// 根据背包ID获取物品（跨Tab查询）
        /// </summary>
        BagItemData GetItemByBagId(long bagId);
        
        /// <summary>
        /// 根据物品ID获取物品（可指定Tab或全Tab搜索）
        /// </summary>
        BagItemData GetItemByItemId(int itemId, int tabIndex = -1);
        
        /// <summary>
        /// 获取指定物品的总数量
        /// </summary>
        int GetItemCount(int itemId, int tabIndex = -1);
        
        /// <summary>
        /// 检查是否有足够数量的物品
        /// </summary>
        bool HasItem(int itemId, int count = 1, int tabIndex = -1);
        
        /// <summary>
        /// 检查指定Tab是否已加载数据（用于判断是否需要网络请求）
        /// </summary>
        bool IsTabLoaded(int tabIndex);
        
        /// <summary>
        /// 设置指定 Tab 的物品列表（标记为已加载）
        /// </summary>
        void SetItemsByTab(int tabIndex, List<BagItemData> items);
        
        /// <summary>
        /// 清空指定 Tab 的物品（同时移除已加载标记）
        /// </summary>
        void ClearTab(int tabIndex);
        
        /// <summary>
        /// 更新物品数据（增量更新：使用后、丢弃后调用）
        /// </summary>
        void UpdateItem(BagItemData item);
        
        /// <summary>
        /// 移除物品（根据背包ID，增量更新）
        /// </summary>
        void RemoveItem(long bagId);
        
        /// <summary>
        /// 添加物品（增量更新）
        /// </summary>
        void AddItem(BagItemData item, int tabIndex);

        /// <summary>
        /// 根据 BagId 获取所属 Tab 索引（未找到返回 -1）
        /// </summary>
        int GetTabIndexByBagId(long bagId);
    }
}

