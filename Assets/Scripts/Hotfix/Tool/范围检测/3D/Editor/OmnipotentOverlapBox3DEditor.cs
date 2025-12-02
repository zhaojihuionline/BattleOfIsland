using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(OmnipotentOverlapBox3D))]
public class OmnipotentOverlapBox3DEditor : Editor
{
    private SerializedProperty shapeProp;
    private SerializedProperty detectionMethodProp; // 新增：检测方式
    private SerializedProperty cubeSettingsProp;
    private SerializedProperty cylinderSettingsProp;
    private SerializedProperty sector3DSettingsProp;
    private SerializedProperty detectOnAwakeProp;
    private SerializedProperty detectionIntervalProp;
    private SerializedProperty alwaysShowGizmosProp;

    private void OnEnable()
    {
        shapeProp = serializedObject.FindProperty("shape");
        detectionMethodProp = serializedObject.FindProperty("detectionMethod"); // 新增
        cubeSettingsProp = serializedObject.FindProperty("cubeSettings");
        cylinderSettingsProp = serializedObject.FindProperty("cylinderSettings");
        sector3DSettingsProp = serializedObject.FindProperty("sector3DSettings");
        detectOnAwakeProp = serializedObject.FindProperty("detectOnAwake");
        detectionIntervalProp = serializedObject.FindProperty("detectionInterval");
        alwaysShowGizmosProp = serializedObject.FindProperty("alwaysShowGizmos");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var detector = (OmnipotentOverlapBox3D)target;

        EditorGUILayout.LabelField("基础设置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(shapeProp);
        EditorGUILayout.PropertyField(detectionMethodProp, new GUIContent("检测方式"));

        // 显示检测方式说明
        var detectionMethod = (OmnipotentOverlapBox3D.DetectionMethod)detectionMethodProp.enumValueIndex;
        string helpText = detectionMethod == OmnipotentOverlapBox3D.DetectionMethod.CenterPoint
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
            case OmnipotentOverlapBox3D.DetectionShape3D.Cube:
                DrawCubeSettings();
                break;
            case OmnipotentOverlapBox3D.DetectionShape3D.Cylinder:
                DrawCylinderSettings();
                break;
            case OmnipotentOverlapBox3D.DetectionShape3D.Sector3D:
                DrawSector3DSettings();
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawCubeSettings()
    {
        EditorGUILayout.LabelField("立方体设置", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("总体设置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(cubeSettingsProp.FindPropertyRelative("totalSize"), new GUIContent("尺寸"));
        EditorGUILayout.PropertyField(cubeSettingsProp.FindPropertyRelative("totalOffset"), new GUIContent("偏移"));
        EditorGUILayout.PropertyField(cubeSettingsProp.FindPropertyRelative("totalRotation"), new GUIContent("旋转"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("外环设置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(cubeSettingsProp.FindPropertyRelative("outerSizeOffset"), new GUIContent("尺寸偏移"));
        EditorGUILayout.PropertyField(cubeSettingsProp.FindPropertyRelative("outerOffsetOffset"), new GUIContent("位置偏移"));
        EditorGUILayout.PropertyField(cubeSettingsProp.FindPropertyRelative("outerRotation"), new GUIContent("旋转"));
        EditorGUILayout.PropertyField(cubeSettingsProp.FindPropertyRelative("outerColor"), new GUIContent("边框颜色"));
        EditorGUILayout.PropertyField(cubeSettingsProp.FindPropertyRelative("outerFillColor"), new GUIContent("填充颜色"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("内环设置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(cubeSettingsProp.FindPropertyRelative("innerSizeOffset"), new GUIContent("尺寸偏移"));
        EditorGUILayout.PropertyField(cubeSettingsProp.FindPropertyRelative("innerOffsetOffset"), new GUIContent("位置偏移"));
        EditorGUILayout.PropertyField(cubeSettingsProp.FindPropertyRelative("innerRotation"), new GUIContent("旋转"));
        EditorGUILayout.PropertyField(cubeSettingsProp.FindPropertyRelative("innerColor"), new GUIContent("边框颜色"));
        EditorGUILayout.PropertyField(cubeSettingsProp.FindPropertyRelative("innerFillColor"), new GUIContent("填充颜色"));
    }

    private void DrawCylinderSettings()
    {
        EditorGUILayout.LabelField("圆柱体设置", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("外环设置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("outerRadius"), new GUIContent("半径"));
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("outerHeight"), new GUIContent("高度"));
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("outerOffset"), new GUIContent("偏移"));
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("outerRotation"), new GUIContent("旋转"));
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("outerHorizontalAngle"), new GUIContent("水平角度"));
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("outerColor"), new GUIContent("边框颜色"));
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("outerFillColor"), new GUIContent("填充颜色"));
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("outerBoundaryLineColor"), new GUIContent("边界线颜色"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("内环设置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("innerRadius"), new GUIContent("半径"));
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("innerHeight"), new GUIContent("高度"));
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("innerOffset"), new GUIContent("偏移"));
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("innerRotation"), new GUIContent("旋转"));
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("innerHorizontalAngle"), new GUIContent("水平角度"));
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("innerColor"), new GUIContent("边框颜色"));
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("innerFillColor"), new GUIContent("填充颜色"));
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("innerBoundaryLineColor"), new GUIContent("边界线颜色"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("其他设置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(cylinderSettingsProp.FindPropertyRelative("useHorizontalAngle"), new GUIContent("使用水平角度"));

        var detector = (OmnipotentOverlapBox3D)target;
        if (detector.cylinderSettings.useHorizontalAngle)
        {
            EditorGUILayout.HelpBox("圆柱体扇形模式已启用", MessageType.Info);
        }
    }

    private void DrawSector3DSettings()
    {
        EditorGUILayout.LabelField("3D扇形设置", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("外环设置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("outerRadius"), new GUIContent("半径"));
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("outerOffset"), new GUIContent("偏移"));
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("outerRotation"), new GUIContent("旋转"));
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("outerHorizontalAngle"), new GUIContent("水平角度"));
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("outerVerticalAngle"), new GUIContent("垂直角度"));
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("outerColor"), new GUIContent("边框颜色"));
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("outerFillColor"), new GUIContent("填充颜色"));
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("outerBoundaryLineColor"), new GUIContent("边界线颜色"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("内环设置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("innerRadius"), new GUIContent("半径"));
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("innerOffset"), new GUIContent("偏移"));
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("innerRotation"), new GUIContent("旋转"));
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("innerHorizontalAngle"), new GUIContent("水平角度"));
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("innerVerticalAngle"), new GUIContent("垂直角度"));
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("innerColor"), new GUIContent("边框颜色"));
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("innerFillColor"), new GUIContent("填充颜色"));
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("innerBoundaryLineColor"), new GUIContent("边界线颜色"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("其他设置", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(sector3DSettingsProp.FindPropertyRelative("ringColor"), new GUIContent("环状区域颜色"));

        EditorGUILayout.HelpBox("3D扇形检测区域：\n" +
                              "• 水平角度控制左右开合 (0-180°)\n" +
                              "• 垂直角度控制从Z轴向上绕X轴旋转 (0-360°)", MessageType.Info);
    }
}
#endif