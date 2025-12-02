using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BuffEntity : MonoBehaviour
{
    public Buff buff { get; private set; }// buff数据

    public bool isDebuff;
    public BuffKind buffKind;
    public string bName;
    public string bDescription;
    public float bDuration;
    public bool isUsed;
    public bool isOverlay;
    public List<EffectEntity> effects;// 效果器实体列表
    public Transform target;// 需要作用于的目标引用

    private BuffRunner buffRunner;

    float defalutValueDuration;
    /// <summary>
    /// Buff等级
    /// </summary>
    public int Level { get; set; }
    /// <summary>
    /// 是否是常驻buff
    /// </summary>
    public bool IsResident { get; set; }
    public string buff_fxName;// buff特效名称
    public void Init(Buff _buff,Transform target,BuffRunner _buffRunner)
    {
        this.effects = new List<EffectEntity>();
        this.buff = _buff;
        this.isDebuff = _buff.isdebuff;
        this.buffKind = _buff.buffKind;
        this.bName = _buff.name;
        this.bDescription = _buff.des;
        this.bDuration = _buff.bDuration;
        this.isUsed = _buff.isUsed;
        this.isOverlay = _buff.isOverlay;
        this.buffRunner = _buffRunner;
        this.target = target;
        this.Level = 1;
        this.buff_fxName = _buff.buff_fxName;
        defalutValueDuration = this.bDuration;
        IsResident = _buff.isResident;
        // 根据配置文件中的效果器数据列表生成效果器实体
        for (int i = 0; i < _buff.effects.Count; i++)
        {
            var data = _buff.effects[i];
            //var effectEntity = EffectFactory.CreateEffect((AttributeType)data.attributeID);
            //if (effectEntity != null)
            //{
            //    effectEntity.Init(data, this, target);
            //    effects.Add(effectEntity);
            //}
            var effectEntity = GenericFactoryEx<cfg.AttributeType, EffectEntity>.Create((cfg.AttributeType)data.attributeID);
            if (effectEntity != null)
            {
                effectEntity.Init(data, this, target);
                effects.Add(effectEntity);
            }
        }
    }
    /// <summary>
    /// 升级buff
    /// </summary>
    /// <param name="_level"></param>
    public void Upgrade(int _level)
    {
        UpgradeLevel(_level);
        UpgradeEffect();
    }
    /// <summary>
    /// 等级数增加
    /// </summary>
    /// <param name="_level"></param>
    void UpgradeLevel(int _level)
    {
        this.Level += _level;
    }
    /// <summary>
    /// 更新所有效果器的最新数据
    /// </summary>
    public void UpgradeEffect()
    {
        Debug.Log("效果器数为: " + effects.Count);
        for (int i = 0; i < effects.Count; i++)
        {
            effects[i].eDuration += defalutValueDuration * Level;
            effects[i].effect.attributeValue += effects[i].defauleAttributeValue * Level;
            Debug.Log("进入更新" + effects[i].effect.attributeValue);
            this.bDuration = effects[i].eDuration;
        }
    }
    /// <summary>
    /// 执行所有效果器
    /// </summary>
    public void Execute()
    {
        for (int i = 0; i < effects.Count; i++)
        {
            effects[i].Execute();
        }
    }
    /// <summary>
    /// 更新所有效果器
    /// </summary>
    public void BUpdate()
    {
        for (int i = 0; i < effects.Count; i++)
        {
            effects[i].Update();
        }
    }

    /// <summary>
    /// 移除效果器
    /// </summary>
    /// <param name="effect">效果器实例对象</param>
    public void RemoveEffect(EffectEntity effect)
    {
        if (effect.IsFinished)
        {
            int id = effects.IndexOf(effect);
            effect.OnExit();
            effects.RemoveAt(id);
        }
        if (effects.Count <= 0)// 所有效果执行完成
        {
            this.isUsed = true;
            buffRunner.RemoveBuffEntity();
        }
    }
}
