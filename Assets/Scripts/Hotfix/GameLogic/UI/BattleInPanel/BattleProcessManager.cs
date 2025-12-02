/****************************************************************************
 * 2025.10 DESKTOP-HUQFF5N
 ****************************************************************************/

using DG.Tweening;
using QFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Pathfinding.Funnel;
using QFramework.Game;
using Pathfinding;
namespace QFramework.UI
{
    /// <summary>
    /// 战斗流程管理器
    /// </summary>
    public partial class BattleProcessManager : ViewController
    {
        public List<BaseCardItem> baseCardItems;
        [Header("英雄卡牌预制体（测试用）")]
        public GameObject heroCardItemPrefab;
        [Header("小兵卡牌预制体（测试用）")]
        public GameObject mercenaryCardItemPrefab;
        [Header("法术卡牌预制体（测试用）")]
        public GameObject magicCardItemPrefab;
        [Header("法术特效预制体（测试用）")]
        public GameObject magicEFXPrefab;
        public Transform heroListContainerParent;
        public Transform soldierListContainerParent;
        public Transform magicListContainerParent;

        public HeroCardItem CurSelectedHeroCardItem;
        public MercenaryCardItem curSelectedSoldierCardItem;
        public MagicCardItem curSelectedMagicCardItem;

        public Button backToSeachProcess;

        BaseCardItem lastSelectedItem;

        Player player;// 玩家数据

        public const float battleDuration = 180.0f;// 战斗总时长，单位秒
        float curBattleDuration = 0;// 当前战斗时间计时器
        bool isBattleOver = false;// 战斗是否结束

        private void Awake()
        {
            baseCardItems = new List<BaseCardItem>();
        }

        private void Update()
        {
            if (isBattleOver == false)
            {
                curBattleDuration += Time.deltaTime;
                if (curBattleDuration >= battleDuration)
                {
                    // 战斗时间到，结束战斗
                    Debug.Log("战斗时间到，结束战斗");
                    UIKit.OpenPanel<BattleReportPanel>();
                    curBattleDuration = 0;
                    isBattleOver = true;
                }
                int remainSeconds = Mathf.FloorToInt(battleDuration - curBattleDuration);
                int minutes = remainSeconds / 60;
                int seconds = remainSeconds % 60;
                BattleManagerView.Instance.battleInPanel.timerLabel.text = $"{minutes:00}:{seconds:00}s";
            }
        }

        public void InitEvent(Player _player)
        {
            player = _player;
            TypeEventSystem.Global.Register<HeroCardItem>(e =>
            {
                CurSelectedHeroCardItem = e;
                curSelectedSoldierCardItem = null;
                curSelectedMagicCardItem = null;
                lastSelectedItem = e;
                ResetAllCardItemAnim();
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            TypeEventSystem.Global.Register<MercenaryCardItem>(e =>
            {
                curSelectedSoldierCardItem = e;
                CurSelectedHeroCardItem = null;
                curSelectedMagicCardItem = null;
                lastSelectedItem = e;
                ResetAllCardItemAnim();
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
            TypeEventSystem.Global.Register<MagicCardItem>(e =>
            {
                CastMagicProcess();// 释放法术
                lastSelectedItem = e;
                ResetAllCardItemAnim();
            }).UnRegisterWhenGameObjectDestroyed(gameObject);


            backToSeachProcess.onClick.AddListener(BackSearchProcess);
        }

        void ResetAllCardItemAnim()
        {
            foreach (var item in baseCardItems)
            {
                if (item != lastSelectedItem)
                {
                    item.DoUnselectedAnimation();
                }
            }
        }
        void ResetAllCardItemAnim(CardType cardType)
        {
            foreach (var item in baseCardItems)
            {
                if (item.cardType == cardType)
                {
                    item.DoUnselectedAnimation();
                }
            }
        }
        /// <summary>
        /// 生成英雄卡牌
        /// </summary>
        void InitHeroCardItems()
        {
            // 在这里要通过玩家的军队配置来初始化Hero卡牌
            for (int i = 0; i < player.playerData.armyDatas[player.CurrentSelectedArmyID].heroDatas.Count; i++)
            {
                GameObject newHeroCardItem = Instantiate(heroCardItemPrefab, heroListContainerParent);
                newHeroCardItem.name = $"HeroCardItem_{i}";
                baseCardItems.Add(newHeroCardItem.GetComponent<BaseCardItem>());
                newHeroCardItem.GetComponent<BaseCardItem>().OnInit(i, 1);
            }
        }
        /// <summary>
        /// 生成佣兵卡牌
        /// </summary>
        void InitSoldierCardItems()
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject newmercenaryCardItem = Instantiate(mercenaryCardItemPrefab, soldierListContainerParent);
                newmercenaryCardItem.name = $"SoldierCardItem_{i}";
                baseCardItems.Add(newmercenaryCardItem.GetComponent<BaseCardItem>());
                newmercenaryCardItem.GetComponent<BaseCardItem>().OnInit(i, 1);
                var level1List = CfgMgr.Instance.Tables.TbMercenary.DataList.Where(m => m.Level == 1).ToList();
                var tbm = level1List.Where(ll => ll.Hero == CfgMgr.Instance.GetMercenaryIdBySoldierCardItemID(i)).FirstOrDefault();
                //var tbm = CfgMgr.Instance.Tables.TbMercenary.Get();
                newmercenaryCardItem.GetComponent<MercenaryCardItem>().Cost = tbm.Cost;
            }
        }
        /// <summary>
        /// 生成法术卡牌
        /// </summary>
        void InitMagicCardItems()
        {
            // 在这里要通过玩家的军队配置来初始化Magic卡牌
            GameObject newsoldierCardItem = Instantiate(magicCardItemPrefab, magicListContainerParent);
            newsoldierCardItem.transform.localPosition = new Vector3(69.2f, 1.9f, 0);
            newsoldierCardItem.name = "MagicCardItem";
            baseCardItems.Add(newsoldierCardItem.GetComponent<BaseCardItem>());
            newsoldierCardItem.GetComponent<BaseCardItem>().OnInit(0, 1);
        }
        /// <summary>
        /// 释放法术流程
        /// </summary>
        public void CastMagicProcess()
        {
            // 播放前摇特效
            ResLoader loader = ResLoader.Allocate();
            GameObject newQianyao = Instantiate(loader.LoadSync<GameObject>("FX_Spell_Fashuqianyao1"));
            Vector3 pc = Camera.main.transform.localPosition;
            Vector3 targetPosition = Camera.main.transform.localPosition + Camera.main.transform.forward * 6f;
            newQianyao.transform.localPosition = targetPosition - new Vector3(0, 3, 0);

            //this.Delay(1.5f, () =>
            //{
            //    Destroy(newQianyao);
            //    var tbskill = CfgMgr.Instance.Tables.TbSkillTable.Get(20000101);// 20001801   20000101   20000101
            //    BattleInModel battleInModel = this.GetModel<BattleInModel>();
            //    GameObject _target = this.SendCommand(new FindTargetCommand(battleInModel.opponent_allEntitys, tbskill.TagMask, tbskill.CastRanage, tbskill.Preference, Vector3.zero));
            //    this.SendCommand<ReleaseSpellCommand>(new ReleaseSpellCommand(tbskill, null, _target, Vector3.zero));
            //});
            this.Delay(1.5f, () =>
            {
                Destroy(newQianyao);
                var tbskill = CfgMgr.Instance.Tables.TbSkillTable.Get(20000101);// 腐朽之森技能
                this.SendCommand<ReleaseSpellCommand>(new ReleaseSpellCommand(tbskill, null, null, Vector3.zero));
            });
        }
        /// <summary>
        /// 进入战斗流程
        /// </summary>
        public void EnterBattleProcess(int curSearchOpponentIndex)
        {
            //Player playerSelf = PlayerManager.Instance.LocalPlayer;// 获取自身数据
            BattleManagerView.Instance.battleInPanel.ActorInfoViewContainerInBattle.PlayerInfoContainer.Init(player);// 初始化玩家信息
            Player playerOpponent = PlayerManager.Instance.players[curSearchOpponentIndex];// 获取对手数据

            InitHeroCardItems();// 初始化英雄卡牌
            InitSoldierCardItems();// 初始化小兵卡牌
            InitMagicCardItems();// 初始化法术卡牌
        }
        /// <summary>
        /// 返回搜索对手流程
        /// </summary>
        public void BackSearchProcess()
        {
            for (int i = 0; i < heroListContainerParent.childCount; i++)
            {
                Destroy(heroListContainerParent.GetChild(i).gameObject);
            }
            for (int i = 0; i < soldierListContainerParent.childCount; i++)
            {
                Destroy(soldierListContainerParent.GetChild(i).gameObject);
            }
            for (int i = 0; i < magicListContainerParent.childCount; i++)
            {
                Destroy(magicListContainerParent.GetChild(i).gameObject);
            }
            curBattleDuration = 0;
            isBattleOver = false;
            baseCardItems.Clear();

            BattleManagerView.Instance.battleInPanel.UpgradeDestructionRateSliderAndGrab(0);

            CanvasGroup cg = BattleManagerView.Instance.battleInPanel.inBattle.GetComponent<CanvasGroup>();
            cg.DOFade(0, 0.5f).OnComplete(() => {
                BattleManagerView.Instance.battleInPanel.inBattle.gameObject.SetActive(false);

                BattleManagerView.Instance.battleInPanel.searchBattle.gameObject.SetActive(true);
                BattleManagerView.Instance.battleInPanel.searchBattle.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
                BattleManagerView.Instance.battleInPanel.searchBattle.GetComponent<SearchBattleManager>().ResetSearch();
            });
        }
    }
}

