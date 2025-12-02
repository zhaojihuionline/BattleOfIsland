using Cysharp.Threading.Tasks;
using Google.Protobuf.Collections;
using PitayaClient.Network.Manager;
using PitayaGame.GameSvr;
using QFramework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
/// <summary>
/// 军队数据
/// </summary>
public struct ArmyData
{
    /// <summary>
    /// 军队id
    /// </summary>
    public int ArmyID;
    /// <summary>
    /// 军队名称
    /// </summary>
    public string ArmyName;
    /// <summary>
    /// 已拥有的英雄数据列表
    /// </summary>
    public List<HeroData> heroDatas;

    public ArmyData(int armyID, string armyName, List<HeroData> heroDatas)
    {
        ArmyID = armyID;
        ArmyName = armyName;
        this.heroDatas = heroDatas;
    }
}
[System.Serializable]
public struct ResourceData
{
    /// <summary>
    /// 资源id
    /// </summary>
    public int ResourceID;
    /// <summary>
    /// 资源名称
    /// </summary>
    public string ResourceName;
    /// <summary>
    /// 资源数量
    /// </summary>
    public int Quantity;
}
[System.Serializable]
/// <summary>
/// 玩家数据(客户端用)
/// </summary>
public class PlayerData
{
    public string PlayerID;
    public string PlayerName;
    /// <summary>
    /// 总佣兵点数(暂时)
    /// </summary>
    public int TotalCostMercenaryPoints;
    /// <summary>
    /// 资源数据
    /// </summary>
    public List<ResourceData> resourceData;
    /// <summary>
    /// 军队配置列表，一个玩家可以拥有多个军队。获得方式：在局外配置
    /// </summary>
    public List<ArmyData> armyDatas;
}
public class Player
{
    public PlayerData playerData;
    public static int DefaultArmyID;
    public static int DefaultMagicID;
    /// <summary>
    /// 当前选中的军队ID
    /// </summary>
    public int CurrentSelectedArmyID { get; set; }
    /// <summary>
    /// 当前选中的法术ID
    /// </summary>
    public int CurrentSelectedMagicID { get; set; }
    public async void GetUserInfoFromRemote(UnityAction<UserBasicInfo> callBack)
    {
        var getuserReq = new GetUserInfoRequest();
        var res_user = await NetworkManager.Instance.RequestAsync<GetUserInfoResponse>(
            "gamesvr.user.getuserinfo", getuserReq, 10f);

        if (res_user != null)
        {
            Debug.Log("金币: " + res_user.UserInfo.Coin);
            callBack?.Invoke(res_user.UserInfo);
        }
        else
        {
            string errorMsg = res_user?.Resp?.Message ?? "未知错误";
            Debug.LogError(errorMsg);
        }
    }
    private void SetPlayerData(UserBasicInfo testInfo = null)
    {
        if(testInfo != null)
        {
            playerData = new PlayerData();
            playerData.PlayerID = testInfo.UserId;
            playerData.PlayerName = testInfo.Username;
        }
        else
        {
            playerData = new PlayerData();
            playerData.PlayerID = "user_001";
            playerData.PlayerName = "ceshi_user";
        }
    }

    public void Init(ArmyDetailInfo armyDetailInfo,UserBasicInfo testInfo = null)
    {
        DefaultArmyID = 0;
        SetPlayerData(testInfo);
        // 初始化资源数据，服务器未连接时使用默认数据
        List<ResourceData> resourceDatas = new List<ResourceData>();
        if (testInfo != null)
        {
            resourceDatas.Add(new ResourceData { ResourceID = 1, ResourceName = "Gold", Quantity = (int)testInfo.Coin });
            resourceDatas.Add(new ResourceData { ResourceID = 2, ResourceName = "Stone", Quantity = (int)testInfo.Stone });
            resourceDatas.Add(new ResourceData { ResourceID = 3, ResourceName = "Meat", Quantity = (int)testInfo.Meat });
        }
        else
        {
            resourceDatas.Add(new ResourceData { ResourceID = 1, ResourceName = "Gold", Quantity = 333 });
            resourceDatas.Add(new ResourceData { ResourceID = 2, ResourceName = "Stone", Quantity = 666 });
            resourceDatas.Add(new ResourceData { ResourceID = 3, ResourceName = "Meat", Quantity = 999 });
        }
        playerData.resourceData = resourceDatas;

        // 初始化军队数据，服务器未连接时使用默认数据
        List<HeroData> heroDatas = new List<HeroData>();
        // 初始化佣兵数据，服务器未连接时使用默认数据
        List<MercenaryData> soldierDatas = new List<MercenaryData>();

        if (armyDetailInfo == null)
        {
            playerData.armyDatas = new List<ArmyData>() {
                new ArmyData(armyID: 1, armyName: playerData.PlayerName + "RedArmy", heroDatas: null),
            };
            playerData.TotalCostMercenaryPoints = 100;// 默认100点佣兵点数
        }
        else
        {
            // 从服务器获取到军队英雄详细信息，进行初始化
            for (int i = 0; i < armyDetailInfo.DeployedHeroes.Count; i++)
            {
                var heroInfoPayload = armyDetailInfo.DeployedHeroes[i];
                HeroData heroData = new HeroData(heroInfoPayload);
                heroDatas.Add(heroData);
            }
            // 初始化法术数据略...后面再写
            playerData.armyDatas = new List<ArmyData>() {
                new ArmyData(
                    armyID: (int)armyDetailInfo.ArmyId,
                    armyName: armyDetailInfo.ArmyName,
                    heroDatas: heroDatas
                    ),
            };
            playerData.TotalCostMercenaryPoints = 100;// 默认100点佣兵点数
        }
    }

    /// <summary>
    /// 招募英雄
    /// </summary>
    /// <returns></returns>
    public async UniTask<long> AddHero()
    {
        //var newId = await this.SendCommand(new RecruitCommand(1202, "艾菲尔"));
        // var newId = await GameRemoteAPI.RecruitHero(1202, "艾菲尔");// 招募一个英雄测试
        var newId = await GameRemoteAPI.RecruitHero(1201, "艾利佛");// 招募一个英雄测试
        return newId;
    }

    /// <summary>
    /// 部署英雄
    /// </summary>
    /// <param name="armid">部署到的军队Id</param>
    /// <param name="newRecruitHeroId">要部署的英雄id</param>
    public async void DeployHero(long armid,long newRecruitHeroId)
    {
        await GameRemoteAPI.DeployHero(armid, newRecruitHeroId);// 部署英雄测试
    }
}


public class PlayerOpponentData
{
    public string playerName;
    public int WoodCount;	
    public int StoneCount;
    public int MeatCount;
}