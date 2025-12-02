using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using QFramework;
using UnityEngine.UI;
public class HeroCardItem : BaseCardItem
{
    public GameObject bloodObj;
    public override void OnInit(int id,int count)
    {
        ID = id;
        cardType = CardType.Hero;
        // 初始化Hero卡牌的相关数据
        amount = count;// 模拟当前卡牌/英雄已有的数量。即：在兵营中训练的数量
        // 生成哪个具体的英雄？从军队配置中获取，并记录当前英雄的类型以及拥有的数量
        CanClick = true;
        bloodObj.SetActive(false);
        UpgradeAmount(0);
        InitImageGraphic();
    }
    public override void UpgradeAmount(int v)
    {
        amount += v;
        amountLabel.text = "X" + amount.ToString();
        if (amount <= 0)
        {
            CurrentStatus = CardItemStatus.Deployed;
            if (cardType == CardType.Hero)
            {
                transform.Find("bg").GetComponent<Image>().sprite = deployedSprite;
                transform.Find("bg").GetComponent<Image>().SetNativeSize();
            }
            else
            {
                mask.SetActive(true);
            }

            DoUnselectedAnimation();
            CanClick = false;
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (CanClick == false)
        {
            return;
        }
        CurrentStatus = CardItemStatus.Selected;
        if (cardType == CardType.Hero)
        {
            transform.Find("bg").GetComponent<Image>().sprite = selectedSprite;
            transform.Find("bg").GetComponent<Image>().SetNativeSize();

            SetActorImageGrayOrNormal(1);
        }
        TypeEventSystem.Global.Send<HeroCardItem>(this);
        DoSelectedAnimation(() => {
            //Debug.Log($"Hero {gameObject.name} selected!");
        });
    }
}
