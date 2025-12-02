// HealthBelowPercentCondition.cs
using IData;
using UnityEngine;

/// <summary>
/// 血量低于百分比条件
/// </summary>
public class HealthBelowPercentCondition : IConditionStrategy
{
    public ConditionType Type => ConditionType.HealthBelowPercent;
    public bool Check()
    {
        return true;
    }
    public bool Check(Data param)
    {
        var res = param.GetField<IRoleEntity>("test");
        float parameter = param.GetField<float>("param");
        return res?.HealthPercent <= (parameter / 100f);
    }
    public string GetDescription(float parameter)
    {
        return $"自身血量低于{parameter}%";
    }
}