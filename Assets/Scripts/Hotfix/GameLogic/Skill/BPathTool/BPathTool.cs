// BPathTool.cs
using UnityEngine;

/// <summary>
/// 贝塞尔路径构造工具 - 纯逻辑类
/// </summary>
public static class BPathTool
{
    private const int DEFAULT_POINT_COUNT = 20;
    private const float CONTROL_POINT_RADIUS = 0.2f;

    /// <summary>
    /// 创建二次贝塞尔曲线路径
    /// </summary>
    public static Vector3[] CreateQuadraticBezierPath(Vector3 start, Vector3 control, Vector3 end, int pointCount = DEFAULT_POINT_COUNT)
    {
        pointCount = Mathf.Max(2, pointCount); // 确保至少2个点
        Vector3[] path = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            path[i] = CalculateQuadraticBezierPoint(start, control, end, t);
        }

        return path;
    }

    /// <summary>
    /// 创建三次贝塞尔曲线路径
    /// </summary>
    public static Vector3[] CreateCubicBezierPath(Vector3 start, Vector3 control1, Vector3 control2, Vector3 end, int pointCount = DEFAULT_POINT_COUNT)
    {
        pointCount = Mathf.Max(2, pointCount);
        Vector3[] path = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            path[i] = CalculateCubicBezierPoint(start, control1, control2, end, t);
        }

        return path;
    }

    /// <summary>
    /// 计算二次贝塞尔曲线点 (优化计算)
    /// </summary>
    private static Vector3 CalculateQuadraticBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        return u * u * p0 + 2 * u * t * p1 + t * t * p2;
    }

    /// <summary>
    /// 计算三次贝塞尔曲线点 (优化计算)
    /// </summary>
    private static Vector3 CalculateCubicBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float u2 = u * u;
        float t2 = t * t;

        return u2 * u * p0 + 3 * u2 * t * p1 + 3 * u * t2 * p2 + t2 * t * p3;
    }

    /// <summary>
    /// 在场景中绘制路径预览
    /// </summary>
    public static void DrawPathGizmos(Vector3[] path, Color color = default)
    {
        if (path == null || path.Length < 2) return;

        Gizmos.color = color == default ? Color.red : color;

        for (int i = 0; i < path.Length - 1; i++)
        {
            Gizmos.DrawLine(path[i], path[i + 1]);
        }
    }

    /// <summary>
    /// 绘制控制点
    /// </summary>
    public static void DrawControlPointsGizmos(Vector3[] controlPoints, Color color = default)
    {
        if (controlPoints == null || controlPoints.Length == 0) return;

        Gizmos.color = color == default ? Color.yellow : color;
        foreach (var point in controlPoints)
        {
            Gizmos.DrawWireSphere(point, CONTROL_POINT_RADIUS);
        }
    }
}