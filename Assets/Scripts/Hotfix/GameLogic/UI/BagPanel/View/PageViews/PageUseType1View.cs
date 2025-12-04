using UnityEngine;
using UnityEngine.UI;
using TMPro;
using cfg;

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
        [SerializeField] private Button useButton;
        [SerializeField] private Button sellButton;
        
        // 可以根据实际需要添加更多节点引用
        
        private BagItemData currentItemData;

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

            // 根据物品状态更新按钮状态
            UpdateButtonStates(itemData);
        }

        private void ResetView()
        {
            if (titleText != null) titleText.text = "";
            if (descriptionText != null) descriptionText.text = "";
            if (iconImage != null) iconImage.enabled = false;
        }

        private void UpdateButtonStates(BagItemData itemData)
        {
            // 根据物品状态决定按钮是否可用
            bool canUse = itemData != null && itemData.IsInteractable;
            
            if (useButton != null)
            {
                useButton.interactable = canUse;
            }
            
            if (sellButton != null)
            {
                sellButton.interactable = canUse;
            }
        }

        private void OnUseButtonClicked()
        {
            if (currentItemData == null) return;
            
            Debug.Log($"PageUseType1: 使用物品 {currentItemData.ItemId}");
            // 发送使用物品的命令
            // this.SendCommand(new UseBagItemCommand(currentItemData.BagId, 1));
        }

        private void OnSellButtonClicked()
        {
            if (currentItemData == null) return;
            
            Debug.Log($"PageUseType1: 出售物品 {currentItemData.ItemId}");
            // 发送出售物品的命令
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

