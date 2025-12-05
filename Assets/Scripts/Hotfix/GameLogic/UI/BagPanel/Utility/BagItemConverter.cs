using UnityEngine;
using PitayaGame.GameSvr;
using cfg;
using QFramework;

namespace QFramework.UI
{
    /// <summary>
    /// 服务器数据转换为UI数据的工具类
    /// </summary>
    public static class BagItemConverter
    {
        /// <summary>
        /// 根据品质枚举获取对应的品质背景图片资源路径
        /// </summary>
        public static string GetQualitySpritePath(Enum_ItemQuality quality)
        {
            switch (quality)
            {
                case Enum_ItemQuality.None:
                case Enum_ItemQuality.NLevel:  // 普通(白色) -> 灰色
                    return "ui_common_box_bo_grey";
                case Enum_ItemQuality.RLevel:  // 良好(绿色)
                    return "ui_common_box_bo_green";
                case Enum_ItemQuality.SRLevel:  // 稀有(蓝色)
                    return "ui_common_box_bg_blue";
                case Enum_ItemQuality.SSRLevel:  // 史诗(紫色)
                    return "ui_common_box_bo_purple";
                case Enum_ItemQuality.SSSRLevel:  // 传说(橙色)
                    return "ui_common_box_bo_orange";
                case Enum_ItemQuality.URLevel:  // 神话(红色)
                    return "ui_common_box_bo_red";
                default:
                    return "ui_common_box_bo_grey";  // 默认灰色
            }
        }
        
        /// <summary>
        /// 尝试加载资源，支持多个备选路径
        /// </summary>
        private static Sprite TryLoadSprite(ResLoader loader, params string[] paths)
        {
            foreach (var path in paths)
            {
                try
                {
                    var sprite = loader.LoadSync<Sprite>(path);
                    if (sprite != null)
                    {
                        return sprite;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"BagItemConverter: 尝试加载路径失败 {path}, Error={ex.Message}");
                }
            }
            return null;
        }

        /// <summary>
        /// 根据配置表填充静态数据和资源
        /// </summary>
        private static void ApplyItemConfig(BagItemData itemData)
        {
            if (itemData == null) return;

            try
            {
                var itemConfig = CfgMgr.Instance.Tables.TbItem.Get(itemData.ItemId);
                if (itemConfig == null) return;

                itemData.ItemName = itemConfig.Name ?? "";
                itemData.Description = itemConfig.Description ?? "";
                itemData.Quality = itemConfig.Quality;
                itemData.ItemType = itemConfig.ItemType;
                itemData.ItemSubType = itemConfig.ItemSubType;
                itemData.Category = itemConfig.Category;
                itemData.UseType = itemConfig.UseType;
                itemData.UseLevel = itemConfig.UseLevel;
                itemData.UseMax = itemConfig.UseMax;
                itemData.RewardID = itemConfig.RewardID;
                itemData.TipsID = itemConfig.TipsID;
                itemData.IsStackable = itemConfig.IsStackable;
                itemData.MaxStack = itemConfig.MaxStack;

                ResLoader loader = ResLoader.Allocate();

                try
                {
                    if (!string.IsNullOrEmpty(itemConfig.ItemIcon0))
                    {
                        try
                        {
                            itemData.IconSprite = loader.LoadSync<Sprite>(itemConfig.ItemIcon0);
                        }
                        catch
                        {
                            // 可以在此加载默认图标
                        }
                    }

                    string qualityPath = GetQualitySpritePath(itemConfig.Quality);
                    itemData.QualitySprite = TryLoadSprite(loader, qualityPath);

                    if (itemData.QualitySprite == null)
                    {
                        Debug.LogWarning($"BagItemConverter: 品质背景加载失败 Path={qualityPath}, ItemId={itemData.ItemId}, Quality={itemConfig.Quality}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"BagItemConverter: 加载资源失败 ItemId={itemData.ItemId}, Error={ex.Message}");
                }
                finally
                {
                    // loader.Recycle2Cache(); // 如需统一管理，可在此回收
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"BagItemConverter: 无法获取物品配置 ItemId={itemData.ItemId}, Error={ex.Message}");
            }
        }

        /// <summary>
        /// 将服务器 BagItem 转换为 UI 的 BagItemData
        /// </summary>
        public static BagItemData ConvertFromServer(BagItem serverItem)
        {
            var itemData = new BagItemData
            {
                BagId = serverItem.BagId,
                ItemId = serverItem.ItemId,
                Count = serverItem.Count,
                IsLocked = serverItem.Locked,
                IsInteractable = true,
                IsSelected = false,
                IsEquipped = false,
                IsNew = false,
                CooldownPercent = 0f,
                Category = cfg.Enum_Order.NONE  // 默认值
            };

            ApplyItemConfig(itemData);

            return itemData;
        }

        /// <summary>
        /// 将 RewardDelta 转换为 BagItemData
        /// 使用实际变化量（delta.After - delta.Before）作为获得的数量
        /// </summary>
        public static BagItemData CreateFromRewardDelta(RewardDelta delta)
        {
            if (delta?.Reward == null) return null;

            // 计算实际变化量（获得的数量）
            long actualChange = delta.After - delta.Before;
            if (actualChange <= 0) return null;  // 只处理获得的情况（数量增加）

            switch (delta.Reward.RewardDetailCase)
            {
                case RewardItem.RewardDetailOneofCase.Item:
                    // 对于物品奖励，使用实际变化量
                    var itemReward = delta.Reward.Item;
                    if (itemReward == null) return null;
                    return CreateFromSimpleReward(itemReward.ItemId, (int)actualChange, itemReward.BagId);

                case RewardItem.RewardDetailOneofCase.Currency:
                    // 对于货币，使用实际变化量
                    var currency = delta.Reward.Currency;
                    if (currency == null) return null;
                    return CreateFromSimpleReward(currency.ItemId, (int)actualChange);

                case RewardItem.RewardDetailOneofCase.Resource:
                    // 对于资源，使用实际变化量
                    var resource = delta.Reward.Resource;
                    if (resource == null) return null;
                    return CreateFromSimpleReward(resource.ItemId, (int)actualChange);

                default:
                    return null;
            }
        }

        /// <summary>
        /// 将 RewardItem 转换为 BagItemData
        /// </summary>
        public static BagItemData CreateFromRewardItem(RewardItem rewardItem)
        {
            if (rewardItem == null) return null;

            switch (rewardItem.RewardDetailCase)
            {
                case RewardItem.RewardDetailOneofCase.Item:
                    return CreateFromItemReward(rewardItem.Item);
                case RewardItem.RewardDetailOneofCase.Currency:
                    return CreateFromSimpleReward(rewardItem.Currency?.ItemId ?? 0, (int)(rewardItem.Currency?.Delta ?? 0));
                case RewardItem.RewardDetailOneofCase.Resource:
                    return CreateFromSimpleReward(rewardItem.Resource?.ItemId ?? 0, (int)(rewardItem.Resource?.Delta ?? 0));
                default:
                    return null;
            }
        }

        private static BagItemData CreateFromItemReward(ItemReward itemReward)
        {
            if (itemReward == null) return null;
            return CreateFromSimpleReward(itemReward.ItemId, (int)itemReward.Amount, itemReward.BagId);
        }

        private static BagItemData CreateFromSimpleReward(int itemId, int count, long bagId = 0)
        {
            if (itemId == 0 || count == 0) return null;

            var itemData = new BagItemData
            {
                BagId = bagId,
                ItemId = itemId,
                Count = count,
                IsLocked = false,
                IsInteractable = false,
                IsSelected = false,
                IsEquipped = false,
                IsNew = false,
                CooldownPercent = 0f,
                Category = cfg.Enum_Order.NONE
            };

            ApplyItemConfig(itemData);

            return itemData;
        }
    }
}

