using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType
{
    /// <summary>
    /// 正向buff
    /// </summary>
    BUFF,
    /// <summary>
    /// 负向buff
    /// </summary>
    DEBUFF
}

public enum BuffKind
{
    Normal = -1,
    /// <summary>
    /// 可灼烧
    /// </summary>
    Burn
}
