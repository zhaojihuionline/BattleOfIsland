// NearestTargetStrategy.cs
using System.Collections.Generic;
using System.Linq;
using cfg;
using IData;
using UnityEngine;

/// <summary>
/// 最近目标策略 - 距离越近得分越高
/// </summary>
public class NearestTargetStrategy : ITargetPreferenceStrategy
{
    public TargetPreferenceType Type => TargetPreferenceType.Nearest;

    public float CalculateScore()
    {
        return 0;
    }

    public float CalculateScore(Data param)
    {
        GameObject target = param.GetField<GameObject>("target");
        GameObject self = param.GetField<GameObject>("self");
        if (target?.transform == null) return 0f;
        // 计算当前目标距离
        float distance = Vector3.Distance(self.transform.position, target.transform.position);
        // 使用指数衰减：距离越近得分越高
        // parameter 控制衰减速度（50 = 中等衰减  越大衰减越强）
        float decayFactor = Mathf.Max(1, 0.1f) / 50f;
        return Mathf.Exp(-distance * decayFactor);
    }

}