using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using cfg;
using TMPro;

namespace QFramework.UI
{
    /// <summary>
    /// 概率弹窗组件
    /// 用于显示随机奖励的概率列表
    /// </summary>
    public class ProbabilityView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject rootObject;  // 弹窗根节点
        [SerializeField] private Transform contentContainer;  // 概率列表容器
        [SerializeField] private Button closeButton;  // 关闭按钮
        [SerializeField] private Button maskButton;  // 遮罩层按钮(点击关闭)
        [SerializeField] private GameObject itemPrefab;  // OneLineProp 预制体

        [SerializeField] private TextMeshProUGUI descText;  // 描述文本

        private List<OneLinePropView> itemViews = new List<OneLinePropView>();

        private void Awake()
        {
            if (rootObject == null)
            {
                Debug.LogError("ProbabilityView: rootObject 未设置");
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }

            if (maskButton != null)
            {
                maskButton.onClick.AddListener(OnMaskButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            }

            if (maskButton != null)
            {
                maskButton.onClick.RemoveListener(OnMaskButtonClicked);
            }

            ClearList();
        }

        /// <summary>
        /// 显示概率弹窗
        /// </summary>
        /// <param name="rewardConfig">奖励配置</param>
        public void Show(cfg.Reward rewardConfig)
        {
            if (rootObject == null)
            {
                Debug.LogWarning("ProbabilityView: rootObject 未设置");
                return;
            }

            if (rewardConfig.RewardParam <= 1)
            {
                descText.text = "将随机获得其中一项物品";
            }
            else
            {
                descText.text = "将随机获得其中多项物品";
            }

            rootObject.SetActive(true);
            RefreshList(rewardConfig);
        }

        /// <summary>
        /// 隐藏概率弹窗
        /// </summary>
        public void Hide()
        {
            if (rootObject != null)
            {
                rootObject.SetActive(false);
            }

            ClearList();
        }

        private void OnCloseButtonClicked()
        {
            Hide();
        }

        private void OnMaskButtonClicked()
        {
            Debug.Log("ProbabilityView: 点击遮罩层,关闭弹窗");
            Hide();
        }

        private void RefreshList(cfg.Reward rewardConfig)
        {
            ClearList();

            if (contentContainer == null || itemPrefab == null)
            {
                Debug.LogWarning("ProbabilityView: contentContainer 或 itemPrefab 未设置");
                return;
            }

            if (rewardConfig?.RewardDetail == null || rewardConfig.RewardDetail.Count == 0)
            {
                Debug.LogWarning("ProbabilityView: 奖励配置为空");
                return;
            }

            // 计算总权重
            int totalWeight = 0;
            foreach (var detail in rewardConfig.RewardDetail)
            {
                if (detail != null && detail.Weight > 0)
                {
                    totalWeight += detail.Weight;
                }
            }

            // 创建概率列表项
            foreach (var rewardDetail in rewardConfig.RewardDetail)
            {
                if (rewardDetail?.Id_Ref == null) continue;

                var itemObj = Instantiate(itemPrefab, contentContainer);
                var itemView = itemObj.GetComponent<OneLinePropView>();

                if (itemView != null)
                {
                    itemView.SetData(rewardDetail, totalWeight);
                    itemViews.Add(itemView);
                }
                else
                {
                    Debug.LogError("ProbabilityView: OneLineProp 预制体缺少 OneLinePropView 组件！");
                    Destroy(itemObj);
                }
            }
        }

        private void ClearList()
        {
            foreach (var view in itemViews)
            {
                if (view != null && view.gameObject != null)
                {
                    Destroy(view.gameObject);
                }
            }
            itemViews.Clear();
        }
    }
}
