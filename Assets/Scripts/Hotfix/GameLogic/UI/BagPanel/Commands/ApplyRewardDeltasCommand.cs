using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PitayaGame.GameSvr;
using QFramework;
using UnityEngine;

namespace QFramework.UI
{
    /// <summary>
    /// 处理服务器返回的 RewardDelta 列表：
    /// - Currency / Resource：预留 TODO（由经济模块接入）
    /// - ItemReward：发送统一事件，供物品/背包等模块更新本地缓存
    /// </summary>
    public class ApplyRewardDeltasCommand : AbstractCommand<UniTask>
    {
        private readonly IList<RewardDelta> deltas;

        public ApplyRewardDeltasCommand(IList<RewardDelta> deltas)
        {
            this.deltas = deltas;
        }

        protected override async UniTask OnExecute()
        {
            if (deltas == null || deltas.Count == 0)
            {
                return;
            }

            foreach (var delta in deltas)
            {
                var reward = delta.Reward;
                if (reward == null)
                {
                    continue;
                }

                switch (reward.RewardDetailCase)
                {
                    case RewardItem.RewardDetailOneofCase.Currency:
                        // TODO: 在此处接入货币 Model（例如 EconomyModel），根据 CurrencyChange 更新金币/钻石/体力等
                        Debug.Log(
                            $"[Reward] 货币变化 ItemId={reward.Currency.ItemId} Delta={reward.Currency.Delta} Before={delta.Before} After={delta.After}");
                        break;

                    case RewardItem.RewardDetailOneofCase.Resource:
                        // TODO: 在此处接入资源 Model，根据 ResourceChange 更新资源数量
                        Debug.Log(
                            $"[Reward] 资源变化 ItemId={reward.Resource.ItemId} Delta={reward.Resource.Delta} Before={delta.Before} After={delta.After}");
                        break;

                    case RewardItem.RewardDetailOneofCase.Item:
                        // 物品奖励：更新背包缓存
                        ApplyItemReward(reward.Item, delta);
                        break;

                    case RewardItem.RewardDetailOneofCase.None:
                    default:
                        break;
                }
            }

            // 发送一个汇总事件，方便有需要的模块一次性处理所有 RewardDelta
            this.SendEvent(new RewardDeltaAppliedEvent
            {
                Deltas = new List<RewardDelta>(deltas)
            });

            await UniTask.Yield(); // 保持 UniTask 签名一致，便于未来扩展为异步处理
        }

        /// <summary>
        /// 处理物品奖励：更新背包缓存
        /// </summary>
        private void ApplyItemReward(ItemReward itemReward, RewardDelta delta)
        {
            var bagModel = this.GetModel<IBagModel>();
            if (bagModel == null)
            {
                Debug.LogWarning("ApplyRewardDeltasCommand: BagModel 未注册，无法更新物品奖励。");
                return;
            }

            // 在所有 Tab 中查找该物品
            var existingItem = bagModel.GetItemByItemId(itemReward.ItemId);
            if (existingItem != null)
            {
                // 物品已存在，增加数量
                existingItem.Count += (int)itemReward.Amount;
                bagModel.UpdateItem(existingItem);

                // 获取物品所在的 Tab 索引并发送更新事件
                var tabIndex = bagModel.GetTabIndexByBagId(existingItem.BagId);
                if (tabIndex >= 0)
                {
                    this.SendEvent(new BagItemsUpdatedEvent
                    {
                        TabIndex = tabIndex,
                        Items = bagModel.GetItemsByTab(tabIndex)
                    });
                }

                Debug.Log(
                    $"[Reward] 物品奖励已更新 ItemId={itemReward.ItemId} Amount={itemReward.Amount} 新数量={existingItem.Count}");
            }
            else
            {
                // 物品不存在，等待后端同步（因为需要 bag_id 才能创建新物品）
                Debug.LogWarning(
                    $"[Reward] 物品奖励 ItemId={itemReward.ItemId} Amount={itemReward.Amount} 在本地缓存中未找到，等待后端同步");
            }
        }
    }
}


