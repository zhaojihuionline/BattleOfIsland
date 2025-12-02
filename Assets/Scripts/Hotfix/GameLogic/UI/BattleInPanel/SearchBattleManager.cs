using DG.Tweening;
using Newtonsoft.Json;
using PitayaGame.GameSvr;
using QFramework;
using QFramework.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 寻找战斗管理器
/// </summary>
public class SearchBattleManager : MonoBehaviour
{
    public Button btn_start;// 开始战斗按钮
    public Button btn_seachNextOne;// 寻找下一个目标按钮

    public TMP_Text searchDurationText;// 寻找持续时间文本显示


    public int CurSearchIndex = -1;

    bool canStartSearchTimer = false;
    public const float searchDuration = 30.0f;// 寻找对手的持续时间
    float curSearchTimer = 0.0f;// 当前寻找时间计时器
    private void Start()
    {
        btn_start.onClick.AddListener(OnStartBattle);
        btn_seachNextOne.onClick.AddListener(OnSearchNextOne);
        canStartSearchTimer = true;
    }
    public void ResetSearch()
    {
        canStartSearchTimer = true;
        curSearchTimer = 0.0f;
        searchDurationText.text = $"{Mathf.FloorToInt(searchDuration)} s";

        BattleManagerView.Instance.battleInPanel.ClearAllEntitys();
        //BattleManagerView.Instance.RemoveGlobalBuffs();
        OnSearchNextOne();
    }
    private void Update()
    {
        if (canStartSearchTimer)
        {
            curSearchTimer += Time.deltaTime;
            if (curSearchTimer >= searchDuration)
            {
                // 超过寻找时间，自动开始战斗
                OnStartBattle();
                canStartSearchTimer = false;
            }
            searchDurationText.text = $"{Mathf.FloorToInt(searchDuration - curSearchTimer)} s";
        }
    }
    public async void OnSearchNextOne()
    {
        var res = await GameRemoteAPI.MatchRequest();// 请求匹配下一个对手
        //btn_seachNextOne.interactable = false;
        //btn_seachNextOne.GetComponentInChildren<TMP_Text>().text = "你操作太快了......";
        if (res.Candidate == null)
        {
            Debug.Log("没有匹配到对手");
            //if (BattleManagerView.Instance.battleInPanel.battleModel.player_allEntitys.Count <= 0)
            //{
            //    MapManager.instance.RebuildBuildingsUseLocalData(14001);
            //}
            //MapManager.instance.RebuildBuildingsUseLocalData(false, 14001);
            // 如果没有匹配到对手，应该继续匹配

            //await Task.Delay(1000);
            //btn_seachNextOne.interactable = true;
            return;
        }

        string jsonStr = res.Candidate.BaseSnapshot.LayoutData.ToStringUtf8();
        Debug.Log($"匹配到的对手建筑布局:{jsonStr}");
        try
        {
            
            // 使用 JsonConvert.DeserializeObject 解析 JSON 数据
            if (PlayerManager.Instance != null && PlayerManager.Instance.GetLocalPlayer() != null && PlayerManager.Instance.GetLocalPlayer().playerData != null) {     
                PlayerManager.Instance.GetLocalPlayer().playerData.TotalCostMercenaryPoints = 100;// 将默认的佣兵点数设置为100，以便测试
            }
            if (BattleManagerView.Instance.battleInPanel != null)
            {
                var newPdata = new PlayerOpponentData();
                newPdata.playerName = res.Candidate.DefenderUsername;
                newPdata.WoodCount = 111;
                newPdata.MeatCount = 111;
                newPdata.StoneCount = 111;
                BattleManagerView.Instance.battleInPanel.ActorInfoViewContainersearch.OpponentInfoContainer.Init(newPdata);
            }

            BuildingLayoutData layoutData = JsonConvert.DeserializeObject<BuildingLayoutData>(jsonStr);
            if (layoutData != null && layoutData.buildings != null)
            {
                MapManager.instance.RebuildBuildings(layoutData);
            }
            else
            {
                Debug.Log("解析后的数据为空或 buildings 数据为空");
            }
        }
        catch (JsonException ex)
        {
            Debug.LogError($"JSON 解析失败: {ex.Message}");
        }
        //await Task.Delay(25000);
        //btn_seachNextOne.interactable = true;
    }
    //CurSearchIndex = BattleManager.Instance.battleInPanel.SearchNewOpponent();
    void OnStartBattle()
    {

        canStartSearchTimer = false;
        curSearchTimer = 0.0f;
        GetComponent<CanvasGroup>().DOFade(0f, 0.5f).OnComplete(OnFinishAnimation);
    }

    private void OnFinishAnimation()
    {
        if (CurSearchIndex == -1)
        {
            CurSearchIndex = 0;// 这里是对手玩家数据，暂时默认为0
        }

        BattleManagerView.Instance.battleInPanel.inBattle.gameObject.SetActive(true);
        BattleManagerView.Instance.battleInPanel.inBattle.GetComponent<CanvasGroup>().alpha = 1.0f;

        BattleManagerView.Instance.battleInPanel.GotoBattle(CurSearchIndex);
        gameObject.SetActive(false);
    }
}
