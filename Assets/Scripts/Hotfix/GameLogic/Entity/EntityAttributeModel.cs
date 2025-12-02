using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

/// <summary>
/// 
/// </summary>
public interface IEntityAttributeModel : IModel
{
}
/// <summary>
/// 实体属性数据模型
/// </summary>
public class EntityAttributeModel : AbstractModel, IEntityAttributeModel
{
    Dictionary<string,int> AttributeValues = new Dictionary<string, int>();

    public int GetAttribute(string attributeName)
    {
        if (AttributeValues.TryGetValue(attributeName, out int value))
        {
            return value;
        }
        return 0;
    }

    public void SetAttribute(string attributeName, int value)
    {
        if(!AttributeValues.ContainsKey(attributeName))
        {
            AttributeValues[attributeName] = value;
        }
    }
    protected override void OnInit()
    {

    }

    protected override void OnDeinit()
    {
        base.OnDeinit();
        AttributeValues.Clear();
    }
}
