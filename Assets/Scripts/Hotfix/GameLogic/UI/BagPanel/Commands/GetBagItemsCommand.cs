using System;
using Cysharp.Threading.Tasks;
using PitayaClient.Network.Manager;
using PitayaGame.Enums;
using PitayaGame.GameSvr;
using QFramework;
using UnityEngine;

namespace QFramework.UI
{
    /// <summary>
    /// 从服务器获取背包物品列表的命令
    /// </summary>
    public class GetBagItemsCommand : AbstractCommand<UniTask<GetBagItemsResponse>>
    {
        // 开发阶段使用 1 天作为超时时间（足够长且不会溢出）
        private static readonly float MaxTimeout = 86400f; // 1 天 = 86400 秒

        public int Page { get; }
        public int PageSize { get; }
        public ItemType ItemType { get; }
        public float Timeout { get; }

        public GetBagItemsCommand(int page = 1, int pageSize = 100, ItemType itemType = ItemType.Unknown, float timeout = -1f)
        {
            Page = page;
            PageSize = pageSize;
            ItemType = itemType;
            Timeout = timeout < 0 ? MaxTimeout : timeout;
        }

        protected override async UniTask<GetBagItemsResponse> OnExecute()
        {
            var request = new GetBagItemsRequest
            {
                Page = Page,
                PageSize = PageSize,
                ItemType = ItemType
            };

            try
            {
                var response = await NetworkManager.Instance.RequestAsync<GetBagItemsResponse>(
                    "gamesvr.bag.getbagitems",
                    request,
                    Timeout);

                if (response == null)
                {
                    Debug.LogError("获取背包物品失败：服务器无响应");
                    return null;
                }

                if (response.Resp == null || response.Resp.Code != 0)
                {
                    string errorMsg = response.Resp?.Message ?? "未知错误";
                    Debug.LogError($"获取背包物品失败：Code={response.Resp?.Code} Message={errorMsg}");
                    return null;
                }

                Debug.Log($"获取背包物品成功，数量: {response.Items.Count}");
                return response;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"获取背包物品异常: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }
    }
}

