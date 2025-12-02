using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(OmnipotentOverlapBox))]
public class OmnipotentOverlapBoxEditor : Editor
{
    private SerializedProperty shapeProp;
    private SerializedProperty detectionMethodProp;
    private SerializedProperty rectangleSettingsProp;
    private SerializedProperty sectorSettingsProp;
    private SerializedProperty detectOnAwakeProp;
    private SerializedProperty detectionIntervalProp;
    private SerializedProperty alwaysShowGizmosProp;

    private void OnEnable()
    {
        shapeProp = serializedObject.FindProperty("shape");
        detectionMethodProp = serializedObject.FindProperty("detectionMethod");
        rectangleSettingsProp = serializedObject.FindProperty("rectangleSettings");
        sectorSettingsProp = serializedObject.FindProperty("sectorSettings");
        detectOnAwakeProp = serializedObject.FindProperty("detectOnAwake");
        detectionIntervalProp = serializedObject.FindProperty("detectionInterval");
        alwaysShowGizmosProp = serializedObject.FindProperty("alwaysShowGizmos");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var detector = (OmnipotentOverlapBox)target;

        EditorGUILayout.LabelField("基础设置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(shapeProp);
        EditorGUILayout.PropertyField(detectionMethodProp, new GUIContent("检测方式"));

        // 显示检测方式说明
        var detectionMethod = (OmnipotentOverlapBox.DetectionMethod)detectionMethodProp.enumValueIndex;
        string helpText = detectionMethod == OmnipotentOverlapBox.DetectionMethod.CenterPoint
            ? "中心点检测：检测物体的中心点是否在范围内"
            : "边界检测：检测物体的边界是否与范围相交";
        EditorGUILayout.HelpBox(helpText, MessageType.Info);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("检测设置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(detectOnAwakeProp);
        EditorGUILayout.PropertyField(detectionIntervalProp);
        EditorGUILayout.PropertyField(alwaysShowGizmosProp);

        EditorGUILayout.Space();

        // 根据形状显示对应的设置
        switch (detector.shape)
        {
            case OmnipotentOverlapBox.DetectionShape2D.Rectangle:
                DrawRectangleSettings();
                break;
            case OmnipotentOverlapBox.DetectionShape2D.Sector:
                DrawSectorSettings();
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawRectangleSettings()
    {
        EditorGUILayout.LabelField("矩形设置", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("总体设置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(rectangleSettingsProp.FindPropertyRelative("totalSize"), new GUIContent("尺寸"));
        EditorGUILayout.PropertyField(rectangleSettingsProp.FindPropertyRelative("totalOffset"), new GUIContent("偏移"));
        EditorGUILayout.PropertyField(rectangleSettingsProp.FindPropertyRelative("totalRotation"), new GUIContent("旋转"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("外环设置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(rectangleSettingsProp.FindPropertyRelative("outerSizeOffset"), new GUIContent("尺寸偏移"));
        EditorGUILayout.PropertyField(rectangleSettingsProp.FindPropertyRelative("outerOffsetOffset"), new GUIContent("位置偏移"));
        EditorGUILayout.PropertyField(rectangleSettingsProp.FindPropertyRelative("outerRotation"), new GUIContent("旋转"));
        EditorGUILayout.PropertyField(rectangleSettingsProp.FindPropertyRelative("outerColor"), new GUIContent("边框颜色"));
        EditorGUILayout.PropertyField(rectangleSettingsProp.FindPropertyRelative("outerFillColor"), new GUIContent("填充颜色"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("内环设置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(rectangleSettingsProp.FindPropertyRelative("innerSizeOffset"), new GUIContent("尺寸偏移"));
        EditorGUILayout.PropertyField(rectangleSettingsProp.FindPropertyRelative("innerOffsetOffset"), new GUIContent("位置偏移"));
        EditorGUILayout.PropertyField(rectangleSettingsProp.FindPropertyRelative("innerRotation"), new GUIContent("旋转"));
        EditorGUILayout.PropertyField(rectangleSettingsProp.FindPropertyRelative("innerColor"), new GUIContent("边框颜色"));
        EditorGUILayout.PropertyField(rectangleSettingsProp.FindPropertyRelative("innerFillColor"), new GUIContent("填充颜色"));
    }

    private void DrawSectorSettings()
    {
        EditorGUILayout.LabelField("扇形设置", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("外环设置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(sectorSettingsProp.FindPropertyRelative("outerRadius"), new GUIContent("半径"));
        EditorGUILayout.PropertyField(sectorSettingsProp.FindPropertyRelative("outerOffset"), new GUIContent("偏移"));
        EditorGUILayout.PropertyField(sectorSettingsProp.FindPropertyRelative("outerRotation"), new GUIContent("旋转"));
        EditorGUILayout.PropertyField(sectorSettingsProp.FindPropertyRelative("outerAngle"), new GUIContent("角度"));
        EditorGUILayout.PropertyField(sectorSettingsProp.FindPropertyRelative("outerColor"), new GUIContent("边框颜色"));
        EditorGUILayout.PropertyField(sectorSettingsProp.FindPropertyRelative("outerFillColor"), new GUIContent("填充颜色"));
        EditorGUILayout.PropertyField(sectorSettingsProp.FindPropertyRelative("outerBoundaryLineColor"), new GUIContent("边界线颜色"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("内环设置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(sectorSettingsProp.FindPropertyRelative("innerRadius"), new GUIContent("半径"));
        EditorGUILayout.PropertyField(sectorSettingsProp.FindPropertyRelative("innerOffset"), new GUIContent("偏移"));
        EditorGUILayout.PropertyField(sectorSettingsProp.FindPropertyRelative("innerRotation"), new GUIContent("旋转"));
        EditorGUILayout.PropertyField(sectorSettingsProp.FindPropertyRelative("innerAngle"), new GUIContent("角度"));
        EditorGUILayout.PropertyField(sectorSettingsProp.FindPropertyRelative("innerColor"), new GUIContent("边框颜色"));
        EditorGUILayout.PropertyField(sectorSettingsProp.FindPropertyRelative("innerFillColor"), new GUIContent("填充颜色"));
        EditorGUILayout.PropertyField(sectorSettingsProp.FindPropertyRelative("innerBoundaryLineColor"), new GUIContent("边界线颜色"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("其他设置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(sectorSettingsProp.FindPropertyRelative("ringColor"), new GUIContent("环状区域颜色"));

        EditorGUILayout.HelpBox("扇形检测区域：\n" +
                              "• 角度控制扇形开合大小 (0-360°)\n" +
                              "• 半径控制扇形大小", MessageType.Info);
    }
}
#endif