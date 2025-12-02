using UnityEngine;

namespace QFramework.UI
{
    /// <summary>
    /// 背包中用于渲染的道具数据
    /// </summary>
    public class BagItemData
    {
        public long BagId;  // 背包ID（服务器返回的唯一标识）
        public int ItemId;  // 物品配置ID
        public cfg.Enum_Order Category;  // 背包分类（用于tab页面过滤）
        public Sprite IconSprite;
        public Sprite BackgroundSprite;
        public Sprite QualitySprite;
        public int Count;
        public bool IsSelected;
        public bool IsEquipped;
        public bool IsNew;
        public bool IsLocked;
        public bool IsInteractable = true;
        /// <summary>
        /// 冷却遮罩百分比，0-1
        /// </summary>
        public float CooldownPercent;
    }
}

