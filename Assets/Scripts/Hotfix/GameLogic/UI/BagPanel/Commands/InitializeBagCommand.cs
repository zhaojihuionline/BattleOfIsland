using System.Collections.Generic;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using PitayaGame.GameSvr;
using QFramework;
using UnityEngine;
using Random = UnityEngine.Random;

namespace QFramework.UI
{
    /// <summary>
    /// 初始化背包命令：根据最新的 tbitem.json 配置生成测试数据并调用 AddBagItemsCommand
    /// </summary>
    public class InitializeBagCommand : AbstractCommand<UniTask>
    {
        [Serializable]
        private class TbItemRecord
        {
            public int ItemID;
            public int ItemType;
            public int Category;
            public int UseType;
            public int UseLevel;
            public int RewardID;
        }

        [Serializable]
        private class TbItemWrapper
        {
            public TbItemRecord[] Items;
        }

        protected override async UniTask OnExecute()
        {
            Debug.Log("InitializeBagCommand: 开始初始化背包测试数据（动态读取 tbitem.json）...");

            var testItems = new List<RewardItem>();

            // 读取 tbitem.json
            var textAsset = Resources.Load<TextAsset>("LuabanGenerateDatas/tbitem");
            if (textAsset == null || string.IsNullOrEmpty(textAsset.text))
            {
                Debug.LogError("InitializeBagCommand: 无法读取 tbitem.json（路径：Resources/LuabanGenerateDatas/tbitem），初始化终止");
                return;
            }

            // JsonUtility 需要包装一层对象
            var wrappedJson = "{\"Items\":" + textAsset.text + "}";
            TbItemWrapper wrapper = null;
            try
            {
                wrapper = JsonUtility.FromJson<TbItemWrapper>(wrappedJson);
            }
            catch (Exception ex)
            {
                Debug.LogError($"InitializeBagCommand: 解析 tbitem.json 失败: {ex.Message}");
                return;
            }

            if (wrapper?.Items == null || wrapper.Items.Length == 0)
            {
                Debug.LogError("InitializeBagCommand: tbitem.json 数据为空，初始化终止");
                return;
            }

            // 取前 120 条（避免生成过大），可按需要调整
            var sampledItems = wrapper.Items.Take(120).ToList();

            foreach (var item in sampledItems)
            {
                switch (item.ItemType)
                {
                    case 1: // 货币
                        testItems.Add(new RewardItem
                        {
                            Currency = new CurrencyChange
                            {
                                ItemId = item.ItemID,
                                Delta = 1000
                            }
                        });
                        break;
                    case 2: // 资源
                        testItems.Add(new RewardItem
                        {
                            Resource = new ResourceChange
                            {
                                ItemId = item.ItemID,
                                Delta = 500
                            }
                        });
                        break;
                    default: // 其他物品
                        if (item.Category != (int)cfg.Enum_Order.Equit)
                        {
                            testItems.Add(new RewardItem
                            {
                                Item = new ItemReward
                                {
                                    ItemId = item.ItemID,
                                    Amount = Random.Range(100, 1001)
                                }
                            });
                        }
                        break;
                }
            }

            // 调用 AddBagItemsCommand
            var response = await this.SendCommand(new AddBagItemsCommand(
                testItems,
                "测试初始化",
                "init_bag_test"
            ));

            if (response != null)
            {
                Debug.Log($"InitializeBagCommand: 背包初始化成功，添加了 {testItems.Count} 个物品");
            }
            else
            {
                Debug.LogError("InitializeBagCommand: 背包初始化失败");
            }
        }
    }
}

