using UnityEngine;
using UnityEngine.UI;
using TMPro;
using cfg;

namespace QFramework.UI
{
    /// <summary>
    /// PageUseType3 的视图组件
    /// 对应 UseType = JumpUse (3) 的物品
    /// </summary>
    public class PageUseType3View : BagPageViewBase
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button jumpButton;
        
        // 可以根据实际需要添加更多节点引用
        
        private BagItemData currentItemData;

        private void Awake()
        {
            if (jumpButton != null)
            {
                jumpButton.onClick.AddListener(OnJumpButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (jumpButton != null)
            {
                jumpButton.onClick.RemoveListener(OnJumpButtonClicked);
            }
        }

        public override void RefreshData(BagItemData itemData)
        {
            currentItemData = itemData;
            
            if (itemData == null)
            {
                ResetView();
                return;
            }

            // 使用 BagItemData 中已存储的配置表数据，避免重复查询
            // 更新显示内容
            if (titleText != null)
            {
                titleText.text = itemData.ItemName ?? "";
            }

            if (descriptionText != null)
            {
                descriptionText.text = itemData.Description ?? "";
            }

            if (iconImage != null)
            {
                iconImage.sprite = itemData.IconSprite;
                iconImage.enabled = itemData.IconSprite != null;
            }
        }

        private void ResetView()
        {
            if (titleText != null) titleText.text = "";
            if (descriptionText != null) descriptionText.text = "";
            if (iconImage != null) iconImage.enabled = false;
        }

        private void OnJumpButtonClicked()
        {
            if (currentItemData == null) return;
            
            Debug.Log($"PageUseType3: 跳转物品 {currentItemData.ItemId}");
            // TODO: 根据物品配置的跳转目标执行跳转逻辑
            // 可能需要根据 itemConfig 中的跳转参数来决定跳转到哪个界面
        }
    }
}

