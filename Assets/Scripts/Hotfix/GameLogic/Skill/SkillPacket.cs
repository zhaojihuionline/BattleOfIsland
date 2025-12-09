using System.Collections.Generic;
using System.Linq;
using cfg;
using UnityEngine;

/// <summary>
/// 技能数据包 包括释放者 目标 位置之类的
/// 
/// </summary>
public class SkillPacket
{
    public SkillTable _data;
    public DamageDate damageDate = new DamageDate();
    //释放者
    public GameObject caster = null;
    //目标
    public GameObject target 
    {
        get { return targetData.Target;}
        set {targetData.Target = value;}
    }

    //aoe伤害时，aoe范围内的目标单位（包括target）
    public List<GameObject> targets
    {
        get { return targetData.Targets; }
        set { targetData.Targets = value; }
    }
    TargetData targetData;
    public TargetData TargetData { get { return targetData; } set { targetData = value; } }

    //目标点
    public Vector3 targetPoint = Vector3.zero;
    //剩余穿透次数 
    public int _passcount = -1;
    //穿透类型 
    public SkillPassType skillPassType = SkillPassType.NONE;

    public Effect _effect = null;

    int skillEnable = 0;
    bool _canRelease = false;
    public bool CanRelease
    {
        get
        {
            //if (battleInModel.DestructionRate.Value * 100 >= skillEnable)//开启
            {
                return _canRelease;
            }
            //else
            {
                return false;
            }
        }
        set
        {
            _canRelease = value;
        }
    }

    BattleInModel battleInModel;

    public SkillPacket()
    {

    }
    public SkillPacket(SkillTable _data, BattleInModel battleInModel, int skillEnable)
    {
        this.battleInModel = battleInModel;
        this.skillEnable = skillEnable;
        SetTable(_data);
    }

    public void Init(SkillTable _data, GameObject caster, TargetData target, Vector3 point)
    {
        SetTable(_data);
        SetCaster(caster);
        SetTarget(target, point);
    }

    public void SetTable(SkillTable _data)
    {
        this._data = _data;
        skillPassType = _data.PassType.Keys.FirstOrDefault();
        _passcount = _data.PassType.Values.FirstOrDefault();

    }
    public void SetCaster(GameObject caster)
    {
        this.caster = caster;
    }

    public void SetTarget(TargetData target, Vector3 point)
    {
        this.targetData = target;
        targetPoint = point;
    }

    //设置伤害值没放在初始化里边
    public void SetDamsge(DamageDate damage)
    {
        damageDate = damage;
    }

    public void Clear()
    {
        damageDate = null;
        caster = null;
        target = null;
        targetPoint = Vector3.zero;
        _passcount = -1;
        skillPassType = SkillPassType.NONE;
        _effect = null;
        _canRelease = false;
        targetData = default;
    }
}
