using System.Collections.Generic;
using PitayaGame.GameSvr;

namespace QFramework.UI
{
    /// <summary>
    /// 统一的奖励变更事件（由奖励处理模块发送）
    /// </summary>
    public struct RewardDeltaAppliedEvent
    {
        public List<RewardDelta> Deltas;
    }
}


