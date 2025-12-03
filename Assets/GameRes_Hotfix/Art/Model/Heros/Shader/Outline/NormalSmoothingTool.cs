using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace CustomEffects
{
    public class NormalSmoothingTool : EditorWindow
    {
        private GameObject targetObject;
        private float smoothingAngle = 180f;
        private bool bakeToVertexColor = true;
        private bool showPreview = true;
        private Vector2 scrollPosition;

        [MenuItem("Tools/法线平滑工具")]
        public static void ShowWindow()
        {
            GetWindow<NormalSmoothingTool>("法线平滑工具");
        }

        private void OnGUI()
        {
            GUILayout.Label("法线平滑工具 (平滑组生成器)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("此工具会自动生成平滑组并将平滑法线烘焙到顶点颜色中，解决描边时的硬边问题。\n支持静态网格(MeshFilter)和蒙皮网格(SkinnedMeshRenderer)。", MessageType.Info);
            GUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 目标对象选择
            EditorGUILayout.LabelField("设置", EditorStyles.boldLabel);
            targetObject = (GameObject)EditorGUILayout.ObjectField("目标模型", targetObject, typeof(GameObject), true);

            if (targetObject != null)
            {
                MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
                SkinnedMeshRenderer skinnedMeshRenderer = targetObject.GetComponent<SkinnedMeshRenderer>();
                
                if (meshFilter == null && skinnedMeshRenderer == null)
                {
                    EditorGUILayout.HelpBox("选择的对象没有有效的MeshFilter或SkinnedMeshRenderer组件", MessageType.Warning);
                    EditorGUILayout.EndScrollView();
                    return;
                }
                
                if (meshFilter != null && meshFilter.sharedMesh == null)
                {
                    EditorGUILayout.HelpBox("MeshFilter没有有效的Mesh", MessageType.Warning);
                    EditorGUILayout.EndScrollView();
                    return;
                }
                
                if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh == null)
                {
                    EditorGUILayout.HelpBox("SkinnedMeshRenderer没有有效的Mesh", MessageType.Warning);
                    EditorGUILayout.EndScrollView();
                    return;
                }
            }

            GUILayout.Space(5);

            // 平滑参数
            smoothingAngle = EditorGUILayout.Slider("平滑角度 (平滑组阈值)", smoothingAngle, 0f, 180f);
            EditorGUILayout.HelpBox($"当两个面的法线夹角小于 {smoothingAngle:F1}° 时，它们会被归入同一平滑组", MessageType.None);
            bakeToVertexColor = EditorGUILayout.Toggle("烘焙到顶点颜色", bakeToVertexColor);
            showPreview = EditorGUILayout.Toggle("显示预览", showPreview);

            GUILayout.Space(10);

            // 操作按钮
            EditorGUILayout.LabelField("操作", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(targetObject == null);
            
            if (GUILayout.Button("计算平滑法线", GUILayout.Height(30)))
            {
                CalculateSmoothedNormals();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("恢复原始法线", GUILayout.Height(25)))
            {
                RestoreOriginalNormals();
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);

            // 信息显示
            if (targetObject != null)
            {
                EditorGUILayout.LabelField("模型信息", EditorStyles.boldLabel);
                MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
                SkinnedMeshRenderer skinnedMeshRenderer = targetObject.GetComponent<SkinnedMeshRenderer>();
                
                Mesh mesh = null;
                string meshType = "";
                
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    mesh = meshFilter.sharedMesh;
                    meshType = "静态网格 (MeshFilter)";
                }
                else if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
                {
                    mesh = skinnedMeshRenderer.sharedMesh;
                    meshType = "蒙皮网格 (SkinnedMeshRenderer)";
                }
                
                if (mesh != null)
                {
                    EditorGUILayout.LabelField($"类型: {meshType}");
                    EditorGUILayout.LabelField($"顶点数: {mesh.vertexCount}");
                    EditorGUILayout.LabelField($"三角形数: {mesh.triangles.Length / 3}");
                    EditorGUILayout.LabelField($"是否有顶点颜色: {(mesh.colors.Length > 0 ? "是" : "否")}");
                    
                    if (skinnedMeshRenderer != null)
                    {
                        EditorGUILayout.LabelField($"骨骼数: {skinnedMeshRenderer.bones.Length}");
                        EditorGUILayout.LabelField($"混合形状数: {mesh.blendShapeCount}");
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void CalculateSmoothedNormals()
        {
            if (targetObject == null) return;

            MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
            SkinnedMeshRenderer skinnedMeshRenderer = targetObject.GetComponent<SkinnedMeshRenderer>();
            
            Mesh originalMesh = null;
            bool isSkinnedMesh = false;
            
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                originalMesh = meshFilter.sharedMesh;
                isSkinnedMesh = false;
            }
            else if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
            {
                originalMesh = skinnedMeshRenderer.sharedMesh;
                isSkinnedMesh = true;
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "无法找到有效的Mesh", "确定");
                return;
            }

            Mesh newMesh = Instantiate(originalMesh);

            // 计算平滑法线
            Vector3[] smoothedNormals = CalculateSmoothedNormalsForMesh(newMesh, smoothingAngle);

            if (bakeToVertexColor)
            {
                // 将平滑法线烘焙到顶点颜色的RGB通道中
                // 法线范围从[-1,1]转换到[0,1]
                Color[] colors = new Color[smoothedNormals.Length];
                for (int i = 0; i < smoothedNormals.Length; i++)
                {
                    Vector3 normal = smoothedNormals[i];
                    colors[i] = new Color(
                        normal.x * 0.5f + 0.5f,
                        normal.y * 0.5f + 0.5f,
                        normal.z * 0.5f + 0.5f,
                        1.0f
                    );
                }
                newMesh.colors = colors;
            }
            else
            {
                // 直接替换法线
                newMesh.normals = smoothedNormals;
            }

            // 保存处理后的mesh
            string path = EditorUtility.SaveFilePanelInProject(
                "保存平滑法线Mesh",
                targetObject.name + "_SmoothedNormals",
                "asset",
                "选择保存位置"
            );

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newMesh, path);
                AssetDatabase.SaveAssets();
                
                // 应用到对象
                if (isSkinnedMesh)
                {
                    skinnedMeshRenderer.sharedMesh = newMesh;
                }
                else
                {
                    meshFilter.sharedMesh = newMesh;
                }
                
                EditorUtility.DisplayDialog("完成", 
                    bakeToVertexColor ? 
                    "平滑法线已烘焙到顶点颜色中！请确保使用支持顶点颜色法线的shader。" : 
                    "法线平滑完成！", 
                    "确定");
            }
        }

        private Vector3[] CalculateSmoothedNormalsForMesh(Mesh mesh, float smoothingAngle)
        {
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;
            
            // 创建顶点到三角形的映射
            Dictionary<int, List<int>> vertexToTriangles = new Dictionary<int, List<int>>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int triIndex = i / 3;
                for (int j = 0; j < 3; j++)
                {
                    int vertIndex = triangles[i + j];
                    if (!vertexToTriangles.ContainsKey(vertIndex))
                        vertexToTriangles[vertIndex] = new List<int>();
                    vertexToTriangles[vertIndex].Add(triIndex);
                }
            }

            Vector3[] smoothedNormals = new Vector3[vertices.Length];
            float cosThreshold = Mathf.Cos(smoothingAngle * Mathf.Deg2Rad);

            for (int vertIndex = 0; vertIndex < vertices.Length; vertIndex++)
            {
                Vector3 smoothedNormal = Vector3.zero;
                Vector3 currentVertexPos = vertices[vertIndex];
                
                if (vertexToTriangles.ContainsKey(vertIndex))
                {
                    // 找到所有共享相同位置或在平滑角度范围内的顶点
                    HashSet<int> smoothingGroup = new HashSet<int>();
                    FindSmoothingGroup(vertIndex, vertices, normals, vertexToTriangles, 
                                     cosThreshold, smoothingGroup);

                    // 计算平均法线
                    foreach (int smoothVertIndex in smoothingGroup)
                    {
                        smoothedNormal += normals[smoothVertIndex];
                    }
                    
                    smoothedNormal = smoothedNormal.normalized;
                }
                else
                {
                    smoothedNormal = normals[vertIndex];
                }

                smoothedNormals[vertIndex] = smoothedNormal;
            }

            return smoothedNormals;
        }

        private void FindSmoothingGroup(int startVertIndex, Vector3[] vertices, Vector3[] normals,
                                      Dictionary<int, List<int>> vertexToTriangles, float cosThreshold,
                                      HashSet<int> smoothingGroup)
        {
            if (smoothingGroup.Contains(startVertIndex)) return;
            
            smoothingGroup.Add(startVertIndex);
            Vector3 startPos = vertices[startVertIndex];
            Vector3 startNormal = normals[startVertIndex];

            // 检查所有其他顶点
            for (int i = 0; i < vertices.Length; i++)
            {
                if (i == startVertIndex || smoothingGroup.Contains(i)) continue;

                // 检查位置是否相同（合并重复顶点）
                if (Vector3.Distance(vertices[i], startPos) < 0.0001f)
                {
                    // 检查法线角度
                    float dot = Vector3.Dot(startNormal, normals[i]);
                    if (dot >= cosThreshold)
                    {
                        FindSmoothingGroup(i, vertices, normals, vertexToTriangles, 
                                         cosThreshold, smoothingGroup);
                    }
                }
            }
        }

        private void RestoreOriginalNormals()
        {
            if (targetObject == null) return;

            MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
            SkinnedMeshRenderer skinnedMeshRenderer = targetObject.GetComponent<SkinnedMeshRenderer>();
            
            if (meshFilter == null && skinnedMeshRenderer == null) return;

            // 这里需要用户手动选择原始mesh，或者我们可以尝试从项目中找到
            string[] guids = AssetDatabase.FindAssets(targetObject.name.Replace("_SmoothedNormals", "") + " t:Mesh");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Mesh originalMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (originalMesh != null)
                {
                    if (skinnedMeshRenderer != null)
                    {
                        skinnedMeshRenderer.sharedMesh = originalMesh;
                    }
                    else if (meshFilter != null)
                    {
                        meshFilter.sharedMesh = originalMesh;
                    }
                    
                    EditorUtility.DisplayDialog("完成", "已恢复原始法线", "确定");
                    return;
                }
            }

            EditorUtility.DisplayDialog("错误", "无法找到原始mesh文件", "确定");
        }

        private void OnInspectorUpdate()
        {
            if (showPreview)
            {
                Repaint();
            }
        }
    }
} 