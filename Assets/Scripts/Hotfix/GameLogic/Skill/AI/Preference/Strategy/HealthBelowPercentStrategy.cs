// NearestTargetStrategy.cs
using System.Collections.Generic;
using System.Linq;
using cfg;
using IData;
using UnityEngine;

/// <summary>
/// 血量百分比策略 - 低于n  越低得分越高
/// </summary>
public class HealthBelowPercentStrategy : ITargetPreferenceStrategy
{
    public TargetPreferenceType Type => TargetPreferenceType.HealthBelowPercent;

    public float CalculateScore()
    {
        return 0;
    }

    public float CalculateScore(Data param)
    {
        GameObject target = param.GetField<GameObject>("target");

        if (target == null) return 0f;

        // 获取目标的血量组件
        var healthComponent = target.GetComponent<IHaveHP>();
        if (healthComponent == null) return 0f;

        // 计算血量百分比
        float healthPercent = healthComponent.HealthPercent;
        float parameter = param.GetField<float>("parameter");

        // 如果血量高于50%，得分为0
        if (healthPercent > parameter) return 0f;

        // 血量低于50%时，血量越低得分越高
        // 血量0%时得分为1，血量50%时得分为0
        return (parameter - healthPercent) * 2f;
    }

}