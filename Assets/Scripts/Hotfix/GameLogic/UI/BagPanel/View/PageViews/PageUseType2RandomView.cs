using UnityEngine;
using UnityEngine.UI;
using TMPro;
using cfg;

namespace QFramework.UI
{
    /// <summary>
    /// PageUseType2-Random 的视图组件
    /// 对应 UseType = CanUse (2) 且 RewardType = Random 的物品
    /// </summary>
    public class PageUseType2RandomView : BagPageViewBase
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Transform rewardContainer;  // 奖励显示容器
        
        // 可以根据实际需要添加更多节点引用
        
        private BagItemData currentItemData;

        private void Awake()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
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

            // 显示奖励信息（随机奖励）- 需要查询 Reward 配置表
            if (itemData.RewardID > 0)
            {
                var rewardConfig = CfgMgr.Instance.Tables.TbReward.Get(itemData.RewardID);
                if (rewardConfig != null)
                {
                    // TODO: 根据 rewardConfig.RewardDetail 显示随机奖励列表
                }
            }
        }

        private void ResetView()
        {
            if (titleText != null) titleText.text = "";
            if (descriptionText != null) descriptionText.text = "";
            if (iconImage != null) iconImage.enabled = false;
        }

        private void OnConfirmButtonClicked()
        {
            if (currentItemData == null) return;
            
            Debug.Log($"PageUseType2Random: 确认使用物品 {currentItemData.ItemId}");
            // 发送使用物品的命令
            // this.SendCommand(new UseBagItemCommand(currentItemData.BagId, 1));
        }
    }
}

