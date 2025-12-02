// PreferenceStrategyRegistry.cs
using System.Collections.Generic;

/// <summary>
/// 偏好策略注册表
/// </summary>
public static class PreferenceStrategyRegistry
{
    private static readonly Dictionary<TargetPreferenceType, ITargetPreferenceStrategy> _strategies = new Dictionary<TargetPreferenceType, ITargetPreferenceStrategy>();

    // static PreferenceStrategyRegistry()
    // {
    //     // 注册默认策略
    //     RegisterStrategy(TargetPreferenceType.Nearest, new NearestTargetStrategy());
    //     RegisterStrategy(TargetPreferenceType.RandomTarget, new RandomTargetStrategy());
    //     RegisterStrategy(TargetPreferenceType.HealthBelowPercent, new HealthBelowPercentStrategy());
    //     // 可以在这里添加更多默认策略
    // }

    /// <summary>
    /// 注册偏好策略
    /// </summary>
    public static void RegisterStrategy(TargetPreferenceType type, ITargetPreferenceStrategy strategy)
    {
        _strategies[type] = strategy;
    }

    /// <summary>
    /// 获取偏好策略
    /// </summary>
    public static ITargetPreferenceStrategy GetStrategy(TargetPreferenceType type)
    {
        return _strategies.TryGetValue(type, out var strategy) ? strategy : null;
    }

    /// <summary>
    /// 获取所有已注册的策略类型
    /// </summary>
    public static IEnumerable<TargetPreferenceType> GetRegisteredTypes()
    {
        return _strategies.Keys;
    }
}