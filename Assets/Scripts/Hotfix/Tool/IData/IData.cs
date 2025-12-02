using System;
using System.Collections.Generic;
using UnityEngine;

namespace IData
{
    [Serializable]
    public class Data
    {
        public Dictionary<string, IVariable> data = new();

        // 设置字段的方法
        public void SetField<T>(Enum key, T value, bool isadd = true)
        {
            SetField(key.ToString(), value, isadd);
        }
        public void SetField<T>(string key, T value, bool isadd = true)
        {
            if (data.ContainsKey(key))
            {
                if (data[key] is Variable<T> variable)
                {
                    variable.SetValue(value);
                }
                else
                {
                    data[key] = new Variable<T>(value);
                }
                // #if UNITY_EDITOR
                //                 Debug.Log($"Set field '{key}' to value '{value}' type{typeof(T)}");
                // #endif
            }
            else
            {
                if (isadd)
                {
                    data[key] = new Variable<T>(value);
                    // #if UNITY_EDITOR
                    //                     Debug.Log($"Field '{key}' added with value '{value}' type{typeof(T)}");
                    // #endif
                }
                else
                {
                    // #if UNITY_EDITOR
                    //                     Debug.LogError($"修改 '{key}' 值为 '{value}' 失败 未添加 type{typeof(T)}");
                    // #endif
                }
            }
        }

        public void SetVariable<T>(Enum key, T value, bool isadd = true) where T : IVariable
        {
            SetVariable(key.ToString(), value, isadd);
        }
        public void SetVariable<T>(string key, T value, bool isadd = true) where T : IVariable
        {
            if (data.ContainsKey(key))
            {
                data[key] = value;
                // #if UNITY_EDITOR
                //                 Debug.Log($"Set Variable '{key}' to value '{value}' type{typeof(T)}");
                // #endif
            }
            else
            {
                if (isadd)
                {
                    data.Add(key, value);
                    // #if UNITY_EDITOR
                    //                     Debug.Log($"Variable '{key}' added with value '{value}' type{typeof(T)}");
                    // #endif
                }
                else
                {
                    // #if UNITY_EDITOR
                    //                     Debug.LogError($"修改 '{key}' 值为 '{value}' 失败 未添加 type{typeof(T)}");
                    // #endif
                }
            }
        }

        // 获取字段的泛型方法
        public T GetField<T>(Enum key)
        {
            return GetField<T>(key.ToString());
        }
        public T GetField<T>(string key)
        {
            if (data.TryGetValue(key, out var variable) && variable is Variable<T> typedVariable)
            {
                // #if UNITY_EDITOR
                //                 Debug.Log($"Retrieved field '{key}' with value '{typedVariable.GetValue()}'");
                // #endif
                return typedVariable.GetValue();
            }
            // #if UNITY_EDITOR
            //             Debug.Log($"Field '{key}' not found or type mismatch.");
            // #endif
            return default;
        }
        public T GetVariable<T>(Enum key)
        {
            return GetVariable<T>(key.ToString());
        }
        public Variable<T> GetVariable<T>(string key)
        {
            if (data.ContainsKey(key))
            {
                return data[key] as Variable<T>;
            }
            return null;
        }

        // 安全的获取Variable方法
        public bool TryGetVariable<T>(Enum key, out Variable<T> variable)
        {
            return TryGetVariable<T>(key.ToString(), out variable);
        }
        public bool TryGetVariable<T>(string key, out Variable<T> variable)
        {
            if (data.TryGetValue(key, out var varObj) && varObj is Variable<T> typedVariable)
            {
                variable = typedVariable;
                return true;
            }
            variable = null;
            return false;
        }

        // 安全的获取字段方法
        public bool TryGetField<T>(Enum key, out T value)
        {
            return TryGetField<T>(key.ToString(), out value);
        }
        public bool TryGetField<T>(string key, out T value)
        {
            if (data.TryGetValue(key, out var variable) && variable is Variable<T> typedVariable)
            {
                value = typedVariable.GetValue();
                // #if UNITY_EDITOR
                //                 Debug.Log($"Retrieved field '{key}' with value '{value}'");
                // #endif
                return true;
            }

            // #if UNITY_EDITOR
            //             Debug.Log($"Field '{key}' not found or type mismatch.");
            // #endif
            value = default;
            return false;
        }

        public void Clear()
        {
            data.Clear();
        }

        public bool ContainsKey(string key)
        {
            return data.ContainsKey(key);
        }
    }

    public interface IVariable
    {
    }
    [Serializable]
    public class Variable<T> : IVariable
    {
        public T Value { get; set; }

        public Variable() { }
        public Variable(T value) => Value = value;
        public Variable(object value) => Value = (T)value;

        // 允许显式设置值（避免装箱）
        public void SetValue(T value) => Value = value;

        public T GetValue() => Value;

        // 支持隐式类型转换
        public static implicit operator T(Variable<T> variable) => variable.Value;

        // 操作符重载
        public static Variable<T> operator +(Variable<T> a, Variable<T> b)
        {
            dynamic dynamicA = a.Value;
            dynamic dynamicB = b.Value;
            return new Variable<T>(dynamicA + dynamicB);
        }

        public static Variable<T> operator +(Variable<T> a, T b)
        {
            dynamic dynamicA = a.Value;
            a.Value = dynamicA + b;
            return a;
        }

        public static Variable<T> operator -(Variable<T> a, Variable<T> b)
        {
            dynamic dynamicA = a.Value;
            dynamic dynamicB = b.Value;
            return new Variable<T>(dynamicA - dynamicB);
        }

        public static Variable<T> operator -(Variable<T> a, T b)
        {
            dynamic dynamicA = a.Value;
            a.Value = dynamicA - b;
            return a;
        }

        public static Variable<T> operator *(Variable<T> a, Variable<T> b)
        {
            dynamic dynamicA = a.Value;
            dynamic dynamicB = b.Value;
            return new Variable<T>(dynamicA * dynamicB);
        }

        public static Variable<T> operator *(Variable<T> a, T b)
        {
            dynamic dynamicA = a.Value;
            a.Value = dynamicA * b;
            return a;
        }

        public static Variable<T> operator /(Variable<T> a, Variable<T> b)
        {
            dynamic dynamicA = a.Value;
            dynamic dynamicB = b.Value;

            // 处理整数除法的情况
            if (typeof(T) == typeof(int))
            {
                if (dynamicB == 0) throw new DivideByZeroException("Division by zero is not allowed.");
                return new Variable<T>(dynamicA / dynamicB);
            }

            return new Variable<T>(dynamicA / dynamicB);
        }

        public static Variable<T> operator /(Variable<T> a, T b)
        {
            dynamic dynamicA = a.Value;
            dynamic dynamicB = b;

            // 处理整数除法的情况
            if (typeof(T) == typeof(int))
            {
                if (dynamicB == 0) throw new DivideByZeroException("Division by zero is not allowed.");
                return new Variable<T>(dynamicA / dynamicB);
            }
            a.Value = dynamicA / b;
            return a;
        }
    }
}