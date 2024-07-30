using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoanManager : Singleton<LoanManager>
{

    public int debt;
    private int displayDebt;

    public float interestRate;
    public int Interest => Mathf.FloorToInt(debt * interestRate);

    public int expireDay;

    public GameObject loanObject;
    public GameObject loanCompleteObject;

    public TextMeshProUGUI congratulationText;

    public TextMeshProUGUI debtText;
    public TextMeshProUGUI interestText;
    public TextMeshProUGUI expireDayText;
    public RepaySilder repaySlider;

    public GameObject loanWarningObj;
    public RectTransform loanWarningRect;
    public TextMeshProUGUI loanWarningBtn_Text;
    public TextMeshProUGUI loanWarning_Text;
    public TextMeshProUGUI loanWarningDetail_Text;

    private TextManager tm => TextManager.Instance;


    public void Init()
    {
        SetDebt(10000);
        interestRate = 0.05f;
        expireDay = 10;

        interestText.text = $"{tm.GetCommons("Interest")} : <color=#CF3838>{Interest}$</color>";
        expireDayText.text = $"{tm.GetCommons("Expire")} <color=#CF3838>{string.Format(tm.GetCommons("Day"), expireDay)}</color>";

        loanWarningBtn_Text.text = tm.GetCommons("Close");
        loanWarning_Text.text = tm.GetCommons("Warning");

        congratulationText.text = tm.GetCommons("Congratulations");
    }

    public void PayInterest()
    {
        int interest = Interest;
        if (interest > 0)
        {
            int possiblePay = Mathf.Min(GM.Instance.gold, interest);

            //GM.Instance.AddGold(-1 * possiblePay, GM.GetGoldSource.loan);

            int remainPay = interest - possiblePay; // 못 갚은 이자
            if (remainPay > 0)
            {
                AddDebt(remainPay);
                interestText.text = $"{tm.GetCommons("Interest")} : <color=#CF3838>{Interest}$</color>";
            }
        }
    }

    public int NextDayLate()
    {
        if (debt > 0)
        {
            expireDay--;
            expireDayText.text = $"{tm.GetCommons("Expire")} <color=#CF3838>{string.Format(tm.GetCommons("Day"), expireDay)}</color>";

            if (expireDay <= 0)
            {
                // 패배
                return 0;
            }
            else if ((expireDay + 1) % 2 == 0)
            {
                // 경고
                return 1;
            }
        }
        return -1;
    }

    public void ShowLoanWarning(bool on)
    {
        loanWarningObj.SetActive(on);

        if (!on)
        {
            GM.Instance.ShowWarningQueue();
        }
        else
        {
            loanWarningDetail_Text.text = string.Format(tm.GetCommons("LoanWarning"), $"<sprite=2>{debt}$", $"<color=#5600FF>{expireDay}</color>");

            loanWarningRect.localScale = 0.01f * Vector3.one;
            loanWarningRect.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.OutElastic).SetUpdate(true);
            // 관리탭 강조하기
            UIManager.Instance.shopUI.SelectSubPanel(1);
        }
    }

    public void Repay(int value)
    {
        int realRepay = Mathf.Min(value, debt);

        if (realRepay > 0 && GM.Instance.gold >= realRepay)
        {
            //GM.Instance.AddGold(-1 * realRepay, GM.GetGoldSource.loan);
            AddDebt(-1 * realRepay);

            interestText.text = $"{tm.GetCommons("Interest")} : <color=#CF3838>{Interest}$</color>";

            AudioManager.Instance.PlaySFX(Sfx.money);
            AudioManager.Instance.PlaySFX(Sfx.complete);

            if (debt == 0)
            {
                GM.Instance.Congratulation(true);
            }
        }
        else
        {
            AudioManager.Instance.PlaySFX(Sfx.deny);
        }
    }

    public void AddDebt(int value)
    {
        int target = debt + value;
        DOVirtual.Int(debt, target, 0.75f, (x) =>
        {
            displayDebt = x;

            debtText.text = $"{tm.GetCommons("Debt")} : <color=#FF7575>{displayDebt}$</color>";

        }).SetEase(Ease.OutCirc).SetUpdate(true);
        debt = target;

        loanObject.SetActive(debt > 0);
        loanCompleteObject.SetActive(debt <= 0);
    }
    public void SetDebt(int value)
    {
        displayDebt = value;
        debt = value;

        debtText.text = $"{tm.GetCommons("Debt")} : <color=#FF7575>{displayDebt}$</color>";

        loanObject.SetActive(debt > 0);
        loanCompleteObject.SetActive(debt <= 0);
    }
}
