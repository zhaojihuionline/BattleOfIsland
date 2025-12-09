using cfg;
using log4net.Core;
using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
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
    /// <summary>
    /// Ӣ��id
    /// </summary>
    public int HeroID;
    /// <summary>
    /// Ӣ������
    /// </summary>
    public string HeroName;
    /// <summary>
    /// Ӣ��ģ��
    /// </summary>
    public string HeroModelID;
    /// <summary>
    /// Ӣ�۵ȼ�
    /// </summary>
    public int Level;
    /// <summary>
    /// Ӣ������
    /// </summary>
    public string Description;
    /// <summary>
    /// �ƶ�����
    /// </summary>
    public Enum_MoveType moveType;
    /// <summary>
    /// �ƶ��ٶ�
    /// </summary>
    public float moveSpeed;
    /// <summary>
    /// Ѫ��
    /// </summary>
    public float blood;
    /// <summary>
    /// ��������
    /// </summary>
    public Enum_AttackType attackType;
    /// <summary>
    /// �չ������˺�
    /// </summary>
    public float attackDamage;
    /// <summary>
    /// �չ������ٶ�
    /// </summary>
    public float attackSpeed;
    /// <summary>
    /// �չ�������Χ
    /// </summary>
    public float attackRange;
    public float damageRange;
    
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
        
        HeroID = _cfg_HeroData.Id;
        HeroName = _cfg_HeroData.Name;
        HeroModelID = _cfg_HeroData.Mod;
        Level = heroInfo_level.Level;
        Description = _cfg_HeroData.Description1;
        moveType = _cfg_HeroData.MoveType;
        moveSpeed = heroInfo_level.Speed;
        blood = heroInfo_level.Health;
        attackType = _cfg_HeroData.AttackType;
        attackDamage = heroInfo_level.Attack;
        weaponData = null;
        skillID = heroInfo_level.Skill;
    }

    public HeroData Clone()
    {
        var heroInfo_level = CfgMgr.Instance.Tables.TbHeroLevel.Get(heroInfoPayload.HeroConfigId * 100 + heroInfoPayload.Level);
        
        return new HeroData(heroInfoPayload)
        {
            HeroID = this.CFG_HeroData.Id,
            HeroName = this.CFG_HeroData.Name,
            HeroModelID = this.CFG_HeroData.Mod,
            Level = heroInfo_level.Level,
            Description = this.CFG_HeroData.Description1,
            moveType = this.CFG_HeroData.MoveType,
            moveSpeed = heroInfo_level.Speed,
            blood = heroInfo_level.Health,
            attackType = this.CFG_HeroData.AttackType,
            attackDamage = heroInfo_level.Attack,
            weaponData = null,
            skillID = heroInfo_level.Skill
        };
    }

    public override void ChangeAttribute(AttributeChangeData attributeChangeData)
    {
        switch (attributeChangeData.attributeType)
        {
            case cfg.AttributeType.AttackType:
                damageRange = ProcessCalculate(attributeChangeData.baseCalculateType, damageRange, attributeChangeData.value);
                break;
            case cfg.AttributeType.Attack:
                attackDamage = ProcessCalculate(attributeChangeData.baseCalculateType, attackDamage, attributeChangeData.value);
                break;
            case cfg.AttributeType.Speed:// 降低百分比
                moveSpeed = ProcessCalculate(attributeChangeData.baseCalculateType, moveSpeed, attributeChangeData.value);
                break;
            case cfg.AttributeType.Heal_Singlerhero:// 增加x点血量
                //float finalValue = ProcessCalculate(attributeChangeData.baseCalculateType, blood, attributeChangeData.value);
                entityController.AddBlood(attributeChangeData.value);
                break;
        }
    }

    float ProcessCalculate(cfg.EBaseCalculateType baseCalculateType, float attribute,int value)
    {
        float preAttribute = attribute;
        float fValue = value / 100f;
        switch (baseCalculateType)
        {
            case EBaseCalculateType.Add:
                attribute += fValue;
                Debug.Log($"属性加成 {baseCalculateType}属性增加{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{HeroName} GoName:{entityController.name}");
                break;
            case EBaseCalculateType.Subtract:
                attribute -= fValue;
                Debug.Log($"属性加成 {baseCalculateType}属性减少{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{HeroName} GoName:{entityController.name}");
                break;
            case EBaseCalculateType.Multiply:
                attribute *= (1 + (float)fValue);
                Debug.Log($"属性加成 {baseCalculateType}属性加成{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{HeroName} GoName:{entityController.name}");
                break;
            case EBaseCalculateType.Divide:
                attribute *= (1 - (float)fValue);
                Debug.Log($"属性加成 {baseCalculateType}属性减成{fValue} 加成前：{preAttribute} 加成后：{attribute} HeroName:{HeroName} GoName:{entityController.name}");
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
        return damageRange;
    }

    public override int GetBlood()
    {
        return Mathf.FloorToInt(blood);
    }

    public override void SetBlood(int b)
    {
        blood = b;
    }

    public override int GetMoveType()
    {
        return (int)moveType;
    }

    public override Enum_HeroType GetHeroType()
    {
        return CFG_HeroData.HeroType;
    }
}

public class MercenaryData : UnitData
{
    /// <summary>
    /// С��id
    /// </summary>
    public long SoldierID;
    /// <summary>
    /// С������
    /// </summary>
    public string SoldierName;
    /// <summary>
    /// С��ģ��id
    /// </summary>
    public int SoldierModelID;
    /// <summary>
    /// С���ȼ�
    /// </summary>
    public int Level;
    /// <summary>
    /// С������
    /// </summary>
    public string Description;
    /// <summary>
    /// �ƶ�����
    /// </summary>
    public MoveType moveType;
    /// <summary>
    /// �ƶ��ٶ�
    /// </summary>
    public float moveSpeed;
    /// <summary>
    /// Ѫ��
    /// </summary>
    public float blood;
    /// <summary>
    /// ��������
    /// </summary>
    public AttackType attackType;
    /// <summary>
    /// �չ������˺�
    /// </summary>
    public float attackDamage;
    /// <summary>
    /// �չ������ٶ�
    /// </summary>
    public float attackSpeed;
    /// <summary>
    /// �չ�������Χ
    /// </summary>
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
        this.blood = mercenary.Health;
        this.attackDamage = mercenary.Attack;
        this.attackSpeed = mercenary.AttackSpeed;
        this.attackRange = mercenary.AttackRange;
        Cost = mercenary.Cost;
    }

    public MercenaryData Clone()
    {
        return (MercenaryData)this.MemberwiseClone();
    }
    public override int GetBlood()
    {
        return Mathf.FloorToInt(blood);
    }

    public override void SetBlood(int b)
    {
        blood = b;
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

    public int Attack;
    public int Defense;
    public int Health;
    public int SearchRange;
    public int AttackRange;
    public int AttackSpeed;
    public int Penetrate;
    public int Power;

    public BuildingData(cfg.BuildsLevel buildsLevel)
    {
      this.buildsLevel = buildsLevel;
    }
}

public class UnitData : IUnitData
{
    public EntityController entityController;

    public virtual void ChangeAttribute(AttributeChangeData attributeChangeData)
    {
        
    }

    public virtual float GetDamageRange()
    {
        return 0;
    }

    public virtual int GetBlood()
    {
        return 0;
    }
    public virtual void SetBlood(int b)
    {
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
    void Init(EntityController entityController);
    void ChangeAttribute(AttributeChangeData attributeChangeData);

    float GetDamageRange();
    int GetBlood();
    void SetBlood(int b);
    int GetMoveType();
    Enum_HeroType GetHeroType();
    EMercenaryType GetMercenaryType();
}

public struct AttributeChangeData
{
    public cfg.AttributeType attributeType;
    public int value;
    public cfg.EBaseCalculateType baseCalculateType;
}