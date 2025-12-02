using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using QFramework.UI;


/// <summary>
/// 
/// </summary>
public class GameApp : Architecture<GameApp>
{
    protected override void Init()
    {
        this.RegisterModel<BattleInModel>(new BattleInModel());

        // RegisterModel<ISkillSystemModel>(new SkillSystemModel());
        // RegisterSystem<ISkillSystem>(new SkillSystem());
        RegisterModel<SkillSystemModel>(new SkillSystemModel());
        RegisterSystem<SkillSystem>(new SkillSystem());
        RegisterSystem<EntitySystem>(new EntitySystem());
        
        // 注册背包Tab系统
        RegisterSystem<IBagTabSystem>(new BagTabSystem());
        
        // 注册背包数据模型
        RegisterModel<QFramework.UI.IBagModel>(new QFramework.UI.BagModel());

        //RegisterSystem<GlobalBuffRunnerSystem>(new GlobalBuffRunnerSystem());
    }
}
