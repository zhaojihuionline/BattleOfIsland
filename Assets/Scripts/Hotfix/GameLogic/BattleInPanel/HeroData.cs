using cfg;
using log4net.Core;
using PitayaGame.GameSvr;
using QFramework.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using UnityEngine;
using static Codice.CM.Common.CmCallContext;
using static PlasticPipe.Server.MonitorStats;
public enum MagicType
{
    /// <summary>
    /// ���λ��ħ��
    /// </summary>
    RandomMagic,
    /// <summary>
    /// ȫ��ħ��
    /// </summary>
    FullScreenMagic,
}
public enum MoveType
{
    /// <summary>
    /// ½��
    /// </summary>
    Ground,
    /// <summary>
    /// ���� 
    /// </summary>
    Air
}
public enum AttackType
{
    /// <summary>
    /// ��ս
    /// </summary>
    Melee,
    /// <summary>
    /// Զ��
    /// </summary>
    Ranged
}
public class WeaponData
{
    public int WeaponID;
    public string WeaponName;
    public int WeaponModelID;
    public float AttackDamage;
    public float AttackSpeed;
    public float AttackRange;

    public WeaponData(int weaponID, string weaponName, int weaponModelID, float attackDamage, float attackSpeed, float attackRange)
    {
        WeaponID = weaponID;
        WeaponName = weaponName;
        WeaponModelID = weaponModelID;
        AttackDamage = attackDamage;
        AttackSpeed = attackSpeed;
        AttackRange = attackRange;
    }
}
[System.Serializable]
public class HeroData : UnitData
{
    public HeroAttr heroAttr = new HeroAttr();
    public HeroAttr heroAttrExtra = new HeroAttr();//additional 附加属性
    public override List<int> AttackBonus { get => base.AttackBonus; set => base.AttackBonus = value; }//附加额外的buff
    public override int CurHealth { get { return heroAttr.Health; } set { heroAttr.Health = value; } }
    public override int Health { get { return heroAttr.CurHealth; } set { heroAttr.CurHealth = value; } }

    public WeaponData weaponData;

    public List<int> skillID;

    public cfg.Hero CFG_HeroData;

    PitayaGame.GameSvr.HeroData heroInfoPayload;
    public HeroData(PitayaGame.GameSvr.HeroData _heroInfoPayload)
    {
        this.heroInfoPayload = _heroInfoPayload;

        var _cfg_HeroData = CfgMgr.Instance.Tables.TbHero.Get(_heroInfoPayload.HeroConfigId);
        this.CFG_HeroData = _cfg_HeroData;

        var heroInfo_level = CfgMgr.Instance.Tables.TbHeroLevel.Get(_heroInfoPayload.HeroConfigId * 100 + _heroInfoPayload.Level);


        //var heroInfo_level_linq = CfgMgr.Instance.Tables.TbHeroLevel.DataList
        //    .Where(h => h.Level == heroInfoPayload.Level).FirstOrDefault();

        heroAttr.HeroID = _cfg_HeroData.Id;
        heroAttr.HeroName = _cfg_HeroData.Name;
        heroAttr.HeroModelID = _cfg_HeroData.Mod;
        heroAttr.Level = heroInfo_level.Level;
        heroAttr.Description = _cfg_HeroData.Description1;
        heroAttr.moveType = _cfg_HeroData.MoveType;
        heroAttr.moveSpeed = heroInfo_level.Speed;
        CurHealth = Health = heroInfo_level.Health;
        heroAttr.attackType = _cfg_HeroData.AttackType;
        heroAttr.attackDamage = heroInfo_level.Attack;
        weaponData = null;
        skillID = heroInfo_level.Skill;
        AttackBonus = new();
    }

    //public HeroData Clone()
    //{
    //    var heroInfo_level = CfgMgr.Instance.Tables.TbHeroLevel.Get(heroInfoPayload.HeroConfigId * 100 + heroInfoPayload.Level);

    //    return new HeroData(heroInfoPayload)
    //    {
    //        HeroID = this.CFG_HeroData.Id,
    //        HeroName = this.CFG_HeroData.Name,
    //        HeroModelID = this.CFG_HeroData.Mod,
    //        Level = heroInfo_level.Level,
    //        Description = this.CFG_HeroData.Description1,
    //        moveType = this.CFG_HeroData.MoveType,
    //        moveSpeed = heroInfo_level.Speed,
    //        CurHealth = Health = heroInfo_level.Health,
    //        attackType = this.CFG_HeroData.AttackType,
    //        attackDamage = heroInfo_level.Attack,
    //        weaponData = null,
    //        skillID = heroInfo_level.Skill
    //    };
    //}

    public override void ChangeAttribute(AttributeChangeData attributeChangeData)
    {
        Func<EBaseCalculateType, float, int, string, string, string, float> func = !attributeChangeData.isReverse ? SkillKit.ProcessCalculate : SkillKit.ProcessCalculateReverse;
        switch (attributeChangeData.attributeType)
        {
            case cfg.AttributeType.AttackType:
                heroAttr.damageRange = func(attributeChangeData.baseCalculateType, heroAttr.damageRange, attributeChangeData.value, "damageRange", heroAttr.HeroName, entityController.name);
                break;
            case cfg.AttributeType.Attack:
                heroAttr.attackDamage = func(attributeChangeData.baseCalculateType, heroAttr.attackDamage, attributeChangeData.value, "attackDamage", heroAttr.HeroName, entityController.name);
                break;
            case cfg.AttributeType.Speed:// 降低百分比
                heroAttr.moveSpeed = func(attributeChangeData.baseCalculateType, heroAttr.moveSpeed, attributeChangeData.value, "moveSpeed", heroAttr.HeroName, entityController.name);
                break;
            case cfg.AttributeType.CurHealth:// 增加x点血量
                CurHealth = (int)func(attributeChangeData.baseCalculateType, CurHealth, attributeChangeData.value, "CurHealth", heroAttr.HeroName, entityController.name);
                if (CurHealth > Health)
                {
                    CurHealth = Health;
                }
                entityController.AddBlood();
                break;
            case cfg.AttributeType.Damage:
                heroAttr.attackDamage = func(attributeChangeData.baseCalculateType, heroAttr.attackDamage, attributeChangeData.value, "attackDamage", heroAttr.HeroName, entityController.name);
                break;
            case cfg.AttributeType.ExtraDamage:
                heroAttrExtra.attackDamage = func(attributeChangeData.baseCalculateType, heroAttrExtra.attackDamage, attributeChangeData.value, "attackDamage", heroAttr.HeroName, entityController.name);
                break;
        }
    }

    float ProcessCalculate(cfg.EBaseCalculateType baseCalculateType, float attribute, int value, string attributeName)
    {
        float preAttribute = attribute;
        float fValue = value / 100f;
        switch (baseCalculateType)
        {
            case EBaseCalculateType.Add:
                attribute += fValue;
                Debug.Log($"属性加成 属性：{attributeName} {baseCalculateType}属性增加{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{heroAttr.HeroName} GoName:{entityController.name}");
                break;
            case EBaseCalculateType.Subtract:
                attribute -= fValue;
                Debug.Log($"属性加成 属性：{attributeName} {baseCalculateType}属性减少{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{heroAttr.HeroName} GoName:{entityController.name}");
                break;
            case EBaseCalculateType.Multiply:
                attribute *= (1 + (float)fValue);
                Debug.Log($"属性加成 属性：{attributeName} {baseCalculateType}属性加成{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{heroAttr.HeroName} GoName:{entityController.name}");
                break;
            case EBaseCalculateType.Divide:
                attribute *= (1 - (float)fValue);
                Debug.Log($"属性加成 属性：{attributeName} {baseCalculateType}属性减成{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{heroAttr.HeroName} GoName:{entityController.name}");
                break;
        }
        return attribute;
    }
    //反向计算
    float ProcessCalculateReverse(cfg.EBaseCalculateType baseCalculateType, float attribute, int value, string attributeName)
    {
        float preAttribute = attribute;
        float fValue = value / 100f;
        switch (baseCalculateType)
        {
            case EBaseCalculateType.Add:
                attribute -= fValue;
                Debug.Log($"属性移除 属性：{attributeName} {baseCalculateType}属性增加{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{heroAttr.HeroName} GoName:{entityController.name}");
                break;
            case EBaseCalculateType.Subtract:
                attribute += fValue;
                Debug.Log($"属性移除 属性：{attributeName} {baseCalculateType}属性减少{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{heroAttr.HeroName} GoName:{entityController.name}");
                break;
            case EBaseCalculateType.Multiply:
                attribute *= (1 - (float)fValue);
                Debug.Log($"属性移除 属性：{attributeName} {baseCalculateType}属性加成{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{heroAttr.HeroName} GoName:{entityController.name}");
                break;
            case EBaseCalculateType.Divide:
                attribute *= (1 + (float)fValue);
                Debug.Log($"属性移除 属性：{attributeName} {baseCalculateType}属性减成{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{heroAttr.HeroName} GoName:{entityController.name}");
                break;
        }
        return attribute;
    }

    public override float GetDamageRange()
    {
        //foreach(var buffEntity in entityController.buffRunner.buffEntities)
        //{
        //    foreach(var effect in buffEntity.effects)
        //    {
        //        foreach(var effectNode in effect.effect.buffTable.BuffTickEffect)
        //        {

        //        }
        //    }
        //}
        return heroAttr.damageRange;
    }

    public override float GetDamage()
    {
        return heroAttr.attackDamage;
    }

    public override int GetMoveType()
    {
        return (int)heroAttr.moveType;
    }

    public override Enum_HeroType GetHeroType()
    {
        return CFG_HeroData.HeroType;
    }
}

public class HeroAttr
{
    public int HeroID;
    public string HeroName;
    public string HeroModelID;
    public int Level;
    public string Description;
    public Enum_MoveType moveType;
    public float moveSpeed;
    public int CurHealth;
    public int Health;
    public Enum_AttackType attackType;
    public float attackDamage;
    public float attackSpeed;
    public float attackRange;
    public float damageRange;
}

public class MercenaryData : UnitData
{
    public long SoldierID;
    public string SoldierName;
    public int SoldierModelID;
    public int Level;
    public string Description;
    public MoveType moveType;
    public float moveSpeed;
    //public float Health;
    public AttackType attackType;
    public float attackDamage;
    public float attackSpeed;
    public float attackRange;

    public int Counts;
    public int Cost;

    cfg.Mercenary mercenary;
    public MercenaryData(cfg.Mercenary mercenary)
    {
        this.mercenary = mercenary;
        SoldierID = mercenary.Id;
        SoldierName = mercenary.Name;
        SoldierModelID = 1;
        Level = mercenary.Level;
        Description = mercenary.Name;
        this.moveSpeed = mercenary.Speed;
        CurHealth = this.Health = mercenary.Health;
        this.attackDamage = mercenary.Attack;
        this.attackSpeed = mercenary.AttackSpeed;
        this.attackRange = mercenary.AttackRange;
        Cost = mercenary.Cost;
    }

    //public MercenaryData Clone()
    //{
    //    return (MercenaryData)this.MemberwiseClone();
    //}
    public override float GetDamage()
    {
        return this.attackDamage;
    }
    public override int GetMoveType()
    {
        return (int)moveType;
    }
    public override EMercenaryType GetMercenaryType()
    {
        return mercenary.MercenaryType;
    }
}

public class MagicData
{
    public int MagicID;
    public string MagicName;
    public int MagicModelID;
    public MagicType magicType;
}


public class BuildingLayoutData
{
    public int level;
    public string user_id;
    public BuildingData[] buildings;
}
public class BuildingData : UnitData
{
    public float x;
    public float z;
    public int level;
    public long build_id;
    public int config_id;
    public int building_type;
    public int upgrade_end_time;
    public float rotation_w;
    public float rotation_x;
    public float rotation_y;
    public float rotation_z;

    cfg.BuildsLevel buildsLevel = null;

    public BuildingAttr buildingAttr = new BuildingAttr();
    public BuildingAttr buildingAttrExtra = new BuildingAttr();//additional 附加属性
    //public int Attack;
    //public int Defense;
    //public int Health;
    //public int SearchRange;
    //public int AttackRange;
    //public int AttackSpeed;
    //public int Penetrate;
    //public int Power;

    public override List<int> AttackBonus { get => base.AttackBonus; set => base.AttackBonus = value; }//附加额外的buff
    public override int CurHealth { get { return buildingAttr.CurHealth; } set { buildingAttr.CurHealth = value; } }
    public override int Health { get { return buildingAttr.Health; } set { buildingAttr.Health = value; } }

    public BuildingData()
    {

    }

    public BuildingData(cfg.BuildsLevel buildsLevel)
    {
        Init(buildsLevel);
    }
    public override float GetDamage()
    {
        return buildingAttr.Attack;
    }
    public override void Init(EntityController entityController)
    {
        base.Init(entityController);
        Init(config_id, level);
    }

    public void Init()
    {
        if (buildsLevel != null) return;
        buildsLevel = CfgMgr.Instance.Tables.TbBuildsLevel.Get(config_id * 100 + level);
        Init(buildsLevel);
    }

    public void Init(int config_id, int level)
    {
        cfg.BuildsLevel buildLevel = CfgMgr.Instance.Tables.TbBuildsLevel.Get(config_id * 100 + level);
        Init(buildLevel);
    }

    public void Init(cfg.BuildsLevel buildsLevel)
    {
        this.buildsLevel = buildsLevel;
        buildingAttr.Attack = buildsLevel.Attack;
        buildingAttr.Defense = buildsLevel.Defense;
        CurHealth = Health = buildsLevel.Health;
        buildingAttr.SearchRange = buildsLevel.SeachRange;
        buildingAttr.AttackRange = buildsLevel.AttackRange;
        buildingAttr.AttackSpeed = buildsLevel.AttackSpeed;
        buildingAttr.Penetrate = buildsLevel.Penetrate;
        buildingAttr.Power = buildsLevel.Power;
    }

    public override void ChangeAttribute(AttributeChangeData attributeChangeData)
    {
        Func<EBaseCalculateType, float, int, string, string, string, float> func = !attributeChangeData.isReverse ? SkillKit.ProcessCalculate : SkillKit.ProcessCalculateReverse;
        switch (attributeChangeData.attributeType)
        {
            //case cfg.AttributeType.AttackType:
            //    buildingAttr.damageRange = func(attributeChangeData.baseCalculateType, buildingAttr.damageRange, attributeChangeData.value, "damageRange");
            //    break;
            //case cfg.AttributeType.Attack:
            //    buildingAttr.attackDamage = func(attributeChangeData.baseCalculateType, buildingAttr.attackDamage, attributeChangeData.value, "attackDamage");
            //    break;
            //case cfg.AttributeType.Speed:// 降低百分比
            //    buildingAttr.moveSpeed = func(attributeChangeData.baseCalculateType, buildingAttr.moveSpeed, attributeChangeData.value, "moveSpeed");
            //    break;
            case cfg.AttributeType.CurHealth:// 增加x点血量
                CurHealth = (int)func(attributeChangeData.baseCalculateType, CurHealth, attributeChangeData.value, "CurHealth", buildsLevel.Name, entityController.name);
                if (CurHealth > Health)
                {
                    CurHealth = Health;
                }
                entityController.AddBlood();
                break;
            //case cfg.AttributeType.Damage:
            //    buildingAttr.attackDamage = func(attributeChangeData.baseCalculateType, buildingAttr.attackDamage, attributeChangeData.value, "attackDamage");
            //    break;
            case cfg.AttributeType.ExtraDamage:
                buildingAttrExtra.Attack = (int)func(attributeChangeData.baseCalculateType, buildingAttrExtra.Attack, attributeChangeData.value, "Attack", buildsLevel.Name, entityController.name);
                break;
        }
    }
}

public class BuildingAttr
{
    public int Attack;
    public int Defense;
    public int CurHealth;
    public int Health;
    public int SearchRange;
    public int AttackRange;
    public float AttackSpeed;
    public int Penetrate;
    public int Power;
}

public class UnitData : IUnitData
{
    public virtual int CurHealth { get; set; }
    public virtual int Health { get; set; }
    public virtual List<int> AttackBonus { get; set; }//攻击附加debuff

    public EntityController entityController;
    public virtual void ChangeAttribute(AttributeChangeData attributeChangeData)
    {

    }

    public virtual float GetDamage()
    {
        return 0;
    }

    public virtual float GetDamageRange()
    {
        return 0;
    }

    public virtual int GetMoveType()
    {
        return 0;
    }

    public virtual Enum_HeroType GetHeroType()
    {
        return Enum_HeroType.None;
    }

    public virtual EMercenaryType GetMercenaryType()
    {
        return EMercenaryType.NONE;
    }

    public virtual void Init(EntityController entityController)
    {
        this.entityController = entityController;
    }
}

public interface IUnitData
{
    int CurHealth { get; set; }
    int Health { get; set; }
    public List<int> AttackBonus { get; set; }//攻击附加debuff
    void Init(EntityController entityController);
    void ChangeAttribute(AttributeChangeData attributeChangeData);

    float GetDamageRange();

    float GetDamage();

    int GetMoveType();
    Enum_HeroType GetHeroType();
    EMercenaryType GetMercenaryType();
}

public struct AttributeChangeData
{
    public AttributeChangeData(cfg.AttributeType attributeType, int value, cfg.EBaseCalculateType baseCalculateType, bool isReverse = false)
    {
        this.isReverse = isReverse;
        this.attributeType = attributeType;
        this.value = value;
        this.baseCalculateType = baseCalculateType;
    }
    public bool isReverse;//反向运算
    public cfg.AttributeType attributeType;
    public int value;
    public cfg.EBaseCalculateType baseCalculateType;
}