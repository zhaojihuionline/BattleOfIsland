using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using QFramework;
using UnityEngine;

/// <summary>
/// 单例通用工厂 - 支持按需注册和创建任意类型
/// </summary>
public class UniversalFactory : Singleton<UniversalFactory>
{
    private readonly Dictionary<Type, Func<object>> _creators = new();
    private readonly Dictionary<Type, Type> _interfaceMappings = new();
    private readonly Dictionary<Type, Type> _baseClassMappings = new();

    private UniversalFactory() { }

    /// <summary>
    /// 注册具体类型
    /// </summary>
    public void Register<T>() where T : class, new()
    {
        var type = typeof(T);
        if (_creators.ContainsKey(type)) return;

        try
        {
            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
                throw new InvalidOperationException($"类型 {type.Name} 必须有无参构造函数");

            var lambda = Expression.Lambda<Func<object>>(Expression.New(ctor));
            _creators[type] = lambda.Compile();

            Debug.Log($"[UniversalFactory] 注册类型: {type.Name}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[UniversalFactory] 注册失败 {type.Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// 注册接口/抽象类到具体实现的映射
    /// </summary>
    public void Register<TInterface, TImplementation>()
        where TImplementation : class, TInterface, new()
    {
        var interfaceType = typeof(TInterface);
        var implType = typeof(TImplementation);

        // 注册具体实现
        Register<TImplementation>();

        // 记录映射关系
        if (interfaceType.IsInterface)
        {
            _interfaceMappings[interfaceType] = implType;
        }
        else if (interfaceType.IsAbstract)
        {
            _baseClassMappings[interfaceType] = implType;
        }

        Debug.Log($"[UniversalFactory] 注册映射: {interfaceType.Name} -> {implType.Name}");
    }

    /// <summary>
    /// 创建类型实例
    /// </summary>
    public T Create<T>() where T : class
    {
        var type = typeof(T);
        return Create(type) as T;
    }

    /// <summary>
    /// 创建类型实例（非泛型版本）
    /// </summary>
    public object Create(Type type)
    {
        // 1. 如果是具体类，直接创建
        if (!type.IsAbstract && _creators.TryGetValue(type, out var creator))
        {
            return creator();
        }

        // 2. 如果是接口，查找接口映射
        if (type.IsInterface && _interfaceMappings.TryGetValue(type, out var implType))
        {
            return Create(implType);
        }

        // 3. 如果是抽象类，查找基类映射
        if (type.IsAbstract && _baseClassMappings.TryGetValue(type, out implType))
        {
            return Create(implType);
        }

        // 4. 尝试自动注册并创建
        if (!type.IsAbstract && !type.IsInterface)
        {
            try
            {
                // 动态注册
                var ctor = type.GetConstructor(Type.EmptyTypes);
                if (ctor != null)
                {
                    var lambda = Expression.Lambda<Func<object>>(Expression.New(ctor));
                    _creators[type] = lambda.Compile();
                    return _creators[type]();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UniversalFactory] 自动创建失败 {type.Name}: {ex.Message}");
            }
        }

        throw new InvalidOperationException($"无法创建类型 {type.Name}，请先注册该类型或对应的映射关系");
    }

    /// <summary>
    /// 检查类型是否已注册
    /// </summary>
    public bool IsRegistered<T>() => IsRegistered(typeof(T));

    public bool IsRegistered(Type type)
    {
        if (_creators.ContainsKey(type)) return true;
        if (type.IsInterface && _interfaceMappings.ContainsKey(type)) return true;
        if (type.IsAbstract && _baseClassMappings.ContainsKey(type)) return true;
        return false;
    }

    /// <summary>
    /// 清空所有注册
    /// </summary>
    public void Clear()
    {
        _creators.Clear();
        _interfaceMappings.Clear();
        _baseClassMappings.Clear();
        Debug.Log("[UniversalFactory] 已清空所有注册");
    }
}