using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QFramework.UI
{
    /// <summary>
    /// ItemToast 预制体的自包含展示逻辑：填充文案并根据目标物品位置自动摆放。
    /// </summary>
    public class ItemToastView : MonoBehaviour
    {
        [SerializeField] private float margin = 10f;

        private RectTransform toastRect;

        private void Awake()
        {
            toastRect = transform as RectTransform;
        }

        /// <summary>
        /// 填充数据并相对目标物品布局。
        /// </summary>
        public void Show(BagItemData data, RectTransform targetRect, Canvas overrideCanvas = null)
        {
            if (toastRect == null)
            {
                toastRect = transform as RectTransform;
            }

            if (toastRect == null || targetRect == null) return;

            ApplyContent(data);

            // 确保布局最新，避免尺寸为旧值
            LayoutRebuilder.ForceRebuildLayoutImmediate(toastRect);

            Position(targetRect, overrideCanvas);
        }

        private void ApplyContent(BagItemData data)
        {
            if (data == null) return;

            var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in texts)
            {
                var goName = tmp.gameObject.name;
                if (goName == "Text (TMP)-Title")
                {
                    tmp.text = data.ItemName ?? string.Empty;
                }
                else if (goName == "Text (TMP)-Desc")
                {
                    tmp.text = data.Description ?? string.Empty;
                }
                else if (goName == "Text (TMP)-Owned")
                {
                    tmp.text = $"拥有：{data.Count}";
                }
            }
        }

        private void Position(RectTransform targetRect, Canvas overrideCanvas)
        {
            // 使用目标 Canvas（优先外部传入，其次 Toast 父级 Canvas）
            var canvas = overrideCanvas != null ? overrideCanvas : toastRect.GetComponentInParent<Canvas>();
            var camera = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas != null ? canvas.worldCamera : null;

            var itemSize = targetRect.rect.size;
            var toastSize = toastRect.rect.size;

            Vector2 offset = Vector2.zero;
            Vector2 pivot = new Vector2(0.5f, 0.5f);

            // 方向优先级：右->左->上->下。基于屏幕剩余空间决定方向，偏移基于物品尺寸。
            if (canvas != null)
            {
                var itemCenterWorld = targetRect.TransformPoint(targetRect.rect.center);
                var itemCenterScreen = RectTransformUtility.WorldToScreenPoint(camera, itemCenterWorld);

                float rightSpace = canvas.pixelRect.xMax - itemCenterScreen.x;
                float leftSpace = itemCenterScreen.x - canvas.pixelRect.xMin;
                float topSpace = canvas.pixelRect.yMax - itemCenterScreen.y;
                float bottomSpace = itemCenterScreen.y - canvas.pixelRect.yMin;

                if (rightSpace >= toastSize.x + margin)
                {
                    // offset = new Vector2(itemSize.x * 0.5f + toastSize.x * 0.5f + margin, 0);
                    offset = new Vector2(itemSize.x * 0.5f + margin, 0);
                    pivot = new Vector2(0f, 0.5f);
                }
                else if (leftSpace >= toastSize.x + margin)
                {
                    // offset = new Vector2(-(itemSize.x * 0.5f + toastSize.x * 0.5f + margin), 0);
                    offset = new Vector2(-(itemSize.x * 0.5f + margin), 0);
                    pivot = new Vector2(1f, 0.5f);
                }
                else if (topSpace >= toastSize.y + margin)
                {
                    // offset = new Vector2(0, itemSize.y * 0.5f + toastSize.y * 0.5f + margin);
                    offset = new Vector2(0, itemSize.y * 0.5f + margin);
                    pivot = new Vector2(0.5f, 0f);
                }
                else
                {
                    // offset = new Vector2(0, -(itemSize.y * 0.5f + toastSize.y * 0.5f + margin));
                    offset = new Vector2(0, -(itemSize.y * 0.5f + margin));
                    pivot = new Vector2(0.5f, 1f);
                }
            }
            else
            {
                offset = new Vector2(itemSize.x * 0.5f + toastSize.x * 0.5f + margin, 0);
                pivot = new Vector2(0f, 0.5f);
            }

            toastRect.pivot = pivot;
            // 将物品中心转换到目标 Canvas 的局部坐标，再加偏移
            Vector2 itemCenterLocal = Vector2.zero;
            if (canvas != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    RectTransformUtility.WorldToScreenPoint(camera, targetRect.TransformPoint(targetRect.rect.center)),
                    camera,
                    out itemCenterLocal);
            }
            toastRect.anchoredPosition = itemCenterLocal + offset;
        }
    }
}

