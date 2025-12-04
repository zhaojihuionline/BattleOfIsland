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
            
            // 从配置表获取物品信息（包括Category、Quality等）
            try
            {
                var itemConfig = CfgMgr.Instance.Tables.TbItem.Get(serverItem.ItemId);
                if (itemConfig != null)
                {
                    // 填充配置表静态数据
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
                    
                    // 加载资源
                    ResLoader loader = ResLoader.Allocate();
                    
                    try
                    {
                        // 加载物品图标（使用配置表中的 ItemIcon0）
                        if (!string.IsNullOrEmpty(itemConfig.ItemIcon0))
                        {
                            // 尝试加载图标，如果失败则使用默认图标
                            try
                            {
                                itemData.IconSprite = loader.LoadSync<Sprite>(itemConfig.ItemIcon0);
                            }
                            catch
                            {
                                // 如果配置的图标路径加载失败，使用默认图标
                                // itemData.IconSprite = loader.LoadSync<Sprite>("ui_icon_@3x");
                            }
                        }
                        else
                        {
                            // 如果没有配置图标，使用默认图标
                            // itemData.IconSprite = loader.LoadSync<Sprite>("ui_icon_@3x");
                        }
                        
                        // 根据品质加载对应的品质背景图片
                        string qualityPath = GetQualitySpritePath(itemConfig.Quality);
                        
                        // 对于有空格的文件名，尝试多个可能的路径
                        string[] tryPaths = null;
                        tryPaths = new string[] { qualityPath };
                        
                        itemData.QualitySprite = TryLoadSprite(loader, tryPaths);
                        
                        if (itemData.QualitySprite == null)
                        {
                            Debug.LogError($"BagItemConverter: 品质背景加载失败，尝试的路径={string.Join(", ", tryPaths)}, ItemId={serverItem.ItemId}, Quality={itemConfig.Quality}");
                        }
                        else
                        {
                            Debug.Log($"BagItemConverter: 成功加载品质背景，路径={qualityPath}, ItemId={serverItem.ItemId}, Quality={itemConfig.Quality}");
                        }
                        
                        // 背景图片暂时使用品质图片（如果需要单独的背景，可以后续扩展）
                        // itemData.BackgroundSprite = itemData.QualitySprite;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"BagItemConverter: 加载资源失败 ItemId={serverItem.ItemId}, Error={ex.Message}");
                    }
                    finally
                    {
                        // 注意：ResLoader 需要在适当的时候回收，但这里不能立即回收
                        // 因为 Sprite 资源可能还在使用中。可以考虑在 BagItemView 销毁时回收
                        // 或者使用全局的 ResLoader 管理
                        // loader.Recycle2Cache();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"BagItemConverter: 无法获取物品配置 ItemId={serverItem.ItemId}, Error={ex.Message}");
            }
            
            return itemData;
        }
    }
}

