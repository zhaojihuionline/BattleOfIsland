using UnityEngine;
using UnityEngine.UI;
using TMPro;
using QFramework;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace QFramework.UI
{
    /// <summary>
    /// 提示面板 (Toast/飘字提示)
    /// 用于显示短暂的非阻塞式消息，支持队列、富文本、多种样式
    /// 优化版本：流畅的动画效果，正确的位置管理
    /// </summary>
    public partial class TipsPanel : UIPanel, IController
    {
        [Header("UI References")]
        [SerializeField] private RectTransform tipsContainer;      // 提示容器
        [SerializeField] private GameObject tipsPrefab;            // 单个提示预制体
        
        [Header("Animation Settings")]
        [SerializeField] private float displayDuration = 2f;       // 显示时长
        [SerializeField] private float fadeInDuration = 0.3f;      // 淡入时长
        [SerializeField] private float fadeOutDuration = 0.3f;     // 淡出时长
        [SerializeField] private float slideUpDistance = 80f;      // 上移总距离
        [SerializeField] private float itemSpacing = 15f;          // 提示项间距
        
        [Header("Queue Settings")]
        [SerializeField] private int maxQueueSize = 10;            // 最大队列长度
        [SerializeField] private bool autoQueue = true;             // 自动排队显示
        
        private Queue<TipsData> tipsQueue = new Queue<TipsData>();
        private List<TipsItem> activeTipsItems = new List<TipsItem>();
        private bool isProcessing = false;
        private const float START_OFFSET_Y = -50f;  // 初始位置偏移（从容器顶部向下）

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

            // 确保容器设置正确
            if (tipsContainer != null)
            {
                SetupContainer();
            }
        }

        /// <summary>
        /// 检查并验证容器的配置
        /// 注意：容器的锚点、轴心点、VerticalLayoutGroup 的启用状态应在预制体中配置
        /// </summary>
        private void SetupContainer()
        {
            // 检查容器锚点配置（应在预制体中设置为顶部居中）
            if (tipsContainer.anchorMin.x != 0.5f || tipsContainer.anchorMin.y != 1f ||
                tipsContainer.anchorMax.x != 0.5f || tipsContainer.anchorMax.y != 1f)
            {
                Debug.LogWarning("TipsPanel: TipsContainer 的锚点应在预制体中设置为顶部居中 (0.5, 1)");
            }

            // 检查 VerticalLayoutGroup 是否已禁用（应在预制体中禁用，因为我们要手动管理位置）
            var layoutGroup = tipsContainer.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup != null && layoutGroup.enabled)
            {
                Debug.LogWarning("TipsPanel: TipsContainer 的 VerticalLayoutGroup 应在预制体中禁用，因为我们要手动管理子对象位置");
                // 如果预制体中没有禁用，运行时禁用（但建议在预制体中配置）
                layoutGroup.enabled = false;
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
            if (data == null || string.IsNullOrEmpty(data.Message))
            {
                return;
            }

            // 如果队列已满，移除最早的
            if (tipsQueue.Count >= maxQueueSize)
            {
                Debug.LogWarning($"TipsPanel: 队列已满({maxQueueSize}),丢弃最早的提示");
                tipsQueue.Dequeue();
            }

            tipsQueue.Enqueue(data);

            // 如果当前没有在处理队列，开始处理
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
                ShowTipsItem(data).Forget();
                
                // 等待一小段时间再显示下一个，避免同时出现太多
                await UniTask.Delay(200);
            }

            isProcessing = false;
        }

        /// <summary>
        /// 显示单个提示项（异步，不阻塞）
        /// </summary>
        private async UniTaskVoid ShowTipsItem(TipsData data)
        {
            if (tipsPrefab == null || tipsContainer == null)
            {
                Debug.LogError("TipsPanel: 预制体或容器未设置!");
                return;
            }

            // 创建提示项
            GameObject itemObj = Instantiate(tipsPrefab, tipsContainer);
            TipsItem tipsItem = itemObj.GetComponent<TipsItem>();

            // 初始化 TipsItem（检查预制体配置，设置必要的运行时属性）
            tipsItem.Initialize(this);

            // 添加到活动列表
            activeTipsItems.Add(tipsItem);

            // 设置数据（这会触发文本更新，可能影响布局）
            tipsItem.SetData(data);

            // 强制布局重建，确保文本更新后的尺寸正确
            LayoutRebuilder.ForceRebuildLayoutImmediate(tipsContainer);

            // 计算初始位置（从容器顶部开始）
            float startY = START_OFFSET_Y;
            tipsItem.SetPosition(startY);

            // 更新所有提示项的位置（新提示出现时，旧的向下移动）
            UpdateAllItemsPosition();

            // 使用自定义时长或默认时长
            float duration = data.CustomDuration > 0 ? data.CustomDuration : displayDuration;

            // 播放动画
            await tipsItem.PlayAnimationAsync(
                duration,
                fadeInDuration,
                fadeOutDuration,
                slideUpDistance
            );

            // 从活动列表中移除
            activeTipsItems.Remove(tipsItem);

            // 更新剩余提示项位置
            UpdateAllItemsPosition();

            // 销毁对象
            if (itemObj != null)
            {
                Destroy(itemObj);
            }
        }

        /// <summary>
        /// 更新所有活动提示项的位置
        /// </summary>
        private void UpdateAllItemsPosition()
        {
            // 从顶部开始，向下排列
            float currentY = START_OFFSET_Y;
            
            // 按添加顺序排列（最新的在最上面）
            for (int i = activeTipsItems.Count - 1; i >= 0; i--)
            {
                var item = activeTipsItems[i];
                if (item != null && !item.IsDestroyed)
                {
                    item.SetTargetPosition(currentY);
                    currentY -= (item.GetHeight() + itemSpacing);
                }
            }
        }

        /// <summary>
        /// 清空所有提示
        /// </summary>
        public void ClearAll()
        {
            tipsQueue.Clear();

            foreach (var item in activeTipsItems.ToList())
            {
                if (item != null && item.gameObject != null)
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
}
