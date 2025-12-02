using DG.Tweening;
using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MercenaryCardItem : BaseCardItem
{
    public int Cost { get; set; }
    public override void OnInit(int id,int count)
    {
        ID = id;
        cardType = CardType.Soldier;
        amount = count;// 模拟当前卡牌/士兵已有的数量。即：在兵营中训练的数量
        CanClick = true;
        UpgradeAmount(0);
        InitImageGraphic();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (CanClick == false)
        {
            return;
        }
        if(PlayerManager.Instance.GetLocalPlayer().playerData.TotalCostMercenaryPoints <= 0)
        {
            return;
        }
        CurrentStatus = CardItemStatus.Selected;

        TypeEventSystem.Global.Send<MercenaryCardItem>(this);
        DoSelectedAnimation(() => {
            Debug.Log($"{gameObject.name} selected!");
        });
    }

    public override void UpgradeAmount(int v)
    {
        //amount += v;
        amountLabel.text = "X1";
        PlayerManager.Instance.GetLocalPlayer().playerData.TotalCostMercenaryPoints -= Cost;
        Debug.Log($"还剩{PlayerManager.Instance.GetLocalPlayer().playerData.TotalCostMercenaryPoints}雇佣点数");
        if (PlayerManager.Instance.GetLocalPlayer().playerData.TotalCostMercenaryPoints <= 0)
        {
            mask.SetActive(true);
            CanClick = false;

            DoUnselectedAnimation();
        }
    }
}
