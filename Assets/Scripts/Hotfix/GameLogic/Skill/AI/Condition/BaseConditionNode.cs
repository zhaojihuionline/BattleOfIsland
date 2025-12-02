// BaseConditionNode.cs
using System.Collections.Generic;
using System.Linq;
using IData;

/// <summary>
/// 条件节点基类
/// </summary>
[System.Serializable]
public abstract class BaseConditionNode
{
    public abstract bool Check();
    public abstract bool Check(Data param);
    public abstract string GetExpressionString();

    /// <summary>
    /// 获取所有子节点（用于编辑器）
    /// </summary>
    public virtual List<BaseConditionNode> GetChildren() => new List<BaseConditionNode>();
}