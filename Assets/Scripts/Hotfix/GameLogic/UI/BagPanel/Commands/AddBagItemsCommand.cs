using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PitayaClient.Network.Manager;
using PitayaGame.GameSvr;
using QFramework;
using UnityEngine;

namespace QFramework.UI
{
    /// <summary>
    /// 添加物品到背包的命令（支持货币、资源、背包物品）
    /// </summary>
    public class AddBagItemsCommand : AbstractCommand<UniTask<AddBagItemsResponse>>
    {
        // 开发阶段使用 1 天作为超时时间（足够长且不会溢出）
        private static readonly float MaxTimeout = 86400f; // 1 天 = 86400 秒

        private readonly List<RewardItem> items;
        private readonly string reason;
        private readonly string referenceId;
        private readonly float timeout;

        public AddBagItemsCommand(List<RewardItem> items, string reason = "测试初始化", string referenceId = null, float timeout = -1f)
        {
            this.items = items ?? new List<RewardItem>();
            this.reason = reason;
            this.referenceId = referenceId;
            this.timeout = timeout < 0 ? MaxTimeout : timeout;
        }

        protected override async UniTask<AddBagItemsResponse> OnExecute()
        {
            if (items == null || items.Count == 0)
            {
                Debug.LogWarning("AddBagItemsCommand: 物品列表为空，跳过请求");
                return null;
            }

            var request = new AddBagItemsRequest
            {
                Reason = reason
            };

            if (!string.IsNullOrEmpty(referenceId))
            {
                request.ReferenceId = referenceId;
            }

            // 添加物品列表
            request.Items.AddRange(items);

            try
            {
                var response = await NetworkManager.Instance.RequestAsync<AddBagItemsResponse>(
                    "gamesvr.bag.addbagitems",
                    request,
                    timeout);

                if (response == null)
                {
                    string requestInfo = FormatRequestInfo();
                    Debug.LogError($"AddBagItemsCommand: 服务器无响应\n请求参数: {requestInfo}");
                    return null;
                }

                if (response.Resp == null || response.Resp.Code != 0)
                {
                    string errorMsg = response.Resp?.Message ?? "未知错误";
                    string requestInfo = FormatRequestInfo();
                    Debug.LogError($"AddBagItemsCommand: 添加失败 Code={response.Resp?.Code} Message={errorMsg}\n请求参数: {requestInfo}");
                    return null;
                }

                Debug.Log($"AddBagItemsCommand: 成功添加 {items.Count} 个物品，返回 {response.AppliedItems.Count} 个变更详情");

                // 处理返回的 RewardDelta，更新本地缓存
                if (response.AppliedItems != null && response.AppliedItems.Count > 0)
                {
                    await this.SendCommand(new ApplyRewardDeltasCommand(response.AppliedItems));
                }

                return response;
            }
            catch (System.Exception ex)
            {
                string requestInfo = FormatRequestInfo();
                Debug.LogError($"AddBagItemsCommand: 异常 {ex.Message}\n请求参数: {requestInfo}\n{ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// 格式化请求参数信息，用于错误日志
        /// </summary>
        private string FormatRequestInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine($"Reason: {reason ?? "null"}");
            info.AppendLine($"ReferenceId: {referenceId ?? "null"}");
            info.AppendLine($"ItemsCount: {items?.Count ?? 0}");
            
            if (items != null && items.Count > 0)
            {
                info.AppendLine("Items:");
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item == null)
                    {
                        info.AppendLine($"  [{i}] null");
                        continue;
                    }

                    switch (item.RewardDetailCase)
                    {
                        case RewardItem.RewardDetailOneofCase.Currency:
                            info.AppendLine($"  [{i}] Currency - ItemId: {item.Currency.ItemId}, Delta: {item.Currency.Delta}");
                            break;
                        case RewardItem.RewardDetailOneofCase.Resource:
                            info.AppendLine($"  [{i}] Resource - ItemId: {item.Resource.ItemId}, Delta: {item.Resource.Delta}");
                            break;
                        case RewardItem.RewardDetailOneofCase.Item:
                            info.AppendLine($"  [{i}] Item - ItemId: {item.Item.ItemId}, Amount: {item.Item.Amount}");
                            break;
                        default:
                            info.AppendLine($"  [{i}] Unknown type: {item.RewardDetailCase}");
                            break;
                    }
                }
            }

            return info.ToString();
        }
    }
}

