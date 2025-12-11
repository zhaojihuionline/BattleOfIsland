using UnityEngine;
using UnityEngine.UI;
using TMPro;
using cfg;
using System.Collections.Generic;
using QFramework;

namespace QFramework.UI
{
    /// <summary>
    /// PageUseType2-Fixed 的视图组件
    /// 对应 UseType = CanUse (2) 且 RewardType = Fixed 的物品
    /// </summary>
    public class PageUseType2FixedView : BagPageViewBase
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image qualityFrameImage;  // 品质框
        [SerializeField] private TextMeshProUGUI countText;  // 数量文本
        [SerializeField] private AdjusterView adjusterView;  // 数量调节器
        [SerializeField] private Button confirmButton;
        [SerializeField] private Transform rewardContainer;  // 奖励显示容器
        [SerializeField] private GameObject oneLineItemPrefab;  // OneLineItem 预制体
        
        private BagItemData currentItemData;
        private List<OneLineItemView> rewardItemViews = new List<OneLineItemView>();  // 奖励项视图列表
        private int useCount = 1;  // 当前选择的使用数量

        private void Awake()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            }

            if (adjusterView != null)
            {
                adjusterView.OnValueChanged += OnAdjusterValueChanged;
            }
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
            }

            if (adjusterView != null)
            {
                adjusterView.OnValueChanged -= OnAdjusterValueChanged;
            }

            ClearRewardItems();
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

            // 更新数量调节器
            if (adjusterView != null)
            {
                adjusterView.SetRange(1, itemData.Count);
                useCount = Mathf.Min(useCount, itemData.Count);
                adjusterView.SetValue(useCount);
            }

            // 显示奖励信息（固定奖励）- 需要查询 Reward 配置表
            if (itemData.RewardID > 0)
            {
                var rewardConfig = CfgMgr.Instance.Tables.TbReward.Get(itemData.RewardID);
                if (rewardConfig != null)
                {
                    RefreshRewardList(rewardConfig);
                }
            }
            else
            {
                ClearRewardItems();
            }
        }

        /// <summary>
        /// 刷新奖励列表
        /// </summary>
        private void RefreshRewardList(cfg.Reward rewardConfig)
        {
            ClearRewardItems();

            if (rewardContainer == null || oneLineItemPrefab == null)
            {
                Debug.LogWarning("PageUseType2FixedView: rewardContainer 或 oneLineItemPrefab 未设置");
                return;
            }

            if (rewardConfig.RewardDetail == null || rewardConfig.RewardDetail.Count == 0)
            {
                return;
            }

            // 显示所有固定奖励
            foreach (var rewardDetail in rewardConfig.RewardDetail)
            {
                if (rewardDetail.Id_Ref == null) continue;

                var itemObj = GameObject.Instantiate(oneLineItemPrefab, rewardContainer);
                var itemView = itemObj.GetComponent<OneLineItemView>();

                if (itemView != null)
                {
                    itemView.SetData(rewardDetail, showWeight: false, enableSelection: false);
                    rewardItemViews.Add(itemView);
                }
                else
                {
                    Debug.LogError("PageUseType2FixedView: OneLineItem 预制体缺少 OneLineItemView 组件！");
                    GameObject.Destroy(itemObj);
                }
            }
        }

        /// <summary>
        /// 清空奖励列表
        /// </summary>
        private void ClearRewardItems()
        {
            foreach (var itemView in rewardItemViews)
            {
                if (itemView != null && itemView.gameObject != null)
                {
                    GameObject.Destroy(itemView.gameObject);
                }
            }
            rewardItemViews.Clear();
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
            ClearRewardItems();
        }

        private void OnAdjusterValueChanged(int value)
        {
            useCount = value;
        }

        private void OnConfirmButtonClicked()
        {
            if (currentItemData == null) return;
            
            if (useCount <= 0 || useCount > currentItemData.Count)
            {
                Debug.LogWarning($"PageUseType2Fixed: 使用数量无效 {useCount}, 物品数量: {currentItemData.Count}");
                return;
            }

            Debug.Log($"PageUseType2Fixed: 确认使用物品 {currentItemData.ItemId}, 数量: {useCount}");
            // 发送使用物品的命令
            this.SendCommand(new UseBagItemCommand(currentItemData.BagId, useCount));
        }
    }
}

