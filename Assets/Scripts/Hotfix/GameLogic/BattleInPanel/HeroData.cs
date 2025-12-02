using cfg;
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
public class HeroData
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
    /// <summary>
    /// ��������
    /// </summary>
    public WeaponData weaponData;
    /// <summary>
    /// ����ID�б�(��������Ϊbuff����������ֱ��д����buff��id����buffϵͳ������)
    /// </summary>
    public List<int> skillID;

    public cfg.Hero CFG_HeroData;

    PitayaGame.GameSvr.HeroData heroInfoPayload;
    public HeroData(PitayaGame.GameSvr.HeroData _heroInfoPayload)
    {
        this.heroInfoPayload = _heroInfoPayload;
        // �õ������ļ��е�Ӣ������
        var _cfg_HeroData = CfgMgr.Instance.Tables.TbHero.Get(_heroInfoPayload.HeroConfigId);
        this.CFG_HeroData = _cfg_HeroData;
        // ���ݴӷ������ϻ�ȡ����Ӣ�۵ȼ����õ���Ӧ��Ӣ�۵ĵȼ�������
        var heroInfo_level = CfgMgr.Instance.Tables.TbHeroLevel.Get(_heroInfoPayload.HeroConfigId * 100 + _heroInfoPayload.Level);

        // ��ȡӢ�۵ȼ�������
        //var heroInfo_level_linq = CfgMgr.Instance.Tables.TbHeroLevel.DataList
        //    .Where(h => h.Level == heroInfoPayload.Level).FirstOrDefault();
        // ͨ���õ��ĵȼ������ݣ�����ʼ��Ӣ������
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
        // ���HeroData
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
}

public class MercenaryData
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

    public MercenaryData(cfg.Mercenary mercenary)
    {
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
public class BuildingData
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
}