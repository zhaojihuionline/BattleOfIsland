// ITargetPreferenceStrategy.cs
using System.Collections.Generic;
using IData;
using UnityEngine;

/// <summary>
/// 攻击偏好策略接口
/// </summary>
public interface ITargetPreferenceStrategy
{
    TargetPreferenceType Type { get; }
    float CalculateScore(Data param);
}