using QFramework;
using QFramework.Game;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// buffִ执行器
/// </summary>
public class BuffRunner : IRunner
{
    // buff
    public List<Buff> buffDatas;
    // buff
    public List<BuffEntity> buffEntities { get; private set; }

    public void Init()
    {
        buffDatas = new List<Buff>();
        buffEntities = new List<BuffEntity>();
    }
    /// <summary>
    /// 获得一个buff组
    /// </summary>
    /// <param name="target"></param>
    /// <param name="buffids"></param>
    public void GiveBuff(Transform target, params int[] buffids)
    {
        for (int i = 0; i < buffids.Length; i++)
        {
            int buffid = buffids[i];
            bool notmatter = true;

            for (int j = 0; j < buffDatas.Count; j++)
            {
                if (buffDatas[j].id == buffid)
                {
                    notmatter = false;
                    if (buffDatas[j].isOverlay == false)
                    {
                        buffDatas.RemoveAt(j);
                        MonoBehaviour.Destroy(buffEntities[j]);
                        buffEntities.RemoveAt(j);

                        CeateBuff(buffid);
                    }
                    break;
                }
            }

            if (notmatter == true)
            {
                CeateBuff(buffid);
            }
        }

        CreateAllBuffEntity(target);
    }
    /// <summary>
    /// 获得一个buff
    /// </summary>
    /// <param name="target"></param>
    /// <param name="buffid">buffid</param>
    public BuffRunner GiveBuff(Transform target, int buffid)
    {
        if (target.GetComponent<EntityController>() != null)
        {
            bool notmatter = true;
            for (int i = 0; i < buffDatas.Count; i++)
            {
                if (buffDatas[i].id == buffid)
                {
                    notmatter = false;
                    if (buffDatas[i].isOverlay == false)
                    {
                        buffDatas.RemoveAt(i);
                        MonoBehaviour.Destroy(buffEntities[i]);
                        buffEntities.RemoveAt(i);
                        CeateBuff(buffid);
                        AddNewBuffEntity(target, buffDatas[i]);
                        //UIManager.Instance.GenBuffCell(buffEntities[i]);
                    }
                    else
                    {
                        buffEntities[i].UpgradeEffect();
                        //UIManager.Instance.UpgradeBuffCell(buffEntities[i]);
                    }
                    break;
                }
            }
            if (notmatter == true)
            {
                CeateBuff(buffid);
                Buff b = buffDatas.Where(b => b.id == buffid).First();
                BuffEntity _buffEntity = AddNewBuffEntity(target, b);
                //UIManager.Instance.GenBuffCell(_buffEntity);
            }
        }
        else
        {
            Debug.Log("No EntityController");
        }
        return this;
    }
    /// <summary>
    /// 根据id创建buff数据
    /// </summary>
    /// <param name="bid"></param>
    void CeateBuff(int bid)
    {
        //BuffProtocol buff_Protocol =  BattleManager.Instance.buffJsonsLoader.configDatas.GetBuffProtocolById(bid);
        cfg.BuffTable buff_Protocol = CfgMgr.Instance.Tables.TbBuff.Get(bid);
        //List<(int id, int value)> result = Utils.ParseString(buff_Protocol.effects);
        List<Effect> _effects = new List<Effect>();
        var results = buff_Protocol.BuffEffect;

        for (int i = 0; i < results.Count; i++)
        {
            Effect newEffect = new Effect(buff_Protocol,i);
            _effects.Add(newEffect);
        }
        Buff _buff = new Buff(
            id: buff_Protocol.Id,
            name: buff_Protocol.Name,
            isdebuff: buff_Protocol.Isdebuff,
            buffKind: BuffKind.Burn,
            selectedSprite: null,
            des: buff_Protocol.Des,
            effects: _effects,
            bDuration: buff_Protocol.Time,
            isUsed: false,
            isOverlay: buff_Protocol.Overlay,
            buff_fxName: buff_Protocol.BuffFX
        );

        buffDatas.Add(_buff);
    }
    /// <summary>
    /// 创建buff实体
    /// </summary>
    /// <param name="target"></param>
    void CreateAllBuffEntity(Transform target)
    {
        for (int i = 0; i < buffDatas.Count; i++)
        {
            BuffEntity newBuffEntity = target.gameObject.AddComponent<BuffEntity>();
            newBuffEntity.Init(buffDatas[i], target, this);
            buffEntities.Add(newBuffEntity);
        }

        
    }

    BuffEntity AddNewBuffEntity(Transform target, Buff buff)
    {
        BuffEntity newBuffEntity = target.gameObject.AddComponent<BuffEntity>();
        newBuffEntity.Init(buff, target, this);
        buffEntities.Add(newBuffEntity);
        return newBuffEntity;
    }

    /// <summary>
    /// 更新buff
    /// </summary>
    public void UpdateBuffs()
    {
        for (int i = 0; i < buffEntities.Count; i++)
        {
            buffEntities[i].BUpdate();
        }
    }
    /// <summary>
    /// 升级buff
    /// </summary>
    public void UpgradeBuffs()
    {
        if (buffEntities.Count > 0)
        {
            for (int i = 0; i < buffEntities.Count; i++)
            {
                buffEntities[i].Upgrade(1);
            }
        }
    }
    /// <summary>
    /// 升级指定id的buff
    /// </summary>
    /// <param name="id"></param>
    public void UpgradeBuff(int id)
    {
        if (buffEntities.Count > 0 && buffEntities[id] != null)
        {
            buffEntities[id].Upgrade(1);
        }
    }

    /// <summary>
    /// 执行buff
    /// </summary>
    public void ExecuteBuffs()
    {
        for (int i = buffEntities.Count - 1; i >= 0; i--)
        {
            if (buffEntities[i].isUsed == false)
            {
                buffEntities[i].Execute();
            }
        }
    }

    // 执行指定id的buff
    public void ExecuteBuff(int id)
    {
        if (buffEntities.Count > 0 )
        {
            for (int i = 0; i < buffEntities.Count; i++)
            {
                if (buffEntities[i].buff.id == id && buffEntities[i].isUsed == false)
                {
                    buffEntities[i].Execute();
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 移除buff
    /// </summary>
    public void RemoveBuffEntity()
    {
        for (int i = 0; i < buffEntities.Count; i++)
        {
            if (buffEntities[i].isUsed)
            {
                MonoBehaviour.Destroy(buffEntities[i]);
                //UIManager.Instance.RemoveBuffCell(buffEntities[i].buff.id);
                buffEntities.RemoveAt(i);
            }
        }

        // ��ʹ�õ�buff�����ռ�
        HashSet<int> usedDataIds = new HashSet<int>();
        foreach (var entity in buffEntities)
        {
            usedDataIds.Add(entity.buff.id);
        }
        // �Ƴ���ʹ�õ�buff����
        for (int i = buffDatas.Count - 1; i >= 0; i--)
        {
            if (!usedDataIds.Contains(buffDatas[i].id))
            {
                buffDatas.RemoveAt(i);
            }
        }
    }

    // 移除指定id的buff实体
    public void RemoveBuffEntity(int id)
    {
        for (int i = 0; i < buffEntities.Count; i++)
        {
            if (buffEntities[i].buff.id == id)
            {
                MonoBehaviour.Destroy(buffEntities[i]);
                buffEntities.RemoveAt(i);
                break;
            }
        }

        HashSet<int> usedDataIds = new HashSet<int>();
        foreach (var entity in buffEntities)
        {
            usedDataIds.Add(entity.buff.id);
        }

        for (int i = buffDatas.Count - 1; i >= 0; i--)
        {
            if (!usedDataIds.Contains(buffDatas[i].id))
            {
                buffDatas.RemoveAt(i);
            }
        }
    }
}
