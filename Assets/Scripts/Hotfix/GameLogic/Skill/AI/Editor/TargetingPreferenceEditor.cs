// TargetingPreferenceEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(TargetingPreference))]
public class TargetingPreferenceEditor : Editor
{
    private TargetingPreference preference;
    private SerializedProperty preferenceGroupsProp;
    private SerializedProperty selectionModeProp;
    private SerializedProperty defaultGroupNameProp;

    // 折叠状态
    private bool showBasicSettings = true;
    private bool showGroups = true;
    private bool showDebugInfo = true;
    private Dictionary<string, bool> groupFoldouts = new Dictionary<string, bool>();
    private Dictionary<string, bool> nodeFoldouts = new Dictionary<string, bool>();

    // 样式
    private GUIStyle headerStyle;
    private GUIStyle groupHeaderStyle;
    private GUIStyle nodeStyle;

    private void OnEnable()
    {
        preference = (TargetingPreference)target;
        preferenceGroupsProp = serializedObject.FindProperty("preferenceGroups");
        selectionModeProp = serializedObject.FindProperty("selectionMode");
        defaultGroupNameProp = serializedObject.FindProperty("defaultGroupName");

        InitializeStyles();
        InitializeFoldoutStates();
    }

    private void InitializeStyles()
    {
        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            margin = new RectOffset(0, 0, 10, 10)
        };

        groupHeaderStyle = new GUIStyle(EditorStyles.foldoutHeader)
        {
            fontStyle = FontStyle.Bold
        };

        nodeStyle = new GUIStyle(EditorStyles.helpBox)
        {
            margin = new RectOffset(5, 5, 2, 2),
            padding = new RectOffset(8, 8, 6, 6)
        };
    }

    private void InitializeFoldoutStates()
    {
        for (int i = 0; i < preferenceGroupsProp.arraySize; i++)
        {
            var groupProp = preferenceGroupsProp.GetArrayElementAtIndex(i);
            string groupName = groupProp.FindPropertyRelative("groupName").stringValue;

            if (!groupFoldouts.ContainsKey(groupName))
            {
                groupFoldouts[groupName] = i == 0; // 默认展开第一个组
            }

            string nodeKey = $"{groupName}_nodes";
            if (!nodeFoldouts.ContainsKey(nodeKey))
            {
                nodeFoldouts[nodeKey] = false;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        DrawBasicSettings();
        EditorGUILayout.Space();
        DrawPreferenceGroups();
        EditorGUILayout.Space();
        DrawDebugInfo();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawBasicSettings()
    {
        showBasicSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showBasicSettings, "🎯 基础设置");
        if (showBasicSettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(selectionModeProp);
            EditorGUILayout.PropertyField(defaultGroupNameProp);

            EditorGUILayout.Space();

            // 手动控制区域
            EditorGUILayout.LabelField("手动控制", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("🔄 自动模式", EditorStyles.miniButton))
            {
                preference.SwitchToAuto();
                EditorUtility.SetDirty(preference);
                Repaint();
            }

            if (GUILayout.Button("📋 复制配置", EditorStyles.miniButton))
            {
                EditorGUIUtility.systemCopyBuffer = preference.UpdateDescription();
                Debug.Log("配置信息已复制到剪贴板");
            }

            if (GUILayout.Button("🔄 刷新描述", EditorStyles.miniButton))
            {
                preference.UpdateDescription();
                EditorUtility.SetDirty(preference);
                Repaint();
            }

            EditorGUILayout.EndHorizontal();

            // 显示当前激活组
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("当前激活:", EditorStyles.miniBoldLabel, GUILayout.Width(60));
            EditorGUILayout.LabelField(preference.GetActiveGroupName(), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawPreferenceGroups()
    {
        showGroups = EditorGUILayout.BeginFoldoutHeaderGroup(showGroups, "📊 偏好组配置");
        if (showGroups)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 统计信息
            int enabledGroups = preference.PreferenceGroups.Count(g => g.enabled);
            int totalNodes = preference.PreferenceGroups.Sum(g => g.preferenceNodes.Count);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"组: {enabledGroups}/{preference.PreferenceGroups.Count} 启用", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"节点: {totalNodes}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 添加新组按钮
            if (GUILayout.Button("➕ 添加新偏好组", GUILayout.Height(25)))
            {
                AddNewPreferenceGroup();
            }

            EditorGUILayout.Space();

            // 绘制所有偏好组
            for (int i = 0; i < preferenceGroupsProp.arraySize; i++)
            {
                DrawPreferenceGroup(i);
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawPreferenceGroup(int groupIndex)
    {
        var groupProp = preferenceGroupsProp.GetArrayElementAtIndex(groupIndex);
        var groupNameProp = groupProp.FindPropertyRelative("groupName");
        var weightProp = groupProp.FindPropertyRelative("weight");
        var enabledProp = groupProp.FindPropertyRelative("enabled");
        var nodesProp = groupProp.FindPropertyRelative("preferenceNodes");

        string groupName = groupNameProp.stringValue;
        string safeGroupName = groupName.Replace(" ", "_");

        // 确保折叠状态存在
        if (!groupFoldouts.ContainsKey(safeGroupName))
        {
            groupFoldouts[safeGroupName] = true;
        }

        EditorGUILayout.BeginVertical(nodeStyle);

        // 组标题行
        EditorGUILayout.BeginHorizontal();

        // 折叠箭头和组名
        groupFoldouts[safeGroupName] = EditorGUILayout.Foldout(groupFoldouts[safeGroupName],
            $"{groupName} (权重:{weightProp.floatValue})", true, groupHeaderStyle);

        // 状态指示器
        GUIStyle statusStyle = enabledProp.boolValue ?
            new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.green } } :
            new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.gray } };

        EditorGUILayout.LabelField(enabledProp.boolValue ? "● 启用" : "○ 禁用", statusStyle, GUILayout.Width(50));

        // 激活按钮
        if (GUILayout.Button("⚡", GUILayout.Width(25)))
        {
            preference.SwitchToGroup(groupName);
            EditorUtility.SetDirty(preference);
            Repaint();
        }

        // 删除按钮
        if (GUILayout.Button("×", GUILayout.Width(20)))
        {
            if (EditorUtility.DisplayDialog("删除偏好组",
                $"确定要删除偏好组 '{groupName}' 吗？这个操作无法撤销。", "删除", "取消"))
            {
                RemovePreferenceGroup(groupName, safeGroupName);
                return;
            }
        }

        EditorGUILayout.EndHorizontal();

        // 组内容
        if (groupFoldouts[safeGroupName])
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            // 组基础设置
            EditorGUILayout.PropertyField(groupNameProp, new GUIContent("组名称"));
            EditorGUILayout.PropertyField(weightProp, new GUIContent("权重"));
            EditorGUILayout.PropertyField(enabledProp, new GUIContent("启用"));

            EditorGUILayout.Space();

            // 节点区域
            DrawNodesSection(safeGroupName, groupName, nodesProp);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawNodesSection(string safeGroupName, string groupName, SerializedProperty nodesProp)
    {
        EditorGUILayout.LabelField("偏好节点", EditorStyles.miniBoldLabel);

        // 节点统计
        int enabledNodes = 0;
        for (int i = 0; i < nodesProp.arraySize; i++)
        {
            var nodeProp = nodesProp.GetArrayElementAtIndex(i);
            var enabledProp = nodeProp.FindPropertyRelative("enabled");
            if (enabledProp.boolValue) enabledNodes++;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"节点: {enabledNodes}/{nodesProp.arraySize} 启用", EditorStyles.miniLabel);

        // 添加节点按钮
        if (GUILayout.Button("➕ 添加节点", EditorStyles.miniButton))
        {
            ShowPreferenceTypeMenu(groupName);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 绘制所有节点
        for (int j = 0; j < nodesProp.arraySize; j++)
        {
            DrawPreferenceNode(safeGroupName, groupName, nodesProp, j);
        }

        if (nodesProp.arraySize == 0)
        {
            EditorGUILayout.HelpBox("此组还没有任何偏好节点。点击上面的按钮添加节点。", MessageType.Info);
        }
    }

    private void DrawPreferenceNode(string safeGroupName, string groupName, SerializedProperty nodesProp, int nodeIndex)
    {
        var nodeProp = nodesProp.GetArrayElementAtIndex(nodeIndex);
        var typeProp = nodeProp.FindPropertyRelative("preferenceType");
        var paramProp = nodeProp.FindPropertyRelative("parameter");
        var enabledProp = nodeProp.FindPropertyRelative("enabled");

        string nodeKey = $"{safeGroupName}_node_{nodeIndex}";

        if (!nodeFoldouts.ContainsKey(nodeKey))
        {
            nodeFoldouts[nodeKey] = false;
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // 节点标题行
        EditorGUILayout.BeginHorizontal();

        // 节点图标和名称
        string nodeIcon = GetPreferenceTypeIcon((TargetPreferenceType)typeProp.enumValueIndex);
        string nodeDescription = GetNodeDescription((TargetPreferenceType)typeProp.enumValueIndex, paramProp.floatValue);

        nodeFoldouts[nodeKey] = EditorGUILayout.Foldout(nodeFoldouts[nodeKey],
            $"{nodeIcon} {nodeDescription}", true);

        // 启用开关
        enabledProp.boolValue = EditorGUILayout.Toggle(enabledProp.boolValue, GUILayout.Width(20));

        // 删除按钮
        if (GUILayout.Button("×", GUILayout.Width(20)))
        {
            nodesProp.DeleteArrayElementAtIndex(nodeIndex);
            nodeFoldouts.Remove(nodeKey);
            EditorUtility.SetDirty(preference);
            return;
        }

        EditorGUILayout.EndHorizontal();

        // 节点内容
        if (nodeFoldouts[nodeKey])
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.PropertyField(typeProp, new GUIContent("偏好类型"));

            // 根据类型显示参数控件
            TargetPreferenceType prefType = (TargetPreferenceType)typeProp.enumValueIndex;
            switch (prefType)
            {
                case TargetPreferenceType.HealthBelowPercent:
                    EditorGUILayout.PropertyField(paramProp, new GUIContent("血量百分比阈值"));
                    paramProp.floatValue = Mathf.Clamp(paramProp.floatValue, 1f, 100f);
                    break;
            }

            // 显示详细描述
            EditorGUILayout.HelpBox(GetNodeDetailedDescription(prefType, paramProp.floatValue), MessageType.Info);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawDebugInfo()
    {
        showDebugInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showDebugInfo, "📝 配置预览");
        if (showDebugInfo)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            string description = preference.UpdateDescription();
            EditorGUILayout.TextArea(description, EditorStyles.wordWrappedLabel, GUILayout.MinHeight(80));

            EditorGUILayout.Space();

            if (GUILayout.Button("📋 复制配置信息"))
            {
                EditorGUIUtility.systemCopyBuffer = description;
                Debug.Log("配置信息已复制到剪贴板");
            }

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void AddNewPreferenceGroup()
    {
        var newGroup = preference.AddPreferenceGroup($"偏好组{preference.PreferenceGroups.Count + 1}");
        string safeGroupName = newGroup.groupName.Replace(" ", "_");
        groupFoldouts[safeGroupName] = true;
        EditorUtility.SetDirty(preference);
        Repaint();
    }

    private void RemovePreferenceGroup(string groupName, string safeGroupName)
    {
        var group = preference.GetPreferenceGroup(groupName);
        if (group != null)
        {
            preference.RemovePreferenceGroup(group);
            groupFoldouts.Remove(safeGroupName);

            // 清理相关的节点折叠状态
            var keysToRemove = nodeFoldouts.Keys.Where(k => k.StartsWith(safeGroupName)).ToList();
            foreach (var key in keysToRemove)
            {
                nodeFoldouts.Remove(key);
            }

            EditorUtility.SetDirty(preference);
            Repaint();
        }
    }

    private void ShowPreferenceTypeMenu(string groupName)
    {
        var menu = new GenericMenu();
        var types = System.Enum.GetValues(typeof(TargetPreferenceType));

        foreach (TargetPreferenceType type in types)
        {
            string icon = GetPreferenceTypeIcon(type);
            menu.AddItem(new GUIContent($"{icon} {type.ToString()}"), false, () =>
            {
                var group = preference.GetPreferenceGroup(groupName);
                if (group != null)
                {
                    float defaultParam = type switch
                    {
                        TargetPreferenceType.HealthBelowPercent => 50f,
                        _ => 0f
                    };

                    group.AddPreferenceNode(type, defaultParam);
                    EditorUtility.SetDirty(preference);
                    Repaint();
                }
            });
        }
        menu.ShowAsContext();
    }

    private string GetPreferenceTypeIcon(TargetPreferenceType type)
    {
        return type switch
        {
            TargetPreferenceType.Nearest => "📍",
            TargetPreferenceType.HealthBelowPercent => "💔",
            TargetPreferenceType.RandomTarget => "📏",
            _ => "❓"
        };
    }

    private string GetNodeDescription(TargetPreferenceType type, float parameter)
    {
        return type switch
        {
            TargetPreferenceType.Nearest => "最近的敌人",
            TargetPreferenceType.HealthBelowPercent => $"血量 < {parameter}%",
            TargetPreferenceType.RandomTarget => $"纯随机敌人%",
            _ => "未知偏好"
        };
    }

    private string GetNodeDetailedDescription(TargetPreferenceType type, float parameter)
    {
        return type switch
        {
            TargetPreferenceType.Nearest => "优先选择距离最近的目标，距离越近得分越高",
            TargetPreferenceType.HealthBelowPercent => $"优先选择血量低于 {parameter}% 的目标，血量越低得分越高",
            TargetPreferenceType.RandomTarget => $"纯随机敌人%",
            _ => "未知偏好类型"
        };
    }
}
#endif