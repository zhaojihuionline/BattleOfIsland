// ConditionStrategyRegistry.cs
using System;
using System.Collections.Generic;

/// <summary>
/// 条件策略注册表
/// </summary>
public static class ConditionStrategyRegistry
{
    private static readonly Dictionary<ConditionType, IConditionStrategy> _strategies = new Dictionary<ConditionType, IConditionStrategy>();

    static ConditionStrategyRegistry()
    {
        // 注册默认策略
        RegisterStrategy(ConditionType.HealthBelowPercent, new HealthBelowPercentCondition());
        // 可以在这里添加更多默认策略
    }

    /// <summary>
    /// 注册条件策略
    /// </summary>
    public static void RegisterStrategy(ConditionType type, IConditionStrategy strategy)
    {
        _strategies[type] = strategy;
    }

    /// <summary>
    /// 获取条件策略
    /// </summary>
    public static IConditionStrategy GetStrategy(ConditionType type)
    {
        return _strategies.TryGetValue(type, out var strategy) ? strategy : null;
    }

    /// <summary>
    /// 获取所有已注册的策略类型
    /// </summary>
    public static IEnumerable<ConditionType> GetRegisteredTypes()
    {
        return _strategies.Keys;
    }
}