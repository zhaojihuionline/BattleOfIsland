using Pathfinding;
using QFramework;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class AStarBaker : MonoSingleton<AStarBaker>
{
    public KeyCode bakeKey = KeyCode.R;
    public float localBakeRadius = 5f;
    public bool autoBakeOnChanges = true;

    private AstarPath astar;
    private bool isBaking = false;

    void Start()
    {
        astar = AstarPath.active;

        // // 监听动态障碍物变化
        // if (autoBakeOnChanges)
        // {
        //     StartCoroutine(PeriodicBakeCheck());
        // }
    }

    void Update()
    {
        // 按键烘焙
        if (Input.GetKeyDown(bakeKey) && !isBaking)
        {
            StartCoroutine(BakeRoutine());
        }

        // 鼠标点击位置局部烘焙
        if (Input.GetMouseButtonDown(1)) // 右键
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                LocalBake(hit.point, localBakeRadius);
            }
        }
    }

    public void DoLocalBake(Vector3 _point,float radius)
    {
        LocalBake(_point, radius);
    }


    IEnumerator BakeRoutine()
    {
        isBaking = true;
        Debug.Log("开始动态烘焙...");

        yield return new WaitForEndOfFrame();

        // 执行烘焙
        astar.Scan();

        yield return new WaitForEndOfFrame();

        isBaking = false;
        Debug.Log("动态烘焙完成！");
    }

    IEnumerator PeriodicBakeCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f); // 每2秒检查一次

            // 检查是否有动态障碍物移动
            if (HasDynamicObstaclesMoved())
            {
                Debug.Log("检测到障碍物移动，执行局部烘焙...");
                // 在移动的障碍物位置进行局部烘焙
                BakeAroundMovingObstacles();
            }
        }
    }

    bool HasDynamicObstaclesMoved()
    {
        // 实现你的障碍物移动检测逻辑
        return false;
    }

    void BakeAroundMovingObstacles()
    {
        // 在移动的障碍物周围进行局部烘焙
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
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
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

            // 方法1：完全重新扫描
            astar.Scan();

            // 方法2：如果有数据，可以先清除再扫描
            // astar.data.ClearGraphs();
            // astar.Scan();

            Debug.Log("全局烘焙完成！");
        }
    }

    void LocalBake(Vector3 center, float radius)
    {
        if (astar == null) return;

        //Debug.Log($"在位置 {center} 进行局部烘焙，半径: {radius}");

        // 方法1：使用GraphUpdateScene（适合预定义区域）
        GraphUpdateObject guo = new GraphUpdateObject();
        guo.bounds = new Bounds(center, new Vector3(radius * 2, radius * 2, radius * 2));

        // 设置更新参数
        guo.updatePhysics = true;    // 更新物理碰撞
        guo.resetPenaltyOnPhysics = true; // 重置代价

        // 应用局部更新
        astar.UpdateGraphs(guo);

        Debug.Log("局部烘焙完成！");
    }
}