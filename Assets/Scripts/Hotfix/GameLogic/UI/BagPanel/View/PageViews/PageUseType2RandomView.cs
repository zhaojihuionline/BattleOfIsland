using UnityEngine;
using UnityEngine.UI;
using TMPro;
using cfg;
using QFramework;
using System.Collections.Generic;

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
        [SerializeField] private Image qualityFrameImage;  // 品质框
        [SerializeField] private TextMeshProUGUI countText;  // 数量文本
        [SerializeField] private AdjusterView adjusterView;  // 数量调节器
        [SerializeField] private TextMeshProUGUI randomHintText;  // 随机提示文本
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button previewButton;  // 预览按钮（弹出随机奖励详情）

        [Header("Probability Popup")]
        [SerializeField] private GameObject probabilityRoot;  // popup/Probability 根节点
        [SerializeField] private Transform probabilityContent;  // 概率列表 Content
        [SerializeField] private Button probabilityCloseButton;
        [SerializeField] private GameObject probabilityItemPrefab;  // OneLineProp 预制体
        
        private BagItemData currentItemData;
        private int useCount = 1;  // 当前选择的使用数量
        private List<OneLinePropView> probabilityItemViews = new List<OneLinePropView>();

        private void Awake()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            }

            if (previewButton != null)
            {
                previewButton.onClick.AddListener(OnPreviewButtonClicked);
            }

            if (adjusterView != null)
            {
                adjusterView.OnValueChanged += OnAdjusterValueChanged;
            }

            if (probabilityCloseButton != null)
            {
                probabilityCloseButton.onClick.AddListener(HideProbabilityPopup);
            }
            else if (probabilityRoot != null)
            {
                // 如果概率弹窗内部的关闭按钮未直接引用，可尝试在子节点中查找
                var closeBtn = probabilityRoot.transform.Find("Button-Close")?.GetComponent<Button>();
                if (closeBtn != null)
                {
                    probabilityCloseButton = closeBtn;
                    probabilityCloseButton.onClick.AddListener(HideProbabilityPopup);
                }
            }
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
            }

            if (previewButton != null)
            {
                previewButton.onClick.RemoveListener(OnPreviewButtonClicked);
            }

            if (adjusterView != null)
            {
                adjusterView.OnValueChanged -= OnAdjusterValueChanged;
            }

            if (probabilityCloseButton != null)
            {
                probabilityCloseButton.onClick.RemoveListener(HideProbabilityPopup);
            }

            ClearProbabilityList();
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

            // 显示随机奖励提示信息
            if (randomHintText != null)
            {
                randomHintText.text = "使用后将随机获得以下奖励之一";
            }

            // 显示奖励信息（随机奖励）- 需要查询 Reward 配置表
            if (itemData.RewardID > 0)
            {
                var rewardConfig = CfgMgr.Instance.Tables.TbReward.Get(itemData.RewardID);
                if (rewardConfig != null)
                {
                    // 随机奖励详情通过按钮弹出界面提示（暂不实现）
                    // 这里只显示提示文本
                }
            }
        }

        private void ResetView()
        {
            if (titleText != null) titleText.text = "";
            if (descriptionText != null) descriptionText.text = "";
            if (iconImage != null) iconImage.enabled = false;
            if (qualityFrameImage != null) qualityFrameImage.enabled = false;
            if (countText != null) countText.text = "";
            if (randomHintText != null) randomHintText.text = "";
            if (adjusterView != null)
            {
                adjusterView.SetRange(1, 1);
                adjusterView.SetValue(1);
            }
            useCount = 1;

            HideProbabilityPopup();
        }

        private void OnAdjusterValueChanged(int value)
        {
            useCount = value;
        }

        private void OnPreviewButtonClicked()
        {
            if (currentItemData == null || currentItemData.RewardID <= 0)
            {
                Debug.LogWarning("PageUseType2Random: 当前物品没有随机奖励配置，无法展示概率");
                return;
            }

            var rewardConfig = CfgMgr.Instance.Tables.TbReward.Get(currentItemData.RewardID);
            if (rewardConfig == null)
            {
                Debug.LogWarning($"PageUseType2Random: Reward 配置不存在 RewardID={currentItemData.RewardID}");
                return;
            }

            ShowProbabilityPopup(rewardConfig);
        }

        private void OnConfirmButtonClicked()
        {
            if (currentItemData == null) return;
            
            if (useCount <= 0 || useCount > currentItemData.Count)
            {
                Debug.LogWarning($"PageUseType2Random: 使用数量无效 {useCount}, 物品数量: {currentItemData.Count}");
                return;
            }

            Debug.Log($"PageUseType2Random: 确认使用物品 {currentItemData.ItemId}, 数量: {useCount}");
            // 发送使用物品的命令
            this.SendCommand(new UseBagItemCommand(currentItemData.BagId, useCount));
        }

        #region Probability Popup

        private void ShowProbabilityPopup(cfg.Reward rewardConfig)
        {
            if (probabilityRoot == null)
            {
                Debug.LogWarning("PageUseType2Random: probabilityRoot 未设置，无法展示概率弹窗");
                return;
            }

            probabilityRoot.SetActive(true);
            RefreshProbabilityList(rewardConfig);
        }

        private void HideProbabilityPopup()
        {
            if (probabilityRoot != null)
            {
                probabilityRoot.SetActive(false);
            }

            ClearProbabilityList();
        }

        private void RefreshProbabilityList(cfg.Reward rewardConfig)
        {
            ClearProbabilityList();

            if (probabilityContent == null || probabilityItemPrefab == null)
            {
                Debug.LogWarning("PageUseType2Random: probabilityContent 或 probabilityItemPrefab 未设置");
                return;
            }

            if (rewardConfig.RewardDetail == null || rewardConfig.RewardDetail.Count == 0)
            {
                return;
            }

            int totalWeight = 0;
            foreach (var detail in rewardConfig.RewardDetail)
            {
                if (detail != null && detail.Weight > 0)
                {
                    totalWeight += detail.Weight;
                }
            }

            foreach (var rewardDetail in rewardConfig.RewardDetail)
            {
                if (rewardDetail?.Id_Ref == null) continue;

                var itemObj = GameObject.Instantiate(probabilityItemPrefab, probabilityContent);
                var itemView = itemObj.GetComponent<OneLinePropView>();

                if (itemView != null)
                {
                    itemView.SetData(rewardDetail, totalWeight);
                    probabilityItemViews.Add(itemView);
                }
                else
                {
                    Debug.LogError("PageUseType2Random: OneLineProp 预制体缺少 OneLinePropView 组件！");
                    GameObject.Destroy(itemObj);
                }
            }
        }

        private void ClearProbabilityList()
        {
            foreach (var view in probabilityItemViews)
            {
                if (view != null && view.gameObject != null)
                {
                    GameObject.Destroy(view.gameObject);
                }
            }
            probabilityItemViews.Clear();
        }

        #endregion
    }
}

