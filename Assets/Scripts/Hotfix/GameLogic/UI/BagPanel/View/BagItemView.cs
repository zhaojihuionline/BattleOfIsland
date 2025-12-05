using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace QFramework.UI
{
    /// <summary>
    /// 背包道具 Item 的视图脚本
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class BagItemView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image qualityFrameImage;
        [SerializeField] private Image selectedMaskImage;
        [SerializeField] private Image cooldownMaskImage;
        [SerializeField] private GameObject countBadge;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private TextMeshProUGUI nameText;  // 物品名称文本
        [SerializeField] private Image equipTagImage;
        [SerializeField] private Image newTagImage;
        [SerializeField] private Image lockedTagImage;
        [SerializeField] private Button clickButton;

        // 编辑器预览用的 Sprite 字段
        [Header("Editor Preview - Sprites")]
        [SerializeField] private Sprite previewIconSprite;
        [SerializeField] private Sprite previewBackgroundSprite;
        [SerializeField] private Sprite previewQualitySprite;

        [Header("Editor Preview - States")]
        [SerializeField] private bool previewSelected = false;
        [SerializeField] private bool previewEquipped = false;
        [SerializeField] private bool previewNew = false;
        [SerializeField] private bool previewLocked = false;
        [SerializeField] private bool previewInteractable = true;
        [SerializeField] private int previewCount = 1;
        [SerializeField] [Range(0f, 1f)] private float previewCooldown = 0f;

        // private CanvasGroup canvasGroup;

        public BagItemData Data { get; private set; }
        public Action<BagItemView> OnClicked;

        private void Awake()
        {
            // canvasGroup = GetComponent<CanvasGroup>();

            if (clickButton == null)
            {
                clickButton = GetComponent<Button>();
            }

            if (clickButton != null)
            {
                clickButton.onClick.AddListener(HandleClick);
            }
        }

        private void OnDestroy()
        {
            if (clickButton != null)
            {
                clickButton.onClick.RemoveListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            OnClicked?.Invoke(this);
        }

        public void SetData(BagItemData data)
        {
            Data = data ?? new BagItemData();

            if (iconImage != null)
            {
                iconImage.sprite = Data.IconSprite;
                iconImage.enabled = Data.IconSprite != null;
            }

            if (backgroundImage != null)
            {
                backgroundImage.sprite = Data.BackgroundSprite;
            }

            if (qualityFrameImage != null)
            {
                qualityFrameImage.sprite = Data.QualitySprite;
                qualityFrameImage.enabled = Data.QualitySprite != null;
            }

            UpdateCount(Data.Count);
            UpdateName(Data.ItemName);
            SetSelected(Data.IsSelected);
            SetEquipped(Data.IsEquipped);
            SetNew(Data.IsNew);
            SetLocked(Data.IsLocked);
            SetInteractable(Data.IsInteractable);
            UpdateCooldown(Data.CooldownPercent);
        }

        public void UpdateName(string itemName)
        {
            if (nameText != null)
            {
                nameText.text = itemName ?? "";
                nameText.gameObject.SetActive(!string.IsNullOrEmpty(itemName));
            }
        }

        public void UpdateCount(int count)
        {
            if (countBadge == null || countText == null) return;

            bool show = count > 1;
            countBadge.SetActive(show);
            if (show)
            {
                countText.text = count.ToString();
            }

            if (Data != null)
            {
                Data.Count = count;
            }
        }

        public void SetSelected(bool selected)
        {
            if (selectedMaskImage != null)
            {
                selectedMaskImage.enabled = selected;
            }

            if (Data != null)
            {
                Data.IsSelected = selected;
            }
        }

        public void SetEquipped(bool equipped)
        {
            if (equipTagImage != null)
            {
                equipTagImage.gameObject.SetActive(equipped);
            }

            if (Data != null)
            {
                Data.IsEquipped = equipped;
            }
        }

        public void SetNew(bool isNew)
        {
            if (newTagImage != null)
            {
                newTagImage.gameObject.SetActive(isNew);
            }

            if (Data != null)
            {
                Data.IsNew = isNew;
            }
        }

        public void SetLocked(bool isLocked)
        {
            if (lockedTagImage != null)
            {
                lockedTagImage.gameObject.SetActive(isLocked);
            }

            if (Data != null)
            {
                Data.IsLocked = isLocked;
            }
        }

        public void SetInteractable(bool interactable)
        {
            // if (canvasGroup != null)
            // {
            //     canvasGroup.interactable = interactable;
            //     canvasGroup.blocksRaycasts = interactable;
            //     canvasGroup.alpha = interactable ? 1f : 0.5f;
            // }

            if (Data != null)
            {
                Data.IsInteractable = interactable;
            }
        }

        public void UpdateCooldown(float percent)
        {
            percent = Mathf.Clamp01(percent);
            if (cooldownMaskImage != null)
            {
                cooldownMaskImage.fillAmount = percent;
                cooldownMaskImage.enabled = percent > 0f;
            }

            if (Data != null)
            {
                Data.CooldownPercent = percent;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器中实时预览效果
        /// </summary>
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            // 预览图标
            if (iconImage != null)
            {
                iconImage.sprite = previewIconSprite;
                iconImage.enabled = previewIconSprite != null;
            }

            // 预览背景
            if (backgroundImage != null && previewBackgroundSprite != null)
            {
                backgroundImage.sprite = previewBackgroundSprite;
            }

            // 预览品质框
            if (qualityFrameImage != null)
            {
                qualityFrameImage.sprite = previewQualitySprite;
                qualityFrameImage.enabled = previewQualitySprite != null;
            }

            // 预览选中状态
            if (selectedMaskImage != null)
            {
                selectedMaskImage.enabled = previewSelected;
            }

            // 预览装备标签
            if (equipTagImage != null)
            {
                equipTagImage.gameObject.SetActive(previewEquipped);
            }

            // 预览新品标签
            if (newTagImage != null)
            {
                newTagImage.gameObject.SetActive(previewNew);
            }

            // 预览锁定标签
            if (lockedTagImage != null)
            {
                lockedTagImage.gameObject.SetActive(previewLocked);
            }

            // 预览数量
            if (countBadge != null && countText != null)
            {
                bool showCount = previewCount > 1;
                countBadge.SetActive(showCount);
                if (showCount)
                {
                    countText.text = previewCount.ToString();
                }
            }

            // 预览冷却遮罩
            if (cooldownMaskImage != null)
            {
                cooldownMaskImage.fillAmount = previewCooldown;
                cooldownMaskImage.enabled = previewCooldown > 0f;
            }

            // 预览可交互状态
            // if (canvasGroup == null)
            // {
            //     canvasGroup = GetComponent<CanvasGroup>();
            // }
            // if (canvasGroup != null)
            // {
            //     canvasGroup.alpha = previewInteractable ? 1f : 0.5f;
            // }
        }
#endif
    }
}

