using UnityEngine;
using UnityEngine.UI;
using TMPro;
using cfg;

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
                countText.text = $"数量: {itemData.Count}";
            }

            // 显示等级要求
            if (levelRequirementText != null)
            {
                int playerHallLevel = GetPlayerHallLevel();
                levelRequirementText.text = $"需要建筑大厅等级: {requiredLevel}\n当前等级: {playerHallLevel}";
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
        }

        private void OnCloseButtonClicked()
        {
            // 关闭提示，可以隐藏当前 Page 或返回空状态
            Debug.Log("PageUseTypeX: 关闭等级不足提示");
        }

        /// <summary>
        /// 获取玩家建筑大厅等级
        /// TODO: 根据项目实际情况从 Model 或 System 获取
        /// </summary>
        private int GetPlayerHallLevel()
        {
            // 暂时返回固定值，后续需要从实际的 Model 或 System 获取
            // 例如：return this.GetModel<IBuildingModel>().GetHallLevel();
            return 1;  // 临时值
        }
    }
}

