// ConditionBuilder.cs
using System.Collections.Generic;

/// <summary>
/// 条件构建器 - 支持链式配置复杂条件
/// </summary>
public class ConditionBuilder
{
    private OperatorNode currentNode;
    private Stack<OperatorNode> nodeStack = new Stack<OperatorNode>();

    public ConditionBuilder(OperatorNode rootNode)
    {
        currentNode = rootNode;
        nodeStack.Push(currentNode);
    }

    public ConditionBuilder Condition(ConditionType type, float param)
    {
        CurrentNode().AddCondition(type, param);
        return this;
    }

    public ConditionBuilder And()
    {
        var andNode = new OperatorNode(LogicOperator.AND);
        CurrentNode().children.Add(andNode);
        nodeStack.Push(andNode);
        return this;
    }

    public ConditionBuilder Or()
    {
        var orNode = new OperatorNode(LogicOperator.OR);
        CurrentNode().children.Add(orNode);
        nodeStack.Push(orNode);
        return this;
    }

    public ConditionBuilder Not()
    {
        var notNode = new OperatorNode(LogicOperator.NOT);
        CurrentNode().children.Add(notNode);
        nodeStack.Push(notNode);
        return this;
    }

    public ConditionBuilder End()
    {
        if (nodeStack.Count > 1)
            nodeStack.Pop();
        return this;
    }

    private OperatorNode CurrentNode()
    {
        return nodeStack.Peek();
    }
}