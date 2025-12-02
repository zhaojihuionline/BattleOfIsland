using System.Collections.Generic;
using UnityEngine;

public static class BezierCurve
{
    /// <summary>
    /// 计算二次贝塞尔曲线点集
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="control">控制点</param>
    /// <param name="end">终点</param>
    /// <param name="segments">采样段数（越大越平滑）</param>
    /// <returns>曲线上的点列表</returns>
    public static List<Vector3> GetBezierPoints(Vector3 start, Vector3 control, Vector3 end, int segments = 30)
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 p = CalculateBezierPoint(t, start, control, end);
            points.Add(p);
        }
        return points;
    }

    /// <summary>
    /// 计算贝塞尔曲线单点
    /// </summary>
    private static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        // 二次贝塞尔公式：B(t) = (1-t)^2 * p0 + 2*(1-t)*t*p1 + t^2*p2
        return u * u * p0 + 2 * u * t * p1 + t * t * p2;
    }
}
