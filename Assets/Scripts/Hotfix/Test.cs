using QFramework;
using QFramework.UI;
using UnityEngine;

public class Test : MonoBehaviour
{
    [UnityEditor.MenuItem("Test/Test1")]
    static void Test21()
    {
        Time.timeScale = 1;
    }
    [UnityEditor.MenuItem("Test/Test2")]
    static void Test2()
    {
        Time.timeScale = 2;
    }
    [UnityEditor.MenuItem("Test/Test4")]
    static void Test4()
    {
        Time.timeScale = 4;
    }

    [UnityEditor.MenuItem("Test/ÕÐÄ¼°¬Àû·ð")]
    static void DeployHero4()
    {
        //UIKit.GetPanel<BattleInPanel>().DeployHero4();
    }
}
