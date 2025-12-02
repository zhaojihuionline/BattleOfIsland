using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeChooseOneManager : MonoSingleton<ThreeChooseOneManager>
{
    public List<ChooseCard> chooseCards;
    public void OnOpen()
    {
        foreach (var card in chooseCards)
        {
            card.Init();
        }
        UnityEngine.Time.timeScale = 0;
    }

    public void OnClose()
    {
        foreach (var card in chooseCards)
        {
            card.OnDispose();
        }
    }
}
