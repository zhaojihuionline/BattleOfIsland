using UnityEngine;
using UnityEngine.UI;
using TMPro;
using cfg;
using QFramework;

namespace QFramework.UI
{
    /// <summary>
    /// PageUseTypeX 的视图组件
    /// 当玩家建筑大厅等级不足时显示的提示页面
    /// </summary>
    public class PageUseTypeXView : BagPageViewBase
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI levelRequirementText;  // 等级要求文本
        [SerializeField] private Image iconImage;
        [SerializeField] private Image qualityFrameImage;  // 品质框
        [SerializeField] private TextMeshProUGUI countText;  // 数量文本
        [SerializeField] private Button closeButton;

        private BagItemData currentItemData;
        private int requiredLevel;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
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
            requiredLevel = itemData.UseLevel;

            // 更新显示内容
            if (titleText != null)
            {
                titleText.text = !string.IsNullOrEmpty(itemData.ItemName) ? itemData.ItemName : "物品";
            }

            if (descriptionText != null)
            {
                descriptionText.text = "建筑大厅等级不足，无法使用此物品";
            }

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
                countText.text = $"<color=#FBC39A>已拥有: </color><color=#1FFF6C>{itemData.Count}</color>";
            }

            // 显示等级要求
            if (levelRequirementText != null)
            {
                // 通过 IController 接口获取 BagPanel，使用统一的 GetPlayerHallLevel 方法
                var bagPanel = GetComponentInParent<BagPanel>();
                int playerHallLevel = bagPanel != null ? bagPanel.GetPlayerHallLevel() : 1;
                levelRequirementText.text = $"需要大本营等级{playerHallLevel}/{requiredLevel}";
            }

            // 等级不够时隐藏关闭按钮
            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(false);
            }
        }

        private void ResetView()
        {
            if (titleText != null) titleText.text = "";
            if (descriptionText != null) descriptionText.text = "";
            if (levelRequirementText != null) levelRequirementText.text = "";
            if (iconImage != null) iconImage.enabled = false;
            if (qualityFrameImage != null) qualityFrameImage.enabled = false;
            if (countText != null) countText.text = "";

            // 重置时显示关闭按钮
            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(true);
            }
        }

        private void OnCloseButtonClicked()
        {
            // 关闭背包面板，返回主界面
            Debug.Log("PageUseTypeX: 关闭等级不足提示，关闭背包面板");
            UIKit.ClosePanel<BagPanel>();
            UIKit.ShowPanel<HomeMainPanel>();
        }
    }
}

