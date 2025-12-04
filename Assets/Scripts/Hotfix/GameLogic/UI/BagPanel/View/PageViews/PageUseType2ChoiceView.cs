using UnityEngine;
using UnityEngine.UI;
using TMPro;
using cfg;
using System.Collections.Generic;
using QFramework;

namespace QFramework.UI
{
    /// <summary>
    /// PageUseType2-Choice 的视图组件
    /// 对应 UseType = CanUse (2) 且 RewardType = Choice 的物品
    /// </summary>
    public class PageUseType2ChoiceView : BagPageViewBase
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image qualityFrameImage;  // 品质框
        [SerializeField] private TextMeshProUGUI countText;  // 数量文本
        [SerializeField] private AdjusterView adjusterView;  // 数量调节器
        [SerializeField] private TextMeshProUGUI choiceHintText;  // 选择提示文本
        [SerializeField] private Button confirmButton;
        [SerializeField] private Transform rewardContainer;  // 奖励选择容器
        [SerializeField] private GameObject oneLineItemPrefab;  // OneLineItem 预制体
        
        private BagItemData currentItemData;
        private List<OneLineItemView> rewardItemViews = new List<OneLineItemView>();  // 奖励项视图列表
        private List<int> selectedRewardIds = new List<int>();  // 选中的奖励ID列表
        private cfg.Reward currentRewardConfig;
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
                countText.text = $"数量: {itemData.Count}";
            }

            // 更新数量调节器
            if (adjusterView != null)
            {
                adjusterView.SetRange(1, itemData.Count);
                useCount = Mathf.Min(useCount, itemData.Count);
                adjusterView.SetValue(useCount);
            }

            // 显示选择提示
            if (choiceHintText != null)
            {
                choiceHintText.text = "请选择你想要的奖励（可多选）：";
            }

            // 显示奖励选择列表（自选奖励）- 需要查询 Reward 配置表
            selectedRewardIds.Clear();
            if (itemData.RewardID > 0)
            {
                currentRewardConfig = CfgMgr.Instance.Tables.TbReward.Get(itemData.RewardID);
                if (currentRewardConfig != null)
                {
                    RefreshRewardChoiceList(currentRewardConfig);
                }
                else
                {
                    ClearRewardItems();
                }
            }
            else
            {
                ClearRewardItems();
            }

            UpdateConfirmButtonState();
        }

        /// <summary>
        /// 刷新可选择奖励列表
        /// </summary>
        private void RefreshRewardChoiceList(cfg.Reward rewardConfig)
        {
            ClearRewardItems();

            if (rewardContainer == null || oneLineItemPrefab == null)
            {
                Debug.LogWarning("PageUseType2ChoiceView: rewardContainer 或 oneLineItemPrefab 未设置");
                return;
            }

            if (rewardConfig.RewardDetail == null || rewardConfig.RewardDetail.Count == 0)
            {
                return;
            }

            // 显示所有可选择的奖励（带选择功能）
            foreach (var rewardDetail in rewardConfig.RewardDetail)
            {
                if (rewardDetail.Id_Ref == null) continue;

                var itemObj = GameObject.Instantiate(oneLineItemPrefab, rewardContainer);
                var itemView = itemObj.GetComponent<OneLineItemView>();

                if (itemView != null)
                {
                    itemView.SetData(rewardDetail, showWeight: false, enableSelection: true);
                    itemView.OnSelectionChanged += OnRewardItemSelectionChanged;
                    rewardItemViews.Add(itemView);
                }
                else
                {
                    Debug.LogError("PageUseType2ChoiceView: OneLineItem 预制体缺少 OneLineItemView 组件！");
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
                if (itemView != null)
                {
                    itemView.OnSelectionChanged -= OnRewardItemSelectionChanged;
                    if (itemView.gameObject != null)
                    {
                        GameObject.Destroy(itemView.gameObject);
                    }
                }
            }
            rewardItemViews.Clear();
        }

        /// <summary>
        /// 奖励项选择状态变化回调
        /// </summary>
        private void OnRewardItemSelectionChanged(OneLineItemView itemView, bool selected)
        {
            if (itemView?.RewardDetail == null) return;

            int rewardId = itemView.RewardDetail.Id;

            if (selected)
            {
                // 检查选择数量限制
                int maxSelect = currentRewardConfig?.RewardParam ?? 0;
                if (maxSelect > 0 && selectedRewardIds.Count >= maxSelect)
                {
                    // 已达到最大选择数量，取消选择
                    itemView.SetSelected(false);
                    Debug.LogWarning($"PageUseType2Choice: 最多只能选择 {maxSelect} 项奖励");
                    return;
                }

                if (!selectedRewardIds.Contains(rewardId))
                {
                    selectedRewardIds.Add(rewardId);
                }
            }
            else
            {
                selectedRewardIds.Remove(rewardId);
            }

            UpdateConfirmButtonState();
        }

        /// <summary>
        /// 更新确认按钮状态
        /// </summary>
        private void UpdateConfirmButtonState()
        {
            if (confirmButton != null)
            {
                // 至少需要选择一个奖励
                confirmButton.interactable = selectedRewardIds.Count > 0;
            }
        }

        private void ResetView()
        {
            if (titleText != null) titleText.text = "";
            if (descriptionText != null) descriptionText.text = "";
            if (iconImage != null) iconImage.enabled = false;
            if (qualityFrameImage != null) qualityFrameImage.enabled = false;
            if (countText != null) countText.text = "";
            if (choiceHintText != null) choiceHintText.text = "";
            if (adjusterView != null)
            {
                adjusterView.SetRange(1, 1);
                adjusterView.SetValue(1);
            }
            useCount = 1;
            selectedRewardIds.Clear();
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
                Debug.LogWarning($"PageUseType2Choice: 使用数量无效 {useCount}, 物品数量: {currentItemData.Count}");
                return;
            }

            if (selectedRewardIds.Count == 0)
            {
                Debug.LogWarning("PageUseType2Choice: 请至少选择一个奖励");
                return;
            }

            Debug.Log($"PageUseType2Choice: 确认使用物品 {currentItemData.ItemId}, 数量: {useCount}, 选中的奖励: {string.Join(", ", selectedRewardIds)}");
            // 发送使用物品的命令，需要传递选中的奖励ID列表
            this.SendCommand(new UseBagItemCommand(currentItemData.BagId, useCount, selectedRewardIds: new List<int>(selectedRewardIds)));
        }
    }
}

