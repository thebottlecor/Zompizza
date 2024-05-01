using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class OrderMiniUI : MonoBehaviour
{
    public Image pin;
    public Image profile;

    public Image pizzaHpGauge;

    public TextMeshProUGUI timerTMP;

    public bool isActive;
    public OrderInfo info;

    private float prevHp;

    public Image emoji;
    public Image profileBg;

    UILibrary uiLib => DataManager.Instance.uiLib;

    public void Init(OrderInfo info)
    {
        prevHp = info.hp;
        pizzaHpGauge.color = uiLib.pizaaHpGradient.Evaluate(1f);

        this.info = info;
        isActive = true;

        pin.color = uiLib.customerPinColor[info.customerIdx];
        profile.sprite = uiLib.customerProfile[info.customerIdx];

        pizzaHpGauge.DOKill();
        pizzaHpGauge.fillAmount = info.hp;
        UpdateTimer(info);

        emoji.gameObject.SetActive(false);
        profileBg.color = uiLib.miniOrderUI_maskColor;

        gameObject.SetActive(false);
    }
    public void Hide()
    {
        info = null;
        isActive = false;
        gameObject.SetActive(false);
    }

    public void UpdateTimer(OrderInfo info)
    {
        float remainTime = info.timeLimit - info.timer;
        bool overLimit = false;

        if (remainTime <= 0)
        {
            remainTime *= -1f;
            overLimit = true;
        }

        int hour = (int)(remainTime / Constant.oneHour);
        int minute = (int)((remainTime - hour * Constant.oneHour) / Constant.oneMinute);

        if (overLimit)
        {
            timerTMP.text = $"<color=#ff0000>{hour:00}:{minute:00}</color>";
        }
        else
            timerTMP.text = $"{hour:00}:{minute:00}";

        bool angry = CalcAngry(overLimit, info.hp);
        emoji.gameObject.SetActive(angry);
        if (angry)
            profileBg.color = uiLib.miniOrderUI_maskColor_angry;
        else
            profileBg.color = uiLib.miniOrderUI_maskColor;
    }

    private bool CalcAngry(bool overLimit, float hpPercent)
    {
        int rate = 100;

        if (overLimit) rate -= 100;

        rate += (int)(rate - 100 + (100 * hpPercent));

        return rate <= 0;
    }

    public void UpdateHpGauge(float goalPercent)
    {
        pizzaHpGauge.DOKill();

        float diff = Mathf.Abs(prevHp - goalPercent) * 100f;
        float length = Mathf.Min(diff, 1f);

        pizzaHpGauge.DOColor(new Color(1f, 0f, 0f, 0.66f), length * 0.1f).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            pizzaHpGauge.DOColor(uiLib.pizaaHpGradient.Evaluate(goalPercent), length * 0.1f).SetEase(Ease.OutBack);
        });
        pizzaHpGauge.DOFillAmount(goalPercent, length).SetEase(Ease.OutQuart);
        prevHp = goalPercent;
    }


}
