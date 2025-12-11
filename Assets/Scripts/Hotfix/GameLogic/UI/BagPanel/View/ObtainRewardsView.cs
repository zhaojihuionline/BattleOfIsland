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
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button maskButton;  // 遮罩层按钮(点击关闭)
        [Header("Item Toast")]
        [SerializeField] private GameObject itemToastPrefab; // 指向 Assets/GameRes_Hotfix/Prefabs/BagPrefab/ItemToast.prefab
        [SerializeField] private RectTransform toastParent;  // 可选指定父节点，默认使用 root

        private GameObject currentToast;

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

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(Hide);
            }

            if (maskButton != null)
            {
                maskButton.onClick.AddListener(OnMaskButtonClicked);
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
			// 确保 root 引用正确
			if (root == null)
			{
				root = gameObject;
			}

			// 确保挂载脚本的 GameObject 处于激活状态（避免第一次点击时 Awake 未执行的问题）
			if (!gameObject.activeSelf)
			{
				gameObject.SetActive(true);
			}

			// 显示弹窗根节点
			if (root != null && !root.activeSelf)
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
			if (root == null)
			{
				root = gameObject;
			}

			if (root != null)
			{
				root.SetActive(false);
			}

            ClearRewards();
            DestroyCurrentToast();
        }

        private void OnMaskButtonClicked()
        {
            Debug.Log("ObtainRewardsView: 点击遮罩层,关闭弹窗");
            Hide();
        }

        private void RefreshRewards(IList<RewardDelta> deltas)
        {
            ClearRewards();

            if (content == null || bagItemPrefab == null || deltas == null) return;

            foreach (var delta in deltas)
            {
                // 过滤掉消耗的物品，只显示获得的奖励
                // delta.After > delta.Before 表示数量增加（获得）
                // delta.After <= delta.Before 表示数量不变或减少（消耗）
                if (delta.After <= delta.Before)
                {
                    continue;  // 跳过消耗的物品
                }

                var itemData = BagItemConverter.CreateFromRewardDelta(delta);
                if (itemData == null) continue;

                var itemObj = Instantiate(bagItemPrefab, content);
                rewardItems.Add(itemObj);

                var itemView = itemObj.GetComponent<BagItemView>();
                if (itemView != null)
                {
                    itemView.SetData(itemData);
                    itemView.SetInteractable(false);
                    itemView.OnClicked -= OnRewardItemClicked;
                    itemView.OnClicked += OnRewardItemClicked;
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

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(Hide);
            }

            if (maskButton != null)
            {
                maskButton.onClick.RemoveListener(OnMaskButtonClicked);
            }
        }

        private void OnRewardItemClicked(BagItemView itemView)
        {
            if (itemView == null) return;

            ShowItemToast(itemView);
        }

        private void ShowItemToast(BagItemView itemView)
        {
            if (itemToastPrefab == null)
            {
                Debug.LogWarning("ObtainRewardsView: itemToastPrefab 未配置");
                return;
            }

            DestroyCurrentToast();

            var parent = toastParent as RectTransform;
            if (parent == null)
            {
                parent = toastParent != null ? toastParent : root != null ? root.transform as RectTransform : transform as RectTransform;
            }

            var toastObj = Instantiate(itemToastPrefab, parent);
            currentToast = toastObj;

            var toastView = toastObj.GetComponent<ItemToastView>();
            if (toastView == null)
            {
                toastView = toastObj.AddComponent<ItemToastView>();
            }

            var toastRect = toastObj.transform as RectTransform;
            if (toastRect != null)
            {
                toastRect.anchorMin = new Vector2(0.5f, 0.5f);
                toastRect.anchorMax = new Vector2(0.5f, 0.5f);
                toastRect.anchoredPosition = Vector2.zero;
                toastRect.localScale = Vector3.one;
                toastRect.localRotation = Quaternion.identity;
            }

            toastView.Show(itemView.Data, parent);
        }

        private void DestroyCurrentToast()
        {
            if (currentToast != null)
            {
                Destroy(currentToast);
                currentToast = null;
            }
        }
    }
}

