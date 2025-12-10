using UnityEngine;
using UnityEngine.UI;
using TMPro;
using QFramework;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace QFramework.UI
{
    /// <summary>
    /// 单个提示项组件
    /// 负责单个 Toast 提示的显示、动画和交互
    /// </summary>
    public class TipsItem : MonoBehaviour
    {
        private TextMeshProUGUI messageText;
        private Image backgroundImage;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private TipsPanel parentPanel;
        private Tweener positionTweener;
        private bool isDestroyed = false;

        public RectTransform RectTransform => rectTransform;
        public bool IsDestroyed => isDestroyed;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            messageText = GetComponentInChildren<TextMeshProUGUI>();
            backgroundImage = GetComponent<Image>();
        }

        private void OnDestroy()
        {
            isDestroyed = true;
            if (positionTweener != null && positionTweener.IsActive())
            {
                positionTweener.Kill();
            }
        }

        /// <summary>
        /// 初始化 TipsItem
        /// </summary>
        public void Initialize(TipsPanel panel)
        {
            parentPanel = panel;
            SetupRectTransform();
        }

        /// <summary>
        /// 设置 RectTransform，确保正确显示
        /// </summary>
        private void SetupRectTransform()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (rectTransform != null)
            {
                // 设置锚点为顶部居中
                rectTransform.anchorMin = new Vector2(0.5f, 1f);
                rectTransform.anchorMax = new Vector2(0.5f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 1f);
                
                // 确保宽度填充容器
                rectTransform.sizeDelta = new Vector2(0f, rectTransform.sizeDelta.y);
            }
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        public void SetData(TipsData data)
        {
            // 设置文本
            if (messageText != null)
            {
                messageText.text = data.Message;
                messageText.richText = true;
                messageText.alignment = TMPro.TextAlignmentOptions.Center;
            }

            // 设置背景颜色
            if (backgroundImage != null)
            {
                Color bgColor = data.TipsType switch
                {
                    TipsType.Success => new Color(0.2f, 0.8f, 0.2f, 0.9f),   // 绿色
                    TipsType.Error => new Color(0.8f, 0.2f, 0.2f, 0.9f),     // 红色
                    TipsType.Warning => new Color(0.9f, 0.7f, 0.2f, 0.9f),   // 黄色
                    _ => new Color(0.2f, 0.2f, 0.2f, 0.9f)                   // 灰色
                };

                backgroundImage.color = bgColor;
            }

            // 初始化状态（完全透明）
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        /// <summary>
        /// 设置位置（立即，无动画）
        /// </summary>
        public void SetPosition(float y)
        {
            if (rectTransform != null)
            {
                // 强制设置 X 为 0，确保居中显示
                rectTransform.anchoredPosition = new Vector2(0f, y);
                
                // 如果位置被其他组件修改了，再次强制设置
                if (Mathf.Abs(rectTransform.anchoredPosition.x) > 0.01f)
                {
                    rectTransform.anchoredPosition = new Vector2(0f, y);
                }
            }
        }

        /// <summary>
        /// 设置目标位置（带动画）
        /// </summary>
        public void SetTargetPosition(float targetY)
        {
            if (rectTransform == null || isDestroyed) return;

            // 停止之前的动画
            if (positionTweener != null && positionTweener.IsActive())
            {
                positionTweener.Kill();
            }

            // 创建新的位置动画
            positionTweener = rectTransform.DOAnchorPosY(targetY, 0.3f)
                .SetEase(Ease.OutCubic);
        }

        /// <summary>
        /// 播放完整的显示动画
        /// </summary>
        public async UniTask PlayAnimationAsync(
            float displayDuration,
            float fadeInDuration,
            float fadeOutDuration,
            float slideUpDistance)
        {
            if (rectTransform == null || canvasGroup == null || isDestroyed)
            {
                return;
            }

            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 midPos = new Vector2(0f, startPos.y + slideUpDistance * 0.3f);  // 淡入时稍微上移
            Vector2 endPos = new Vector2(0f, startPos.y + slideUpDistance);          // 最终位置

            // 创建动画序列
            Sequence sequence = DOTween.Sequence();

            // 第一阶段：淡入 + 轻微上移（出现动画）
            sequence.Append(canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad));
            sequence.Join(rectTransform.DOAnchorPos(midPos, fadeInDuration).SetEase(Ease.OutQuad));

            // 第二阶段：继续上移到最终位置（停留期间）
            sequence.Append(rectTransform.DOAnchorPos(endPos, displayDuration * 0.3f).SetEase(Ease.OutCubic));
            
            // 第三阶段：停留
            sequence.AppendInterval(displayDuration * 0.7f);

            // 第四阶段：继续上移 + 淡出（消失动画）
            Vector2 finalPos = new Vector2(0f, endPos.y + slideUpDistance * 0.2f);
            sequence.Append(rectTransform.DOAnchorPosY(finalPos.y, fadeOutDuration).SetEase(Ease.InQuad));
            sequence.Join(canvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad));

            // 等待动画完成
            await sequence.AsyncWaitForCompletion();
        }

        /// <summary>
        /// 获取高度
        /// </summary>
        public float GetHeight()
        {
            if (rectTransform == null)
            {
                return 0f;
            }

            // 强制布局重建以获取正确高度
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            return rectTransform.rect.height > 0 ? rectTransform.rect.height : 50f; // 默认高度
        }
    }
}

