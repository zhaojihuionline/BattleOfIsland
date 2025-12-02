using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2D多功能碰撞检测 - Physics2D.OverlapBox版本
/// </summary>
public class OmnipotentOverlapBox : MonoBehaviour
{
    [Header("基础设置")]
    public DetectionShape2D shape = DetectionShape2D.Rectangle;
    public DetectionMethod detectionMethod = DetectionMethod.CenterPoint; // 新增：检测方式

    [Header("检测设置")]
    public LayerMask detectionLayers = -1;
    public TagMask2D detectionTags = new TagMask2D();
    public bool detectOnAwake = false;
    public float detectionInterval = 0f;
    public bool alwaysShowGizmos = true;

    [Header("矩形设置")]
    public RectangleSettings rectangleSettings = new RectangleSettings();

    [Header("扇形设置")]
    public SectorSettings sectorSettings = new SectorSettings();

    [Header("检测结果")]
    [SerializeField] private List<GameObject> innerRingObjects = new List<GameObject>();
    [SerializeField] private List<GameObject> outerRingObjects = new List<GameObject>();

    private float lastDetectionTime = 0f;

    public enum DetectionShape2D { Rectangle, Sector }
    public enum DetectionMethod { CenterPoint, Bounds } // 新增：检测方式枚举

    [System.Serializable]
    public class RectangleSettings
    {
        [Header("总体设置")]
        public Vector2 totalSize = new Vector2(5, 5);
        public Vector2 totalOffset = Vector2.zero;
        public float totalRotation = 0f;

        [Header("外环设置")]
        public Vector2 outerSizeOffset = Vector2.zero;
        public Vector2 outerOffsetOffset = Vector2.zero;
        public float outerRotation = 0f;
        public Color outerColor = new Color(0, 1, 0, 0.3f);
        public Color outerFillColor = new Color(0, 1, 0, 0.1f);

        [Header("内环设置")]
        public Vector2 innerSizeOffset = new Vector2(-3, -3);
        public Vector2 innerOffsetOffset = Vector2.zero;
        public float innerRotation = 0f;
        public Color innerColor = new Color(1, 0, 0, 0.3f);
        public Color innerFillColor = new Color(1, 0, 0, 0.1f);
    }

    [System.Serializable]
    public class SectorSettings
    {
        [Header("外环设置")]
        public float outerRadius = 5f;
        public Vector2 outerOffset = Vector2.zero;
        public float outerRotation = 0f;
        [Range(0, 360)] public float outerAngle = 90f;
        public Color outerColor = new Color(0, 1, 0, 0.3f);
        public Color outerFillColor = new Color(0, 1, 0, 0.1f);
        public Color outerBoundaryLineColor = new Color(1, 1, 1, 0.8f);

        [Header("内环设置")]
        public float innerRadius = 2f;
        public Vector2 innerOffset = Vector2.zero;
        public float innerRotation = 0f;
        [Range(0, 360)] public float innerAngle = 90f;
        public Color innerColor = new Color(1, 0, 0, 0.3f);
        public Color innerFillColor = new Color(1, 0, 0, 0.1f);
        public Color innerBoundaryLineColor = new Color(1, 1, 1, 0.8f);

        [Header("其他")]
        public Color ringColor = new Color(0, 0, 1, 0.3f);
    }

    private void Awake()
    {
        if (detectOnAwake) DetectCollisions();
    }

    private void Update()
    {
        if (detectionInterval > 0 && Time.time - lastDetectionTime >= detectionInterval)
        {
            DetectCollisions();
            lastDetectionTime = Time.time;
        }
    }

    /// <summary>
    /// 手动触发碰撞检测
    /// </summary>
    public void DetectCollisions()
    {
        innerRingObjects.Clear();
        outerRingObjects.Clear();

        Vector2 worldPos = (Vector2)transform.position + GetCurrentOffset();
        float worldRot = transform.rotation.eulerAngles.z + GetCurrentRotation();

        // 使用优化的候选集获取方法
        Collider2D[] allColliders = GetOptimalCandidates(worldPos, worldRot);

        foreach (var collider in allColliders)
        {
            if (collider.gameObject == gameObject) continue;
            if (!detectionTags.IsTagValid(collider.tag)) continue;

            // 根据检测方式选择检测点
            Vector2 detectionPoint = GetDetectionPoint(collider);

            bool isInInner = IsPointInInnerBounds(detectionPoint, worldPos, worldRot);
            bool isInOuter = IsPointInOuterBounds(detectionPoint, worldPos, worldRot);

            if (isInInner)
                innerRingObjects.Add(collider.gameObject);
            else if (isInOuter)
                outerRingObjects.Add(collider.gameObject);
        }
    }

    /// <summary>
    /// 根据形状选择最优的候选集获取方式
    /// </summary>
    private Collider2D[] GetOptimalCandidates(Vector2 worldPos, float rotation)
    {
        switch (shape)
        {
            case DetectionShape2D.Rectangle:
                // 矩形：使用OverlapBox
                Vector2 detectionSize = GetCurrentOuterSize();
                return Physics2D.OverlapBoxAll(worldPos, detectionSize, rotation, detectionLayers);

            case DetectionShape2D.Sector:
                // 扇形：使用OverlapCircle（最高效）
                float radius = sectorSettings.outerRadius;
                return Physics2D.OverlapCircleAll(worldPos, radius, detectionLayers);

            default:
                Vector2 size = GetCurrentOuterSize();
                return Physics2D.OverlapBoxAll(worldPos, size, rotation, detectionLayers);
        }
    }

    /// <summary>
    /// 根据检测方式获取检测点
    /// </summary>
    private Vector2 GetDetectionPoint(Collider2D collider)
    {
        switch (detectionMethod)
        {
            case DetectionMethod.CenterPoint:
                // 中心点检测：使用碰撞体的中心点
                return collider.bounds.center;

            case DetectionMethod.Bounds:
                // 边界检测：找到碰撞体边界上离检测区域最近的点
                return GetClosestPointOnBounds(collider);

            default:
                return collider.bounds.center;
        }
    }

    /// <summary>
    /// 获取碰撞体边界上离检测区域最近的点
    /// </summary>
    private Vector2 GetClosestPointOnBounds(Collider2D collider)
    {
        Vector2 detectionCenter = (Vector2)transform.position + GetCurrentOffset();

        switch (shape)
        {
            case DetectionShape2D.Rectangle:
                // 对于矩形，使用矩形的最近边界点
                return collider.ClosestPoint(detectionCenter);

            case DetectionShape2D.Sector:
                // 对于扇形，计算从检测中心到碰撞体边界的方向向量
                Vector2 directionToCollider = (Vector2)collider.bounds.center - detectionCenter;
                if (directionToCollider.magnitude < 0.001f)
                {
                    // 如果中心重合，使用随机方向
                    directionToCollider = Vector2.up;
                }

                // 沿着方向找到边界上的点
                RaycastHit2D hit = Physics2D.Raycast(detectionCenter, directionToCollider.normalized, Mathf.Infinity, detectionLayers);
                if (hit.collider == collider)
                {
                    return hit.point;
                }
                else
                {
                    // 如果射线检测失败，回退到边界最近点
                    return collider.ClosestPoint(detectionCenter);
                }

            default:
                return collider.bounds.center;
        }
    }

    #region 位置和旋转计算
    private Vector2 GetCurrentOffset()
    {
        switch (shape)
        {
            case DetectionShape2D.Rectangle:
                return rectangleSettings.totalOffset;
            case DetectionShape2D.Sector:
                return sectorSettings.outerOffset;
            default:
                return Vector2.zero;
        }
    }

    private float GetCurrentRotation()
    {
        switch (shape)
        {
            case DetectionShape2D.Rectangle:
                return rectangleSettings.totalRotation;
            case DetectionShape2D.Sector:
                return sectorSettings.outerRotation;
            default:
                return 0f;
        }
    }

    private Vector2 GetCurrentOuterSize()
    {
        switch (shape)
        {
            case DetectionShape2D.Rectangle:
                return rectangleSettings.totalSize + rectangleSettings.outerSizeOffset;
            case DetectionShape2D.Sector:
                return new Vector2(sectorSettings.outerAngle, sectorSettings.outerRadius);
            default:
                return Vector2.one;
        }
    }

    private Vector2 GetCurrentInnerSize()
    {
        switch (shape)
        {
            case DetectionShape2D.Rectangle:
                return rectangleSettings.totalSize + rectangleSettings.innerSizeOffset;
            case DetectionShape2D.Sector:
                return new Vector2(sectorSettings.innerAngle, sectorSettings.innerRadius);
            default:
                return Vector2.one;
        }
    }
    #endregion

    #region 碰撞检测逻辑
    private bool IsPointInOuterBounds(Vector2 worldPoint, Vector2 center, float rotation)
    {
        Vector2 localPoint = Quaternion.Euler(0, 0, -rotation) * (worldPoint - center);

        switch (shape)
        {
            case DetectionShape2D.Rectangle:
                return IsPointInRectangle(localPoint, GetCurrentOuterSize());
            case DetectionShape2D.Sector:
                return IsPointInSector(localPoint, sectorSettings.outerRadius, sectorSettings.outerAngle, false);
            default:
                return false;
        }
    }

    private bool IsPointInInnerBounds(Vector2 worldPoint, Vector2 center, float rotation)
    {
        Vector2 localPoint = Quaternion.Euler(0, 0, -rotation) * (worldPoint - center);

        switch (shape)
        {
            case DetectionShape2D.Rectangle:
                return IsPointInRectangle(localPoint, GetCurrentInnerSize());
            case DetectionShape2D.Sector:
                return IsPointInSector(localPoint, sectorSettings.innerRadius, sectorSettings.innerAngle, true);
            default:
                return false;
        }
    }

    private bool IsPointInRectangle(Vector2 localPoint, Vector2 size)
    {
        return Mathf.Abs(localPoint.x) <= size.x * 0.5f && Mathf.Abs(localPoint.y) <= size.y * 0.5f;
    }

    private bool IsPointInSector(Vector2 localPoint, float radius, float angle, bool isInner)
    {
        float distance = localPoint.magnitude;
        if (distance > radius) return false;

        float pointAngle = Vector2.SignedAngle(Vector2.up, localPoint);
        float halfAngle = angle * 0.5f;

        return Mathf.Abs(pointAngle) <= halfAngle;
    }
    #endregion

    #region Gizmos绘制
    private void OnDrawGizmos()
    {
        if (!alwaysShowGizmos) return;
        DrawDetectionArea();
    }

    private void OnDrawGizmosSelected()
    {
        if (alwaysShowGizmos) return;
        DrawDetectionArea();
    }

    private void DrawDetectionArea()
    {
        Vector2 worldPos = (Vector2)transform.position + GetCurrentOffset();
        float worldRot = transform.rotation.eulerAngles.z + GetCurrentRotation();

        // 绘制外环
        DrawOuterShape(worldPos, worldRot);

        // 绘制内环
        if (HasInnerRing())
        {
            DrawInnerShape(worldPos, worldRot);
        }

        // 绘制环状区域
        if (shape == DetectionShape2D.Sector && sectorSettings.innerRadius > 0)
        {
            DrawRingArea(worldPos, worldRot);
        }

        DrawDetectedObjects();
    }

    private bool HasInnerRing()
    {
        switch (shape)
        {
            case DetectionShape2D.Rectangle:
                return (rectangleSettings.totalSize + rectangleSettings.innerSizeOffset).x > 0;
            case DetectionShape2D.Sector:
                return sectorSettings.innerRadius > 0;
            default:
                return false;
        }
    }

    private void DrawOuterShape(Vector2 center, float rotation)
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, rotation), Vector3.one);

        switch (shape)
        {
            case DetectionShape2D.Rectangle:
                Gizmos.color = rectangleSettings.outerColor;
                DrawRectangleGizmo(GetCurrentOuterSize(), rectangleSettings.outerFillColor);
                break;
            case DetectionShape2D.Sector:
                Gizmos.color = sectorSettings.outerColor;
                DrawImprovedSectorGizmo(sectorSettings.outerRadius, sectorSettings.outerAngle,
                    sectorSettings.outerFillColor, sectorSettings.outerBoundaryLineColor, false);
                break;
        }

        Gizmos.matrix = originalMatrix;
    }

    private void DrawInnerShape(Vector2 center, float rotation)
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;

        switch (shape)
        {
            case DetectionShape2D.Rectangle:
                Vector2 rectInnerCenter = center + rectangleSettings.innerOffsetOffset;
                float rectInnerRot = rotation + rectangleSettings.innerRotation;
                Gizmos.matrix = Matrix4x4.TRS(rectInnerCenter, Quaternion.Euler(0, 0, rectInnerRot), Vector3.one);
                Gizmos.color = rectangleSettings.innerColor;
                DrawRectangleGizmo(GetCurrentInnerSize(), rectangleSettings.innerFillColor);
                break;

            case DetectionShape2D.Sector:
                Vector2 sectorInnerCenter = center + sectorSettings.innerOffset;
                float sectorInnerRot = rotation + sectorSettings.innerRotation;
                Gizmos.matrix = Matrix4x4.TRS(sectorInnerCenter, Quaternion.Euler(0, 0, sectorInnerRot), Vector3.one);
                Gizmos.color = sectorSettings.innerColor;
                DrawImprovedSectorGizmo(sectorSettings.innerRadius, sectorSettings.innerAngle,
                    sectorSettings.innerFillColor, sectorSettings.innerBoundaryLineColor, true);
                break;
        }

        Gizmos.matrix = originalMatrix;
    }

    private void DrawRectangleGizmo(Vector2 size, Color fillColor)
    {
        Gizmos.DrawWireCube(Vector3.zero, size);
        var originalColor = Gizmos.color;
        Gizmos.color = fillColor;
        Gizmos.DrawCube(Vector3.zero, size);
        Gizmos.color = originalColor;
    }

    /// <summary>
    /// 改进的扇形绘制 - 使用更平滑的弧线
    /// </summary>
    private void DrawImprovedSectorGizmo(float radius, float angle, Color fillColor, Color boundaryColor, bool isInner)
    {
        int segments = Mathf.Max(16, (int)(angle / 5f) * 2); // 自适应分段
        float startAngle = -angle * 0.5f;

        Vector3[] points = new Vector3[segments + 1];

        // 生成弧线点
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = startAngle + i * angle / segments;
            float angleRad = currentAngle * Mathf.Deg2Rad;
            points[i] = new Vector3(Mathf.Sin(angleRad) * radius, Mathf.Cos(angleRad) * radius, 0);
        }

        // 绘制边界线
        Gizmos.DrawLine(Vector3.zero, points[0]);
        Gizmos.DrawLine(Vector3.zero, points[segments]);

        // 绘制弧线
        for (int i = 0; i < segments; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }

        // 绘制填充
        var originalColor = Gizmos.color;
        Gizmos.color = fillColor;
        for (int i = 0; i < segments; i++)
        {
            Gizmos.DrawLine(Vector3.zero, points[i]);
            Gizmos.DrawLine(Vector3.zero, points[i + 1]);
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
        Gizmos.color = originalColor;

        // 绘制边界高亮
        if (angle < 360f)
        {
            Gizmos.color = boundaryColor;
            Gizmos.DrawLine(Vector3.zero, points[0]);
            Gizmos.DrawLine(Vector3.zero, points[segments]);
            Gizmos.color = sectorSettings.outerColor; // 恢复颜色
        }
    }

    private void DrawRingArea(Vector2 center, float rotation)
    {
        Gizmos.color = sectorSettings.ringColor;

        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, rotation), Vector3.one);

        // 绘制扇形环状区域边界
        DrawSectorWireframe(sectorSettings.outerRadius, sectorSettings.outerAngle);
        DrawSectorWireframe(sectorSettings.innerRadius, sectorSettings.innerAngle);

        Gizmos.matrix = originalMatrix;
    }

    private void DrawSectorWireframe(float radius, float angle)
    {
        int segments = 24;
        float startAngle = -angle * 0.5f;

        Vector3 prevPoint = Vector3.zero;
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = startAngle + i * angle / segments;
            float angleRad = currentAngle * Mathf.Deg2Rad;
            Vector3 point = new Vector3(Mathf.Sin(angleRad) * radius, Mathf.Cos(angleRad) * radius, 0);

            if (i > 0) Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }

        // 绘制径向线
        Gizmos.DrawLine(Vector3.zero,
            new Vector3(Mathf.Sin(startAngle * Mathf.Deg2Rad) * radius,
                       Mathf.Cos(startAngle * Mathf.Deg2Rad) * radius, 0));
        Gizmos.DrawLine(Vector3.zero,
            new Vector3(Mathf.Sin((startAngle + angle) * Mathf.Deg2Rad) * radius,
                       Mathf.Cos((startAngle + angle) * Mathf.Deg2Rad) * radius, 0));
    }

    private void DrawDetectedObjects()
    {
        Gizmos.color = Color.red;
        foreach (var obj in innerRingObjects)
        {
            if (obj != null) Gizmos.DrawWireSphere(obj.transform.position, 0.3f);
        }

        Gizmos.color = Color.blue;
        foreach (var obj in outerRingObjects)
        {
            if (obj != null) Gizmos.DrawWireSphere(obj.transform.position, 0.2f);
        }
    }
    #endregion

    #region 公共方法
    public List<GameObject> GetInnerRingObjects() => new List<GameObject>(innerRingObjects);
    public List<GameObject> GetOuterRingObjects() => new List<GameObject>(outerRingObjects);
    public List<GameObject> GetAllDetectedObjects()
    {
        var all = new List<GameObject>();
        all.AddRange(innerRingObjects);
        all.AddRange(outerRingObjects);
        return all;
    }

    public int GetInnerRingCount() => innerRingObjects.Count;
    public int GetOuterRingCount() => outerRingObjects.Count;
    public int GetAllDetectedCount() => innerRingObjects.Count + outerRingObjects.Count;
    public void ClearDetection() { innerRingObjects.Clear(); outerRingObjects.Clear(); }
    public float GetLastDetectionTime() => lastDetectionTime;

    /// <summary>
    /// 获取当前检测方式
    /// </summary>
    public DetectionMethod GetDetectionMethod() => detectionMethod;

    /// <summary>
    /// 设置检测方式
    /// </summary>
    public void SetDetectionMethod(DetectionMethod method) => detectionMethod = method;
    #endregion
}

[System.Serializable]
public class TagMask2D
{
    public bool everything = false;
    public bool nothing = false;
    public List<string> includedTags = new List<string>();

    public bool IsTagValid(string tag)
    {
        if (nothing) return false;
        if (everything) return true;
        return includedTags.Count == 0 || includedTags.Contains(tag);
    }
}