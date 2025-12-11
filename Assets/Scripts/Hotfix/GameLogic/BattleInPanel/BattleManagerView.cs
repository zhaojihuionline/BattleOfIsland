using cfg;
using PitayaGame.GameSvr;
using QFramework;
using QFramework.Game;
using QFramework.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UIElements;

namespace QFramework.UI
{
    public struct BattleDestructionRateConfig
    {
        public float MinRate;
        public bool IsPopuped;
    }
    public partial class BattleManagerView : ViewController
    {
        public static BattleManagerView Instance;
        public QFramework.UI.BattleInPanel battleInPanel { get; set; }

        Vector3 m_pos;

        float incessantTime = 0f;
        public float incessantInterval = 0.5f;

        public List<BattleDestructionRateConfig> battleProcessData = new List<BattleDestructionRateConfig>();
        private void Awake()
        {
            Instance = this;
            ResKit.Init();
            GenericFactoryEx<cfg.AttributeType, EffectEntity>.Initialize(keyPropertyName: "attributeType");
            UIKit.OpenPanel<BattleInPanel>();

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            DynamicGI.UpdateEnvironment();
            RenderSettings.skybox = Resources.Load<Material>("commonskybox");

            battleProcessData.Add(new BattleDestructionRateConfig() { MinRate = 0.3f, IsPopuped = false });
            battleProcessData.Add(new BattleDestructionRateConfig() { MinRate = 0.6f, IsPopuped = false });

            battleInPanel = GameObject.Find("UIRoot/Common").transform.Find("BattleInPanel").GetComponent<BattleInPanel>();
        }
        bool isPointerDownOverUI = false;
        private void Update()
        {

            if (battleInPanel && battleInPanel.State == PanelState.Opening)
            {
#if UNITY_EDITOR
                if (Input.GetMouseButtonDown(0))
                {
                    isPointerDownOverUI = EventSystem.current.IsPointerOverGameObject(-1);
                    if (!isPointerDownOverUI)
                        PlaceHandler();
                }
                else if (Input.GetMouseButton(0) && !isPointerDownOverUI)
                {
                    incessantTime += Time.deltaTime;
                    if (incessantTime >= incessantInterval)
                    {
                        PlaceHandler();
                        incessantTime = 0f;
                    }
                }
#else
    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            isPointerDownOverUI = EventSystem.current.IsPointerOverGameObject(touch.fingerId);
            if (!isPointerDownOverUI)
                PlaceHandler();
        }
        else if (touch.phase == TouchPhase.Moved && !isPointerDownOverUI)
        {
            incessantTime += Time.deltaTime;
            if (incessantTime >= incessantInterval)
            {
                PlaceHandler();
                incessantTime = 0f;
            }
        }
    }
#endif
            }

        }
        void PlaceHandler()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 10000, LayerMask.GetMask("Panel")))
            {
                m_pos = hitInfo.point;
            }
            PlaceActor(m_pos);
        }
        void PlaceActor(Vector3 position)
        {
            // 当选择一张英雄卡牌时
            if (battleInPanel.battleProcessManager.CurSelectedHeroCardItem != null)
            {
                if (battleInPanel.battleProcessManager.CurSelectedHeroCardItem.amount > 0)
                {
                    battleInPanel.battleProcessManager.CurSelectedHeroCardItem.UpgradeAmount(-1);
                    int selectedHeroID = battleInPanel.battleProcessManager.CurSelectedHeroCardItem.ID;//根据卡牌ID，实现对应英雄id方面的生成
                    HeroData heroData = battleInPanel.playerSelf.playerData.armyDatas[battleInPanel.playerSelf.CurrentSelectedArmyID].heroDatas[selectedHeroID];//<- selectedHeroID 这里由于我们只有1个英雄，所以默认使用第一个
                    switch (heroData.heroAttr.moveType)
                    {
                        case Enum_MoveType.Air:
                            position.y += 4.2f;//拉高4，为了防止穿透，多加了0.2f
                            break;
                    }
                    var _system = this.GetSystem<EntitySystem>();
                    GameObject newHero = _system.CreatEntityHreo(heroData.heroAttr.HeroID, heroData.heroAttr.Level, position);
                    //this.GetModel<BattleInModel>().player_allEntitys.Add(newHero);
                    battleInPanel.battleProcessManager.CurSelectedHeroCardItem.bloodObj.SetActive(true);
                }
                else
                {
                    battleInPanel.battleProcessManager.CurSelectedHeroCardItem.DoNotClickEffect();
                }
            }

            // 当选择一张士兵卡牌时
            if (battleInPanel.battleProcessManager.curSelectedSoldierCardItem != null)
            {
                if (PlayerManager.Instance.GetLocalPlayer().playerData.TotalCostMercenaryPoints > 0)
                {
                    battleInPanel.battleProcessManager.curSelectedSoldierCardItem.UpgradeAmount(-1);
                    int selectedSoldierID = battleInPanel.battleProcessManager.curSelectedSoldierCardItem.ID;
                    //foreach (var tbm in CfgMgr.Instance.Tables.TbMercenary.DataList)
                    //{
                    //    Debug.Log($"{tbm.Hero}{tbm.Name}");
                    //}
                    //Mercenary m = CfgMgr.Instance.Tables.TbMercenary.DataList[selectedSoldierID];
                    var _system = this.GetSystem<EntitySystem>();
                    GameObject newMercenary = _system.CreatEntitySoldier(GetMercenaryIndex(selectedSoldierID), 1, position);
                    //this.GetModel<BattleInModel>().player_allEntitys.Add(newMercenary);
                }
                else
                {
                    battleInPanel.battleProcessManager.curSelectedSoldierCardItem.DoNotClickEffect();
                }
            }
        }

        int GetMercenaryIndex(int selectedSoldierID)
        {
            if (selectedSoldierID == 0) return 10001;
            if (selectedSoldierID == 1) return 30001;
            if (selectedSoldierID == 2) return 30001;
            return 10001;
        }

        public GameObject BuildBuildingsEntity(BuildingData buildingData, bool isEnemy = false)
        {
            var _system = this.GetSystem<EntitySystem>();
            var bmodel = this.GetModel<BattleInModel>();
            bmodel.TotalEntityCount.Value++;
            GameObject newBuildings = _system.CreatEntityBuilding(buildingData, isEnemy);
            newBuildings.transform.eulerAngles = new Vector3(buildingData.rotation_x, buildingData.rotation_y, buildingData.rotation_z);
            buildingData.Init(newBuildings.GetComponent<EntityController>());
            return newBuildings;
        }

        public void RemoveAllOpponent_allEntitys()
        {
            var bmodel = this.GetModel<BattleInModel>();
            bmodel.DestroyedEntityCount.Value = 0;
            bmodel.TotalEntityCount.Value = 0;
            this.SendCommand(new RemoveAllPlayerEntitysCommand());
        }

        //public void RemoveGlobalBuffs()
        //{
        //    this.GetSystem<GlobalBuffRunnerSystem>().buffRunners.Clear();
        //}
    }
}