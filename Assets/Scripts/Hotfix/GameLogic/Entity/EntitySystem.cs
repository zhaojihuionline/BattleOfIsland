using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using QFramework.Game;
using cfg;

/// <summary>
/// 
/// </summary>
public interface IEntitySystem : ISystem
{

}
/// <summary>
/// 
/// </summary>
public class EntitySystem : AbstractSystem, IEntitySystem
{
    ResLoader loader;
    public Dictionary<int, GameObject> resourcesRoleMap = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject> resourcesBuildMap = new Dictionary<int, GameObject>();
    protected override void OnInit()
    {
        ResKit.Init();
        loader = ResLoader.Allocate();
    }

    /// <summary>
    /// 根据ID和等级创建一个英雄  
    /// </summary>
    /// <param name="id"></param>
    /// <param name="lv"></param>
    public GameObject CreatEntityHreo(int id, int lv, Vector3 spawnPoint, bool isEnemy = false)
    {
        cfg.Hero hero = CfgMgr.Instance.Tables.TbHero.Get(id);

        GameObject HEntity = null;
        if (resourcesRoleMap.ContainsKey(id))
        {
            HEntity = resourcesRoleMap[id];
        }
        else
        {
            HEntity = loader.LoadSync<GameObject>(hero.Mod);
            resourcesRoleMap.Add(id, HEntity);
        }

        EntityController res = Object.Instantiate<GameObject>(HEntity, spawnPoint, Quaternion.identity).GetComponent<EntityController>();
        //根据等级获取对应表数据
        HeroLevel hlvtable = CfgMgr.Instance.Tables.TbHeroLevel.Get(id * 100 + lv);

        res.Init(isEnemy);
        res.InitHaveHp(hlvtable.Health);
        //res.currentHP = 100;
        List<int> paramList = new List<int>();
        //获取技能列表  等级需要从服务器拉取  现在默认1级别  服务器数据为英雄等级表+技能等级 
        foreach (var item in hlvtable.Skill)
        {
            paramList.Add(item * 100 + lv);
        }
        res.SetMoveSpeed(hlvtable.Speed / 100f);
        res.InitCanSkill(paramList,hlvtable.SkillEnable);

        if (isEnemy)
        {
            BattleInModel BM = this.GetModel<BattleInModel>();
            BM.opponent_allEntitys.Add(res.gameObject);
        }
        else
        {
            BattleInModel BM = this.GetModel<BattleInModel>();
            BM.player_allEntitys.Add(res.gameObject);
        }
        return res.gameObject;

    }

    /// <summary>
    /// 根据ID创建一个小兵
    /// </summary>
    /// <param name="id"></param>
    /// <param name="lv"></param>
    public GameObject CreatEntitySoldier(int id, int lv, Vector3 spawnPoint, bool isEnemy = false)
    {
        cfg.Mercenary mercenary = CfgMgr.Instance.Tables.TbMercenary.Get(id * 100 + lv);
        GameObject BEntity = loader.LoadSync<GameObject>(mercenary.Mod);

        GameObject HEntity = null;
        if (resourcesRoleMap.ContainsKey(id))
        {
            HEntity = resourcesRoleMap[id];
        }
        else
        {
            HEntity = loader.LoadSync<GameObject>(mercenary.Mod);
            resourcesRoleMap.Add(id, HEntity);
        }
        EntityController res = Object.Instantiate<GameObject>(HEntity, spawnPoint, Quaternion.identity).GetComponent<EntityController>();
        res.InitHaveHp(mercenary.Health);
        List<int> paramList = new List<int>();
        //获取技能列表  等级需要从服务器拉取  现在默认1级别  服务器数据为英雄等级表+技能等级 
        foreach (var item in mercenary.Skill)
        {
            paramList.Add(item * 100 + lv);
        }
        res.SetMoveSpeed(mercenary.Speed / 100f);
        res.InitCanSkill(paramList);
        res.Init(isEnemy);
        if (isEnemy)
        {
            BattleInModel BM = this.GetModel<BattleInModel>();
            BM.opponent_allEntitys.Add(res.gameObject);
        }
        else
        {
            BattleInModel BM = this.GetModel<BattleInModel>();
            BM.player_allEntitys.Add(res.gameObject);
        }
        return null;
    }

    /// <summary>
    /// 根据ID和等级创建一个建筑
    /// </summary>
    /// <param name="id"></param>
    /// <param name="lv"></param>
    public GameObject CreatEntityBuilding(int id, int lv, Vector3 spawnPoint, bool isEnemy = false)
    {
        cfg.BuildsLevel buildLevel = CfgMgr.Instance.Tables.TbBuildsLevel.Get(id * 100 + lv);

        GameObject BEntity = null;
        if (resourcesBuildMap.ContainsKey(id))
        {
            BEntity = resourcesBuildMap[id];
        }
        else
        {
            BEntity = loader.LoadSync<GameObject>(buildLevel.BuildModel);
            resourcesBuildMap.Add(id, BEntity);
        }
        EntityController res = Object.Instantiate<GameObject>(BEntity, spawnPoint, Quaternion.identity).GetComponent<EntityController>();
        //根据等级获取对应表数据

        res.Init(isEnemy);
        res.InitHaveHp(buildLevel.Health);
        List<int> paramList = new List<int>();
        //获取技能列表  等级需要从服务器拉取  现在默认1级别  服务器数据为英雄等级表+技能等级 
        foreach (var item in buildLevel.Skill)
        {
            paramList.Add(item * 100 + lv);
        }
        res.InitCanSkill(paramList);

        if (isEnemy)
        {
            BattleInModel BM = this.GetModel<BattleInModel>();
            BM.opponent_allEntitys.Add(res.gameObject);
        }
        else
        {
            BattleInModel BM = this.GetModel<BattleInModel>();
            BM.player_allEntitys.Add(res.gameObject);
        }
        return res.gameObject;
    }
}
