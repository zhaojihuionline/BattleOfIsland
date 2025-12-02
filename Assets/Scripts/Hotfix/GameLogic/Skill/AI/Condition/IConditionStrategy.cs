// IConditionStrategy.cs
using IData;
using UnityEngine;

/// <summary>
/// 条件策略接口
/// </summary>
public interface IConditionStrategy
{
    ConditionType Type { get; }
    bool Check();
    /// <summary>
    /// 标准的随意上下文
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    bool Check(Data param);
    string GetDescription(float parameter);
}