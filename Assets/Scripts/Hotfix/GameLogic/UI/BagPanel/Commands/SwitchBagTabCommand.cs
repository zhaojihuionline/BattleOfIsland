using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace QFramework.UI
{
    /// <summary>
    /// 切换背包 Tab 命令：只使用缓存数据，不再从网络请求
    /// 注意：在打开背包时（BagPanel.OnInit）会从网络获取一次所有数据并分配到各个 tab
    /// </summary>
    public class SwitchBagTabCommand : AbstractCommand
    {
        public int OldIndex { get; set; }
        public int NewIndex { get; set; }


        protected override void OnExecute()
        {
            var bagModel = this.GetModel<IBagModel>();
            if (bagModel == null)
            {
                Debug.LogError("SwitchBagTabCommand: BagModel未注册！");
                return;
            }

            // 只使用缓存数据，不再从网络请求
            // 如果 tab 未加载，返回空列表
            var items = bagModel.GetItemsByTab(NewIndex);
            this.SendEvent(new BagItemsUpdatedEvent
            {
                TabIndex = NewIndex,
                Items = items
            });
            
            if (bagModel.IsTabLoaded(NewIndex))
            {
                Debug.Log($"背包Tab切换（使用缓存）: {OldIndex} -> {NewIndex}, 物品数量: {items.Count}");
            }
            else
            {
                Debug.LogWarning($"背包Tab切换: Tab {NewIndex} 未加载，返回空列表。请确保在打开背包时已从网络加载所有数据。");
            }
        }

    }
}

