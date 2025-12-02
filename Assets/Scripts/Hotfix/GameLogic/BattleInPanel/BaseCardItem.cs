using DG.Tweening;
using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public abstract class BaseCardItem : MonoBehaviour, IPointerClickHandler
{
    public CardItemStatus CurrentStatus { get; set; }
    public CardType cardType { get; set; }
    public int amount { get; set; }

    public int ID { get; set; }

    public TMP_Text amountLabel;
    public GameObject mask;

    public Sprite normalSprite;
    public Sprite selectedSprite;
    public Sprite deployedSprite;
    public Sprite deadSprite;

    public Image actorIcon;
    Material selfMaterial;

    public Image bgCardIcon;
    Material bgCardMaterial;
    public bool CanClick { get; set; }
    public abstract void OnInit(int id,int counts);
    public abstract void OnPointerClick(PointerEventData eventData);
    public abstract void UpgradeAmount(int v);

    protected void InitImageGraphic()
    {
        selfMaterial = new Material(actorIcon.material);
        actorIcon.material = selfMaterial;

        bgCardMaterial = new Material(bgCardIcon.material);
        bgCardIcon.material = bgCardMaterial;
    }
    protected void SetbgCardImageGrayOrNormal(float v)
    {
        bgCardMaterial.SetFloat("_GrayAmount", v);
    }
    protected void SetActorImageGrayOrNormal(float v)
    {
        selfMaterial.SetFloat("_GrayAmount", v);
    }
    public virtual void DoNotClickEffect() {
        amountLabel.DOColor(Color.red, 0.15f).OnComplete(() =>
        {
            amountLabel.DOColor(Color.black, 0.15f);
        });
    }

    public virtual void DoSelectedAnimation(UnityAction callBack = null,bool justScale = false)
    {
        if (justScale)
        {
            GetComponent<RectTransform>().DOScale(1.15f, 0.2f).OnComplete(() =>
            {
                GetComponent<RectTransform>().DOScale(1.0f, 0.2f).OnComplete(() => { 
                    callBack?.Invoke();
                });
            });
        }
        else
        {
            Sequence blendAnim = DOTween.Sequence();
            GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.45f);
            Tweener scaleT = GetComponent<RectTransform>().DOScale(1.15f, 0.2f).OnComplete(() =>
            {
                callBack?.Invoke();
            });
            //blendAnim.Join(moveYT);
            blendAnim.Join(scaleT);
            blendAnim.Play();
        }
    }

    public virtual void DoUnselectedAnimation(UnityAction callBack = null)
    {
        //if (CurrentStatus != CardItemStatus.Deployed)
        //{
        //    Sequence blendAnim = DOTween.Sequence();
        //    GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        //    Tweener scaleT = GetComponent<RectTransform>().DOScale(1.0f, 0.2f).OnComplete(() => {
        //        callBack?.Invoke();
        //    });
        //    if (cardType == CardType.Hero)
        //    {
        //        transform.Find("bg").GetComponent<Image>().sprite = normalSprite;
        //        transform.Find("bg").GetComponent<Image>().SetNativeSize();
        //    }
        //    blendAnim.Join(scaleT);
        //    blendAnim.Play();
        //}
        Sequence blendAnim = DOTween.Sequence();
        GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        Tweener scaleT = GetComponent<RectTransform>().DOScale(1.0f, 0.2f).OnComplete(() => {
            callBack?.Invoke();
        });
        if (cardType == CardType.Hero)
        {
            if (CurrentStatus == CardItemStatus.Deployed)
            {
                transform.Find("bg").GetComponent<Image>().sprite = deployedSprite;
                transform.Find("bg").GetComponent<Image>().SetNativeSize();
            }
            else
            {
                transform.Find("bg").GetComponent<Image>().sprite = normalSprite;
                transform.Find("bg").GetComponent<Image>().SetNativeSize();
            }
        }
        blendAnim.Join(scaleT);
        blendAnim.Play();

    }
}
