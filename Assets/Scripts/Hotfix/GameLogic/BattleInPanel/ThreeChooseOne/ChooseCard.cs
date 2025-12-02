using DG.Tweening;
using QFramework.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using QFramework;
using UnityEngine.Events;
public class ChooseCardApp : Architecture<ChooseCardApp>
{
    protected override void Init()
    {
        this.RegisterModel<ChooseCardModel>(new ChooseCardModel());
    }
}
public class ChooseCardModel : AbstractModel
{
    public string cardName;
    public string cardDescription;
    public int cardLevel;
    protected override void OnInit()
    {
        this.cardName = "火球术";
        this.cardDescription = "对敌人造成大量火焰伤害";
        this.cardLevel = 1;
    }
}
public class ChooseCard : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IController
{
    public Image cardBg;// 卡片背景，需要根据品质变换
    public Image cardIcon;// 卡片图标
    public TMP_Text cardName;// 卡片名称
    public TMP_Text cardDescription;// 卡片描述
    public TMP_Text cardLevel;// 卡片等级

    public Button btn_ReRoll;

    ChooseCardModel cardModel;
    public void Init()
    {
        // 在这里进行卡牌数据的初始化，每次打开三选一界面都会调用，且每次数据都不一样
        PlayAnim(null);
        OnRoll();

        btn_ReRoll.onClick.AddListener(OnRoll);
    }

    void PlayAnim(UnityAction callBack)
    {
        // 播放卡牌进入的一个旋转动画
        Transform body = transform.Find("body");
        body.DORotate(new Vector3(0, 360, 0), 1.0f + Random.Range(0, 0.8f), RotateMode.FastBeyond360).SetEase(Ease.OutBack).SetUpdate(UpdateType.Normal, true).OnComplete(() =>
        {
            callBack();
        });
    }

    void OnRoll()
    {
        Debug.Log("初始化一个法术卡牌数据");
        
        cardModel = this.GetModel<ChooseCardModel>();

        cardName.text = cardModel.cardName;
        cardDescription.text = cardModel.cardDescription;
        cardLevel.text = cardModel.cardLevel.ToString();


    }
    public void OnPointerUp(PointerEventData eventData)
    {
        transform.Find("body").DOScale(Vector3.one, 0.1f).SetUpdate(UpdateType.Normal, true).OnComplete(() =>
        {
            // 将法术数据存到玩家法术数据中
            BattleManagerView.Instance.battleInPanel.ResetMagicSkillContainerParent();
        });
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        transform.Find("body").DOScale(Vector3.one * 0.9f, 0.1f).SetUpdate(UpdateType.Normal,true);
    }
    public void OnDispose()
    {
        // 在这里进行卡牌数据的清理
        // 移除cardModel中的数据
        cardName.text = "";
        cardDescription.text = "";
        cardLevel.text = "";
        transform.Find("body").DOScale(Vector3.one, 0f);
    }

    public IArchitecture GetArchitecture()
    {
        return ChooseCardApp.Interface;
    }
}
