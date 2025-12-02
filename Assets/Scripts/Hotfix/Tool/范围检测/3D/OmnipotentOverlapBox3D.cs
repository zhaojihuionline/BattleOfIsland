using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3D多功能碰撞检测 - Physics.OverlapBox版本
/// </summary>
public class OmnipotentOverlapBox3D : MonoBehaviour
{
    [Header("基础设置")]
    public DetectionShape3D shape = DetectionShape3D.Cube;
    public DetectionMethod detectionMethod = DetectionMethod.CenterPoint; // 新增：检测方式

    [Header("检测设置")]
    public LayerMask detectionLayers = -1;
    public TagMask3D detectionTags = new TagMask3D();
    public bool detectOnAwake = false;
    public float detectionInterval = 0f;
    public bool alwaysShowGizmos = true;

    [Header("立方体设置")]
    public CubeSettings cubeSettings = new CubeSettings();

    [Header("圆柱体设置")]
    public CylinderSettings cylinderSettings = new CylinderSettings();

    [Header("3D扇形设置")]
    public Sector3DSettings sector3DSettings = new Sector3DSettings();

    [Header("检测结果")]
    [SerializeField] private List<GameObject> innerRingObjects = new List<GameObject>();
    [SerializeField] private List<GameObject> outerRingObjects = new List<GameObject>();

    private float lastDetectionTime = 0f;

    public enum DetectionShape3D { Cube, Cylinder, Sector3D }
    public enum DetectionMethod { CenterPoint, Bounds } // 新增：检测方式枚举

    [System.Serializable]
    public class CubeSettings
    {
        [Header("总体设置")]
        public Vector3 totalSize = new Vector3(5, 5, 5);
        public Vector3 totalOffset = Vector3.zero;
        public Vector3 totalRotation = Vector3.zero;

        [Header("外环设置")]
        public Vector3 outerSizeOffset = Vector3.zero;
        public Vector3 outerOffsetOffset = Vector3.zero;
        public Vector3 outerRotation = Vector3.zero;
        public Color outerColor = new Color(0, 1, 0, 0.3f);
        public Color outerFillColor = new Color(0, 1, 0, 0.1f);

        [Header("内环设置")]
        public Vector3 innerSizeOffset = new Vector3(-3, -3, -3);
        public Vector3 innerOffsetOffset = Vector3.zero;
        public Vector3 innerRotation = Vector3.zero;
        public Color innerColor = new Color(1, 0, 0, 0.3f);
        public Color innerFillColor = new Color(1, 0, 0, 0.1f);
    }

    [System.Serializable]
    public class CylinderSettings
    {
        [Header("外环设置")]
        public float outerRadius = 5f;
        public float outerHeight = 5f;
        public Vector3 outerOffset = Vector3.zero;
        public Vector3 outerRotation = Vector3.zero;
        public float outerHorizontalAngle = 360f;
        public Color outerColor = new Color(0, 1, 0, 0.3f);
        public Color outerFillColor = new Color(0, 1, 0, 0.1f);
        public Color outerBoundaryLineColor = new Color(1, 1, 1, 0.8f);

        [Header("内环设置")]
        public float innerRadius = 2f;
        public float innerHeight = 3f;
        public Vector3 innerOffset = Vector3.zero;
        public Vector3 innerRotation = Vector3.zero;
        public float innerHorizontalAngle = 360f;
        public Color innerColor = new Color(1, 0, 0, 0.3f);
        public Color innerFillColor = new Color(1, 0, 0, 0.1f);
        public Color innerBoundaryLineColor = new Color(1, 1, 1, 0.8f);

        [Header("其他设置")]
        public bool useHorizontalAngle = false;
    }

    [System.Serializable]
    public class Sector3DSettings
    {
        [Header("外环设置")]
        public float outerRadius = 5f;
        public Vector3 outerOffset = Vector3.zero;
        public Vector3 outerRotation = Vector3.zero;
        [Range(0, 180)] public float outerHorizontalAngle = 90f;
        [Range(0, 360)] public float outerVerticalAngle = 90f;
        public Color outerColor = new Color(0, 1, 0, 0.3f);
        public Color outerFillColor = new Color(0, 1, 0, 0.1f);
        public Color outerBoundaryLineColor = new Color(1, 1, 1, 0.8f);

        [Header("内环设置")]
        public float innerRadius = 2f;
        public Vector3 innerOffset = Vector3.zero;
        public Vector3 innerRotation = Vector3.zero;
        [Range(0, 180)] public float innerHorizontalAngle = 90f;
        [Range(0, 360)] public float innerVerticalAngle = 90f;
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

        Vector3 worldPos = transform.position + GetCurrentOffset();
        Quaternion worldRot = transform.rotation * GetCurrentRotation();

        // 使用优化的候选集获取方法
        Collider[] allColliders = GetOptimalCandidates(worldPos, worldRot);

        foreach (var collider in allColliders)
        {
            if (collider.gameObject == gameObject) continue;
            if (!detectionTags.IsTagValid(collider.tag)) continue;

            // 根据检测方式选择检测点
            Vector3 detectionPoint = GetDetectionPoint(collider);

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
    private Collider[] GetOptimalCandidates(Vector3 worldPos, Quaternion worldRot)
    {
        switch (shape)
        {
            case DetectionShape3D.Cube:
                // 立方体：使用OverlapBox
                Vector3 detectionSize = GetCurrentOuterSize();
                return Physics.OverlapBox(worldPos, detectionSize * 0.5f, worldRot, detectionLayers);

            case DetectionShape3D.Cylinder:
                // 圆柱体：使用OverlapCapsule（更高效）
                return GetCylinderCandidates(worldPos, worldRot);

            case DetectionShape3D.Sector3D:
                // 扇形：使用OverlapSphere（最高效）
                return Physics.OverlapSphere(worldPos, sector3DSettings.outerRadius, detectionLayers);

            default:
                Vector3 size = GetCurrentOuterSize();
                return Physics.OverlapBox(worldPos, size * 0.5f, worldRot, detectionLayers);
        }
    }

    /// <summary>
    /// 获取圆柱体检测的候选集
    /// </summary>
    private Collider[] GetCylinderCandidates(Vector3 worldPos, Quaternion worldRot)
    {
        float height = cylinderSettings.outerHeight;
        float radius = cylinderSettings.outerRadius;

        // 计算胶囊体的顶部和底部点（考虑旋转）
        Vector3 top = worldPos + worldRot * Vector3.up * height * 0.5f;
        Vector3 bottom = worldPos + worldRot * Vector3.down * height * 0.5f;

        return Physics.OverlapCapsule(top, bottom, radius, detectionLayers);
    }

    /// <summary>
    /// 根据检测方式获取检测点
    /// </summary>
    private Vector3 GetDetectionPoint(Collider collider)
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
    private Vector3 GetClosestPointOnBounds(Collider collider)
    {
        Vector3 detectionCenter = transform.position + GetCurrentOffset();

        switch (shape)
        {
            case DetectionShape3D.Cube:
                // 对于立方体，使用立方体的最近边界点
                return collider.ClosestPointOnBounds(detectionCenter);

            case DetectionShape3D.Cylinder:
            case DetectionShape3D.Sector3D:
                // 对于圆柱体和扇形，计算从检测中心到碰撞体边界的方向向量
                Vector3 directionToCollider = collider.bounds.center - detectionCenter;
                if (directionToCollider.magnitude < 0.001f)
                {
                    // 如果中心重合，使用随机方向
                    directionToCollider = Vector3.forward;
                }

                // 沿着方向找到边界上的点
                Ray ray = new Ray(detectionCenter, directionToCollider.normalized);
                if (collider.Raycast(ray, out RaycastHit hit, float.MaxValue))
                {
                    return hit.point;
                }
                else
                {
                    // 如果射线检测失败，回退到边界最近点
                    return collider.ClosestPointOnBounds(detectionCenter);
                }

            default:
                return collider.bounds.center;
        }
    }

    #region 位置和旋转计算
    private Vector3 GetCurrentOffset()
    {
        switch (shape)
        {
            case DetectionShape3D.Cube:
                return cubeSettings.totalOffset;
            case DetectionShape3D.Cylinder:
                return cylinderSettings.outerOffset;
            case DetectionShape3D.Sector3D:
                return sector3DSettings.outerOffset;
            default:
                return Vector3.zero;
        }
    }

    private Quaternion GetCurrentRotation()
    {
        Vector3 rotation = Vector3.zero;
        switch (shape)
        {
            case DetectionShape3D.Cube:
                rotation = cubeSettings.totalRotation;
                break;
            case DetectionShape3D.Cylinder:
                rotation = cylinderSettings.outerRotation;
                break;
            case DetectionShape3D.Sector3D:
                rotation = sector3DSettings.outerRotation;
                break;
        }
        return Quaternion.Euler(rotation);
    }

    private Vector3 GetCurrentOuterSize()
    {
        switch (shape)
        {
            case DetectionShape3D.Cube:
                return cubeSettings.totalSize + cubeSettings.outerSizeOffset;
            case DetectionShape3D.Cylinder:
                float radius = cylinderSettings.outerRadius;
                return new Vector3(radius * 2f, cylinderSettings.outerHeight, radius * 2f);
            case DetectionShape3D.Sector3D:
                return Vector3.one * sector3DSettings.outerRadius * 2f;
            default:
                return Vector3.one;
        }
    }

    private Vector3 GetCurrentInnerSize()
    {
        switch (shape)
        {
            case DetectionShape3D.Cube:
                return cubeSettings.totalSize + cubeSettings.innerSizeOffset;
            case DetectionShape3D.Cylinder:
                float radius = cylinderSettings.innerRadius;
                return new Vector3(radius * 2f, cylinderSettings.innerHeight, radius * 2f);
            case DetectionShape3D.Sector3D:
                return Vector3.one * sector3DSettings.innerRadius * 2f;
            default:
                return Vector3.one;
        }
    }
    #endregion

    #region 碰撞检测逻辑
    private bool IsPointInOuterBounds(Vector3 worldPoint, Vector3 center, Quaternion rotation)
    {
        Vector3 localPoint = Quaternion.Inverse(rotation) * (worldPoint - center);

        switch (shape)
        {
            case DetectionShape3D.Cube:
                return IsPointInCube(localPoint, GetCurrentOuterSize());
            case DetectionShape3D.Cylinder:
                return IsPointInCylinder(localPoint, cylinderSettings.outerRadius, cylinderSettings.outerHeight,
                    cylinderSettings.outerHorizontalAngle, false);
            case DetectionShape3D.Sector3D:
                return IsPointInSector3D(localPoint, sector3DSettings.outerRadius,
                    sector3DSettings.outerHorizontalAngle, sector3DSettings.outerVerticalAngle, false);
            default:
                return false;
        }
    }

    private bool IsPointInInnerBounds(Vector3 worldPoint, Vector3 center, Quaternion rotation)
    {
        Vector3 localPoint = Quaternion.Inverse(rotation) * (worldPoint - center);

        switch (shape)
        {
            case DetectionShape3D.Cube:
                return IsPointInCube(localPoint, GetCurrentInnerSize());
            case DetectionShape3D.Cylinder:
                return IsPointInCylinder(localPoint, cylinderSettings.innerRadius, cylinderSettings.innerHeight,
                    cylinderSettings.innerHorizontalAngle, true);
            case DetectionShape3D.Sector3D:
                return IsPointInSector3D(localPoint, sector3DSettings.innerRadius,
                    sector3DSettings.innerHorizontalAngle, sector3DSettings.innerVerticalAngle, true);
            default:
                return false;
        }
    }

    private bool IsPointInCube(Vector3 localPoint, Vector3 size)
    {
        return Mathf.Abs(localPoint.x) <= size.x * 0.5f &&
               Mathf.Abs(localPoint.y) <= size.y * 0.5f &&
               Mathf.Abs(localPoint.z) <= size.z * 0.5f;
    }

    private bool IsPointInCylinder(Vector3 localPoint, float radius, float height, float horizontalAngle, bool isInner)
    {
        if (Mathf.Abs(localPoint.y) > height * 0.5f) return false;

        float horizontalDistance = Mathf.Sqrt(localPoint.x * localPoint.x + localPoint.z * localPoint.z);
        if (horizontalDistance > radius) return false;

        if (cylinderSettings.useHorizontalAngle && horizontalAngle < 360f)
        {
            float angle = Mathf.Atan2(localPoint.x, localPoint.z) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            float halfAngle = horizontalAngle * 0.5f;
            if (Mathf.Abs(angle) > halfAngle && Mathf.Abs(angle - 360f) > halfAngle)
                return false;
        }

        return true;
    }

    private bool IsPointInSector3D(Vector3 localPoint, float radius, float horizontalAngle, float verticalAngle, bool isInner)
    {
        float distance = localPoint.magnitude;
        if (distance > radius) return false;

        float horizontal = Mathf.Atan2(localPoint.x, localPoint.z) * Mathf.Rad2Deg;
        float vertical = Vector3.Angle(new Vector3(0, 0, 1), new Vector3(0, localPoint.y, localPoint.z));
        if (localPoint.y < 0) vertical = -vertical;

        if (horizontalAngle < 180f)
        {
            float halfHorizontal = horizontalAngle * 0.5f;
            if (Mathf.Abs(horizontal) > halfHorizontal)
                return false;
        }

        if (verticalAngle < 360f)
        {
            float normalizedVertical = vertical;
            if (normalizedVertical < 0) normalizedVertical += 360f;
            if (normalizedVertical > verticalAngle)
                return false;
        }

        return true;
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
        Vector3 worldPos = transform.position + GetCurrentOffset();
        Quaternion worldRot = transform.rotation * GetCurrentRotation();

        // 绘制外环
        DrawOuterShape(worldPos, worldRot);

        // 绘制内环
        if (HasInnerRing())
        {
            DrawInnerShape(worldPos, worldRot);
        }

        // 绘制环状区域
        if (shape == DetectionShape3D.Sector3D && sector3DSettings.innerRadius > 0)
        {
            DrawRingArea(worldPos, worldRot);
        }

        DrawDetectedObjects();
    }

    private bool HasInnerRing()
    {
        switch (shape)
        {
            case DetectionShape3D.Cube:
                return (cubeSettings.totalSize + cubeSettings.innerSizeOffset).x > 0;
            case DetectionShape3D.Cylinder:
                return cylinderSettings.innerRadius > 0;
            case DetectionShape3D.Sector3D:
                return sector3DSettings.innerRadius > 0;
            default:
                return false;
        }
    }

    private void DrawOuterShape(Vector3 center, Quaternion rotation)
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);

        switch (shape)
        {
            case DetectionShape3D.Cube:
                Gizmos.color = cubeSettings.outerColor;
                DrawCubeGizmo(GetCurrentOuterSize(), cubeSettings.outerFillColor);
                break;
            case DetectionShape3D.Cylinder:
                Gizmos.color = cylinderSettings.outerColor;
                DrawImprovedCylinderGizmo(cylinderSettings.outerRadius, cylinderSettings.outerHeight,
                    cylinderSettings.outerHorizontalAngle, cylinderSettings.outerFillColor, cylinderSettings.outerBoundaryLineColor, false);
                break;
            case DetectionShape3D.Sector3D:
                Gizmos.color = sector3DSettings.outerColor;
                DrawImprovedSector3DGizmo(sector3DSettings.outerRadius, sector3DSettings.outerHorizontalAngle,
                    sector3DSettings.outerVerticalAngle, sector3DSettings.outerFillColor, sector3DSettings.outerBoundaryLineColor, false);
                break;
        }

        Gizmos.matrix = originalMatrix;
    }

    private void DrawInnerShape(Vector3 center, Quaternion rotation)
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;

        switch (shape)
        {
            case DetectionShape3D.Cube:
                Vector3 innerCenter = center + cubeSettings.innerOffsetOffset;
                Quaternion innerRot = rotation * Quaternion.Euler(cubeSettings.innerRotation);
                Gizmos.matrix = Matrix4x4.TRS(innerCenter, innerRot, Vector3.one);
                Gizmos.color = cubeSettings.innerColor;
                DrawCubeGizmo(GetCurrentInnerSize(), cubeSettings.innerFillColor);
                break;

            case DetectionShape3D.Cylinder:
                Vector3 cylinderInnerCenter = center + cylinderSettings.innerOffset;
                Quaternion cylinderInnerRot = rotation * Quaternion.Euler(cylinderSettings.innerRotation);
                Gizmos.matrix = Matrix4x4.TRS(cylinderInnerCenter, cylinderInnerRot, Vector3.one);
                Gizmos.color = cylinderSettings.innerColor;
                DrawImprovedCylinderGizmo(cylinderSettings.innerRadius, cylinderSettings.innerHeight,
                    cylinderSettings.innerHorizontalAngle, cylinderSettings.innerFillColor, cylinderSettings.innerBoundaryLineColor, true);
                break;

            case DetectionShape3D.Sector3D:
                Vector3 sectorInnerCenter = center + sector3DSettings.innerOffset;
                Quaternion sectorInnerRot = rotation * Quaternion.Euler(sector3DSettings.innerRotation);
                Gizmos.matrix = Matrix4x4.TRS(sectorInnerCenter, sectorInnerRot, Vector3.one);
                Gizmos.color = sector3DSettings.innerColor;
                DrawImprovedSector3DGizmo(sector3DSettings.innerRadius, sector3DSettings.innerHorizontalAngle,
                    sector3DSettings.innerVerticalAngle, sector3DSettings.innerFillColor, sector3DSettings.innerBoundaryLineColor, true);
                break;
        }

        Gizmos.matrix = originalMatrix;
    }

    private void DrawCubeGizmo(Vector3 size, Color fillColor)
    {
        Gizmos.DrawWireCube(Vector3.zero, size);
        var originalColor = Gizmos.color;
        Gizmos.color = fillColor;
        Gizmos.DrawCube(Vector3.zero, size);
        Gizmos.color = originalColor;
    }

    /// <summary>
    /// 改进的圆柱体绘制 - 使用更平滑的网格
    /// </summary>
    private void DrawImprovedCylinderGizmo(float radius, float height, float horizontalAngle, Color fillColor, Color boundaryColor, bool isInner)
    {
        int segments = Mathf.Max(24, (int)(horizontalAngle / 15f) * 4); // 自适应分段
        float startAngle = -horizontalAngle * 0.5f;
        float endAngle = horizontalAngle * 0.5f;

        // 绘制顶部和底部圆盘
        DrawImprovedCircleGizmo(Vector3.up * height * 0.5f, radius, horizontalAngle, fillColor, 32);
        DrawImprovedCircleGizmo(Vector3.down * height * 0.5f, radius, horizontalAngle, fillColor, 32);

        // 绘制侧面 - 使用更密集的网格
        Vector3[] topPoints = new Vector3[segments + 1];
        Vector3[] bottomPoints = new Vector3[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float angle = startAngle + i * horizontalAngle / segments;
            float angleRad = angle * Mathf.Deg2Rad;

            Vector3 point = new Vector3(Mathf.Sin(angleRad) * radius, 0, Mathf.Cos(angleRad) * radius);
            topPoints[i] = point + Vector3.up * height * 0.5f;
            bottomPoints[i] = point + Vector3.down * height * 0.5f;

            // 绘制垂直线
            if (i > 0 && i < segments)
            {
                Gizmos.DrawLine(topPoints[i], bottomPoints[i]);
            }
        }

        // 绘制顶部和底部边界
        for (int i = 0; i < segments; i++)
        {
            Gizmos.DrawLine(topPoints[i], topPoints[i + 1]);
            Gizmos.DrawLine(bottomPoints[i], bottomPoints[i + 1]);
        }

        // 绘制侧面填充
        DrawCylinderSideFill(topPoints, bottomPoints, fillColor);

        // 绘制边界线
        if (horizontalAngle < 360f)
        {
            Gizmos.color = boundaryColor;
            Gizmos.DrawLine(topPoints[0], bottomPoints[0]);
            Gizmos.DrawLine(topPoints[segments], bottomPoints[segments]);

            // 绘制径向线
            Gizmos.DrawLine(Vector3.zero, topPoints[0]);
            Gizmos.DrawLine(Vector3.zero, topPoints[segments]);
            Gizmos.DrawLine(Vector3.zero, bottomPoints[0]);
            Gizmos.DrawLine(Vector3.zero, bottomPoints[segments]);

            Gizmos.color = cylinderSettings.outerColor; // 恢复颜色
        }
    }

    /// <summary>
    /// 改进的3D扇形绘制 - 使用更平滑的球面网格
    /// </summary>
    private void DrawImprovedSector3DGizmo(float radius, float horizontalAngle, float verticalAngle, Color fillColor, Color boundaryColor, bool isInner)
    {
        int horizontalSegments = Mathf.Max(16, (int)(horizontalAngle / 10f) * 2);
        int verticalSegments = Mathf.Max(8, (int)(verticalAngle / 30f) * 2);

        Vector3[,] points = new Vector3[horizontalSegments + 1, verticalSegments + 1];

        // 生成球面网格点
        for (int i = 0; i <= horizontalSegments; i++)
        {
            float horizontal = -horizontalAngle * 0.5f + i * horizontalAngle / horizontalSegments;

            for (int j = 0; j <= verticalSegments; j++)
            {
                float vertical = j * verticalAngle / verticalSegments;
                points[i, j] = GetImprovedSectorPoint(radius, horizontal, vertical);
            }
        }

        // 绘制经线
        for (int i = 0; i <= horizontalSegments; i++)
        {
            for (int j = 0; j < verticalSegments; j++)
            {
                Gizmos.DrawLine(points[i, j], points[i, j + 1]);
            }
        }

        // 绘制纬线
        for (int j = 0; j <= verticalSegments; j++)
        {
            for (int i = 0; i < horizontalSegments; i++)
            {
                Gizmos.DrawLine(points[i, j], points[i + 1, j]);
            }
        }

        // 绘制填充面 - 使用三角形网格
        DrawSector3DFill(points, horizontalSegments, verticalSegments, fillColor);

        // 绘制边界线
        Gizmos.color = boundaryColor;

        // 水平边界
        for (int j = 0; j <= verticalSegments; j++)
        {
            Gizmos.DrawLine(Vector3.zero, points[0, j]);
            Gizmos.DrawLine(Vector3.zero, points[horizontalSegments, j]);
        }

        // 垂直边界
        for (int i = 0; i <= horizontalSegments; i++)
        {
            Gizmos.DrawLine(Vector3.zero, points[i, verticalSegments]);
        }

        Gizmos.color = sector3DSettings.outerColor; // 恢复颜色
    }

    private void DrawRingArea(Vector3 center, Quaternion rotation)
    {
        Gizmos.color = sector3DSettings.ringColor;

        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);

        // 绘制扇形环状区域边界
        DrawSector3DWireframe(sector3DSettings.outerRadius, sector3DSettings.outerHorizontalAngle, sector3DSettings.outerVerticalAngle);
        DrawSector3DWireframe(sector3DSettings.innerRadius, sector3DSettings.innerHorizontalAngle, sector3DSettings.innerVerticalAngle);

        Gizmos.matrix = originalMatrix;
    }

    private void DrawSector3DWireframe(float radius, float horizontalAngle, float verticalAngle)
    {
        int horizontalSegments = 16;
        int verticalSegments = 8;

        // 绘制经线
        for (int i = 0; i <= horizontalSegments; i++)
        {
            float horizontal = -horizontalAngle * 0.5f + i * horizontalAngle / horizontalSegments;
            Vector3 prevPoint = Vector3.zero;

            for (int j = 0; j <= verticalSegments; j++)
            {
                float vertical = j * verticalAngle / verticalSegments;
                Vector3 point = GetImprovedSectorPoint(radius, horizontal, vertical);
                if (j > 0) Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }
        }

        // 绘制纬线
        for (int j = 0; j <= verticalSegments; j++)
        {
            float vertical = j * verticalAngle / verticalSegments;
            Vector3 prevPoint = Vector3.zero;

            for (int i = 0; i <= horizontalSegments; i++)
            {
                float horizontal = -horizontalAngle * 0.5f + i * horizontalAngle / horizontalSegments;
                Vector3 point = GetImprovedSectorPoint(radius, horizontal, vertical);
                if (i > 0) Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }
        }
    }

    /// <summary>
    /// 改进的球面坐标计算
    /// </summary>
    private Vector3 GetImprovedSectorPoint(float radius, float horizontal, float vertical)
    {
        float hRad = horizontal * Mathf.Deg2Rad;
        float vRad = vertical * Mathf.Deg2Rad;

        // 使用标准的球面坐标转换
        float x = radius * Mathf.Sin(hRad) * Mathf.Cos(vRad);
        float y = radius * Mathf.Sin(vRad);
        float z = radius * Mathf.Cos(hRad) * Mathf.Cos(vRad);

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// 改进的圆形绘制 - 更平滑的圆环
    /// </summary>
    private void DrawImprovedCircleGizmo(Vector3 center, float radius, float angle, Color fillColor, int segments = 32)
    {
        Vector3[] points = new Vector3[segments + 1];
        float startAngle = -angle * 0.5f;

        // 生成圆环点
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = startAngle + i * angle / segments;
            float angleRad = currentAngle * Mathf.Deg2Rad;
            points[i] = center + new Vector3(Mathf.Sin(angleRad) * radius, 0, Mathf.Cos(angleRad) * radius);
        }

        // 绘制圆环
        for (int i = 0; i < segments; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }

        // 绘制填充三角形
        var originalColor = Gizmos.color;
        Gizmos.color = fillColor;
        for (int i = 0; i < segments; i++)
        {
            Gizmos.DrawLine(center, points[i]);
            Gizmos.DrawLine(center, points[i + 1]);
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
        Gizmos.color = originalColor;
    }

    /// <summary>
    /// 绘制圆柱体侧面填充
    /// </summary>
    private void DrawCylinderSideFill(Vector3[] topPoints, Vector3[] bottomPoints, Color fillColor)
    {
        var originalColor = Gizmos.color;
        Gizmos.color = fillColor;

        for (int i = 0; i < topPoints.Length - 1; i++)
        {
            // 绘制侧面四边形
            Gizmos.DrawLine(topPoints[i], topPoints[i + 1]);
            Gizmos.DrawLine(bottomPoints[i], bottomPoints[i + 1]);
            Gizmos.DrawLine(topPoints[i], bottomPoints[i]);
        }

        Gizmos.color = originalColor;
    }

    /// <summary>
    /// 绘制3D扇形填充
    /// </summary>
    private void DrawSector3DFill(Vector3[,] points, int hSegments, int vSegments, Color fillColor)
    {
        var originalColor = Gizmos.color;
        Gizmos.color = fillColor;

        for (int i = 0; i < hSegments; i++)
        {
            for (int j = 0; j < vSegments; j++)
            {
                // 绘制两个三角形组成四边形
                Vector3 p1 = points[i, j];
                Vector3 p2 = points[i + 1, j];
                Vector3 p3 = points[i + 1, j + 1];
                Vector3 p4 = points[i, j + 1];

                // 第一个三角形
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p2, p3);
                Gizmos.DrawLine(p3, p1);

                // 第二个三角形
                Gizmos.DrawLine(p1, p3);
                Gizmos.DrawLine(p3, p4);
                Gizmos.DrawLine(p4, p1);
            }
        }

        Gizmos.color = originalColor;
    }

    private void DrawDetectedObjects()
    {
        Gizmos.color = Color.red;
        foreach (var obj in innerRingObjects)
        {
            if (obj != null) Gizmos.DrawWireSphere(obj.transform.position, 0.5f);
        }

        Gizmos.color = Color.blue;
        foreach (var obj in outerRingObjects)
        {
            if (obj != null) Gizmos.DrawWireSphere(obj.transform.position, 0.3f);
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
public class TagMask3D
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