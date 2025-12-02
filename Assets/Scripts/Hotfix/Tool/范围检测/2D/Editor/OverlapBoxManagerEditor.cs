using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
[CustomEditor(typeof(OverlapBoxManager))]
public class OverlapBoxManagerEditor : Editor
{
    private SerializedProperty detectionLayersProp;
    private SerializedProperty detectionTagsProp;
    private SerializedProperty managedOverlapBoxesProp;

    private bool showTagSettings = true;
    private bool showManagedBoxes = true;
    private bool showResults = true;

    private void OnEnable()
    {
        detectionLayersProp = serializedObject.FindProperty("detectionLayers");
        detectionTagsProp = serializedObject.FindProperty("detectionTags");
        managedOverlapBoxesProp = serializedObject.FindProperty("managedOverlapBoxes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var manager = (OverlapBoxManager)target;

        EditorGUILayout.LabelField("碰撞检测管理器", EditorStyles.boldLabel);

        // 检测设置
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("检测设置", EditorStyles.boldLabel);

        // 层级选择
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("检测层级", GUILayout.Width(120));
        detectionLayersProp.intValue = EditorGUILayout.MaskField(detectionLayersProp.intValue, GetLayerNames());
        EditorGUILayout.EndHorizontal();

        // 标签设置
        showTagSettings = EditorGUILayout.Foldout(showTagSettings, "标签过滤设置", true);
        if (showTagSettings)
        {
            DrawTagMaskSelector();
        }

        // 管理器设置
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("管理器设置", EditorStyles.boldLabel);

        showManagedBoxes = EditorGUILayout.Foldout(showManagedBoxes, $"管理的检测器 ({manager.GetManagedBoxCount()})", true);
        if (showManagedBoxes)
        {
            EditorGUILayout.PropertyField(managedOverlapBoxesProp, true);
        }

        // 操作按钮
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("操作", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("收集所有检测器"))
        {
            manager.CollectAllOverlapBoxes();
            EditorUtility.SetDirty(target);
            Repaint();
        }
        if (GUILayout.Button("一键检测"))
        {
            manager.DetectAllCollisions();
            EditorUtility.SetDirty(target);
            Repaint();
        }
        if (GUILayout.Button("清空结果"))
        {
            manager.ClearAllDetection();
            EditorUtility.SetDirty(target);
            Repaint();
        }
        EditorGUILayout.EndHorizontal();

        // 检测结果
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("检测结果", EditorStyles.boldLabel);

        EditorGUILayout.LabelField($"内环对象: {manager.GetInnerRingCount()}个");
        EditorGUILayout.LabelField($"外环对象: {manager.GetOuterRingCount()}个");
        EditorGUILayout.LabelField($"总计: {manager.GetAllUniqueCount()}个");
        EditorGUILayout.LabelField($"检测器数量: {manager.GetActiveBoxCount()}/{manager.GetManagedBoxCount()}个");

        // 显示对象列表
        showResults = EditorGUILayout.Foldout(showResults, "检测结果详情", true);
        if (showResults)
        {
            DrawObjectLists(manager);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTagMaskSelector()
    {
        var everythingProp = detectionTagsProp.FindPropertyRelative("everything");
        var nothingProp = detectionTagsProp.FindPropertyRelative("nothing");
        var includedTagsProp = detectionTagsProp.FindPropertyRelative("includedTags");

        EditorGUILayout.BeginVertical("box");

        if (!everythingProp.boolValue && !nothingProp.boolValue)
        {
            EditorGUILayout.LabelField($"已选择 {includedTagsProp.arraySize} 个标签", EditorStyles.miniLabel);
            if (GUILayout.Button("选择标签...", EditorStyles.miniButton))
                ShowTagSelectionMenu(includedTagsProp);

            if (includedTagsProp.arraySize > 0)
            {
                EditorGUILayout.LabelField("已选标签:", EditorStyles.miniBoldLabel);
                for (int i = 0; i < includedTagsProp.arraySize; i++)
                {
                    var tagProp = includedTagsProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(tagProp.stringValue, EditorStyles.miniLabel);
                    if (GUILayout.Button("×", EditorStyles.miniButton, GUILayout.Width(25)))
                    {
                        includedTagsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawObjectLists(OverlapBoxManager manager)
    {
        // 内环对象
        EditorGUILayout.LabelField($"内环对象 ({manager.GetInnerRingCount()}):", EditorStyles.miniBoldLabel);
        DrawObjectList(manager.GetAllInnerRingObjects(), Color.red);

        // 外环对象
        EditorGUILayout.LabelField($"外环对象 ({manager.GetOuterRingCount()}):", EditorStyles.miniBoldLabel);
        DrawObjectList(manager.GetAllOuterRingObjects(), Color.blue);
    }

    private void DrawObjectList(List<GameObject> objects, Color color)
    {
        EditorGUI.indentLevel++;
        if (objects.Count == 0)
        {
            EditorGUILayout.LabelField("无", EditorStyles.miniLabel);
        }
        else
        {
            for (int i = 0; i < objects.Count; i++)
            {
                var obj = objects[i];
                EditorGUILayout.BeginHorizontal();

                if (obj == null)
                {
                    EditorGUILayout.LabelField($"({i}) [已销毁的对象]");
                }
                else
                {
                    var originalColor = GUI.color;
                    GUI.color = color;
                    EditorGUILayout.ObjectField($"({i}) {obj.name} ({obj.tag})", obj, typeof(GameObject), true);
                    GUI.color = originalColor;

                    if (GUILayout.Button("Ping", GUILayout.Width(50)))
                        EditorGUIUtility.PingObject(obj);
                    if (GUILayout.Button("选择", GUILayout.Width(50)))
                        Selection.activeGameObject = obj;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUI.indentLevel--;
    }

    private void ShowTagSelectionMenu(SerializedProperty includedTagsProp)
    {
        var menu = new GenericMenu();
        var allTags = UnityEditorInternal.InternalEditorUtility.tags;

        menu.AddItem(new GUIContent("Select All"), false, () =>
        {
            includedTagsProp.ClearArray();
            foreach (string tag in allTags)
            {
                includedTagsProp.arraySize++;
                includedTagsProp.GetArrayElementAtIndex(includedTagsProp.arraySize - 1).stringValue = tag;
            }
            serializedObject.ApplyModifiedProperties();
        });

        menu.AddSeparator("");
        foreach (string tag in allTags)
        {
            bool isSelected = false;
            for (int i = 0; i < includedTagsProp.arraySize; i++)
            {
                if (includedTagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    isSelected = true;
                    break;
                }
            }

            menu.AddItem(new GUIContent(tag), isSelected, () =>
            {
                if (isSelected)
                {
                    for (int i = 0; i < includedTagsProp.arraySize; i++)
                    {
                        if (includedTagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                        {
                            includedTagsProp.DeleteArrayElementAtIndex(i);
                            break;
                        }
                    }
                }
                else
                {
                    includedTagsProp.arraySize++;
                    includedTagsProp.GetArrayElementAtIndex(includedTagsProp.arraySize - 1).stringValue = tag;
                }
                serializedObject.ApplyModifiedProperties();
            });
        }
        menu.ShowAsContext();
    }

    private string[] GetLayerNames()
    {
        var layers = new List<string>();
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(layerName)) layers.Add(layerName);
        }
        return layers.ToArray();
    }
}
#endif