using Pathfinding;
using QFramework;
using System.Collections;
using UnityEngine;

public class AStarBaker : MonoSingleton<AStarBaker>
{
    public KeyCode bakeKey = KeyCode.R;
    public float localBakeRadius = 5f;
    public bool autoBakeOnChanges = true;

    public AstarPath astar;
    private bool isBaking = false;

    // 新增：当前图的 graphMask（-1 表示不限制，用所有 Graph）
    private int currentGraphMask = -1;

    void Start()
    {
        astar = AstarPath.active;
    }

    /// <summary>
    /// 切换 Astar 使用的 GridGraph
    /// 正确方式：只更新 graphMask，而不是修改 AstarPath.active
    /// </summary>
    public void ChangeAstarGraph(int index)
    {
        // index = 1 → 想选择第一个 Graph
        int arrIndex = index - 1;

        if (arrIndex < 0 || arrIndex >= astar.graphs.Length)
        {
            Debug.LogError("Graph index out of range: " + index);
            return;
        }

        NavGraph graph = astar.graphs[arrIndex];
        Debug.Log(graph.name);
        // 关键：使用 graph.graphIndex，不是 arrIndex
        int realIndex = (int)graph.graphIndex;

        currentGraphMask = 1 << realIndex;

        Debug.Log($"切换图: 输入 {index}, 数组下标 {arrIndex}, Graph.graphIndex={realIndex}, mask={currentGraphMask}");
    }

    void Update()
    {
        // 按键烘焙
        if (Input.GetKeyDown(bakeKey) && !isBaking)
        {
            StartCoroutine(BakeRoutine());
        }

        // 鼠标右键局部烘焙
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                LocalBake(hit.point, localBakeRadius);
            }
        }
    }

    public void DoLocalBake(Vector3 _point, float radius)
    {
        LocalBake(_point, radius);
    }

    IEnumerator BakeRoutine()
    {
        isBaking = true;
        Debug.Log("开始动态烘焙...");

        yield return new WaitForEndOfFrame();

        // 如果有当前激活 graph → 单独扫描该 graph
        if (currentGraphMask >= 0)
        {
            int index = Mathf.RoundToInt(Mathf.Log(currentGraphMask, 2));
            var gg = astar.graphs[index] as GridGraph;

            if (gg != null)
            {
                gg.Scan(); // 只扫描当前图
                Debug.Log($"已扫描 Graph {index}");
            }
            else
            {
                // 如果不是 GridGraph，退回全局扫描
                astar.Scan();
            }
        }
        else
        {
            // 默认：全局扫描所有图
            astar.Scan();
        }

        yield return new WaitForEndOfFrame();

        isBaking = false;
        Debug.Log("动态烘焙完成！");
    }

    // （你的周期检测保持不动）
    IEnumerator PeriodicBakeCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);

            if (HasDynamicObstaclesMoved())
            {
                Debug.Log("检测到障碍物移动，执行局部烘焙...");
                BakeAroundMovingObstacles();
            }
        }
    }

    bool HasDynamicObstaclesMoved()
    {
        return false;
    }

    void BakeAroundMovingObstacles()
    {
        GameObject[] movingObstacles = GameObject.FindGameObjectsWithTag("DynamicObstacle");
        foreach (GameObject obstacle in movingObstacles)
        {
            LocalBake(obstacle.transform.position, 3f);
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 210, 200, 200, 150));

        GUILayout.Label("A* 动态烘焙控制");

        if (GUILayout.Button("全局扫描", GUILayout.Height(30)))
        {
            StartCoroutine(BakeRoutine());
        }

        if (GUILayout.Button("鼠标位置局部烘焙", GUILayout.Height(30)))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                LocalBake(hit.point, localBakeRadius);
            }
        }

        GUILayout.EndArea();
    }

    void FullBake()
    {
        if (astar != null)
        {
            Debug.Log("开始全局烘焙...");
            astar.Scan();
            Debug.Log("全局烘焙完成！");
        }
    }

    /// <summary>
    /// 局部烘焙只更新当前激活的 Graph
    /// </summary>
    void LocalBake(Vector3 center, float radius)
    {
        if (astar == null) return;

        GraphUpdateObject guo = new GraphUpdateObject();
        guo.bounds = new Bounds(center, new Vector3(radius * 2, radius * 2, radius * 2));

        guo.updatePhysics = true;
        guo.resetPenaltyOnPhysics = true;

        // 正确：指定更新哪个 Graph，用 graphMask 而不是 nnConstraint
        if (currentGraphMask >= 0)
        {
            guo.graphMask = new GraphMask((uint)currentGraphMask);
        }

        astar.UpdateGraphs(guo);

        Debug.Log("局部烘焙完成！");
    }
}
