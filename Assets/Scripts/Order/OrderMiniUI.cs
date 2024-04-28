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

    public void Init(OrderInfo info)
    {
        prevHp = info.hp;
        pizzaHpGauge.color = DataManager.Instance.uiLibrary.pizaaHpGradient.Evaluate(1f);

        this.info = info;
        isActive = true;

        pin.color = DataManager.Instance.uiLibrary.customerPinColor[info.customerIdx];
        profile.sprite = DataManager.Instance.uiLibrary.customerProfile[info.customerIdx];

        pizzaHpGauge.DOKill();
        pizzaHpGauge.fillAmount = info.hp;
        UpdateTimer(info);

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

        int hour = (int)(remainTime / GM.oneHour);
        int minute = (int)((remainTime - hour * GM.oneHour) / GM.oneMinute);

        if (overLimit)
            timerTMP.text = $"<color=#ff0000>{hour:00}:{minute:00}</color>";
        else
            timerTMP.text = $"{hour:00}:{minute:00}";
    }

    public void UpdateHpGauge(float goalPercent)
    {
        pizzaHpGauge.DOKill();

        //float diff = Mathf.Abs(prevHp - goalPercent) * 100f;
        float diff = 1f;
        float length = Mathf.Max(diff, 0.1f);

        pizzaHpGauge.DOColor(new Color(1f, 0f, 0f, 0.66f), length * 0.1f).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            pizzaHpGauge.DOColor(DataManager.Instance.uiLibrary.pizaaHpGradient.Evaluate(goalPercent), length * 0.1f).SetEase(Ease.OutBack);
        });
        pizzaHpGauge.DOFillAmount(goalPercent, length).SetEase(Ease.OutQuart);
        prevHp = goalPercent;
    }


}
