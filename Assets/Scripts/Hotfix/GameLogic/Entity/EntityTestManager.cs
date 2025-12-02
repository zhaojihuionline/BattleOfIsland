using System.Collections;
using System.Collections.Generic;
using QFramework;
using DG;
using UnityEngine;
using UnityEngine.PlayerLoop;
using DG.Tweening;

public class EntityTestManager : MonoBehaviour, IController
{
    void Awake()
    {
        // StrategyAutoRegister.AutoRegisterStrategies();
        
    }
    private void Update()
    {
        //现在制作一件事 就是点击按钮生成一个英雄
        if (Input.GetKeyDown(KeyCode.Q))
        {
            var aaa = this.GetModel<BattleInModel>();
            this.SendCommand(new ReleaseSkillCommand(10001, 1, gameObject, aaa.opponent_allEntitys[0], Vector3.zero));
        }
        //现在制作一件事 就是点击按钮生成一个英雄
        if (Input.GetKeyDown(KeyCode.W))
        {
            EntitySystem system = this.GetSystem<EntitySystem>();
            //简单传入id和等级
            system.CreatEntityHreo(1202, 1, Vector3.zero);
        }

        //现在制作一件事 就是点击按钮生成一个箭塔
        if (Input.GetKeyDown(KeyCode.S))
        {
            EntitySystem system = this.GetSystem<EntitySystem>();
            //简单传入id和等级
            system.CreatEntityBuilding(14001, 1, Vector3.zero, true);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {

            //释放法术牌
            var table = CfgMgr.GetSkillTableS(20001801);
            this.SendCommand<ReleaseSpellCommand>(new ReleaseSpellCommand(table, null, null, Vector3.zero));
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            EntitySystem system = this.GetSystem<EntitySystem>();
            //简单传入id和等级
            system.CreatEntitySoldier(30001, 1, Vector3.zero);
        }

        if (Input.GetKeyDown(KeyCode.Space))/*  */
        {
            ShootArrow();
        }
    }
    public IArchitecture GetArchitecture()
    {
        return GameApp.Interface;
    }

    public GameObject arrowPrefab;
    public Transform target;

    private void ShootArrow()
    {
        GameObject arrowObj = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
        arrowObj.SetActive(true);
        BPathMove pathConfig = arrowObj.GetComponent<BPathMove>();
        // pathConfig.DOTweenMovePath(target.position);
        pathConfig.DOTweenMovePath(target);

    }
}
