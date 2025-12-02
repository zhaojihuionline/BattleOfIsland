// AIEnums.cs
/// <summary>
/// 攻击偏好类型枚举
/// </summary>
public enum TargetPreferenceType
{
    Nearest,
    HealthBelowPercent,
    RandomTarget,
}

/// <summary>
/// 条件类型枚举
/// </summary>
public enum ConditionType
{
    HealthBelowPercent,
    DistanceToTargetLessThan,
    AttackPowerOver
}

/// <summary>
/// 逻辑运算符
/// </summary>
public enum LogicOperator
{
    AND,
    OR,
    NOT
}

/// <summary>
/// 偏好组选择模式
/// </summary>
public enum PreferenceSelectionMode
{
    WeightedRandom,  // 加权随机
    HighestWeight,   // 最高权重
    RoundRobin       // 轮询
}