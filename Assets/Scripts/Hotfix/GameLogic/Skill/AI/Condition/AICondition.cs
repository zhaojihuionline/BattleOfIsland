// AICondition.cs
using IData;
using UnityEngine;

/// <summary>
/// 条件配置 - ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "NewAICondition", menuName = "AI/AI Condition")]
public class AICondition : ScriptableObject
{
    [Header("条件表达式根节点")]
    [SerializeField] private BaseConditionNode conditionRoot;

    [Header("调试信息")]
    [SerializeField, TextArea(3, 6)] private string conditionExpression;

    public BaseConditionNode ConditionRoot => conditionRoot;

    private void OnValidate()
    {
        UpdateExpression();
    }

    /// <summary>
    /// 检查条件
    /// </summary>
    public bool Check()
    {
        return conditionRoot?.Check() ?? true;
    }

    /// <summary>
    /// 检查条件
    /// </summary>
    public bool Check(Data param)
    {
        return conditionRoot?.Check(param) ?? true;
    }

    /// <summary>
    /// 创建条件表达式构建器
    /// </summary>
    public ConditionBuilder CreateBuilder()
    {
        conditionRoot = new OperatorNode(LogicOperator.AND);
        UpdateExpression();
        return new ConditionBuilder((OperatorNode)conditionRoot);
    }

    /// <summary>
    /// 更新表达式字符串
    /// </summary>
    private void UpdateExpression()
    {
        conditionExpression = conditionRoot?.GetExpressionString() ?? "未配置条件";
    }
}