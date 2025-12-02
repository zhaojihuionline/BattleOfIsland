// ConditionNode.cs
using IData;
using UnityEngine;

/// <summary>
/// 具体条件节点
/// </summary>
[System.Serializable]
public class ConditionNode : BaseConditionNode
{
    [Header("条件类型")]
    public ConditionType conditionType;

    [Header("参数")]
    public float parameter;

    public ConditionNode() { }

    public ConditionNode(ConditionType type, float param)
    {
        conditionType = type;
        parameter = param;
    }

    public override bool Check()
    {
        var strategy = ConditionStrategyRegistry.GetStrategy(conditionType);
        return strategy?.Check() ?? false;
    }

    public override bool Check(Data param)
    {
        var strategy = ConditionStrategyRegistry.GetStrategy(conditionType);
        return strategy?.Check(param) ?? false;
    }


    public override string GetExpressionString()
    {
        var strategy = ConditionStrategyRegistry.GetStrategy(conditionType);
        return strategy?.GetDescription(parameter) ?? "未知条件";
    }
}