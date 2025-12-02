using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 邻居感知实体 - 根据周围邻居状态自动切换显示对应的模型
/// 支持双向邻居关系维护，适用于城墙、道路、围栏等需要根据连接关系改变外观的实体
/// 修复版本：正确处理移动过程中的邻居关系变化
/// </summary>
public class NeighborAwareEntity : MonoBehaviour
{
    [Header("模型配置")]
    [Tooltip("连接状态对应的模型数组，按索引对应不同连接状态的模型")]
    public GameObject[] connectionModels; // 连接状态对应的模型

    [Header("检测设置")]
    [Tooltip("邻居检测的射线长度，通常等于网格大小")]
    public float neighborCheckDistance = 1f;
    [Tooltip("指定要检测的物理层，避免检测到不相关的物体")]
    public LayerMask targetLayer;
    [Tooltip("指定要检测的标签，为空则检测所有该层的物体")]
    public string targetTag = "";

    [Header("连接规则")]
    [Tooltip("自定义连接规则数组，按顺序匹配第一个符合条件的规则")]
    public ConnectionRule[] connectionRules;

    [Tooltip("当前激活的模型引用")]
    private GameObject currentModel;

    /// <summary>
    /// 邻居字典 - 维护每个方向的邻居实体引用
    /// Key: 方向向量, Value: 邻居实体
    /// </summary>
    private Dictionary<Vector3, NeighborAwareEntity> neighborDictionary = new Dictionary<Vector3, NeighborAwareEntity>();

    /// <summary>
    /// 前邻居字典 - 记录上一次的邻居状态，用于检测离开
    /// </summary>
    private Dictionary<Vector3, NeighborAwareEntity> previousNeighborDictionary = new Dictionary<Vector3, NeighborAwareEntity>();

    /// <summary>
    /// 方向定义 - 用于邻居字典的键值
    /// </summary>
    private readonly Vector3[] directions = {
        Vector3.forward,    // 前 (0, 0, 1)
        Vector3.back,       // 后 (0, 0, -1)
        Vector3.left,       // 左 (-1, 0, 0)
        Vector3.right       // 右 (1, 0, 0)
    };
    //private readonly Vector3[] directions = {
    //    Vector3.forward + new Vector3(0.1f,0,0),    // 前 (0, 0, 1)
    //    Vector3.back + new Vector3(-0.1f,0,0),       // 后 (0, 0, -1)
    //    Vector3.left + new Vector3(0,0,0.1f),       // 左 (-1, 0, 0)
    //    Vector3.right + new Vector3(0,0,-0.1f)       // 右 (1, 0, 0)
    //};

    /// <summary>
    /// 更新标记 - 防止递归调用
    /// </summary>
    private bool isUpdating = false;

    /// <summary>
    /// 是否是拖动状态
    /// </summary>
    private bool isDragging = false;

    /// <summary>
    /// 延迟更新队列 - 处理批量更新
    /// </summary>
    private static Queue<NeighborAwareEntity> updateQueue = new Queue<NeighborAwareEntity>();
    private static bool isProcessingQueue = false;

    /// <summary>
    /// 连接规则定义 - 定义在特定邻居条件下使用哪个模型
    /// </summary>
    [System.Serializable]
    public class ConnectionRule
    {
        [Tooltip("规则名称，用于识别和调试")]
        public string ruleName;

        [Tooltip("是否要求前方有邻居")]
        public bool requireFront;

        [Tooltip("是否要求后方有邻居")]
        public bool requireBack;

        [Tooltip("是否要求左方有邻居")]
        public bool requireLeft;

        [Tooltip("是否要求右方有邻居")]
        public bool requireRight;

        [Tooltip("匹配此规则时使用的模型在connectionModels中的索引")]
        public int modelIndex;
    }

    /// <summary>
    /// 开始拖动 - 设置拖动状态
    /// </summary>
    public void StartDragging()
    {
        isDragging = true;
        // 保存当前的邻居状态，用于后续检测离开
        SaveCurrentNeighborState();

        // 立即通知所有当前邻居：我要离开了
        NotifyAllNeighborsImmediate(false); // false表示离开

        // 清空自己的邻居关系（因为开始移动了）
        ClearAllNeighbors();

        // 更新自己的外观（变成孤立状态）
        UpdateAppearance();
    }

    /// <summary>
    /// 结束拖动 - 清除拖动状态并进行最终更新
    /// </summary>
    public void StopDragging()
    {
        isDragging = false;
        // 最终更新，确保所有状态正确
        ExternalUpdate();
    }

    /// <summary>
    /// 拖动过程中更新 - 实时检测邻居变化并通知
    /// </summary>
    public void UpdateWhileDragging()
    {
        if (!isDragging) return;
        if (isUpdating) return;

        try
        {
            isUpdating = true;

            // 保存上一次的邻居状态
            SaveCurrentNeighborState();

            // 重新检测邻居
            UpdateNeighborDictionary();

            // 第一步：先通知离开的邻居
            NotifyLeftNeighbors();

            // 第二步：再通知新来的邻居  
            NotifyNewNeighbors();

            // 第三步：更新自己的外观
            UpdateAppearance();

            Debug.Log($"Dragging Update - Previous: {previousNeighborDictionary.Count}, Current: {neighborDictionary.Count}");
        }
        finally
        {
            isUpdating = false;
        }
    }

    /// <summary>
    /// 立即通知所有邻居（用于开始拖动时）
    /// </summary>
    /// <param name="isArriving">true表示到达，false表示离开</param>
    private void NotifyAllNeighborsImmediate(bool isArriving)
    {
        foreach (var neighborPair in previousNeighborDictionary)
        {
            NeighborAwareEntity neighbor = neighborPair.Value;
            if (neighbor != null && !neighbor.isUpdating)
            {
                Vector3 reverseDirection = -neighborPair.Key;

                if (isArriving)
                {
                    // 通知邻居：我来了
                    neighbor.AddNeighbor(reverseDirection, this);
                }
                else
                {
                    // 通知邻居：我走了
                    neighbor.RemoveNeighbor(reverseDirection);
                }

                neighbor.InternalUpdate();
            }
        }
    }

    /// <summary>
    /// 外部更新 - 由外部调用（如放置时），会通知邻居
    /// </summary>
    public void ExternalUpdate()
    {
        if (isUpdating) return;

        try
        {
            isUpdating = true;

            // 保存当前的邻居状态，用于检测离开
            SaveCurrentNeighborState();

            // 更新自己
            UpdateEntityInternal();

            // 通知所有邻居更新（但不继续传播）
            NotifyNeighbors();

            // 检测并通知离开的邻居
            NotifyLeftNeighbors();
        }
        finally
        {
            isUpdating = false;
        }
    }

    /// <summary>
    /// 离开时通知 - 当实体被移除时调用（不是拖动）
    /// </summary>
    public void NotifyOnLeave()
    {
        if (isUpdating) return;

        try
        {
            isUpdating = true;

            // 通知所有当前邻居：我要离开了
            NotifyAllNeighborsImmediate(false);

            // 清空自己的邻居字典
            ClearAllNeighbors();

            // 隐藏当前模型
            if (currentModel != null)
            {
                currentModel.SetActive(false);
                currentModel = null;
            }
        }
        finally
        {
            isUpdating = false;
        }
    }

    /// <summary>
    /// 保存当前邻居状态，用于检测离开
    /// </summary>
    private void SaveCurrentNeighborState()
    {
        previousNeighborDictionary.Clear();
        foreach (var pair in neighborDictionary)
        {
            previousNeighborDictionary[pair.Key] = pair.Value;
        }
    }

    /// <summary>
    /// 通知离开的邻居
    /// </summary>
    private void NotifyLeftNeighbors()
    {
        // 第一步：收集所有离开的邻居
        foreach (var previousPair in previousNeighborDictionary)
        {
            Vector3 direction = previousPair.Key;
            NeighborAwareEntity previousNeighbor = previousPair.Value;

            // 如果这个方向的邻居现在不存在了，且之前的邻居不为空
            if (!neighborDictionary.ContainsKey(direction) && previousNeighbor != null && !previousNeighbor.isUpdating)
            {
                // 通知之前的邻居：我离开了
                Vector3 reverseDirection = -direction;
                previousNeighbor.RemoveNeighbor(reverseDirection);
                previousNeighbor.InternalUpdate();

                Debug.Log($"Notified neighbor LEFT: {previousNeighbor.name} in direction {direction}");
            }
        }
    }

    /// <summary>
    /// 通知新来的邻居
    /// </summary>
    private void NotifyNewNeighbors()
    {
        foreach (var currentPair in neighborDictionary)
        {
            Vector3 direction = currentPair.Key;
            NeighborAwareEntity currentNeighbor = currentPair.Value;

            // 如果这个邻居是新的（在previous中不存在），或者之前的邻居不同
            bool isNewNeighbor = !previousNeighborDictionary.ContainsKey(direction) ||
                                previousNeighborDictionary[direction] != currentNeighbor;

            if (isNewNeighbor && currentNeighbor != null && !currentNeighbor.isUpdating)
            {
                // 确保邻居也知道我的存在（双向关系）
                Vector3 reverseDirection = -direction;
                currentNeighbor.AddNeighbor(reverseDirection, this);

                // 让邻居更新自己
                currentNeighbor.InternalUpdate();

                Debug.Log($"Notified neighbor ARRIVED: {currentNeighbor.name} in direction {direction}");
            }
        }
    }

    /// <summary>
    /// 内部更新 - 由邻居触发，只更新自己，不通知其他人
    /// </summary>
    public void InternalUpdate()
    {
        if (isUpdating) return;

        try
        {
            isUpdating = true;
            UpdateEntityInternal(); // 只更新自己，不通知邻居
        }
        finally
        {
            isUpdating = false;
        }
    }

    /// <summary>
    /// 延迟外部更新 - 将更新加入队列处理，避免性能峰值
    /// </summary>
    public void ExternalUpdateDelayed()
    {
        if (!updateQueue.Contains(this))
        {
            updateQueue.Enqueue(this);
        }

        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessUpdateQueue());
        }
    }

    /// <summary>
    /// 处理更新队列 - 逐帧处理避免性能峰值
    /// </summary>
    private System.Collections.IEnumerator ProcessUpdateQueue()
    {
        isProcessingQueue = true;

        // 每帧处理一定数量的更新，避免卡顿
        int updatesPerFrame = 5;
        int processedThisFrame = 0;

        while (updateQueue.Count > 0)
        {
            NeighborAwareEntity entity = updateQueue.Dequeue();
            if (entity != null)
            {
                entity.ExternalUpdate(); // 使用外部更新，会通知邻居
                processedThisFrame++;
            }

            // 如果这帧处理够了，等待下一帧
            if (processedThisFrame >= updatesPerFrame)
            {
                processedThisFrame = 0;
                yield return null; // 等待下一帧
            }
        }

        isProcessingQueue = false;
    }

    /// <summary>
    /// 更新实体内部逻辑 - 不包含通知邻居的部分
    /// </summary>
    private void UpdateEntityInternal()
    {
        // 如果不是拖动状态，才更新邻居字典（拖动状态在UpdateWhileDragging中已经更新）
        if (!isDragging)
        {
            UpdateNeighborDictionary();
        }

        // 更新外观
        UpdateAppearance();
    }

    /// <summary>
    /// 更新外观显示
    /// </summary>
    private void UpdateAppearance()
    {
        // 隐藏当前显示的模型（如果存在）
        if (currentModel != null)
        {
            currentModel.SetActive(false);
        }

        // 根据邻居字典获取邻居信息
        NeighborInfo neighbors = GetNeighborInfoFromDictionary();

        // 根据连接规则评估应该使用哪个模型
        int modelIndex = EvaluateConnectionRules(neighbors);

        // 激活对应的模型
        if (connectionModels.Length > modelIndex && connectionModels[modelIndex] != null)
        {
            currentModel = connectionModels[modelIndex];
            currentModel.SetActive(true);
        }
    }

    /// <summary>
    /// 通知邻居更新 - 只让直接邻居更新自己，不继续传播
    /// </summary>
    private void NotifyNeighbors()
    {
        foreach (var neighborPair in neighborDictionary)
        {
            NeighborAwareEntity neighbor = neighborPair.Value;
            if (neighbor != null && !neighbor.isUpdating)
            {
                // 确保邻居也知道我的存在（双向关系）
                Vector3 reverseDirection = -neighborPair.Key;
                neighbor.AddNeighbor(reverseDirection, this);

                neighbor.InternalUpdate(); // 使用内部更新，避免传播
            }
        }
    }

    /// <summary>
    /// 立即通知邻居（危险，可能递归，仅用于初始化或简单场景）
    /// </summary>
    public void NotifyNeighborsImmediate()
    {
        List<NeighborAwareEntity> neighborsToUpdate = new List<NeighborAwareEntity>();

        // 先收集邻居，避免在遍历过程中修改字典
        foreach (var neighborPair in neighborDictionary)
        {
            if (neighborPair.Value != null)
            {
                neighborsToUpdate.Add(neighborPair.Value);
            }
        }

        // 然后更新收集到的邻居
        foreach (var neighbor in neighborsToUpdate)
        {
            if (!neighbor.isUpdating)
            {
                neighbor.InternalUpdate(); // 使用内部更新
            }
        }
    }

    /// <summary>
    /// 扫描周围环境并更新邻居字典
    /// </summary>
    private void UpdateNeighborDictionary()
    {
        // 清空当前邻居字典
        neighborDictionary.Clear();

        // 扫描每个方向，检测邻居
        foreach (Vector3 direction in directions)
        {
            NeighborAwareEntity neighbor = FindNeighborInDirection(direction);
            if (neighbor != null)
            {
                neighborDictionary[direction] = neighbor;
            }
        }
    }

    /// <summary>
    /// 在指定方向查找邻居
    /// </summary>
    /// <param name="direction">检测方向</param>
    /// <returns>找到的邻居实体，如果没有则返回null</returns>
    private NeighborAwareEntity FindNeighborInDirection(Vector3 direction)
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, neighborCheckDistance, targetLayer);

        foreach (RaycastHit hit in hits)
        {
            // 如果设置了标签要求，检查标签匹配
            if (string.IsNullOrEmpty(targetTag) || hit.collider.CompareTag(targetTag))
            {
                NeighborAwareEntity entity = hit.collider.GetComponent<NeighborAwareEntity>();
                if (entity != null && entity != this)
                {
                    return entity;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 从邻居字典生成邻居信息
    /// </summary>
    /// <returns>邻居信息结构体</returns>
    private NeighborInfo GetNeighborInfoFromDictionary()
    {
        NeighborInfo info = new NeighborInfo();

        info.hasFront = neighborDictionary.ContainsKey(Vector3.forward);
        info.hasBack = neighborDictionary.ContainsKey(Vector3.back);
        info.hasLeft = neighborDictionary.ContainsKey(Vector3.left);
        info.hasRight = neighborDictionary.ContainsKey(Vector3.right);

        return info;
    }

    /// <summary>
    /// 获取指定方向的邻居
    /// </summary>
    /// <param name="direction">方向</param>
    /// <returns>邻居实体，如果没有则返回null</returns>
    public NeighborAwareEntity GetNeighborInDirection(Vector3 direction)
    {
        neighborDictionary.TryGetValue(direction, out NeighborAwareEntity neighbor);
        return neighbor;
    }

    /// <summary>
    /// 获取所有邻居的列表
    /// </summary>
    /// <returns>邻居实体列表</returns>
    public List<NeighborAwareEntity> GetAllNeighbors()
    {
        return new List<NeighborAwareEntity>(neighborDictionary.Values);
    }

    /// <summary>
    /// 检查指定方向是否有邻居
    /// </summary>
    /// <param name="direction">方向</param>
    /// <returns>是否有邻居</returns>
    public bool HasNeighborInDirection(Vector3 direction)
    {
        return neighborDictionary.ContainsKey(direction);
    }

    /// <summary>
    /// 手动添加邻居（用于特殊情况）
    /// </summary>
    /// <param name="direction">方向</param>
    /// <param name="neighbor">邻居实体</param>
    public void AddNeighbor(Vector3 direction, NeighborAwareEntity neighbor)
    {
        if (neighbor != null && neighbor != this)
        {
            neighborDictionary[direction] = neighbor;
        }
    }

    /// <summary>
    /// 手动移除邻居（用于特殊情况）
    /// </summary>
    /// <param name="direction">方向</param>
    public void RemoveNeighbor(Vector3 direction)
    {
        neighborDictionary.Remove(direction);
    }

    /// <summary>
    /// 清空所有邻居关系（用于实体被销毁时）
    /// </summary>
    public void ClearAllNeighbors()
    {
        neighborDictionary.Clear();
    }

    /// <summary>
    /// 评估连接规则，确定应该使用哪个模型
    /// </summary>
    /// <param name="neighbors">邻居检测结果</param>
    /// <returns>应该使用的模型索引</returns>
    private int EvaluateConnectionRules(NeighborInfo neighbors)
    {
        // 如果没有设置自定义规则，使用默认逻辑
        if (connectionRules == null || connectionRules.Length == 0)
        {
            return GetDefaultModelIndex(neighbors);
        }

        // 按顺序检查所有自定义规则，返回第一个匹配的规则
        foreach (ConnectionRule rule in connectionRules)
        {
            if (CheckRule(rule, neighbors))
            {
                return rule.modelIndex;
            }
        }

        // 没有规则匹配时使用默认逻辑
        return GetDefaultModelIndex(neighbors);
    }

    /// <summary>
    /// 检查单个规则是否匹配当前的邻居状态
    /// </summary>
    /// <param name="rule">要检查的规则</param>
    /// <param name="neighbors">当前的邻居状态</param>
    /// <returns>规则是否匹配</returns>
    private bool CheckRule(ConnectionRule rule, NeighborInfo neighbors)
    {
        // 检查规则要求的每个方向条件是否满足
        if (rule.requireFront && !neighbors.hasFront) return false;
        if (rule.requireBack && !neighbors.hasBack) return false;
        if (rule.requireLeft && !neighbors.hasLeft) return false;
        if (rule.requireRight && !neighbors.hasRight) return false;

        return true;
    }

    /// <summary>
    /// 默认的连接规则逻辑
    /// </summary>
    /// <param name="neighbors">邻居检测结果</param>
    /// <returns>模型索引</returns>
    private int GetDefaultModelIndex(NeighborInfo neighbors)
    {
        bool hasFrontBack = neighbors.hasFront || neighbors.hasBack;  // 前后方向是否有连接
        bool hasLeftRight = neighbors.hasLeft || neighbors.hasRight;  // 左右方向是否有连接

        // 默认规则：
        if (hasFrontBack && !hasLeftRight) return 0;    // 只有前后有连接 - 使用模型0
        if (!hasFrontBack && hasLeftRight) return 1;    // 只有左右有连接 - 使用模型1
        if (hasFrontBack && hasLeftRight) return 2;     // 前后左右都有连接 - 使用模型2

        return 0;                                       // 默认情况（孤立） - 使用模型0
    }

    /// <summary>
    /// 获取当前使用的模型索引（用于调试或UI显示）
    /// </summary>
    /// <returns>当前模型索引，如果没有激活的模型返回-1</returns>
    public int GetCurrentModelIndex()
    {
        if (currentModel == null) return -1;

        for (int i = 0; i < connectionModels.Length; i++)
        {
            if (connectionModels[i] == currentModel)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 强制刷新邻居关系（用于移动实体后）
    /// </summary>
    public void RefreshNeighbors()
    {
        UpdateNeighborDictionary();
    }

    void OnDrawGizmos()
    {
        // 绘制四个方向的检测射线
        Gizmos.color = isDragging ? Color.red : Color.cyan;
        foreach (Vector3 direction in directions)
        {
            Gizmos.DrawRay(transform.position, direction * neighborCheckDistance);
        }

        // 在实体位置绘制半透明球体，便于识别
        Gizmos.color = new Color(0, 1, 1, 0.1f);
        Gizmos.DrawSphere(transform.position, 0.3f);

        // 绘制邻居连接线
        Gizmos.color = Color.yellow;
        foreach (var neighborPair in neighborDictionary)
        {
            if (neighborPair.Value != null)
            {
                Gizmos.DrawLine(transform.position, neighborPair.Value.transform.position);
            }
        }

        // 显示当前状态
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = isDragging ? Color.red : Color.white;
        style.alignment = TextAnchor.MiddleCenter;
        string dragStatus = isDragging ? " (Dragging)" : "";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f,
                                $"Model: {GetCurrentModelIndex()}\nNeighbors: {neighborDictionary.Count}{dragStatus}", style);
#endif
    }

    /// <summary>
    /// 实体被销毁时清理邻居关系
    /// </summary>
    private void OnDestroy()
    {
        NotifyOnLeave(); // 使用统一的离开通知逻辑
    }

    /// <summary>
    /// 启用时自动初始化
    /// </summary>
    private void OnEnable()
    {
        // 延迟一帧更新，确保所有实体都已初始化
        StartCoroutine(DelayedInit());
    }

    private System.Collections.IEnumerator DelayedInit()
    {
        yield return null;
        if (neighborDictionary.Count == 0) // 如果没有初始化过
        {
            ExternalUpdate(); // 初始更新
        }
    }

    /// <summary>
    /// 邻居信息结构 - 存储四个方向的邻居检测结果
    /// </summary>
    public struct NeighborInfo
    {
        public bool hasFront;   // 前方是否有邻居
        public bool hasBack;    // 后方是否有邻居
        public bool hasLeft;    // 左方是否有邻居
        public bool hasRight;   // 右方是否有邻居

        public override string ToString()
        {
            return $"Front: {hasFront}, Back: {hasBack}, Left: {hasLeft}, Right: {hasRight}";
        }
    }
}