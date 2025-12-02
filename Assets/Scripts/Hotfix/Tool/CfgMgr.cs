using cfg;
using QFramework;
using UnityEngine;

public class CfgMgr : Singleton<CfgMgr>
{
    ResLoader loader;
    public Tables Tables { get { return tables; } }
    private Tables tables { get; set; }

    public static SkillTable GetSkillTableS(int key) => Instance.GetSkillTable(key);

    private CfgMgr()
    {
        loader = ResLoader.Allocate();
        Init();
    }

    public void Init()
    {
        tables = new cfg.Tables(file =>
        {
            var textAsset = loader.LoadSync<TextAsset>(file);
            if (textAsset == null)
                return null;
            return Newtonsoft.Json.Linq.JArray.Parse(textAsset.text);
        });
    }

    public SkillTable GetSkillTable(int key)
    {
        return tables.TbSkillTable.Get(key);
    }

    public override void Dispose()
    {
        loader.Dispose();
        loader = null;
        base.Dispose();
    }

    public int GetMercenaryIdBySoldierCardItemID(int cardItemID)
    {
        if (cardItemID == 0)
        {
            return 10001;
        }
        else if (cardItemID == 1)
        {
            return 20001;
        }
        else if (cardItemID == 2)
        {
            return 30001;
        }
        else
        {
            return 10001;
        }
    }
}
