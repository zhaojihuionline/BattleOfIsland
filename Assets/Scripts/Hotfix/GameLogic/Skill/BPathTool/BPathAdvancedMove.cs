// BPathConfigAdvanced.cs
using UnityEngine;

/// <summary>
/// 高级贝塞尔路径配置组件 三阶
/// </summary>
public class BPathAdvancedMove : BPathMove
{
    [Header("控制点1偏移")]
    public Vector3 controlOffset1 = new Vector3(0, 2, 0);
    [Header("控制点2偏移")]
    public Vector3 controlOffset2 = new Vector3(0, 2, 0);
}