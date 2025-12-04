using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PitayaGame.GameSvr;

namespace QFramework.UI
{
    /// <summary>
    /// 展示服务器返回奖励的弹窗视图
    /// </summary>
    public class ObtainRewardsView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject root;
        [SerializeField] private Transform content;
        [SerializeField] private GameObject bagItemPrefab;
        [SerializeField] private Button closeButton;

        private readonly List<GameObject> rewardItems = new List<GameObject>();

        private void Awake()
        {
            if (root == null)
            {
                root = gameObject;
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            if (root != null)
            {
                root.SetActive(false);
            }
        }

        /// <summary>
        /// 显示奖励列表
        /// </summary>
        public void Show(IList<RewardDelta> deltas)
        {
            if (root != null)
            {
                root.SetActive(true);
            }

            RefreshRewards(deltas);
        }

        /// <summary>
        /// 隐藏弹窗
        /// </summary>
        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }

            ClearRewards();
        }

        private void RefreshRewards(IList<RewardDelta> deltas)
        {
            ClearRewards();

            if (content == null || bagItemPrefab == null || deltas == null) return;

            foreach (var delta in deltas)
            {
                var itemData = BagItemConverter.CreateFromRewardDelta(delta);
                if (itemData == null) continue;

                var itemObj = Instantiate(bagItemPrefab, content);
                rewardItems.Add(itemObj);

                var itemView = itemObj.GetComponent<BagItemView>();
                if (itemView != null)
                {
                    itemView.SetData(itemData);
                    itemView.SetInteractable(false);
                }
            }
        }

        private void ClearRewards()
        {
            foreach (var go in rewardItems)
            {
                if (go != null)
                {
                    Destroy(go);
                }
            }
            rewardItems.Clear();
        }
    }
}

