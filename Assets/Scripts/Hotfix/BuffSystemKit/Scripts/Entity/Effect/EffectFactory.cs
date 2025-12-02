using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class EffectFactory
{
    private static Dictionary<cfg.AttributeType, Type> _effectTypeMap;

    static EffectFactory()
    {
        _effectTypeMap = new Dictionary<cfg.AttributeType, Type>();

        var effectTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t != typeof(EffectEntity) && typeof(EffectEntity).IsAssignableFrom(t))
            .Select(t => (Type: t, Instance: (EffectEntity)Activator.CreateInstance(t)));

        foreach (var (type, instance) in effectTypes)
        {
            _effectTypeMap[instance.attributeType] = type;
        }
    }

    public static EffectEntity CreateEffect(cfg.AttributeType type)
    {
        if (_effectTypeMap.TryGetValue(type, out var effectType))
        {
            return (EffectEntity)Activator.CreateInstance(effectType);
        }

        UnityEngine.Debug.LogWarning($"未找到对应的Effect类型: {type}");
        return null;
    }
}
