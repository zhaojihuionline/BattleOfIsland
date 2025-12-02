using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using cfg;

/// <summary>
/// 
/// </summary>
public interface ISkillSystemModel : IModel
{
    TbSkillTable _tables { get; set; }
    SkillTable GetTable(int key);
    List<GameObject> SelfList { get; set; }
    List<GameObject> Target { get; set; }

}
/// <summary>
/// 
/// </summary>
public class SkillSystemModel : AbstractModel, ISkillSystemModel
{
    public TbSkillTable _tables { get; set; }
    public List<GameObject> SelfList { get; set; }
    public List<GameObject> Target { get; set; }

    protected override void OnInit()
    {
        _tables = CfgMgr.Instance.Tables.TbSkillTable;
    }

    public SkillTable GetTable(int key)
    {
        return _tables.Get(key);
    }
}
