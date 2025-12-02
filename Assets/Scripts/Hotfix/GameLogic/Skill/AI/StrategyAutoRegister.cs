// StrategyAutoRegister.cs
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class StrategyAutoRegister
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void AutoRegisterStrategies()
    {
        var assembly = Assembly.GetExecutingAssembly();

        // 自动注册条件策略
        var conditionTypes = assembly.GetTypes()
            .Where(t => typeof(IConditionStrategy).IsAssignableFrom(t)
                     && !t.IsInterface
                     && !t.IsAbstract);

        foreach (var type in conditionTypes)
        {
            var instance = Activator.CreateInstance(type) as IConditionStrategy;
            if (instance != null)
            {
                ConditionStrategyRegistry.RegisterStrategy(instance.Type, instance);
            }
        }

        // 自动注册偏好策略
        var preferenceTypes = assembly.GetTypes()
            .Where(t => typeof(ITargetPreferenceStrategy).IsAssignableFrom(t)
                     && !t.IsInterface
                     && !t.IsAbstract);

        foreach (var type in preferenceTypes)
        {
            var instance = Activator.CreateInstance(type) as ITargetPreferenceStrategy;
            if (instance != null)
            {
                PreferenceStrategyRegistry.RegisterStrategy(instance.Type, instance);
            }
        }
    }
}