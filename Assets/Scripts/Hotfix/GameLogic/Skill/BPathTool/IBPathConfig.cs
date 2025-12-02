// IBPathConfig.cs
using UnityEngine;

/// <summary>
/// 贝塞尔路径配置接口
/// </summary>
public interface IBPathConfig
{
    Vector3 StartPoint { get; }
    Vector3 EndPoint { get; }
    Vector3 ControlPoint { get; }
    int PathPoints { get; }
    float CompleteDistance { get; }
    bool IsMoving { get; }

    // 获取路径
    Vector3[] GetPath();

    // 设置目标
    void SetTargetPosition(Vector3 targetPos);
    void SetTrackingTarget(Transform target);

    // 移动控制
    void StartMove();
    void StopMove();
    void SetMoveSpeed(float speed);
    void CompleteMove();
}