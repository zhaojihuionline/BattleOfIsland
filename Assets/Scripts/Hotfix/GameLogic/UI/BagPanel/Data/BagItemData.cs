using UnityEngine;

namespace QFramework.UI
{
    /// <summary>
    /// 背包中用于渲染的道具数据
    /// </summary>
    public class BagItemData
    {
        // ========== 服务器数据 ==========
        public long BagId;  // 背包ID（服务器返回的唯一标识）
        public int ItemId;  // 物品配置ID
        public int Count;  // 数量
        
        // ========== 配置表静态数据（从 TbItem 读取） ==========
        public string ItemName;  // 物品名称
        public string Description;  // 物品描述
        public cfg.Enum_ItemQuality Quality;  // 品质
        public cfg.Enum_ItemType ItemType;  // 物品类型
        public cfg.Enum_SubType ItemSubType;  // 子类型
        public cfg.Enum_Order Category;  // 背包分类（用于tab页面过滤）
        public cfg.Enum_UseType UseType;  // 使用类型
        public int UseLevel;  // 使用等级要求
        public int UseMax;  // 每天使用上限
        public int RewardID;  // 关联奖励ID
        public int TipsID;  // 提示ID
        public bool IsStackable;  // 能否堆叠
        public int MaxStack;  // 堆叠最大数量
        
        // ========== UI 资源 ==========
        public Sprite IconSprite;
        public Sprite BackgroundSprite;
        public Sprite QualitySprite;
        
        // ========== UI 状态 ==========
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

