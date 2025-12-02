using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public static class GenericFactoryEx<TKey, TBase> where TBase : class
{
    private static readonly Dictionary<TKey, Func<TBase>> _creatorMap = new();
    private static readonly Dictionary<TKey, Type> _typeMap = new();

    /// <summary>
    /// 初始化工厂。
    /// 可指定 keySelector (如何从实例提取 key)
    /// 或 keyPropertyName (指定实例属性名，如 "attributeType"、"Id"、"CardType" 等)。
    /// </summary>
    public static void Initialize(Func<TBase, TKey> keySelector = null, string keyPropertyName = "attributeType")
    {
        _creatorMap.Clear();
        _typeMap.Clear();

        try
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .Where(t => t.IsClass && !t.IsAbstract && typeof(TBase).IsAssignableFrom(t));

            foreach (var type in types)
            {
                try
                {
                    TKey key = default;

                    // -----------------------------
                    // 1?? 若用户自定义了提取逻辑
                    // -----------------------------
                    if (keySelector != null)
                    {
                        var temp = Activator.CreateInstance(type) as TBase;
                        key = keySelector(temp);
                    }
                    else
                    {
                        // -----------------------------
                        // 2?? 优先读取实例属性 (默认名 attributeType)
                        // -----------------------------
                        var prop = type.GetProperty(keyPropertyName,
                            BindingFlags.Public | BindingFlags.Instance);

                        if (prop != null && typeof(TKey).IsAssignableFrom(prop.PropertyType))
                        {
                            var temp = Activator.CreateInstance(type) as TBase;
                            key = (TKey)prop.GetValue(temp);
                        }
                        else
                        {
                            // -----------------------------
                            // 3?? 回退读取静态字段 Key/Id/Type
                            // -----------------------------
                            var field = type.GetField("Key", BindingFlags.Public | BindingFlags.Static)
                                        ?? type.GetField("Id", BindingFlags.Public | BindingFlags.Static)
                                        ?? type.GetField("Type", BindingFlags.Public | BindingFlags.Static);

                            if (field != null && typeof(TKey).IsAssignableFrom(field.FieldType))
                            {
                                key = (TKey)field.GetValue(null);
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    $"类型 {type.Name} 未声明可识别的 Key。" +
                                    $"请实现属性 '{keyPropertyName}' 或提供静态字段 Key。");
                            }
                        }
                    }

                    // -----------------------------
                    // 构建表达式树创建委托
                    // -----------------------------
                    var ctor = type.GetConstructor(Type.EmptyTypes);
                    if (ctor == null)
                        throw new MissingMethodException($"类型 {type.Name} 无无参构造。");

                    var lambda = Expression.Lambda<Func<TBase>>(Expression.New(ctor));
                    var compiled = lambda.Compile();

                    _creatorMap[key] = compiled;
                    _typeMap[key] = type;

                    Debug.Log($"[Factory]Registered {type.Name} with key {key}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Factory]注册失败：{type.Name} → {ex.Message}");
                }
            }

            Debug.Log($"[Factory] 初始化完成，共注册 {_creatorMap.Count} 个类型。");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public static TBase Create(TKey key)
    {
        if (_creatorMap.TryGetValue(key, out var creator))
            return creator();

        Debug.LogWarning($"[Factory] 未找到 Key = {key} 的类型。");
        return null;
    }

    public static IEnumerable<TKey> GetAllKeys() => _creatorMap.Keys;
}
