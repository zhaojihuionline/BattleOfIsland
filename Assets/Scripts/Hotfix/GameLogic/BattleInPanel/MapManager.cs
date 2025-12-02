using cfg;
using Codice.CM.Common;
using Cysharp.Threading.Tasks;
using log4net.Core;
using PitayaClient.Network.Manager;
using PitayaGame.GameSvr;
using QFramework;
using QFramework.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEditor.PlayerSettings;
/// <summary>
/// 地图管理器
/// </summary>
public class MapManager : MonoBehaviour
{
    public static MapManager instance;
    [Header("当前摆放的所有物体")]
    public List<GameObject> objectsToPlace;
    public Vector2 areaSize = new Vector2(20, 20);
    public float minDistance = 3f;
    public int maxAttempts = 100;

    public int TotalPlacedObjects = 0;

    private List<Vector3> placedPositions = new List<Vector3>();
    private void Awake()
    {
        instance = this;
        TotalPlacedObjects = objectsToPlace.Count;
    }
    void GetLocalBuidingsPosAndType(int bid, out Vector3 pos, out int btype)
    {
        pos = Vector3.zero;
        btype = 0;
        if (bid == 14001)
        {
            pos = new Vector3(0, -0.29f, 2.27f);
            btype = 3;
        }
        else if (bid == 12001)
        {
            pos = new Vector3(7.13f, -0.29f, 0.75f);
            btype = 4;
        }
        else if (bid == 12002)
        {
            pos = new Vector3(-4.25f, -0.29f, -0.73f);
            btype = 4;
        }
        else if (bid == 12003)
        {
            pos = new Vector3(3.5f, -0.29f, -2.48f);
            btype = 4;
        }
        else if (bid == 13001)
        {
            pos = new Vector3(8.25f, -0.29f, -4.23f);
            btype = 2;
        }
        else
        {
            pos = Vector3.zero;
            btype = 0;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="bids"></param>
    public async void RebuildBuildingsUseLocalData(bool isUpload = false,params int[] bids)
    {
        BattleManagerView.Instance.RemoveAllOpponent_allEntitys();
        for (int i = 0; i < bids.Length; i++)
        {
            //var newBuildings = Instantiate(Resources.Load<GameObject>("runtime/Buildings/" + bids[i]));// 涓存椂鍔犺浇鍐欐硶
            //GetLocalBuidingsPosAndType(bids[i], out Vector3 _pos, out int btype);
            //newBuildings.GetComponent<BuildEntity>().SetBuildData(1, btype).Init();
            //BattleManagerView.Instance.battleInPanel.SetAllEntitys(newBuildings);
            //objectsToPlace.Add(newBuildings);
            Vector3 p = GetNonOverlappingPosition();
            BattleManagerView.Instance.BuildBuildingsEntity(bids[i], 1, p, true);
            if (isUpload)
            {
                await GameRemoteAPI.ConstructBuilding(bids[i], p.x, p.z);
            }
        }
        TotalPlacedObjects = bids.Length;
    }
    public void RebuildBuildings(BuildingLayoutData layoutData)
    {
        BattleManagerView.Instance.RemoveAllOpponent_allEntitys();
        for (int i = 0; i < layoutData.buildings.Length; i++)
        {
            BuildingData buildingData = layoutData.buildings[i];
            //Debug.Log(buildingData.config_id);
            //if (buildingData.config_id != 11001)
            //{
            //    //var newBuildings = Instantiate(Resources.Load<GameObject>("runtime/Buildings/" + buildingData.config_id));
            //    //newBuildings.GetComponent<BuildEntity>().SetBuildData(buildingData.level, buildingData.building_type).Init();
            //    //newBuildings.transform.position = new Vector3(buildingData.x, 0, buildingData.z);
            //    //BattleManagerView.Instance.battleInPanel.SetAllEntitys(newBuildings);
            //    //objectsToPlace.Add(newBuildings);
                BattleManagerView.Instance.BuildBuildingsEntity(buildingData.config_id, buildingData.level, new Vector3(buildingData.x, 0, buildingData.z), true);
            //}
        }
        TotalPlacedObjects = layoutData.buildings.Length;
    }

    public async void Init()
    {
        var myBuildings = await GameRemoteAPI.GetMyBuilds();
        if (myBuildings.Count <= 0 || myBuildings == null)
        {
            RebuildBuildingsUseLocalData(true,14001, 14001, 12001, 12001, 12001, 12002, 12003, 13001);
        }
    }
    [ContextMenu("摧毁我的所有建筑")]
    public async void ClearAllBuildingsOnServer()
    {
        if (NetworkManager.Instance)
        {
            // 先获取当前账户的所有建筑数据
            if (NetworkManager.Instance.IsConnected)
            {
                var myBuildings = await GameRemoteAPI.GetMyBuilds();
                if(myBuildings.Count > 0)
                {
                    var tasks = new List<UniTask>();
                    for (int i = 0; i < myBuildings.Count; i++)
                    {
                        var buildingData = myBuildings[i];
                        tasks.Add(GameRemoteAPI.DestroyBuilding(buildingData.BuildId));
                    }
                    await UniTask.WhenAll(tasks);
                    Debug.Log("摧毁所有建筑成功");
                }
            }
            else
            {
                Debug.LogError("清除建筑数据失败，当前未连接服务器或未登录，请从Launcher场景走正常登录流程后再上传");
            }
        }
        else
        {
            Debug.LogError("操作失败，请从Launcher场景走正常登录流程后再上传");
        }
    }
    [ContextMenu("上传建筑数据到存档")]
    public async void UploadBuildingsToServer()
    {
        objectsToPlace = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            objectsToPlace.Add(transform.GetChild(i).gameObject);
        }
        if (objectsToPlace.Count > 0)
        {
            var tasks = new List<UniTask>();

            foreach (var obj in objectsToPlace)
            {
                if (obj.gameObject.activeInHierarchy)
                {
                    // 使用正则取出建筑ID连续的数字部分
                    Regex regex = new Regex(@"\d+");
                    Match m = Regex.Match(obj.name, @"\d+");
                    //string[] objNameArray = obj.name.Split('_');
                    //string objStr = obj.name.Split('_')[2];
                    //bool success = int.TryParse(objStr, out var bid);
                    //if(!success)
                    //{
                    //    Debug.LogError($"上传建筑数据失败，建筑ID解析错误: {objStr}， 请重新更正名字，例如去除（Clone）、 (1)(2)等字符");
                    //    continue;
                    //}
                    int _targetBid = (int.Parse(m.Value) - 1) / 100;
                    Debug.Log($"最终取出的建筑ID为: {_targetBid}，原始名字为：{obj.name}，未计算前的值为:{m.Value}\n如这里打印的ID不正确，请联系程序员");
                    if (NetworkManager.Instance)
                    {
                        if (NetworkManager.Instance.IsConnected)
                        {
                            tasks.Add(GameRemoteAPI.ConstructBuilding(_targetBid, obj.transform.position.x, obj.transform.position.z));
                        }
                        else
                        {
                            Debug.LogError("上传建筑数据失败，当前未连接服务器或未登录，请从Launcher场景走正常登录流程后再上传");
                        }
                    }
                    else
                    {
                        Debug.LogError("操作失败，请从Launcher场景走正常登录流程后再上传");
                    }
                }
            }
            await UniTask.WhenAll(tasks);
        }
    }
    public void PlaceObjects(List<int> buildsDatas)
    {
        // 鏍规嵁buildsDatas涓殑鏁版嵁鐢熸垚骞舵憜鏀惧缓绛戠墿
        foreach (var obj in objectsToPlace)
        {
            Vector3 pos = GetNonOverlappingPosition();
            obj.transform.position = pos;
            placedPositions.Add(pos);
        }
    }

    public Vector3 GetNonOverlappingPosition()
    {
        int attempts = 0;
        Vector3 candidate = Vector3.zero;

        while (attempts < maxAttempts)
        {
            attempts++;

            candidate = new Vector3(
                Random.Range(-areaSize.x / 2, areaSize.x / 2),
                0f,
                Random.Range(-areaSize.y / 2, areaSize.y / 2)
            );

            bool overlap = false;
            foreach (var p in placedPositions)
            {
                if (Vector3.Distance(p, candidate) < minDistance)
                {
                    overlap = true;
                    break;
                }
            }

            if (!overlap)
                return candidate;
        }

        return candidate;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(areaSize.x, 0.1f, areaSize.y));
    }

    public float CalculateDestructionRate()
    {
        return 1.0f - (objectsToPlace.Count / (float)TotalPlacedObjects);
    }
}
