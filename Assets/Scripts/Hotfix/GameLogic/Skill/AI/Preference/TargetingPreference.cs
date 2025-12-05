// TargetingPreference.cs
using System.Collections.Generic;
using System.Linq;
using IData;
using UnityEngine;

/// <summary>
/// 攻击偏好配置 - 一个角色在各种情况下的攻击偏好集合
/// </summary>
[CreateAssetMenu(fileName = "NewTargetingPreference", menuName = "AI/Targeting Preference")]
public class TargetingPreference : ScriptableObject
{
    [Header("偏好组列表")]
    [SerializeField] private List<PreferenceGroup> preferenceGroups = new List<PreferenceGroup>();

    [Header("选择模式")]
    [SerializeField] private PreferenceSelectionMode selectionMode = PreferenceSelectionMode.WeightedRandom;

    [Header("默认偏好组")]
    [SerializeField] private string defaultGroupName = "默认偏好";

    [Header("调试信息")]
    [SerializeField, TextArea(3, 10)] private string preferenceDescription;

    public List<PreferenceGroup> PreferenceGroups => preferenceGroups;

    // 当前激活的偏好组（用于手动切换）
    private string activeGroupName;

    private void OnValidate()
    {
        UpdateDescription();

        // 确保至少有一个偏好组
        if (preferenceGroups.Count == 0)
        {
            AddPreferenceGroup(defaultGroupName, 1f);
        }
    }

    /// <summary>
    /// 选择目标
    /// </summary>
    // public ITargetable SelectTarget(List<ITargetable> availableTargets, Vector3 referencePosition)
    public GameObject SelectTarget(Data param)
    {
        List<GameObject> availableTargets = param.GetField<List<GameObject>>("targetList");
        if (preferenceGroups.Count == 0)
        {
            Debug.LogWarning("No preference groups configured!");
            return availableTargets.FirstOrDefault();
        }

        var aliveTargets = availableTargets.Where(t => t.activeSelf).ToList();
        if (aliveTargets.Count == 0) return null;
        if (aliveTargets.Count == 1) return aliveTargets[0];

        // 选择偏好组
        var selectedGroup = SelectPreferenceGroup();
        if (selectedGroup == null)
        {
            Debug.LogWarning("No valid preference group selected!");
            return aliveTargets.FirstOrDefault();
        }
        param.SetField("targetList", aliveTargets);
        return selectedGroup.SelectTarget(param);
    }

    public List<GameObject> SelectTargets(Data param)
    {
        List<GameObject> availableTargets = param.GetField<List<GameObject>>("targetList");
        if (preferenceGroups.Count == 0)
        {
            Debug.LogWarning("No preference groups configured!");
            return availableTargets;
        }

        var aliveTargets = availableTargets.Where(t => t.activeSelf).ToList();
        if (aliveTargets.Count == 0) return null;

        // 选择偏好组
        var selectedGroup = SelectPreferenceGroup();
        if (selectedGroup == null)
        {
            Debug.LogWarning("No valid preference group selected!");
            return aliveTargets;
        }
        param.SetField("targetList", aliveTargets);
        return selectedGroup.SelectTargets(param);
    }

    /// <summary>
    /// 选择偏好组
    /// </summary>
    private PreferenceGroup SelectPreferenceGroup()
    {
        var enabledGroups = preferenceGroups.Where(g => g.enabled).ToList();
        if (enabledGroups.Count == 0) return null;

        // 如果设置了激活组，优先使用
        if (!string.IsNullOrEmpty(activeGroupName))
        {
            var activeGroup = enabledGroups.Find(g => g.groupName == activeGroupName);
            if (activeGroup != null)
            {
                return activeGroup;
            }
        }

        if (enabledGroups.Count == 1) return enabledGroups[0];

        switch (selectionMode)
        {
            case PreferenceSelectionMode.WeightedRandom:
                return SelectByWeightedRandom(enabledGroups);

            case PreferenceSelectionMode.HighestWeight:
                return SelectByHighestWeight(enabledGroups);

            case PreferenceSelectionMode.RoundRobin:
                return SelectByRoundRobin(enabledGroups);

            default:
                return SelectByWeightedRandom(enabledGroups);
        }
    }

    /// <summary>
    /// 加权随机选择
    /// </summary>
    private PreferenceGroup SelectByWeightedRandom(List<PreferenceGroup> groups)
    {
        float totalWeight = groups.Sum(g => g.weight);
        float randomValue = Random.Range(0f, totalWeight);

        float currentWeight = 0f;
        foreach (var group in groups)
        {
            currentWeight += group.weight;
            if (randomValue <= currentWeight)
                return group;
        }

        return groups.Last();
    }

    /// <summary>
    /// 选择最高权重的组
    /// </summary>
    private PreferenceGroup SelectByHighestWeight(List<PreferenceGroup> groups)
    {
        return groups.OrderByDescending(g => g.weight).First();
    }

    /// <summary>
    /// 轮询选择
    /// </summary>
    private PreferenceGroup SelectByRoundRobin(List<PreferenceGroup> groups)
    {
        int index = Mathf.FloorToInt(Time.time) % groups.Count;
        return groups[index];
    }

    /// <summary>
    /// 切换到指定偏好组
    /// </summary>
    public void SwitchToGroup(string groupName)
    {
        if (preferenceGroups.Any(g => g.groupName == groupName))
        {
            activeGroupName = groupName;
            Debug.Log($"切换到偏好组: {groupName}");
        }
        else
        {
            Debug.LogWarning($"偏好组 '{groupName}' 不存在!");
        }
    }

    /// <summary>
    /// 切换回自动选择模式
    /// </summary>
    public void SwitchToAuto()
    {
        activeGroupName = null;
        Debug.Log("切换回自动选择模式");
    }

    /// <summary>
    /// 添加偏好组
    /// </summary>
    public PreferenceGroup AddPreferenceGroup(string name = "新偏好组", float weight = 1f)
    {
        var group = new PreferenceGroup
        {
            groupName = name,
            weight = weight
        };
        preferenceGroups.Add(group);
        UpdateDescription();
        return group;
    }

    /// <summary>
    /// 移除偏好组
    /// </summary>
    public void RemovePreferenceGroup(PreferenceGroup group)
    {
        preferenceGroups.Remove(group);
        UpdateDescription();
    }

    /// <summary>
    /// 获取偏好组
    /// </summary>
    public PreferenceGroup GetPreferenceGroup(string groupName)
    {
        return preferenceGroups.Find(g => g.groupName == groupName);
    }

    /// <summary>
    /// 设置偏好组权重
    /// </summary>
    public void SetGroupWeight(string groupName, float weight)
    {
        var group = preferenceGroups.Find(g => g.groupName == groupName);
        if (group != null)
        {
            group.weight = weight;
            UpdateDescription();
        }
    }

    /// <summary>
    /// 启用/禁用偏好组
    /// </summary>
    public void SetGroupEnabled(string groupName, bool enabled)
    {
        var group = preferenceGroups.Find(g => g.groupName == groupName);
        if (group != null)
        {
            group.enabled = enabled;
        }
    }

    /// <summary>
    /// 获取当前激活的偏好组名称
    /// </summary>
    public string GetActiveGroupName()
    {
        return activeGroupName ?? "自动选择";
    }

    /// <summary>
    /// 更新描述信息
    /// </summary>
    public string UpdateDescription()
    {
        if (preferenceGroups.Count == 0)
        {
            preferenceDescription = "未配置偏好组";
            return string.Empty;
        }

        preferenceDescription = $"选择模式: {selectionMode}\n";
        preferenceDescription += $"当前激活: {GetActiveGroupName()}\n\n";
        preferenceDescription += "配置的偏好组:\n";

        foreach (var group in preferenceGroups)
        {
            string status = group.enabled ? "启用" : "禁用";
            string activeIndicator = group.groupName == activeGroupName ? " [激活]" : "";
            preferenceDescription += $"• {group.GetDescription()} [{status}]{activeIndicator}\n";
        }

        // 显示权重统计
        if (selectionMode == PreferenceSelectionMode.WeightedRandom)
        {
            var enabledGroups = preferenceGroups.Where(g => g.enabled).ToList();
            float totalWeight = enabledGroups.Sum(g => g.weight);

            preferenceDescription += $"\n总权重: {totalWeight}";
            preferenceDescription += "\n\n加权随机概率:";
            foreach (var group in enabledGroups)
            {
                float probability = totalWeight > 0 ? (group.weight / totalWeight) * 100f : 0f;
                preferenceDescription += $"\n  {group.groupName}: {probability:F1}%";
            }
        }

        return preferenceDescription;
    }
}
