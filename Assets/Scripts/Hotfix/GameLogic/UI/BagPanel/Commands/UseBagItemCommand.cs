using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PitayaClient.Network.Manager;
using PitayaGame.GameSvr;
using QFramework;
using UnityEngine;
using PitayaGame.Errors;

namespace QFramework.UI
{
    /// <summary>
    /// 使用背包物品命令
    /// </summary>
    public class UseBagItemCommand : AbstractCommand<UniTask<UseBagItemResponse>>
    {
        // 开发阶段使用 1 天作为超时时间（足够长且不会溢出）
        private static readonly float MaxTimeout = 86400f; // 1 天 = 86400 秒

        private readonly long bagId;
        private readonly int useCount;
        private readonly int tabIndex;
        private readonly string clientSequence;
        private readonly List<int> selectedRewardIds;
        private readonly float timeout;

        public UseBagItemCommand(long bagId, int count, int tabIndex = -1, string clientSequence = null,
            List<int> selectedRewardIds = null, float timeout = -1f)
        {
            this.bagId = bagId;
            this.useCount = Mathf.Max(1, count);
            this.tabIndex = tabIndex;
            this.clientSequence = clientSequence;
            this.selectedRewardIds = selectedRewardIds;
            this.timeout = timeout < 0 ? MaxTimeout : timeout;
        }

        protected override async UniTask<UseBagItemResponse> OnExecute()
        {
            var request = new UseBagItemRequest
            {
                BagId = bagId,
                Count = useCount,
                ClientSequence = string.IsNullOrEmpty(clientSequence) ? System.Guid.NewGuid().ToString() : clientSequence
            };

            if (selectedRewardIds != null && selectedRewardIds.Count > 0)
            {
                request.SelectedRewardIds.AddRange(selectedRewardIds);
            }

            var response = await NetworkManager.Instance.RequestAsync<UseBagItemResponse>(
                "gamesvr.bag.usebagitem",
                request,
                timeout);

            if (response == null)
            {
                Debug.LogError($"UseBagItemCommand: 服务器无响应，BagId:{bagId}");
                return null;
            }

            if (response.Resp == null || response.Resp.Code != 0)
            {
                string errorMsg = response.Resp?.Message ?? "未知错误";
                if(response.Resp?.Code == (int)ErrorCode.ErrInventoryUseLimit)
                {
                    Tips.ShowWarning($"{errorMsg}");
                }
                else
                {
                    Debug.LogError($"UseBagItemCommand: 使用失败 Code={response.Resp?.Code} Message={errorMsg}");
                }
                
                return null;
            }

            // 处理使用物品带来的货币/资源/物品奖励变更
            if (response.Rewards != null && response.Rewards.Count > 0)
            {
                await this.SendCommand(new ApplyRewardDeltasCommand(response.Rewards));
                this.SendEvent(new RewardsObtainedEvent
                {
                    Deltas = new List<RewardDelta>(response.Rewards)
                });
            }

            return response;
        }
    }
}
