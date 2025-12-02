// PreferenceNode.cs
using System.Collections.Generic;
using IData;
using UnityEngine;

/// <summary>
/// 攻击偏好节点
/// </summary>
[System.Serializable]
public class PreferenceNode
{
    [Header("偏好类型")]
    public TargetPreferenceType preferenceType;

    [Header("参数配置")]
    public float parameter = 50f;

    [Header("启用状态")]
    public bool enabled = true;

    /// <summary>
    /// 计算目标得分
    /// </summary>
    public float CalculateScore(Data param)
    {
        if (!enabled) return 0f;
        param.SetField("parameter", parameter);
        var strategy = PreferenceStrategyRegistry.GetStrategy(preferenceType);
        if (strategy == null) return 0f;
        return strategy.CalculateScore(param);

    }

    /// <summary>
    /// 获取偏好描述
    /// </summary>
    public string GetDescription()
    {
        return preferenceType switch
        {
            TargetPreferenceType.Nearest => "最近的敌人",
            TargetPreferenceType.HealthBelowPercent => $"血量低于{parameter}%的敌人",
            TargetPreferenceType.RandomTarget => $"纯随机敌人",
            _ => "未知偏好"
        };
    }
}