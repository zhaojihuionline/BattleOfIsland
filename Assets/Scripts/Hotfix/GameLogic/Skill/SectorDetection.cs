using System.Collections.Generic;
using UnityEngine;

public static class SectorDetection
{
    /// <summary>
    /// 获取前方扇形区域内的所有单位
    /// </summary>
    /// <typeparam name="T">要检测的单位类型</typeparam>
    /// <param name="center">扇形中心点（检测者的位置）</param>
    /// <param name="forward">扇形前方方向</param>
    /// <param name="radius">扇形半径</param>
    /// <param name="angle">扇形角度（0-360）</param>
    /// <param name="layerMask">检测的层级</param>
    /// <returns>扇形区域内的单位列表</returns>
    public static List<T> GetUnitsInSector<T>(Vector3 center, Vector3 forward, float radius, float angle, LayerMask layerMask) where T : Component
    {
        List<T> unitsInSector = new List<T>();

        // 获取半径内的所有碰撞体
        Collider[] colliders = Physics.OverlapSphere(center, radius, layerMask);

        foreach (Collider collider in colliders)
        {
            T unit = collider.GetComponent<T>();
            if (unit != null)
            {
                // 计算到目标的向量
                Vector3 directionToTarget = (collider.transform.position - center).normalized;

                // 计算与前方方向的夹角
                float currentAngle = Vector3.Angle(forward, directionToTarget);

                // 如果角度在扇形范围内
                if (currentAngle <= angle / 2f)
                {
                    unitsInSector.Add(unit);
                }
            }
        }

        return unitsInSector;
    }

    /// <summary>
    /// 获取前方扇形区域内的所有单位（使用Transform的便捷方法）
    /// </summary>
    /// <typeparam name="T">要检测的单位类型</typeparam>
    /// <param name="detector">检测者的Transform</param>
    /// <param name="radius">扇形半径</param>
    /// <param name="angle">扇形角度（0-360）</param>
    /// <param name="layerMask">检测的层级</param>
    /// <returns>扇形区域内的单位列表</returns>
    public static List<T> GetUnitsInSector<T>(Transform detector, float radius, float angle, LayerMask layerMask) where T : Component
    {
        return GetUnitsInSector<T>(detector.position, detector.forward, radius, angle, layerMask);
    }

    /// <summary>
    /// 获取前方扇形区域内的所有单位（包含调试绘制）
    /// </summary>
    /// <typeparam name="T">要检测的单位类型</typeparam>
    /// <param name="center">扇形中心点</param>
    /// <param name="forward">扇形前方方向</param>
    /// <param name="radius">扇形半径</param>
    /// <param name="angle">扇形角度</param>
    /// <param name="layerMask">检测的层级</param>
    /// <param name="debugColor">调试颜色</param>
    /// <param name="debugDuration">调试显示时间</param>
    /// <returns>扇形区域内的单位列表</returns>
    public static List<T> GetUnitsInSectorWithDebug<T>(Vector3 center, Vector3 forward, float radius, float angle, LayerMask layerMask, Color debugColor, float debugDuration = 0f) where T : Component
    {
        // 检测单位
        List<T> units = GetUnitsInSector<T>(center, forward, radius, angle, layerMask);

        // 绘制扇形调试
        DrawSector(center, forward, radius, angle, debugColor, debugDuration);

        return units;
    }

    /// <summary>
    /// 绘制扇形区域（调试用）
    /// </summary>
    private static void DrawSector(Vector3 center, Vector3 forward, float radius, float angle, Color color, float duration)
    {
#if UNITY_EDITOR
        float halfAngle = angle / 2f;
        Quaternion leftRotation = Quaternion.AngleAxis(-halfAngle, Vector3.up);
        Quaternion rightRotation = Quaternion.AngleAxis(halfAngle, Vector3.up);

        Vector3 leftDirection = leftRotation * forward;
        Vector3 rightDirection = rightRotation * forward;

        Vector3 leftEdge = center + leftDirection * radius;
        Vector3 rightEdge = center + rightDirection * radius;

        // 绘制扇形边界线
        Debug.DrawRay(center, leftDirection * radius, color, duration);
        Debug.DrawRay(center, rightDirection * radius, color, duration);

        // 绘制扇形弧线
        int segments = Mathf.RoundToInt(angle / 10f);
        Vector3 previousPoint = leftEdge;

        for (int i = 1; i <= segments; i++)
        {
            float segmentAngle = -halfAngle + (angle / segments) * i;
            Quaternion segmentRotation = Quaternion.AngleAxis(segmentAngle, Vector3.up);
            Vector3 segmentDirection = segmentRotation * forward;
            Vector3 segmentPoint = center + segmentDirection * radius;

            Debug.DrawLine(previousPoint, segmentPoint, color, duration);
            previousPoint = segmentPoint;
        }

        // 连接弧线两端
        Debug.DrawLine(leftEdge, center, color, duration);
        Debug.DrawLine(rightEdge, center, color, duration);
#endif
    }
}