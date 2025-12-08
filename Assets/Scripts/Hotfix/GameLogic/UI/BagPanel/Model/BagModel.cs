using System.Collections.Generic;
using System.Linq;
using QFramework;

namespace QFramework.UI
{
    /// <summary>
    /// 背包数据模型实现
    /// </summary>
    public class BagModel : AbstractModel, IBagModel
    {
        /// <summary>
        /// Tab索引 → 物品列表
        /// </summary>
        private Dictionary<int, List<BagItemData>> tabItems = new Dictionary<int, List<BagItemData>>();
        
        /// <summary>
        /// 已加载的Tab索引，用于判断是否需要从网络获取
        /// </summary>
        private HashSet<int> loadedTabs = new HashSet<int>();
        
        /// <summary>
        /// 背包ID → 物品数据（用于快速查找）
        /// </summary>
        private Dictionary<long, BagItemData> bagIdToItem = new Dictionary<long, BagItemData>();

        /// <summary>
        /// 背包ID → Tab索引
        /// </summary>
        private Dictionary<long, int> bagIdToTabIndex = new Dictionary<long, int>();
        
        protected override void OnInit()
        {
            // 初始化逻辑
        }
        
        public List<BagItemData> GetItemsByTab(int tabIndex)
        {
            if (tabItems.TryGetValue(tabIndex, out var items))
            {
                // 在同一分类(Category)下，道具首先按子类型ID升序排列，
                // 子类型ID相同的道具再按品质ID升序排列
                return items
                    .OrderBy(item => item.ItemSubType)    // 先按子类型升序
                    .ThenBy(item => item.Quality)         // 再按品质升序
                    .ToList();
            }
            return new List<BagItemData>();
        }
        
        public BagItemData GetItemByBagId(long bagId)
        {
            if (bagIdToItem.TryGetValue(bagId, out var item))
            {
                return item;
            }
            return null;
        }
        
        public BagItemData GetItemByItemId(int itemId, int tabIndex = -1)
        {
            if (tabIndex >= 0)
            {
                // 在指定Tab中查找
                if (tabItems.TryGetValue(tabIndex, out var items))
                {
                    return items.FirstOrDefault(item => item.ItemId == itemId);
                }
            }
            else
            {
                // 在所有Tab中查找
                foreach (var items in tabItems.Values)
                {
                    var item = items.FirstOrDefault(i => i.ItemId == itemId);
                    if (item != null) return item;
                }
            }
            return null;
        }
        
        public int GetItemCount(int itemId, int tabIndex = -1)
        {
            int count = 0;
            
            if (tabIndex >= 0)
            {
                // 在指定Tab中统计
                if (tabItems.TryGetValue(tabIndex, out var items))
                {
                    count = items.Where(item => item.ItemId == itemId).Sum(item => item.Count);
                }
            }
            else
            {
                // 在所有Tab中统计
                foreach (var items in tabItems.Values)
                {
                    count += items.Where(item => item.ItemId == itemId).Sum(item => item.Count);
                }
            }
            
            return count;
        }
        
        public bool HasItem(int itemId, int count = 1, int tabIndex = -1)
        {
            return GetItemCount(itemId, tabIndex) >= count;
        }
        
        public bool IsTabLoaded(int tabIndex)
        {
            return loadedTabs.Contains(tabIndex);
        }
        
        public void SetItemsByTab(int tabIndex, List<BagItemData> items)
        {
            // 清空旧数据
            if (tabItems.TryGetValue(tabIndex, out var oldItems))
            {
                // 从 bagIdToItem 中移除旧物品
                foreach (var item in oldItems)
                {
                    bagIdToItem.Remove(item.BagId);
                    bagIdToTabIndex.Remove(item.BagId);
                }
            }
            
            // 设置新数据并排序：先按子类型升序，再按品质升序
            var sortedItems = items
                .OrderBy(item => item.ItemSubType)
                .ThenBy(item => item.Quality)
                .ToList();
            
            tabItems[tabIndex] = sortedItems;
            
            // 更新 bagIdToItem 索引
            foreach (var item in sortedItems)
            {
                bagIdToItem[item.BagId] = item;
                bagIdToTabIndex[item.BagId] = tabIndex;
            }
            
            // 标记为已加载
            loadedTabs.Add(tabIndex);
        }
        
        public void ClearTab(int tabIndex)
        {
            if (tabItems.TryGetValue(tabIndex, out var items))
            {
                // 从 bagIdToItem 中移除
                foreach (var item in items)
                {
                    bagIdToItem.Remove(item.BagId);
                    bagIdToTabIndex.Remove(item.BagId);
                }
                tabItems.Remove(tabIndex);
            }
            
            loadedTabs.Remove(tabIndex);
        }
        
        public void UpdateItem(BagItemData item)
        {
            if (bagIdToItem.TryGetValue(item.BagId, out var oldItem))
            {
                // 更新数据
                oldItem.ItemId = item.ItemId;
                oldItem.Count = item.Count;
                oldItem.IconSprite = item.IconSprite;
                oldItem.BackgroundSprite = item.BackgroundSprite;
                oldItem.QualitySprite = item.QualitySprite;
                oldItem.IsSelected = item.IsSelected;
                oldItem.IsEquipped = item.IsEquipped;
                oldItem.IsNew = item.IsNew;
                oldItem.IsLocked = item.IsLocked;
                oldItem.IsInteractable = item.IsInteractable;
                oldItem.CooldownPercent = item.CooldownPercent;
            }
        }
        
        public void RemoveItem(long bagId)
        {
            if (bagIdToItem.TryGetValue(bagId, out var item))
            {
                // 从对应Tab的列表中移除
                foreach (var tabItemsList in tabItems.Values)
                {
                    if (tabItemsList.Remove(item))
                    {
                        break;
                    }
                }
                
                bagIdToItem.Remove(bagId);
                bagIdToTabIndex.Remove(bagId);
            }
        }
        
        public void AddItem(BagItemData item, int tabIndex)
        {
            if (!tabItems.ContainsKey(tabIndex))
            {
                tabItems[tabIndex] = new List<BagItemData>();
            }
            
            tabItems[tabIndex].Add(item);
            
            // 更新 bagIdToItem 索引
            bagIdToItem[item.BagId] = item;
            bagIdToTabIndex[item.BagId] = tabIndex;
        }

        public int GetTabIndexByBagId(long bagId)
        {
            if (bagIdToTabIndex.TryGetValue(bagId, out var tabIndex))
            {
                return tabIndex;
            }

            return -1;
        }
    }
}

