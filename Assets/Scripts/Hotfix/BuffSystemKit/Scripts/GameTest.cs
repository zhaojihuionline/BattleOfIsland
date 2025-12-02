//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class GameTest : MonoBehaviour
//{
//    public static GameTest instance;
//    public List<EntityCanBuff> m_Entities;
//    public Text tip_text;

//    public BuffIndex buffIndex;
//    public DataLoader<BuffsJsons> buffJsonsLoader;
//    private void Awake()
//    {
//        instance = this;
//    }
//    private void Start()
//    {
//        // 初始化buff数据加载器
//        buffJsonsLoader = new DataLoader<BuffsJsons>();
//        buffJsonsLoader.Init();

//        for (int i = 0; i < m_Entities.Count; i++)
//        {
//            m_Entities[i].Init();
//        }
//    }

//    private void Update()
//    {
//        // 测试代码
//        foreach (var player in m_Entities)
//        {
//            player.buffRunner.UpdateBuffs();
//            player.GetComponent<IEntityCycle>().EUpdate();
//        }
//        if (Input.GetKeyDown(KeyCode.Alpha1))
//        {
//            m_Entities[0].buffRunner.GiveBuff(m_Entities[0].transform, Utils.GetBuffIDByEnum(buffIndex));
//        }
//        if (Input.GetKeyDown(KeyCode.Space))
//        {
//            m_Entities[0].buffRunner.ExecuteBuffs();
//        }
//    }

//    /// <summary>
//    /// 给所有实体发放一个buff
//    /// </summary>
//    /// <param name="_buffIndex"></param>
//    void GiveBuffForEveryEntity(BuffIndex _buffIndex)
//    {
//        foreach (var entity in m_Entities)
//        {
//            entity.buffRunner.GiveBuff(entity.transform, Utils.GetBuffIDByEnum(_buffIndex));
//        }
//    }
//}