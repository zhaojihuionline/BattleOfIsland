using System.Collections.Generic;

namespace QFramework.UI
{
    /// <summary>
    /// 背包物品更新事件
    /// </summary>
    public struct BagItemsUpdatedEvent
    {
        /// <summary>
        /// Tab索引
        /// </summary>
        public int TabIndex;
        
        /// <summary>
        /// 物品列表
        /// </summary>
        public List<BagItemData> Items;
    }
}

