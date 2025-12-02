using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PitayaGame.Enums;
using QFramework;
using UnityEngine;

namespace QFramework.UI
{
    /// <summary>
    /// 切换背包 Tab 命令：优先使用缓存，必要时从服务器加载数据
    /// </summary>
    public class SwitchBagTabCommand : AbstractCommand
    {
        public int OldIndex { get; set; }
        public int NewIndex { get; set; }

        private const int DefaultPage = 1;
        private const int DefaultPageSize = 100;
        // 开发阶段使用 1 天作为超时时间（足够长且不会溢出）
        private static readonly float RequestTimeout = 86400f; // 1 天 = 86400 秒

        protected override async void OnExecute()
        {
            var bagModel = this.GetModel<IBagModel>();
            if (bagModel == null)
            {
                Debug.LogError("SwitchBagTabCommand: BagModel未注册！");
                return;
            }

            if (bagModel.IsTabLoaded(NewIndex))
            {
                var items = bagModel.GetItemsByTab(NewIndex);
                this.SendEvent(new BagItemsUpdatedEvent
                {
                    TabIndex = NewIndex,
                    Items = items
                });
                Debug.Log($"背包Tab切换（使用缓存）: {OldIndex} -> {NewIndex}");
            }
            else
            {
                await LoadTabDataFromServer(bagModel);
            }
        }

        private async UniTask LoadTabDataFromServer(IBagModel bagModel)
        {
            // 获取所有物品（使用 ItemType.Unknown），然后在客户端根据 Category 过滤
            var response = await this.SendCommand(new GetBagItemsCommand(
                DefaultPage,
                DefaultPageSize,
                ItemType.Unknown,  // 获取所有物品
                RequestTimeout));
            if (response == null)
            {
                Debug.LogError($"背包Tab切换（网络获取）失败：Tab {NewIndex}");
                return;
            }

            // 转换所有物品并获取它们的 Category
            var allItems = new List<BagItemData>();
            if (response.Items != null)
            {
                foreach (var serverItem in response.Items)
                {
                    var itemData = BagItemConverter.ConvertFromServer(serverItem);
                    if (itemData != null)
                    {
                        allItems.Add(itemData);
                    }
                }
            }

            // 根据 Category 过滤到对应的 tab
            var targetCategory = MapTabIndexToCategory(NewIndex);
            var filteredItems = new List<BagItemData>();
            foreach (var item in allItems)
            {
                if (item.Category == targetCategory)
                {
                    filteredItems.Add(item);
                }
            }

            bagModel.SetItemsByTab(NewIndex, filteredItems);
            this.SendEvent(new BagItemsUpdatedEvent
            {
                TabIndex = NewIndex,
                Items = filteredItems
            });

            Debug.Log($"背包Tab切换（网络获取）: {OldIndex} -> {NewIndex}, Category={targetCategory}, 物品数量: {filteredItems.Count}");
        }

        /// <summary>
        /// 将 Tab 索引映射到 Category（根据 Enum_Order）
        /// </summary>
        private cfg.Enum_Order MapTabIndexToCategory(int tabIndex)
        {
            switch (tabIndex)
            {
                case 0:
                    return cfg.Enum_Order.Resource;  // 资源页
                case 1:
                    return cfg.Enum_Order.Speed;    // 加速页
                case 2:
                    return cfg.Enum_Order.Box;      // 宝箱页
                case 3:
                    return cfg.Enum_Order.Equit;   // 装备页
                case 4:
                    return cfg.Enum_Order.Material; // 材料页
                case 5:
                    return cfg.Enum_Order.Other;   // 其他页
                default:
                    return cfg.Enum_Order.NONE;
            }
        }
    }
}

