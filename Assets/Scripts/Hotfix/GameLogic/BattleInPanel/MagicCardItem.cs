using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MagicCardItem : BaseCardItem
{
    public override void OnInit(int id,int count)
    {
        ID = id;
        cardType = CardType.Magic;
        amount = count;// 模拟当前卡牌/士兵已有的数量。即：在兵营中训练的数量
        CanClick = true;
        UpgradeAmount(0);
        InitImageGraphic();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        //if (CanClick == false)
        //{
        //    return;
        //}
        CurrentStatus = CardItemStatus.Selected;
        //CanClick = false;
        DoSelectedAnimation(() => {
            TypeEventSystem.Global.Send<MagicCardItem>(this);
        }, true);
    }

    public override void UpgradeAmount(int v)
    {
        amount += v;
        amountLabel.text = "X" + amount.ToString();
        if (amount <= 0)
        {
            mask.SetActive(true);
            CanClick = false;

            DoUnselectedAnimation();
        }
    }
}
