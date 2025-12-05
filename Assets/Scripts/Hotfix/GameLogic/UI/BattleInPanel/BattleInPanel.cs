using Cysharp.Threading.Tasks;
using Google.Protobuf.Collections;
using PitayaClient.Network.Manager;
using PitayaGame.GameSvr;
using QFramework;
using QFramework.Game;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PlasticPipe.PlasticProtocol.Messages.NegotiationCommand;

namespace QFramework.UI
{
	public partial class BattleInPanel : UIPanel, IController
	{
		public Player playerSelf;
		UserBasicInfo testInfo;

		public BattleInModel battleModel { get; private set; }
        public Image destructionRateSlider;
		public TMP_Text timerLabel;
        public TMP_Text costLabel;
        public bool TestUploadBuildingDataTestMode;


        public ThreeChooseOneManager threeChooseOneManager;
		public Transform magicSkillContainer;
		Transform magicSkillContainerOldParent;
        protected override async void OnInit(IUIData uiData = null)
		{
            battleModel = this.GetModel<BattleInModel>();
			magicSkillContainerOldParent = battleProcessManager.transform;
            UpgradeUI(null, null, 0);

            battleModel.DestructionRate
            .Register((newRate) => {
                UpgradeUI(battleModel.WhoHitme, battleModel.WillDeathEntity, newRate);
            });

            if (NetworkManager.Instance.IsConnected)
			{
				try
				{
                    // 获取用户信息
                    GetUserInfo();

                    // 获取军队列表数据
                    var arminfo = await this.SendCommand(new GetArmyInfoCommand());
                    if (arminfo.Count > 0)
                    {
                        GetRemoteBattleData(arminfo);
                    }
                    else// 如果没有军队，则创建一个默认军队
                    {
                        Debug.Log("当前账户没有军队，创建一个默认军队");
                        await GameRemoteAPI.CreateArmy(testInfo.Username + "的军队" + (arminfo.Count + 1));
                        var arminfo2 = await this.SendCommand(new GetArmyInfoCommand());
                        GetRemoteBattleData(arminfo2,true);
                    }

                    if(TestUploadBuildingDataTestMode == true)
                    {
                        MapManager.instance.Init();// 仅仅是为了上传一些建筑数据到服务器测试
                    }
                    searchBattle.GetComponent<SearchBattleManager>().OnSearchNextOne();
                }
				catch (System.Exception e)
				{
					Debug.Log(e.Message);
				}
			}
			else
			{
				GetRemoteBattleData_Local();
            }
        }
		void GetRemoteBattleData_Local() {
            PlayerManager.Instance.Init();
            playerSelf = PlayerManager.Instance.players[0];
            battleProcessManager.InitEvent(playerSelf);// 这里是玩家自身，临时取第一个
            searchBattle.gameObject.SetActive(true);
            searchBattle.GetComponent<CanvasGroup>().alpha = 1.0f;

            BattleManagerView.Instance.battleInPanel = GameObject.Find("UIRoot/Common").transform.Find("BattleInPanel").GetComponent<BattleInPanel>();
            ActorInfoViewContainersearch.PlayerInfoContainer.Init(playerSelf);

            //SearchNewOpponent(false);
            MapManager.instance.RebuildBuildingsUseLocalData(false,14001);
        }
        /// <summary>
		/// 获取用户信息
		/// </summary>
		public async void GetUserInfo()
		{
            testInfo = await this.SendCommand(new GetUserInfoCommand());
            var myBuildings = await GameRemoteAPI.GetMyBuilds();
            foreach (var b in myBuildings)
            {
                Debug.Log($"当前账户所有建筑Id是：{b.BuildName}");
            }
        }
		public async void GetRemoteBattleData(RepeatedField<PitayaGame.GameSvr.ArmyBriefInfo> armyBriefInfos,bool newAccount = false)
        {
			if (testInfo != null)
			{
                var armid = armyBriefInfos[Player.DefaultArmyID].ArmyId;
                var armydetail = await this.SendCommand(new GetArmyDetailCommand(armid));// 军队详情

                foreach (var h in armydetail.DeployedHeroes)
                {
                    Debug.Log(h.HeroName);
                }

                if (newAccount || armydetail.DeployedHeroes.Count <= 0)// 如果是新账户或者当前的军队中没有招募任何英雄
                {
                    var newElfvdId = await GameRemoteAPI.RecruitHero(1101, "巴顿");// 招募巴顿英雄
                    await GameRemoteAPI.DeployHero(armydetail.ArmyId, newElfvdId);// 将招募到的英雄部署到军队中

                    var newId = await GameRemoteAPI.RecruitHero(1202, "艾菲尔");// 招募艾菲尔英雄
                    await GameRemoteAPI.DeployHero(armydetail.ArmyId, newId);// 将招募到的英雄部署到军队中

                    var newShieldId = await GameRemoteAPI.RecruitHero(1301, "罗兰");// 招募骑兵英雄
                    await GameRemoteAPI.DeployHero(armydetail.ArmyId, newShieldId);// 将招募到的英雄部署到军队中

   					//var newElfvId = await GameRemoteAPI.RecruitHero(1201, "艾利佛");// 招募艾利佛英雄
                    //await GameRemoteAPI.DeployHero(armydetail.ArmyId, newElfvId);// 将招募到的英雄部署到军队中
                    MapManager.instance.RebuildBuildingsUseLocalData(true,14001,14001,14001,14001);// 生成默认建筑布局
                }
                // 再次重新获取下当前军队详情，不要用之前缓存的数据
                var arminfo2 = await this.SendCommand(new GetArmyInfoCommand());
                if (arminfo2 != null) {
                    var armydetai2 = await this.SendCommand(new GetArmyDetailCommand(arminfo2[0].ArmyId));// 新部署完的军队详情
                    PlayerManager.Instance.Init(testInfo, armydetai2);
                    playerSelf = PlayerManager.Instance.LocalPlayer;
                    PlayerManager.Instance.InitRemotePlayers();
                    battleProcessManager.InitEvent(playerSelf);
                    searchBattle.gameObject.SetActive(true);
                    searchBattle.GetComponent<CanvasGroup>().alpha = 1.0f;
                    costLabel.text = playerSelf.playerData.TotalCostMercenaryPoints.ToString();
                    //BattleManagerView.Instance.battleInPanel = GameObject.Find("UIRoot/Common").transform.Find("BattleInPanel").GetComponent<BattleInPanel>();
                    ActorInfoViewContainersearch.PlayerInfoContainer.Init(playerSelf);

                    // SearchNewOpponent(false);
                }
                else
                {
                    Debug.LogError("重新获取军队详情失败");
                }
            }
        }

        public async void DeployHero4()
        {
            var arminfo = await this.SendCommand(new GetArmyInfoCommand());
            if (arminfo.Count > 0)
            {
                var armid = arminfo[Player.DefaultArmyID].ArmyId;
                var armydetail = await this.SendCommand(new GetArmyDetailCommand(armid));// 军队详情
                var newShieldId = await GameRemoteAPI.RecruitHero(1301, "艾利佛");// 招募骑兵英雄
                await GameRemoteAPI.DeployHero(armydetail.ArmyId, newShieldId);// 将招募到的英雄部署到军队中
            }
        }
		/// <summary>
		/// 随机从一组敌人数据中取一个
		/// </summary>
		public int SearchNewOpponent(Player player)
		{
			//var playerOppData = PlayerManager.Instance.GetRemotePlayerData(5);
            Player playerOpponent = PlayerManager.Instance.players[0];// 后面直接赋值为playerOppData，现在暂时用第一个测试
            
            ActorInfoViewContainersearch.OpponentInfoContainer.Init(player);
			//if (isFirst)
			//{
   //             // 这里根据获取到的对手数据，摆放对手的建筑物
   //             MapManager.instance.RebuildBuildingsUseLocalData(false,14001);// 这里用一个默认建筑物测试

   //         }
			return 0;
		}

        /// <summary>
        /// 更新摧毁率进度条和抓手位置
        /// </summary>
        /// <param name="rate"></param>
		public void UpgradeDestructionRateSliderAndGrab(float rate)
		{
            destructionRateSlider.fillAmount = rate;
            destructionRateSlider.transform.Find("grab").GetComponent<RectTransform>().anchoredPosition = new Vector2(704 * destructionRateSlider.fillAmount, 0);
            destructionRateSlider.transform.Find("grab/rate").GetComponent<TMP_Text>().text = $"{(int)(rate * 100)}%";
        }
        /// <summary>
        /// 更新所有UI，并根据摧毁率触发对应功能
        /// </summary>
        /// <param name="hitTarget">攻击者，这里是某个英雄</param>
        /// <param name="obj">要移除的建筑物</param>
        void UpgradeUI(Transform hitTarget,GameObject obj,float newRate)
		{
			if (obj)
			{
				MapManager.instance.objectsToPlace.Remove(obj);
			}

			UpgradeDestructionRateSliderAndGrab(newRate);
            if (obj)
			{
                Destroy(obj);
                battleModel.player_allEntitys.Remove(obj);
            }
            if(BattleManagerView.Instance!=null && BattleManagerView.Instance.battleProcessData != null && BattleManagerView.Instance.battleProcessData.Count > 0)
            {
                // 计算法术牌三选一界面弹出逻辑
                float minRate = BattleManagerView.Instance.battleProcessData[0].MinRate;// MinRate = 0.3f
                float maxRate = BattleManagerView.Instance.battleProcessData[1].MinRate;// MaxRate = 0.6f
                bool hitMin = (newRate >= minRate) && (newRate < maxRate) && (BattleManagerView.Instance.battleProcessData[0].IsPopuped == false);
                bool hitMax = (newRate >= maxRate) && (newRate <= 1) && (BattleManagerView.Instance.battleProcessData[1].IsPopuped == false);
                if (hitMin)
                {
                    var battleData = BattleManagerView.Instance.battleProcessData[0];
                    battleData.IsPopuped = true;
                    BattleManagerView.Instance.battleProcessData[0] = battleData;
                }
                if (hitMax)
                {
                    var battleData = BattleManagerView.Instance.battleProcessData[1];
                    battleData.IsPopuped = true;
                    BattleManagerView.Instance.battleProcessData[1] = battleData;
                }
                if (hitMin || hitMax)
                {
                    if(MapManager.instance.TotalPlacedObjects > 1)
                    {
                        threeChooseOneManager.gameObject.SetActive(true);
                        threeChooseOneManager.OnOpen();
                        magicSkillContainer.SetParent(transform);
                    }
                }
            }

            //

            if (hitTarget != null)
			{
				this.SendCommand(new BakeAStarPathCommand(hitTarget.position, 5.0f));
			}

			if (newRate >= 1.0f)
			{
				UIKit.OpenPanel<BattleReportPanel>();
			}
        }

		public void ResetMagicSkillContainerParent()
		{
			UnityEngine.Time.timeScale = 1;
            threeChooseOneManager.OnClose();
            threeChooseOneManager.gameObject.SetActive(false);
            magicSkillContainer.SetParent(magicSkillContainerOldParent);
        }

        public void GotoBattle(int CurSearchOpponentIndex)
		{
			// 携带对手玩家数据进入战斗流程
			battleProcessManager.EnterBattleProcess(CurSearchOpponentIndex);
		}

        public void AddEntityToWorld(GameObject o, HeroData heroData = null)
        {
            battleModel.player_allEntitys.Add(o);

            // 临时在这里添加一个生成敌方小兵的逻辑
            //if (heroData != null) {
            //    GameObject newEnemyHero = Instantiate(BattleManager.Instance.enemyPlayerPrefab);
            //    newEnemyHero.transform.position = battleModel.player_allEntitys
            //        .Where(b => b.GetComponent<BuildEntity>().BuildID == 14001)
            //        .FirstOrDefault().transform.position + new Vector3(2, 0, 0);

            //    newEnemyHero.GetComponent<HeroEntity>().heroData = heroData;
            //    newEnemyHero.GetComponent<HeroEntity>().runTimeheroData = heroData.Clone();
            //    newEnemyHero.GetComponent<HeroEntity>().IsEnemy = true;
            //    newEnemyHero.GetComponent<HeroEntity>().Init();

            //    battleModel.opponent_allEntitys.Add(newEnemyHero);
            //}
            
            Debug.Log("添加单位到战斗世界，当前单位数量：" + battleModel.player_allEntitys.Count);
        }

        /// <summary>
        /// 查找所有指定类型的建筑物
        /// </summary>
        /// <param name="enum_BuildingType"> 要查找的建筑物类型 </param>
        /// <returns></returns>
        public List<GameObject> GetAllPlayerEntitys(cfg.Enum_BuildingType enum_BuildingType)
        {
            return this.SendCommand<List<GameObject>>(new GetAllBuildingsOfTypeCommand(enum_BuildingType));
        }

        public void SetAllEntitys(GameObject newBuilding)
        {
            this.SendCommand(new SetAllBuildingsLayerCommand(newBuilding));
        }

        public void ClearAllEntitys()
        {
            this.SendCommand(new RemoveAllPlayerEntitysCommand());
        }

        protected override void OnOpen(IUIData uiData = null)
		{
		}

		protected override void OnShow()
		{
		}

		protected override void OnHide()
		{
		}

		protected override void OnClose()
		{
		}

		public IArchitecture GetArchitecture()
		{
            return GameApp.Interface;
		}
	}
}
