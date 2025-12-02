// OperatorNode.cs
using System.Collections.Generic;
using System.Linq;
using IData;
using UnityEngine;

/// <summary>
/// 逻辑运算符节点
/// </summary>
[System.Serializable]
public class OperatorNode : BaseConditionNode
{
    [Header("逻辑运算符")]
    public LogicOperator logicOperator;

    [Header("子节点")]
    public List<BaseConditionNode> children = new List<BaseConditionNode>();

    public OperatorNode(LogicOperator op)
    {
        logicOperator = op;
    }


    public override bool Check()
    {
        return logicOperator switch
        {
            LogicOperator.AND => children.All(child => child.Check()),
            LogicOperator.OR => children.Any(child => child.Check()),
            LogicOperator.NOT => children.Count > 0 && !children[0].Check(),
            _ => true
        };
    }

    public override bool Check(Data param)
    {
        return logicOperator switch
        {
            LogicOperator.AND => children.All(child => child.Check(param)),
            LogicOperator.OR => children.Any(child => child.Check(param)),
            LogicOperator.NOT => children.Count > 0 && !children[0].Check(param),
            _ => true
        };
    }

    public override string GetExpressionString()
    {
        string operatorSymbol = logicOperator switch
        {
            LogicOperator.AND => "&",
            LogicOperator.OR => "|",
            LogicOperator.NOT => "!",
            _ => "?"
        };

        if (logicOperator == LogicOperator.NOT)
        {
            return children.Count > 0 ? $"{operatorSymbol}({children[0].GetExpressionString()})" : operatorSymbol;
        }

        if (children.Count == 0) return operatorSymbol;
        if (children.Count == 1) return $"{operatorSymbol}{children[0].GetExpressionString()}";

        var childExpressions = children.Select(child =>
            child is OperatorNode ? $"({child.GetExpressionString()})" : child.GetExpressionString());

        return string.Join($" {operatorSymbol} ", childExpressions);
    }

    public override List<BaseConditionNode> GetChildren() => children;

    /// <summary>
    /// 添加条件子节点
    /// </summary>
    public ConditionNode AddCondition(ConditionType type, float param)
    {
        var node = new ConditionNode(type, param);
        children.Add(node);
        return node;
    }

    /// <summary>
    /// 添加运算符子节点
    /// </summary>
    public OperatorNode AddOperator(LogicOperator op)
    {
        var node = new OperatorNode(op);
        children.Add(node);
        return node;
    }
}