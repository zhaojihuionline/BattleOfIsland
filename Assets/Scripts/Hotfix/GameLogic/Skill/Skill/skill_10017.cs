using QFramework.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skill_10017 : SkillController
{
    [SerializeField] GameObject Model;
    protected override void OnStart_WindUp()
    {
        base.OnStart_WindUp();
        Model.SetActive(false);
        KeepAlive = true;
    }
    protected override void OnStart_Cast()
    {
        base.OnStart_Cast();
        Debug.Log("skill_1001701 技能施法开始");
        Model.SetActive(true);
    }
}
