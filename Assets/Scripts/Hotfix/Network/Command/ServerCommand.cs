using cfg;
using Cysharp.Threading.Tasks;
using Google.Protobuf.Collections;
using PitayaClient.Network.Manager;
using PitayaGame.GameSvr;
using PitayaGame.MatchmakingSvr;
using PitayaGame.MatchMakingSvr;
using QFramework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 获取玩家数据信息Command
/// </summary>
public class GetUserInfoCommand : AbstractCommand<UniTask<UserBasicInfo>>
{
    protected override async UniTask<UserBasicInfo> OnExecute()
    {
        var getuserReq = new GetUserInfoRequest();
        var res_user = await NetworkManager.Instance.RequestAsync<GetUserInfoResponse>(
            "gamesvr.user.getuserinfo",
            getuserReq,
            10f
        );

        if (res_user != null && res_user.UserInfo != null)
        {
            Debug.Log("金币: " + res_user.UserInfo.Coin);
            return res_user.UserInfo;
        }
        else
        {
            string errorMsg = res_user?.Resp?.Message ?? "未知错误";
            Debug.LogError($"获取用户信息失败: {errorMsg}");
            return null;
        }
    }
}


#region 英雄相关Commands
/// <summary>
/// 获取所有英雄数据Command
/// </summary>
public class GetHerosInfoCommand : AbstractCommand<UniTask<RepeatedField<PitayaGame.GameSvr.HeroData>>>
{
    protected override async UniTask<RepeatedField<PitayaGame.GameSvr.HeroData>> OnExecute()
    {
        var getheroReq = new GetMyHeroesRequest();
        var res_hero = await NetworkManager.Instance.RequestAsync<GetMyHeroesResponse>(
            "gamesvr.hero.getmyheroes",
            getheroReq,
            10f
        );
        Debug.Log("当前我的英雄数:" + res_hero.Heroes.Count);
        if (res_hero != null && res_hero.Heroes.Count > 0)
        {
            return res_hero.Heroes;
        }
        else
        {
            string errorMsg = res_hero?.Resp?.Message ?? "未知错误";
            Debug.LogError($"获取用户信息失败: {errorMsg}");
            return null;
        }
    }
}

/// <summary>
/// 获取单个英雄数据Command
/// </summary>
public class GetHeroInfoCommand : AbstractCommand<UniTask<PitayaGame.GameSvr.HeroData>>
{
    public long heroId { get; private set; }

    public GetHeroInfoCommand(long heroId)
    {
        this.heroId = heroId;
    }
    protected override async UniTask<PitayaGame.GameSvr.HeroData> OnExecute()
    {
        var getHeroInfo = new GetHeroInfoRequest();
        getHeroInfo.HeroId = heroId; //639754323634425856;
        var res_hero = await NetworkManager.Instance.RequestAsync<GetHeroInfoResponse>(
            "gamesvr.hero.getheroinfo",
            getHeroInfo,
            10f
        );

        if (res_hero != null && res_hero.HeroData != null)
        {
            return res_hero.HeroData;
        }
        else
        {
            string errorMsg = res_hero?.Resp?.Message ?? "未知错误";
            Debug.LogError($"获取英雄信息失败: {errorMsg}");
            return null;
        }
    }
}

/// <summary>
/// 招募英雄Command
/// </summary>
public class RecruitCommand : AbstractCommand<UniTask<long>>
{
    public int heroModelID { get; private set; }
    public string heroName { get; private set; }

    public RecruitCommand(int heromodelId,string heroName)
    {
        this.heroModelID = heromodelId;
        this.heroName = heroName;
    }
    protected override async UniTask<long> OnExecute()
    {
        var recruitHero_req = new RecruitHeroRequest();
        recruitHero_req.HeroConfigId = heroModelID;
        recruitHero_req.HeroName = heroName;
        var res_recruitHero = await NetworkManager.Instance.RequestAsync<RecruitHeroResponse>(
            "gamesvr.hero.recruithero",
            recruitHero_req,
            10f
        );

        if (res_recruitHero != null)
        {
            return res_recruitHero.HeroId;
        }
        else
        {
            string errorMsg = res_recruitHero?.Resp?.Message ?? "未知错误";
            Debug.LogError($"获取英雄信息失败: {errorMsg}");
            return -1;
        }
    }
}
#endregion

/// <summary>
/// 获取军队简易信息Command
/// </summary>
public class GetArmyInfoCommand : AbstractCommand<UniTask<RepeatedField<PitayaGame.GameSvr.ArmyBriefInfo>>>
{
    protected override async UniTask<RepeatedField<PitayaGame.GameSvr.ArmyBriefInfo>> OnExecute()
    {
        var getArmyReq = new GetArmyListRequest();
        var res_army = await NetworkManager.Instance.RequestAsync<GetArmyListResponse>(
            "gamesvr.army.getarmylist",
            getArmyReq,
            10f
        );
        if (res_army != null && res_army.Resp != null)
        {
            Debug.Log("当前我的军队数:" + res_army.Armies.Count);
            return res_army.Armies;
        }
        else
        {
            string errorMsg = res_army?.Resp?.Message ?? "未知错误";
            Debug.LogError($"获取军队信息失败: {errorMsg}");
            return null;
        }
    }
}

/// <summary>
/// 获取军队详情Command
/// </summary>
public class GetArmyDetailCommand : AbstractCommand<UniTask<PitayaGame.GameSvr.ArmyDetailInfo>>
{
    public long armyId { get; private set; }
    public GetArmyDetailCommand(long armyId)
    {
        this.armyId = armyId;
    }
    protected override async UniTask<PitayaGame.GameSvr.ArmyDetailInfo> OnExecute()
    {
        var getArmyDetailReq = new GetArmyDetailRequest();
        getArmyDetailReq.ArmyId = armyId;
        var res_army_detail = await NetworkManager.Instance.RequestAsync<GetArmyDetailResponse>(
            "gamesvr.army.getarmydetail",
            getArmyDetailReq,
            10f
        );
        if (res_army_detail != null && res_army_detail.Resp != null)
        {
            Debug.Log($"获取军队详情成功: {res_army_detail.Resp}");
            return res_army_detail.ArmyDetail;
        }
        else
        {
            string errorMsg = res_army_detail?.Resp?.Message ?? "未知错误";
            Debug.LogError($"获取军队详情失败: {errorMsg}");
            return null;
        }
    }
}


public static class GameRemoteAPI {
    /// <summary>
    /// 创建军队
    /// </summary>
    /// <param name="armyName"></param>
    public static async UniTask<int> CreateArmy(string armyName)
    {
        // 发送创建军队请求到服务器
        var createArmyReq = new CreateArmyRequest
        {
            ArmyName = armyName
        };
        var res_army_create = await NetworkManager.Instance.RequestAsync<CreateArmyResponse>(
            "gamesvr.army.createarmy",
            createArmyReq,
            10f
            );
        if (res_army_create != null)
        {
            Debug.Log("创建军队成功，军队ID: " + res_army_create.ArmyId);
            // 这里可以更新本地玩家的军队数据
            return (int)res_army_create.ArmyId;
        }
        else
        {
            string errorMsg = res_army_create.Resp?.Message ?? "未知错误";
            Debug.LogError("创建军队失败: " + errorMsg);
            return -999;
        }
    }
    /// <summary>
    /// 招募一个英雄
    /// </summary>
    /// <param name="heroConfigId"></param>
    /// <param name="heroName"></param>
    /// <returns></returns>
    public static async UniTask<long> RecruitHero(int heroConfigId, string heroName)
    {
        var recruitHeroReq = new RecruitHeroRequest
        {
            HeroConfigId = heroConfigId,
            HeroName = heroName
        };
        var res_recruitHero = await NetworkManager.Instance.RequestAsync<RecruitHeroResponse>(
            "gamesvr.hero.recruithero",
            recruitHeroReq,
            10f
        );
        if (res_recruitHero != null)
        {
            Debug.Log("英雄招募成功，英雄ID: " + res_recruitHero.HeroId);
            return res_recruitHero.HeroId;
        }
        else
        {
            string errorMsg = res_recruitHero.Resp?.Message ?? "未知错误";
            Debug.LogError("英雄招募失败: " + errorMsg);
            return -1;
        }
    }
    /// <summary>
    /// 部署英雄到军队
    /// </summary>
    /// <param name="armid"></param>
    /// <param name="heroid"></param>
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    public static async UniTask DeployHero(long armid,long heroid,int slotIndex = 0)
    {
        var deployHeroReq = new DeployHeroRequest
        {
            ArmyId = armid,
            HeroId = heroid,
            SlotIndex = slotIndex
        };
        var res_deployHero = await NetworkManager.Instance.RequestAsync<DeployHeroResponse>(
            "gamesvr.army.deployhero",
            deployHeroReq,
            10f
        );
        if (res_deployHero != null)
        {
            Debug.Log("英雄部署成功 " + res_deployHero.Resp);
        }
        else
        {
            string errorMsg = res_deployHero.Resp?.Message ?? "未知错误";
            Debug.LogError("英雄部署失败: " + errorMsg);
        }
    }
    /// <summary>
    /// 获取我的所有佣兵
    /// </summary>
    /// <returns></returns>
    public static async UniTask<RepeatedField<PitayaGame.GameSvr.MercenaryData>> GetMyMercenaries()
    {
        var getMyMercenariesReq = new GetMyMercenariesRequest();
        var res_getMyMercenaries = await NetworkManager.Instance.RequestAsync<GetMyMercenariesResponse>(
            "gamesvr.mercenary.getmymercenaries",
            getMyMercenariesReq,
            10f
        );
        if (res_getMyMercenaries != null)
        {
            Debug.Log("获取佣兵成功 " + res_getMyMercenaries.Resp);
            return res_getMyMercenaries.Mercenaries;
        }
        else
        {
            string errorMsg = res_getMyMercenaries.Resp?.Message ?? "未知错误";
            Debug.LogError("获取佣兵失败: " + errorMsg);
            return null;
        }
    }
    //训练佣兵 
    public static async UniTask<TrainMercenaryResponse> TrainMercenary(int mercenaryId,int quantity = 3)
    {
        var deploySoliersReq = new TrainMercenaryRequest
        {
            MercenaryConfigId = mercenaryId,
            Quantity = quantity
        };
        var res_deploySoldier = await NetworkManager.Instance.RequestAsync<TrainMercenaryResponse>(
                "gamesvr.mercenary.trainmercenary",
                deploySoliersReq,
                10f
            );

        if(res_deploySoldier != null)
        {
            Debug.Log("训练佣兵成功 " + res_deploySoldier.Resp);
            return res_deploySoldier;
        }
        else
        {
            string errorMsg = res_deploySoldier.Resp?.Message ?? "未知错误";
            Debug.LogError("训练佣兵失败: " + errorMsg);
            return null;
        }
    }
    /// <summary>
    /// 建造建筑
    /// </summary>
    /// <param name="buildConfigId">建筑配置表id</param>
    /// <param name="posX">建造的新x位置</param>
    /// <param name="posY">建造的新y(z)位置</param>
    /// <returns></returns>
    public static async UniTask<ConstructBuildingResponse> ConstructBuilding(int buildConfigId, float posX, float posY,Vector3 _quaternion)
    {
        try
        {
            var constructBuildingReq = new ConstructBuildingRequest
            {
                BuildConfigId = buildConfigId,
                Position = new PitayaGame.Types.Vector2
                {
                    X = posX,
                    Z = posY
                },
                Rotation = new PitayaGame.Types.Quaternion
                {
                    X = _quaternion.x,
                    Y = _quaternion.y,
                    Z = _quaternion.z,
                    W = 0
                }
            };
            var res = await NetworkManager.Instance.RequestAsync<ConstructBuildingResponse>(
                "gamesvr.builds.constructbuilding",
                constructBuildingReq,
                10f
            );
            if (res != null)
            {
                Debug.Log("建造异常了,Code：" + res.Resp.Code);
                Debug.Log("建造异常了,Message：" + res.Resp.Message);
                Debug.Log("建筑建造成功，建筑ID: " + res.BuildId);
                return res;
            }
            else
            {
                string errorMsg = res.Resp?.Message ?? "未知错误";
                Debug.LogError("建筑建造失败: " + errorMsg);
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("建筑建造异常: " + e.Message);
            return null;
        }
    }
    /// <summary>
    /// 获取我所有的建筑
    /// </summary>
    /// <returns></returns>
    public static async UniTask<RepeatedField<PitayaGame.GameSvr.BuildingData>> GetMyBuilds()
    {
        try
        {
            var getMyBuildings = new GetMyBuildingsRequest();
            getMyBuildings.BuildingType = 0;
            Debug.Log(getMyBuildings);
            var res_Buildings = await NetworkManager.Instance.RequestAsync<GetMyBuildingsResponse>(
                "gamesvr.builds.getmybuildings",
                getMyBuildings,
                10f
            );
            if (res_Buildings != null && res_Buildings.Buildings != null)
            {
                return res_Buildings.Buildings;
            }
            else
            {
                string errorMsg = res_Buildings?.Resp?.Message ?? "未知错误";
                Debug.LogError($"获取建筑失败: {errorMsg}");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("获取建筑异常: " + e.Message);
            return null;
        }
    }
    /// <summary>
    /// 升级建筑
    /// </summary>
    /// <param name="build_Id">建筑数据库ID</param>
    /// <param name="instant_upgrade">是否立即完成</param>
    /// <returns></returns>
    public static async UniTask<UpgradeBuildingResponse> UpgradeBuilding(long build_Id, bool instant_upgrade)
    {
        var upgradeBuildingReq = new UpgradeBuildingRequest
        {
            BuildId = build_Id,
            InstantUpgrade = instant_upgrade
        };
        var res_upgradeBuilding = await NetworkManager.Instance.RequestAsync<UpgradeBuildingResponse>(
            "gamesvr.builds.upgradebuilding",
            upgradeBuildingReq,
            10f
        );
        if (res_upgradeBuilding != null)
        {
            Debug.Log("建筑升级成功，建筑新等级: " + res_upgradeBuilding.Resp);
            return res_upgradeBuilding;
        }
        else
        {
            string errorMsg = res_upgradeBuilding.Resp?.Message ?? "未知错误";
            Debug.LogError("建筑升级失败: " + errorMsg);
            return null;
        }
    }
    /// <summary>
    /// 拆除建筑
    /// </summary>
    /// <param name="build_Id"></param>
    /// <returns></returns>
    public static async UniTask<DestroyBuildingRequest> DestroyBuilding(long build_Id)
    {
        var destroyBuildingReq = new DestroyBuildingRequest
        {
            BuildId = build_Id
        };
        var res_destroyBuilding = await NetworkManager.Instance.RequestAsync<DestroyBuildingResponse>(
            "gamesvr.builds.destroybuilding",
            destroyBuildingReq,
            10f
        );
        if (res_destroyBuilding != null)
        {
            Debug.Log("建筑拆除成功，建筑ID: " + build_Id);
            return destroyBuildingReq;
        }
        else
        {
            string errorMsg = res_destroyBuilding.Resp?.Message ?? "未知错误";
            Debug.LogError("建筑拆除失败: " + errorMsg);
            return null;
        }
    }
    public static List<string> CandidateStr = new List<string>();
    // 匹配请求
    public static async UniTask<PitayaGame.MatchMakingSvr.GetNextCandidateResponse> MatchRequest()
    {
        var matchReq = new GetNextCandidateRequest();
        matchReq.ExcludeDefenderIds.Add(CandidateStr);
        var res_match = await NetworkManager.Instance.RequestAsync<GetNextCandidateResponse>(
            "matchmakingsvr.match_coc.getnextcandidate",
            matchReq,
            10f
        );
        if (res_match != null && res_match.Candidate != null)
        {
            if (!CandidateStr.Contains(res_match.Candidate.DefenderId))
            {
                CandidateStr.Add(res_match.Candidate.DefenderId);
            }
            Debug.Log("匹配请求成功，匹配ID: " + res_match.Resp);
            return res_match;
        }
        else
        {
            string errorMsg = res_match.Resp?.Message ?? "未知错误";
            Debug.LogError("匹配请求失败: " + errorMsg);

            return null;
        }
    }
}
