using System.Collections.Generic;
using PitayaGame.GameSvr;

namespace QFramework.UI
{
    /// <summary>
    /// 使用物品后服务器返回的奖励列表事件
    /// </summary>
    public struct RewardsObtainedEvent
    {
        public IList<RewardDelta> Deltas;
    }
}

