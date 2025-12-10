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
        private RectTransform rectTransform;
        private TipsPanel parentPanel;
        private Tweener positionTweener;
        private bool isDestroyed = false;
        
        // 保存初始 alpha 值（用于动画）
        private float imageInitialAlpha = 0.9f;  // Image 的初始 alpha（在 SetData 中设置）
        private float textInitialAlpha = 1f;     // TextMeshProUGUI 的初始 alpha

        public RectTransform RectTransform => rectTransform;
        public bool IsDestroyed => isDestroyed;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            messageText = GetComponentInChildren<TextMeshProUGUI>();
            backgroundImage = GetComponent<Image>();
            
            // 保存文本的初始 alpha
            if (messageText != null)
            {
                textInitialAlpha = messageText.color.a;
            }
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
        /// 注意：锚点和轴心点应在预制体中配置为顶部居中 (0.5, 1)
        /// </summary>
        public void Initialize(TipsPanel panel)
        {
            parentPanel = panel;
            ValidateAndSetupRectTransform();
        }

        /// <summary>
        /// 验证并设置 RectTransform
        /// 优先使用预制体配置，只在必要时用代码设置
        /// </summary>
        private void ValidateAndSetupRectTransform()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (rectTransform != null)
            {
                // 检查锚点配置（应在预制体中设置为顶部居中）
                if (rectTransform.anchorMin.x != 0.5f || rectTransform.anchorMin.y != 1f ||
                    rectTransform.anchorMax.x != 0.5f || rectTransform.anchorMax.y != 1f)
                {
                    Debug.LogWarning("TipsItem: 锚点应在预制体中设置为顶部居中 (0.5, 1)，当前代码会自动修正");
                    rectTransform.anchorMin = new Vector2(0.5f, 1f);
                    rectTransform.anchorMax = new Vector2(0.5f, 1f);
                }

                // 检查轴心点配置（应在预制体中设置为顶部居中）
                if (rectTransform.pivot.x != 0.5f || rectTransform.pivot.y != 1f)
                {
                    Debug.LogWarning("TipsItem: 轴心点应在预制体中设置为顶部居中 (0.5, 1)，当前代码会自动修正");
                    rectTransform.pivot = new Vector2(0.5f, 1f);
                }

                // 设置宽度填充容器（这是运行时属性，必须在代码中设置）
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
                
                // 保存文本的初始 alpha（如果之前没有保存）
                if (textInitialAlpha <= 0)
                {
                    textInitialAlpha = messageText.color.a > 0 ? messageText.color.a : 1f;
                }
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
                imageInitialAlpha = bgColor.a;  // 保存 Image 的初始 alpha
            }

            // 初始化状态（完全透明）- 使用 DoTween 直接控制
            SetAlpha(0f);
        }
        
        /// <summary>
        /// 设置透明度（同时设置 Image 和 TextMeshProUGUI）
        /// </summary>
        private void SetAlpha(float alpha)
        {
            if (backgroundImage != null)
            {
                Color color = backgroundImage.color;
                color.a = alpha * imageInitialAlpha;  // 乘以初始 alpha，保持相对透明度
                backgroundImage.color = color;
            }
            
            if (messageText != null)
            {
                Color color = messageText.color;
                color.a = alpha * textInitialAlpha;  // 乘以初始 alpha，保持相对透明度
                messageText.color = color;
            }
        }

        /// <summary>
        /// 设置位置（立即，无动画）
        /// 注意：如果锚点正确配置为顶部居中 (0.5, 1)，X 坐标会自动为 0
        /// </summary>
        public void SetPosition(float y)
        {
            if (rectTransform != null)
            {
                // 设置 Y 位置（X 坐标由锚点自动处理，应该为 0）
                rectTransform.anchoredPosition = new Vector2(0f, y);
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
            if (rectTransform == null || isDestroyed)
            {
                return;
            }

            if (backgroundImage == null && messageText == null)
            {
                return;
            }

            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 midPos = new Vector2(0f, startPos.y + slideUpDistance * 0.3f);  // 淡入时稍微上移
            Vector2 endPos = new Vector2(0f, startPos.y + slideUpDistance);          // 最终位置

            // 创建动画序列
            Sequence sequence = DOTween.Sequence();

            // 第一阶段：淡入 + 轻微上移（出现动画）
            // 同时动画 Image 和 TextMeshProUGUI 的透明度
            if (backgroundImage != null)
            {
                sequence.Join(backgroundImage.DOFade(imageInitialAlpha, fadeInDuration).SetEase(Ease.OutQuad));
            }
            if (messageText != null)
            {
                sequence.Join(messageText.DOFade(textInitialAlpha, fadeInDuration).SetEase(Ease.OutQuad));
            }
            sequence.Join(rectTransform.DOAnchorPos(midPos, fadeInDuration).SetEase(Ease.OutQuad));

            // 第二阶段：继续上移到最终位置（停留期间）
            sequence.Append(rectTransform.DOAnchorPos(endPos, displayDuration * 0.3f).SetEase(Ease.OutCubic));
            
            // 第三阶段：停留
            sequence.AppendInterval(displayDuration * 0.7f);

            // 第四阶段：继续上移 + 淡出（消失动画）
            Vector2 finalPos = new Vector2(0f, endPos.y + slideUpDistance * 0.2f);
            sequence.Append(rectTransform.DOAnchorPosY(finalPos.y, fadeOutDuration).SetEase(Ease.InQuad));
            
            // 同时淡出 Image 和 TextMeshProUGUI
            if (backgroundImage != null)
            {
                sequence.Join(backgroundImage.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad));
            }
            if (messageText != null)
            {
                sequence.Join(messageText.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad));
            }

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

