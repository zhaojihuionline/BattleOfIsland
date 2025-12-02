// PreferenceGroup.cs
using System.Collections.Generic;
using System.Linq;
using IData;
using UnityEngine;

/// <summary>
/// 攻击偏好组 - 包含多个偏好节点
/// </summary>
[System.Serializable]
public class PreferenceGroup
{
    [Header("偏好组名称")]
    public string groupName = "偏好组";

    [Header("偏好节点列表")]
    public List<PreferenceNode> preferenceNodes = new List<PreferenceNode>();

    [Header("权重")]
    [Range(0.1f, 10f)]
    public float weight = 1f;

    [Header("启用状态")]
    public bool enabled = true;

    /// <summary>
    /// 计算该偏好组的得分
    /// </summary>
    public float CalculateScore(Data param)
    {
        if (!enabled) return 0f;

        float totalScore = 0f;
        foreach (var node in preferenceNodes.Where(n => n.enabled))
        {
            totalScore += node.CalculateScore(param);
        }

        return totalScore * weight;
    }

    /// <summary>
    /// 选择目标
    /// </summary>
    public GameObject SelectTarget(Data param)
    {
        List<GameObject> targets = param.GetField<List<GameObject>>("targetList");
        if (targets.Count == 0) return null;
        if (targets.Count == 1) return targets[0];

        var targetScores = new Dictionary<GameObject, float>();
        foreach (var target in targets)
        {
            param.SetField<GameObject>("target", target);

            float score = CalculateScore(param);
            targetScores[target] = score;

            //Debug.Log("========== PreferenceGroup 计算目标得分 ==========");
            //Debug.Log($"偏好组: {groupName}, 目标: {target.name}, 得分: {score}");
        }

        //因为是按照顺序遍历的各个节点 所以  一旦这一轮的得分为0的时候 说明没有目标 下边也可以不用判断了 
        if (targetScores.Values.All(v => Mathf.Approximately(v, 0f)))
        {
            Debug.Log("所有目标得分为0");
            return null;
        }

        return targetScores.OrderByDescending(x => x.Value).First().Key;
    }

    /// <summary>
    /// 添加偏好节点
    /// </summary>
    public PreferenceNode AddPreferenceNode(TargetPreferenceType type, float parameter = 50f)
    {
        var node = new PreferenceNode
        {
            preferenceType = type,
            parameter = parameter
        };
        preferenceNodes.Add(node);
        return node;
    }

    /// <summary>
    /// 移除偏好节点
    /// </summary>
    public void RemovePreferenceNode(PreferenceNode node)
    {
        preferenceNodes.Remove(node);
    }

    /// <summary>
    /// 获取偏好组描述
    /// </summary>
    public string GetDescription()
    {
        if (preferenceNodes.Count == 0) return $"{groupName}: 无偏好节点";

        var nodeDescriptions = preferenceNodes.Select(n => n.GetDescription());
        return $"{groupName} (权重:{weight}): {string.Join(" + ", nodeDescriptions)}";
    }

    /// <summary>
    /// 获取所有目标的得分详情
    /// </summary>
    // public Dictionary<ITargetable, float> GetTargetScores(List<ITargetable> targets, Vector3 referencePosition)
    public Dictionary<IRoleEntity, float> GetTargetScores(Data param)
    {
        List<IRoleEntity> targets = param.GetField<List<IRoleEntity>>("targetList");
        return targets.ToDictionary(
            target => target,
            target => CalculateScore(param)
        );
    }
}