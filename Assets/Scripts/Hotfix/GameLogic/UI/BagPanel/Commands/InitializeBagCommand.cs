using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PitayaGame.GameSvr;
using QFramework;
using UnityEngine;

namespace QFramework.UI
{
    /// <summary>
    /// 初始化背包命令：根据 tbitem.json 配置生成测试数据并调用 AddBagItemsCommand
    /// </summary>
    public class InitializeBagCommand : AbstractCommand<UniTask>
    {
        protected override async UniTask OnExecute()
        {
            Debug.Log("InitializeBagCommand: 开始初始化背包测试数据...");

            var testItems = new List<RewardItem>();

            // ========== Category 0: 货币和资源 ==========
            // 货币（ItemType: 1, ItemID: 1100001）
            testItems.Add(new RewardItem
            {
                Currency = new CurrencyChange { ItemId = 1100001, Delta = 10000 } // 货币
            });

            // 资源（ItemType: 2）
            testItems.Add(new RewardItem
            {
                Resource = new ResourceChange { ItemId = 2100001, Delta = 50 } // 木材
            });
            testItems.Add(new RewardItem
            {
                Resource = new ResourceChange { ItemId = 2100002, Delta = 30 } // 肉
            });
            testItems.Add(new RewardItem
            {
                Resource = new ResourceChange { ItemId = 2100003, Delta = 20 } // 矿
            });

            // ========== Category 2: 消耗品（ItemType: 3） ==========
            // 通用建造加速 (3100001-3100004, ItemSubType: 8)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100001, Amount = 10 } }); // 通用建造加速1分钟 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100002, Amount = 5 } }); // 通用建造加速2分钟 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100003, Amount = 5 } }); // 通用建造加速3分钟 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100004, Amount = 3 } }); // 通用建造加速4分钟 (Quality: 4)

            // 建筑建造加速 (3100005-3100008, ItemSubType: 9)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100005, Amount = 10 } }); // 建筑建造加速1分钟 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100006, Amount = 5 } }); // 建筑建造加速2分钟 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100007, Amount = 5 } }); // 建筑建造加速3分钟 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100008, Amount = 3 } }); // 建筑建造加速4分钟 (Quality: 4)

            // 科研建造加速 (3100009-3100012, ItemSubType: 10)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100009, Amount = 10 } }); // 科研建造加速1分钟 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100010, Amount = 5 } }); // 科研建造加速2分钟 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100011, Amount = 5 } }); // 科研建造加速3分钟 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100012, Amount = 3 } }); // 科研建造加速4分钟 (Quality: 4)

            // 治疗建造加速 (3100013-3100016, ItemSubType: 11)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100013, Amount = 10 } }); // 治疗建造加速1分钟 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100014, Amount = 5 } }); // 治疗建造加速2分钟 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100015, Amount = 5 } }); // 治疗建造加速3分钟 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100016, Amount = 3 } }); // 治疗建造加速4分钟 (Quality: 4)

            // 训练建造加速 (3100017-3100020, ItemSubType: 12)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100017, Amount = 10 } }); // 训练建造加速1分钟 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100018, Amount = 5 } }); // 训练建造加速2分钟 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100019, Amount = 5 } }); // 训练建造加速3分钟 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 3100020, Amount = 3 } }); // 训练建造加速4分钟 (Quality: 4)

            // ========== Category 1: 材料包（ItemType: 4, ItemSubType: 13） ==========
            // 矿物小包 (4100001-4100006)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100001, Amount = 3 } }); // 矿物小包1 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100002, Amount = 3 } }); // 矿物小包2 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100003, Amount = 3 } }); // 矿物小包3 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100004, Amount = 3 } }); // 矿物小包4 (Quality: 4)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100005, Amount = 3 } }); // 矿物小包5 (Quality: 5)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100006, Amount = 3 } }); // 矿物小包6 (Quality: 6)

            // 木材小包 (4100007-4100012)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100007, Amount = 3 } }); // 木材小包1 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100008, Amount = 3 } }); // 木材小包2 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100009, Amount = 3 } }); // 木材小包3 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100010, Amount = 3 } }); // 木材小包4 (Quality: 4)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100011, Amount = 3 } }); // 木材小包5 (Quality: 5)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100012, Amount = 3 } }); // 木材小包6 (Quality: 6)

            // 肉小包 (4100013-4100018)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100013, Amount = 3 } }); // 肉小包1 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100014, Amount = 3 } }); // 肉小包2 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100015, Amount = 3 } }); // 肉小包3 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100016, Amount = 3 } }); // 肉小包4 (Quality: 4)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100017, Amount = 3 } }); // 肉小包5 (Quality: 5)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100018, Amount = 3 } }); // 肉小包6 (Quality: 6)

            // ========== Category 5: 随机资源宝箱（ItemType: 4, ItemSubType: 14） ==========
            // 随机资源宝箱 (4100019-4100024)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100019, Amount = 2 } }); // 随机资源宝箱1 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100020, Amount = 2 } }); // 随机资源宝箱2 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100021, Amount = 2 } }); // 随机资源宝箱3 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100022, Amount = 2 } }); // 随机资源宝箱4 (Quality: 4)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100023, Amount = 2 } }); // 随机资源宝箱5 (Quality: 5)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100024, Amount = 2 } }); // 随机资源宝箱6 (Quality: 6)

            // ========== Category 5: 资源自选宝箱（ItemType: 4, ItemSubType: 15） ==========
            // 资源自选宝箱 (4100025-4100030)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100025, Amount = 2 } }); // 资源自选宝箱1 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100026, Amount = 2 } }); // 资源自选宝箱2 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100027, Amount = 2 } }); // 资源自选宝箱3 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100028, Amount = 2 } }); // 资源自选宝箱4 (Quality: 4)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100029, Amount = 2 } }); // 资源自选宝箱5 (Quality: 5)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 4100030, Amount = 2 } }); // 资源自选宝箱6 (Quality: 6)

            // ========== Category 4: 装备（ItemType: 5, ItemSubType: 19） ==========
            // 注意：装备不可堆叠（IsStackable: false），测试时保持注释状态
            // 弓兵护手 (5100001-5100006)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100001, Amount = 1 } }); // 弓兵护手1 (Quality: 1)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100002, Amount = 1 } }); // 弓兵护手2 (Quality: 2)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100003, Amount = 1 } }); // 弓兵护手3 (Quality: 3)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100004, Amount = 1 } }); // 弓兵护手4 (Quality: 4)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100005, Amount = 1 } }); // 弓兵护手5 (Quality: 5)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100006, Amount = 1 } }); // 弓兵护手6 (Quality: 6)

            // 骑兵护手 (5100007-5100012)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100007, Amount = 1 } }); // 骑兵护手1 (Quality: 1)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100008, Amount = 1 } }); // 骑兵护手2 (Quality: 2)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100009, Amount = 1 } }); // 骑兵护手3 (Quality: 3)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100010, Amount = 1 } }); // 骑兵护手4 (Quality: 4)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100011, Amount = 1 } }); // 骑兵护手5 (Quality: 5)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100012, Amount = 1 } }); // 骑兵护手6 (Quality: 6)

            // 盾兵护手 (5100013-5100018)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100013, Amount = 1 } }); // 盾兵护手1 (Quality: 1)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100014, Amount = 1 } }); // 盾兵护手2 (Quality: 2)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100015, Amount = 1 } }); // 盾兵护手3 (Quality: 3)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100016, Amount = 1 } }); // 盾兵护手4 (Quality: 4)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100017, Amount = 1 } }); // 盾兵护手5 (Quality: 5)
            // testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 5100018, Amount = 1 } }); // 盾兵护手6 (Quality: 6)

            // ========== Category 5: 碎片（ItemType: 6） ==========
            // 橙色通用英雄碎片 (6100001-6100006, ItemSubType: 21)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100001, Amount = 20 } }); // 橙色通用英雄碎片1 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100002, Amount = 20 } }); // 橙色通用英雄碎片2 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100003, Amount = 20 } }); // 橙色通用英雄碎片3 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100004, Amount = 20 } }); // 橙色通用英雄碎片4 (Quality: 4)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100005, Amount = 20 } }); // 橙色通用英雄碎片5 (Quality: 5)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100006, Amount = 20 } }); // 橙色通用英雄碎片6 (Quality: 6)

            // 蓝色通用英雄碎片 (6100007-6100012, ItemSubType: 21)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100007, Amount = 20 } }); // 蓝色通用英雄碎片1 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100008, Amount = 20 } }); // 蓝色通用英雄碎片2 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100009, Amount = 20 } }); // 蓝色通用英雄碎片3 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100010, Amount = 20 } }); // 蓝色通用英雄碎片4 (Quality: 4)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100011, Amount = 20 } }); // 蓝色通用英雄碎片5 (Quality: 5)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100012, Amount = 20 } }); // 蓝色通用英雄碎片6 (Quality: 6)

            // 橙色通用英雄装备碎片 (6100013-6100018, ItemSubType: 22)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100013, Amount = 20 } }); // 橙色通用英雄装备碎片1 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100014, Amount = 20 } }); // 橙色通用英雄装备碎片2 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100015, Amount = 20 } }); // 橙色通用英雄装备碎片3 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100016, Amount = 20 } }); // 橙色通用英雄装备碎片4 (Quality: 4)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100017, Amount = 20 } }); // 橙色通用英雄装备碎片5 (Quality: 5)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100018, Amount = 20 } }); // 橙色通用英雄装备碎片6 (Quality: 6)

            // 蓝色通用英雄装备碎片 (6100019-6100024, ItemSubType: 22)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100019, Amount = 20 } }); // 蓝色通用英雄装备碎片1 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100020, Amount = 20 } }); // 蓝色通用英雄装备碎片2 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100021, Amount = 20 } }); // 蓝色通用英雄装备碎片3 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100022, Amount = 20 } }); // 蓝色通用英雄装备碎片4 (Quality: 4)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100023, Amount = 20 } }); // 蓝色通用英雄装备碎片5 (Quality: 5)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100024, Amount = 20 } }); // 蓝色通用英雄装备碎片6 (Quality: 6)

            // 英雄装备强化素材 (6100025-6100030, ItemSubType: 23)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100025, Amount = 20 } }); // 英雄装备强化素材1 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100026, Amount = 20 } }); // 英雄装备强化素材2 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100027, Amount = 20 } }); // 英雄装备强化素材3 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100028, Amount = 20 } }); // 英雄装备强化素材4 (Quality: 4)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100029, Amount = 20 } }); // 英雄装备强化素材5 (Quality: 5)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100030, Amount = 20 } }); // 英雄装备强化素材6 (Quality: 6)

            // 英雄技能强化素材 (6100031-6100036, ItemSubType: 24)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100031, Amount = 20 } }); // 英雄技能强化素材1 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100032, Amount = 20 } }); // 英雄技能强化素材2 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100033, Amount = 20 } }); // 英雄技能强化素材3 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100034, Amount = 20 } }); // 英雄技能强化素材4 (Quality: 4)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100035, Amount = 20 } }); // 英雄技能强化素材5 (Quality: 5)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 6100036, Amount = 20 } }); // 英雄技能强化素材6 (Quality: 6)

            // ========== Category 6: 活动道具（ItemType: 7, ItemSubType: 30） ==========
            // 活动1道具 (7100001-7100006)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100001, Amount = 5 } }); // 活动1道具1 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100002, Amount = 5 } }); // 活动1道具2 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100003, Amount = 5 } }); // 活动1道具3 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100004, Amount = 5 } }); // 活动1道具4 (Quality: 4)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100005, Amount = 5 } }); // 活动1道具5 (Quality: 5)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100006, Amount = 5 } }); // 活动1道具6 (Quality: 6)

            // 活动2道具 (7100007-7100012)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100007, Amount = 5 } }); // 活动2道具1 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100008, Amount = 5 } }); // 活动2道具2 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100009, Amount = 5 } }); // 活动2道具3 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100010, Amount = 5 } }); // 活动2道具4 (Quality: 4)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100011, Amount = 5 } }); // 活动2道具5 (Quality: 5)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100012, Amount = 5 } }); // 活动2道具6 (Quality: 6)

            // 活动3道具 (7100013-7100018)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100013, Amount = 5 } }); // 活动3道具1 (Quality: 1)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100014, Amount = 5 } }); // 活动3道具2 (Quality: 2)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100015, Amount = 5 } }); // 活动3道具3 (Quality: 3)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100016, Amount = 5 } }); // 活动3道具4 (Quality: 4)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100017, Amount = 5 } }); // 活动3道具5 (Quality: 5)
            testItems.Add(new RewardItem { Item = new ItemReward { ItemId = 7100018, Amount = 5 } }); // 活动3道具6 (Quality: 6)

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

