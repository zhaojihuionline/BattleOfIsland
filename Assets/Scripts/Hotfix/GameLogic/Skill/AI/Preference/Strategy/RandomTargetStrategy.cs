// NearestTargetStrategy.cs
using System.Collections.Generic;
using System.Linq;
using cfg;
using IData;
using UnityEngine;

/// <summary>
/// 纯随机策略 - 随机权重
/// </summary>
public class RandomTargetStrategy : ITargetPreferenceStrategy
{
    public TargetPreferenceType Type => TargetPreferenceType.RandomTarget;

    public float CalculateScore()
    {
        return 0;
    }

    public float CalculateScore(Data param)
    {
        //直接返回一个随机权重  到时候选择最高的就好了
        return Random.Range(0, 1f);
    }

}