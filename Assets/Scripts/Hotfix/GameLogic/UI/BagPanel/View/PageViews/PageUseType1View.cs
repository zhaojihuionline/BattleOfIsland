using UnityEngine;
using UnityEngine.UI;
using TMPro;
using cfg;
using QFramework;

namespace QFramework.UI
{
    /// <summary>
    /// PageUseType1 的视图组件
    /// 对应 UseType = DisplayUse (1) 的物品
    /// 负责管理 PageUseType1 内部的所有节点和业务逻辑
    /// </summary>
    public class PageUseType1View : BagPageViewBase
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image qualityFrameImage;  // 品质框
        [SerializeField] private TextMeshProUGUI countText;  // 数量文本
        [SerializeField] private AdjusterView adjusterView;  // 数量调节器
        [SerializeField] private Button useButton;
        [SerializeField] private Button sellButton;

        private BagItemData currentItemData;
        private int useCount = 1;  // 当前选择的使用数量

        private void Awake()
        {
            // 初始化按钮事件
            if (useButton != null)
            {
                useButton.onClick.AddListener(OnUseButtonClicked);
            }

            if (sellButton != null)
            {
                sellButton.onClick.AddListener(OnSellButtonClicked);
            }

            // 初始化数量调节器
            if (adjusterView != null)
            {
                adjusterView.OnValueChanged += OnAdjusterValueChanged;
            }
        }

        private void OnDestroy()
        {
            // 清理事件
            if (useButton != null)
            {
                useButton.onClick.RemoveListener(OnUseButtonClicked);
            }

            if (sellButton != null)
            {
                sellButton.onClick.RemoveListener(OnSellButtonClicked);
            }

            if (adjusterView != null)
            {
                adjusterView.OnValueChanged -= OnAdjusterValueChanged;
            }
        }

        public override void RefreshData(BagItemData itemData)
        {
            currentItemData = itemData;

            if (itemData == null)
            {
                // 数据为空时，隐藏或重置所有显示
                ResetView();
                return;
            }

            // 使用 BagItemData 中已存储的配置表数据，避免重复查询
            // 更新标题
            if (titleText != null)
            {
                titleText.text = !string.IsNullOrEmpty(itemData.ItemName) ? itemData.ItemName : "未知物品";
            }

            // 更新描述
            if (descriptionText != null)
            {
                descriptionText.text = itemData.Description ?? "";
            }

            // 更新图标
            if (iconImage != null)
            {
                iconImage.sprite = itemData.IconSprite;
                iconImage.enabled = itemData.IconSprite != null;
            }

            // 更新品质框
            if (qualityFrameImage != null)
            {
                qualityFrameImage.sprite = itemData.QualitySprite;
                qualityFrameImage.enabled = itemData.QualitySprite != null;
            }

            // 更新数量显示
            if (countText != null)
            {
                countText.text = $"<color=#FBC39A>已拥有:</color><color=#1FFF6C>{itemData.Count}</color>";
            }

            // 更新数量调节器
            if (adjusterView != null)
            {
                adjusterView.SetRange(1, itemData.Count);
                useCount = Mathf.Min(useCount, itemData.Count);
                adjusterView.SetValue(useCount);
            }

            // 根据物品状态更新按钮状态
            UpdateButtonStates(itemData);
        }

        private void ResetView()
        {
            if (titleText != null) titleText.text = "";
            if (descriptionText != null) descriptionText.text = "";
            if (iconImage != null) iconImage.enabled = false;
            if (qualityFrameImage != null) qualityFrameImage.enabled = false;
            if (countText != null) countText.text = "";
            if (adjusterView != null)
            {
                adjusterView.SetRange(1, 1);
                adjusterView.SetValue(1);
            }
            useCount = 1;
        }

        private void UpdateButtonStates(BagItemData itemData)
        {
            // 根据物品状态决定按钮是否可用
            bool canUse = itemData != null && itemData.IsInteractable && itemData.Count > 0;

            if (useButton != null)
            {
                useButton.interactable = canUse;
            }

            if (sellButton != null)
            {
                sellButton.interactable = canUse;
            }
        }

        private void OnAdjusterValueChanged(int value)
        {
            useCount = value;
        }

        private void OnUseButtonClicked()
        {
            if (currentItemData == null) return;

            if (useCount <= 0 || useCount > currentItemData.Count)
            {
                Debug.LogWarning($"PageUseType1: 使用数量无效 {useCount}, 物品数量: {currentItemData.Count}");
                return;
            }

            Debug.Log($"PageUseType1: 使用物品 {currentItemData.ItemId}, 数量: {useCount}");
            // 发送使用物品的命令
            this.SendCommand(new UseBagItemCommand(currentItemData.BagId, useCount));
        }

        private void OnSellButtonClicked()
        {
            if (currentItemData == null) return;

            if (useCount <= 0 || useCount > currentItemData.Count)
            {
                Debug.LogWarning($"PageUseType1: 出售数量无效 {useCount}, 物品数量: {currentItemData.Count}");
                return;
            }

            Debug.Log($"PageUseType1: 出售物品 {currentItemData.ItemId}, 数量: {useCount}");
            // 发送丢弃物品的命令（出售使用丢弃命令）
            this.SendCommand(new DiscardBagItemCommand(currentItemData.BagId, useCount));
        }

        protected override void OnShow()
        {
            base.OnShow();
            // Page 显示时的额外逻辑
        }

        protected override void OnHide()
        {
            base.OnHide();
            // Page 隐藏时的清理逻辑
        }
    }
}

