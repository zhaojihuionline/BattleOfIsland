using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework.Game;

public class skill_10009 : SkillController
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
        Debug.Log("skill_10009 技能施法开始");
        Model.SetActive(true);
    }
}
