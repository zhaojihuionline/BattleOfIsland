using UnityEngine;
using UnityEngine.UI;
using TMPro;
using cfg;
using QFramework;

namespace QFramework.UI
{
    /// <summary>
    /// 奖励项单行显示组件
    /// 用于显示 OneLineItem 预制体中的奖励信息
    /// </summary>
    public class OneLineItemView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image qualityFrameImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Toggle selectToggle;  // Choice 类型使用
        [SerializeField] private GameObject weightBadge;  // 权重显示（Random 类型使用）
        [SerializeField] private TextMeshProUGUI weightText;  // 权重文本
        
        private cfg.RewardDetai currentRewardDetail;
        private bool isSelected = false;
        
        /// <summary>
        /// 是否被选中（Choice 类型使用）
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            private set
            {
                isSelected = value;
                if (selectToggle != null)
                {
                    selectToggle.isOn = value;
                }
            }
        }
        
        /// <summary>
        /// 奖励详情数据
        /// </summary>
        public cfg.RewardDetai RewardDetail => currentRewardDetail;
        
        /// <summary>
        /// 选择状态变化回调（Choice 类型使用）
        /// </summary>
        public System.Action<OneLineItemView, bool> OnSelectionChanged;

        private void Awake()
        {
            if (selectToggle != null)
            {
                selectToggle.onValueChanged.AddListener(OnToggleValueChanged);
            }
        }

        private void OnDestroy()
        {
            if (selectToggle != null)
            {
                selectToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            }
        }

        /// <summary>
        /// 设置奖励数据
        /// </summary>
        /// <param name="rewardDetail">奖励详情</param>
        /// <param name="showWeight">是否显示权重（Random 类型）</param>
        /// <param name="enableSelection">是否启用选择功能（Choice 类型）</param>
        public void SetData(cfg.RewardDetai rewardDetail, bool showWeight = false, bool enableSelection = false)
        {
            currentRewardDetail = rewardDetail;
            
            if (rewardDetail == null || rewardDetail.Id_Ref == null)
            {
                ResetView();
                return;
            }

            var itemConfig = rewardDetail.Id_Ref;

            // 显示物品名称
            if (nameText != null)
            {
                nameText.text = itemConfig.Name ?? "";
            }

            // 显示数量
            if (countText != null)
            {
                countText.text = rewardDetail.Num.ToString();
            }

            // 加载图标
            if (iconImage != null)
            {
                if (!string.IsNullOrEmpty(itemConfig.ItemIcon0))
                {
                    try
                    {
                        var loader = ResLoader.Allocate();
                        iconImage.sprite = loader.LoadSync<Sprite>(itemConfig.ItemIcon0);
                        iconImage.enabled = iconImage.sprite != null;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"OneLineItemView: 加载图标失败 ItemId={itemConfig.ItemID}, Error={ex.Message}");
                        iconImage.enabled = false;
                    }
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            // 加载品质框
            if (qualityFrameImage != null)
            {
                string qualityPath = GetQualitySpritePath(itemConfig.Quality);
                try
                {
                    var loader = ResLoader.Allocate();
                    qualityFrameImage.sprite = loader.LoadSync<Sprite>(qualityPath);
                    qualityFrameImage.enabled = qualityFrameImage.sprite != null;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"OneLineItemView: 加载品质框失败 Quality={itemConfig.Quality}, Error={ex.Message}");
                    qualityFrameImage.enabled = false;
                }
            }

            // 显示权重（Random 类型）
            if (weightBadge != null)
            {
                weightBadge.SetActive(showWeight);
            }
            if (weightText != null)
            {
                weightText.text = showWeight ? $"权重: {rewardDetail.Weight}" : "";
            }

            // 选择功能（Choice 类型）
            if (selectToggle != null)
            {
                selectToggle.gameObject.SetActive(enableSelection);
                if (enableSelection)
                {
                    IsSelected = false;  // 重置选择状态
                }
            }
        }

        /// <summary>
        /// 设置选中状态（Choice 类型使用）
        /// </summary>
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
        }

        private void OnToggleValueChanged(bool isOn)
        {
            if (isOn != isSelected)
            {
                IsSelected = isOn;
                OnSelectionChanged?.Invoke(this, isOn);
            }
        }

        private void ResetView()
        {
            if (nameText != null) nameText.text = "";
            if (countText != null) countText.text = "";
            if (iconImage != null) iconImage.enabled = false;
            if (qualityFrameImage != null) qualityFrameImage.enabled = false;
            if (weightText != null) weightText.text = "";
            if (weightBadge != null) weightBadge.SetActive(false);
            if (selectToggle != null)
            {
                selectToggle.gameObject.SetActive(false);
                selectToggle.isOn = false;
            }
            isSelected = false;
        }

        /// <summary>
        /// 根据品质枚举获取对应的品质背景图片资源路径
        /// </summary>
        private static string GetQualitySpritePath(Enum_ItemQuality quality)
        {
            switch (quality)
            {
                case Enum_ItemQuality.None:
                case Enum_ItemQuality.NLevel:
                    return "ui_common_box_bo_grey";
                case Enum_ItemQuality.RLevel:
                    return "ui_common_box_bo_green";
                case Enum_ItemQuality.SRLevel:
                    return "ui_common_box_bg_blue";
                case Enum_ItemQuality.SSRLevel:
                    return "ui_common_box_bo_purple";
                case Enum_ItemQuality.SSSRLevel:
                    return "ui_common_box_bo_orange";
                case Enum_ItemQuality.URLevel:
                    return "ui_common_box_bo_red";
                default:
                    return "ui_common_box_bo_grey";
            }
        }
    }
}

