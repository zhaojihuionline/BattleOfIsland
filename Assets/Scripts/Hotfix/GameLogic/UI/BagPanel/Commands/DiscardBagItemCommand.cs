using System;
using Cysharp.Threading.Tasks;
using PitayaClient.Network.Manager;
using PitayaGame.GameSvr;
using QFramework;
using UnityEngine;

namespace QFramework.UI
{
    /// <summary>
    /// 丢弃背包物品命令
    /// </summary>
    public class DiscardBagItemCommand : AbstractCommand<UniTask<DiscardBagItemResponse>>
    {
        // 开发阶段使用 1 天作为超时时间（足够长且不会溢出）
        private static readonly float MaxTimeout = 86400f; // 1 天 = 86400 秒

        private readonly long bagId;
        private readonly int discardCount;
        private readonly int tabIndex;
        private readonly float timeout;

        public DiscardBagItemCommand(long bagId, int count, int tabIndex = -1, float timeout = -1f)
        {
            this.bagId = bagId;
            this.discardCount = Mathf.Max(1, count);
            this.tabIndex = tabIndex;
            this.timeout = timeout < 0 ? MaxTimeout : timeout;
        }

        protected override async UniTask<DiscardBagItemResponse> OnExecute()
        {
            var request = new DiscardBagItemRequest
            {
                BagId = bagId,
                Count = discardCount
            };

            var response = await NetworkManager.Instance.RequestAsync<DiscardBagItemResponse>(
                "gamesvr.bag.discardbagitem",
                request,
                timeout);

            if (response == null)
            {
                Debug.LogError($"DiscardBagItemCommand: 服务器无响应，BagId:{bagId}");
                return null;
            }

            if (response.Resp == null || response.Resp.Code != 0)
            {
                string errorMsg = response.Resp?.Message ?? "未知错误";
                Debug.LogError($"DiscardBagItemCommand: 丢弃失败 Code={response.Resp?.Code} Message={errorMsg}");
                return null;
            }

            ApplyLocalDiscard(response.Item);
            return response;
        }

        private void ApplyLocalDiscard(BagItem serverItem)
        {
            var bagModel = this.GetModel<IBagModel>();
            if (bagModel == null)
            {
                Debug.LogWarning("DiscardBagItemCommand: BagModel 未注册，无法同步本地数据。");
                return;
            }

            var notifyTabIndex = tabIndex >= 0 ? tabIndex : bagModel.GetTabIndexByBagId(bagId);
            var localItem = bagModel.GetItemByBagId(bagId);

            if (serverItem != null)
            {
                if (localItem != null)
                {
                    localItem.Count = serverItem.Count;
                    localItem.IsLocked = serverItem.Locked;
                    if (localItem.Count <= 0)
                    {
                        bagModel.RemoveItem(bagId);
                    }
                }
                else if (notifyTabIndex >= 0)
                {
                    var newData = BagItemConverter.ConvertFromServer(serverItem);
                    bagModel.AddItem(newData, notifyTabIndex);
                }
            }
            else if (localItem != null)
            {
                localItem.Count = Mathf.Max(0, localItem.Count - discardCount);
                if (localItem.Count <= 0)
                {
                    bagModel.RemoveItem(bagId);
                }
            }

            if (notifyTabIndex >= 0)
            {
                this.SendEvent(new BagItemsUpdatedEvent
                {
                    TabIndex = notifyTabIndex,
                    Items = bagModel.GetItemsByTab(notifyTabIndex)
                });
            }
        }
    }
}

