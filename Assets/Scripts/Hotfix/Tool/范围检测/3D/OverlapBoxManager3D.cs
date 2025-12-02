using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3D统一碰撞检测管理器 - 管理多个OmnipotentOverlapBox3D
/// </summary>
public class OverlapBoxManager3D : MonoBehaviour
{
    [Header("检测设置")]
    public LayerMask detectionLayers = -1;
    public TagMask3D detectionTags = new TagMask3D();

    [Header("管理器设置")]
    public List<OmnipotentOverlapBox3D> managedOverlapBoxes = new List<OmnipotentOverlapBox3D>();

    [Header("检测结果")]
    [SerializeField] private List<GameObject> allInnerRingObjects = new List<GameObject>();
    [SerializeField] private List<GameObject> allOuterRingObjects = new List<GameObject>();
    [SerializeField] private List<GameObject> allUniqueObjects = new List<GameObject>();

    /// <summary>
    /// 收集当前物体下所有的OmnipotentOverlapBox3D
    /// </summary>
    public void CollectAllOverlapBoxes()
    {
        managedOverlapBoxes.Clear();

        // 获取当前物体和所有子物体中的OmnipotentOverlapBox3D组件
        var allBoxes = GetComponentsInChildren<OmnipotentOverlapBox3D>(true);
        managedOverlapBoxes.AddRange(allBoxes);

        Debug.Log($"已收集 {managedOverlapBoxes.Count} 个3D碰撞检测器");
    }

    /// <summary>
    /// 一键触发所有检测器的碰撞检测（使用管理器的设置）
    /// </summary>
    public void DetectAllCollisions()
    {
        // 清空之前的结果
        allInnerRingObjects.Clear();
        allOuterRingObjects.Clear();
        allUniqueObjects.Clear();

        int activeBoxCount = 0;

        // 遍历所有管理的OverlapBox，触发检测并收集结果
        foreach (var box in managedOverlapBoxes)
        {
            if (box != null && box.isActiveAndEnabled)
            {
                activeBoxCount++;

                // 使用管理器的设置执行检测
                DetectWithBox(box);
            }
        }

        // 去除重复对象
        RemoveDuplicates();

        Debug.Log($"3D检测完成: {activeBoxCount}个检测器, {allInnerRingObjects.Count}个内环对象, {allOuterRingObjects.Count}个外环对象");
    }

    /// <summary>
    /// 使用管理器的设置对单个Box进行检测
    /// </summary>
    private void DetectWithBox(OmnipotentOverlapBox3D box)
    {
        // 保存检测器原有设置
        var originalLayers = box.detectionLayers;
        var originalTags = new TagMask3D();
        CopyTagMask(box.detectionTags, originalTags);

        try
        {
            // 临时使用管理器的设置
            box.detectionLayers = detectionLayers;
            CopyTagMask(detectionTags, box.detectionTags);

            // 执行检测
            box.DetectCollisions();

            // 收集结果
            allInnerRingObjects.AddRange(box.GetInnerRingObjects());
            allOuterRingObjects.AddRange(box.GetOuterRingObjects());
        }
        finally
        {
            // 恢复检测器原有设置
            box.detectionLayers = originalLayers;
            CopyTagMask(originalTags, box.detectionTags);
        }
    }

    /// <summary>
    /// 复制标签掩码设置
    /// </summary>
    private void CopyTagMask(TagMask3D source, TagMask3D destination)
    {
        destination.everything = source.everything;
        destination.nothing = source.nothing;
        destination.includedTags.Clear();
        destination.includedTags.AddRange(source.includedTags);
    }

    /// <summary>
    /// 去除重复的检测对象
    /// </summary>
    private void RemoveDuplicates()
    {
        // 使用HashSet去除重复项
        var uniqueInner = new HashSet<GameObject>();
        var uniqueOuter = new HashSet<GameObject>();

        // 处理内环对象
        foreach (var obj in allInnerRingObjects)
        {
            if (obj != null && !uniqueInner.Contains(obj))
            {
                uniqueInner.Add(obj);
            }
        }

        // 处理外环对象（排除已经在内环中的对象）
        foreach (var obj in allOuterRingObjects)
        {
            if (obj != null && !uniqueInner.Contains(obj) && !uniqueOuter.Contains(obj))
            {
                uniqueOuter.Add(obj);
            }
        }

        // 更新去重后的列表
        allInnerRingObjects = new List<GameObject>(uniqueInner);
        allOuterRingObjects = new List<GameObject>(uniqueOuter);

        // 创建所有唯一对象的列表
        allUniqueObjects.Clear();
        allUniqueObjects.AddRange(allInnerRingObjects);
        allUniqueObjects.AddRange(allOuterRingObjects);
    }

    /// <summary>
    /// 获取所有内环对象（已去重）
    /// </summary>
    public List<GameObject> GetAllInnerRingObjects() => new List<GameObject>(allInnerRingObjects);

    /// <summary>
    /// 获取所有外环对象（已去重）
    /// </summary>
    public List<GameObject> GetAllOuterRingObjects() => new List<GameObject>(allOuterRingObjects);

    /// <summary>
    /// 获取所有检测到的唯一对象（已去重）
    /// </summary>
    public List<GameObject> GetAllUniqueObjects() => new List<GameObject>(allUniqueObjects);

    /// <summary>
    /// 获取内环对象数量
    /// </summary>
    public int GetInnerRingCount() => allInnerRingObjects.Count;

    /// <summary>
    /// 获取外环对象数量
    /// </summary>
    public int GetOuterRingCount() => allOuterRingObjects.Count;

    /// <summary>
    /// 获取所有唯一对象数量
    /// </summary>
    public int GetAllUniqueCount() => allUniqueObjects.Count;

    /// <summary>
    /// 获取管理的OverlapBox数量
    /// </summary>
    public int GetManagedBoxCount() => managedOverlapBoxes.Count;

    /// <summary>
    /// 获取活跃的OverlapBox数量
    /// </summary>
    public int GetActiveBoxCount()
    {
        int count = 0;
        foreach (var box in managedOverlapBoxes)
        {
            if (box != null && box.isActiveAndEnabled)
                count++;
        }
        return count;
    }

    /// <summary>
    /// 清空所有检测结果
    /// </summary>
    public void ClearAllDetection()
    {
        allInnerRingObjects.Clear();
        allOuterRingObjects.Clear();
        allUniqueObjects.Clear();
        Debug.Log("已清空所有3D检测结果");
    }

    /// <summary>
    /// 检查指定对象是否在检测结果中
    /// </summary>
    public bool ContainsObject(GameObject obj)
    {
        return allUniqueObjects.Contains(obj);
    }

    /// <summary>
    /// 检查指定对象是否在内环中
    /// </summary>
    public bool IsObjectInInnerRing(GameObject obj)
    {
        return allInnerRingObjects.Contains(obj);
    }

    /// <summary>
    /// 检查指定对象是否在外环中
    /// </summary>
    public bool IsObjectInOuterRing(GameObject obj)
    {
        return allOuterRingObjects.Contains(obj);
    }

    /// <summary>
    /// 获取检测器统计信息
    /// </summary>
    public string GetDetectorStats()
    {
        int cubeCount = 0;
        int cylinderCount = 0;
        int sphereCount = 0;

        foreach (var box in managedOverlapBoxes)
        {
            if (box != null)
            {
                switch (box.shape)
                {
                    case OmnipotentOverlapBox3D.DetectionShape3D.Cube:
                        cubeCount++;
                        break;
                    case OmnipotentOverlapBox3D.DetectionShape3D.Cylinder:
                        cylinderCount++;
                        break;
                    case OmnipotentOverlapBox3D.DetectionShape3D.Sector3D:
                        sphereCount++;
                        break;
                }
            }
        }

        return $"立方体: {cubeCount}, 圆柱体: {cylinderCount}, 球体: {sphereCount}";
    }
}