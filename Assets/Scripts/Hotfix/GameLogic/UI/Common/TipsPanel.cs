using UnityEngine;
using UnityEngine.UI;
using TMPro;
using QFramework;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace QFramework.UI
{
    /// <summary>
    /// 提示面板 (Toast/飘字提示)
    /// 用于显示短暂的非阻塞式消息，支持队列、富文本、多种样式
    /// </summary>
    public class TipsPanel : UIPanel, IController
    {
        [Header("UI References")]
        [SerializeField] private RectTransform tipsContainer;      // 提示容器
        [SerializeField] private GameObject tipsPrefab;            // 单个提示预制体
        
        [Header("Animation Settings")]
        [SerializeField] private float displayDuration = 2f;       // 显示时长
        [SerializeField] private float fadeInDuration = 0.3f;      // 淡入时长
        [SerializeField] private float fadeOutDuration = 0.3f;     // 淡出时长
        [SerializeField] private float moveUpDistance = 50f;       // 上移距离
        [SerializeField] private float itemSpacing = 10f;          // 提示项间距
        
        [Header("Queue Settings")]
        [SerializeField] private int maxQueueSize = 5;             // 最大队列长度
        [SerializeField] private bool autoQueue = true;            // 自动排队显示
        
        private Queue<TipsData> tipsQueue = new Queue<TipsData>();
        private List<TipsItem> activeTipsItems = new List<TipsItem>();
        private bool isProcessing = false;

        public IArchitecture GetArchitecture() => GameApp.Interface;

        protected override void OnInit(IUIData uiData = null)
        {
            if (tipsContainer == null)
            {
                Debug.LogError("TipsPanel: tipsContainer 未设置!");
            }

            if (tipsPrefab == null)
            {
                Debug.LogError("TipsPanel: tipsPrefab 未设置!");
            }
        }

        protected override void OnOpen(IUIData uiData = null)
        {
            if (uiData is TipsData tipsData)
            {
                EnqueueTips(tipsData);
            }
        }

        /// <summary>
        /// 将提示加入队列
        /// </summary>
        public void EnqueueTips(TipsData data)
        {
            if (tipsQueue.Count >= maxQueueSize)
            {
                Debug.LogWarning($"TipsPanel: 队列已满({maxQueueSize}),丢弃最早的提示");
                tipsQueue.Dequeue();
            }

            tipsQueue.Enqueue(data);

            if (!isProcessing && autoQueue)
            {
                ProcessQueue().Forget();
            }
        }

        /// <summary>
        /// 处理提示队列
        /// </summary>
        private async UniTaskVoid ProcessQueue()
        {
            isProcessing = true;

            while (tipsQueue.Count > 0)
            {
                var data = tipsQueue.Dequeue();
                await ShowTipsItemAsync(data);
                
                // 等待一小段时间再显示下一个(避免重叠)
                await UniTask.Delay(100);
            }

            isProcessing = false;
        }

        /// <summary>
        /// 显示单个提示项
        /// </summary>
        private async UniTask ShowTipsItemAsync(TipsData data)
        {
            if (tipsPrefab == null || tipsContainer == null)
            {
                Debug.LogError("TipsPanel: 预制体或容器未设置!");
                return;
            }

            // 创建提示项
            GameObject itemObj = Instantiate(tipsPrefab, tipsContainer);
            TipsItem tipsItem = itemObj.GetComponent<TipsItem>();

            if (tipsItem == null)
            {
                tipsItem = itemObj.AddComponent<TipsItem>();
            }

            activeTipsItems.Add(tipsItem);

            // 设置数据
            tipsItem.SetData(data);

            // 调整其他提示项位置(向上移动)
            UpdateItemsPosition();

            // 使用自定义时长或默认时长
            float duration = data.CustomDuration > 0 ? data.CustomDuration : displayDuration;

            // 播放动画
            await tipsItem.PlayAnimationAsync(
                duration,
                fadeInDuration,
                fadeOutDuration,
                moveUpDistance
            );

            // 移除并销毁
            activeTipsItems.Remove(tipsItem);
            Destroy(itemObj);

            // 更新剩余提示项位置
            UpdateItemsPosition();
        }

        /// <summary>
        /// 更新所有活动提示项的位置
        /// </summary>
        private void UpdateItemsPosition()
        {
            for (int i = 0; i < activeTipsItems.Count; i++)
            {
                var item = activeTipsItems[i];
                if (item != null)
                {
                    // 从上到下排列
                    float targetY = -i * (item.GetHeight() + itemSpacing);
                    item.SetTargetPosition(targetY);
                }
            }
        }

        /// <summary>
        /// 清空所有提示
        /// </summary>
        public void ClearAll()
        {
            tipsQueue.Clear();

            foreach (var item in activeTipsItems)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }

            activeTipsItems.Clear();
            isProcessing = false;
        }

        protected override void OnClose()
        {
            ClearAll();
        }
    }

    /// <summary>
    /// 单个提示项组件
    /// </summary>
    public class TipsItem : MonoBehaviour
    {
        private TextMeshProUGUI messageText;
        private Image backgroundImage;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;

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

        public void SetData(TipsData data)
        {
            // 设置文本
            if (messageText != null)
            {
                messageText.text = data.Message;
                
                // 支持富文本
                messageText.richText = true;
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

            // 初始化状态
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
            }
        }

        public async UniTask PlayAnimationAsync(
            float displayDuration,
            float fadeInDuration,
            float fadeOutDuration,
            float moveUpDistance)
        {
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = startPos + Vector2.up * moveUpDistance;

            // 创建动画序列
            Sequence sequence = DOTween.Sequence();

            // 淡入 + 上移
            sequence.Append(canvasGroup.DOFade(1f, fadeInDuration));
            sequence.Join(rectTransform.DOAnchorPos(endPos, fadeInDuration + displayDuration)
                .SetEase(Ease.OutCubic));

            // 等待显示
            sequence.AppendInterval(displayDuration);

            // 淡出
            sequence.Append(canvasGroup.DOFade(0f, fadeOutDuration));

            // 等待动画完成
            await sequence.AsyncWaitForCompletion();
        }

        public void SetTargetPosition(float targetY)
        {
            if (rectTransform != null)
            {
                Vector2 targetPos = new Vector2(rectTransform.anchoredPosition.x, targetY);
                rectTransform.DOAnchorPos(targetPos, 0.3f).SetEase(Ease.OutCubic);
            }
        }

        public float GetHeight()
        {
            return rectTransform != null ? rectTransform.rect.height : 0;
        }
    }
}
