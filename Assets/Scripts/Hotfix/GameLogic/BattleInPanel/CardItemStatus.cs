using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 卡片的几种状态
/// </summary>
public enum CardItemStatus
{
    /// <summary>
    /// 无状态
    /// </summary>
    None,
    /// <summary>
    /// 未上场
    /// </summary>
    NotDeployed,
    /// <summary>
    /// 选中
    /// </summary>
    Selected,
    /// <summary>
    /// 已上场
    /// </summary>
    Deployed,
    /// <summary>
    /// 阵亡
    /// </summary>
    Dead
}
