using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VillagerSosMiniUI : MonoBehaviour
{
    public Image pin;
    public Image profile;

    public Image timerGauge;

    public TextMeshProUGUI timerTMP;
    public TextMeshProUGUI rescuedTMP;

    public Image profileBg;

    public GameObject[] pinObjs;
    public GameObject[] infoObjs;

    UILibrary uiLib => DataManager.Instance.uiLib;

    public void Init(int villagerIdx)
    {
        //pin.color = uiLib.customerPinColor[villagerIdx];
        profile.sprite = uiLib.villagerProfile[villagerIdx];

        UpdateTimer();

        profileBg.color = uiLib.miniOrderUI_maskColor;

        rescuedTMP.text = TextManager.Instance.GetCommons("Rescued");

        pinObjs[0].SetActive(true);
        pinObjs[1].SetActive(false);
        infoObjs[0].SetActive(true);
        infoObjs[1].SetActive(false);

        gameObject.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(this.transform as RectTransform);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void RescuedMode()
    {
        pinObjs[0].SetActive(false);
        pinObjs[1].SetActive(true);
        infoObjs[0].SetActive(false);
        infoObjs[1].SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(this.transform as RectTransform);
    }

    public void UpdateTimer()
    {
        float remainTime = VillagerManager.Instance.sosTimeLimit - VillagerManager.Instance.sosTimer;

        if (remainTime <= 0)
        {
            // ½ÇÆÐ!
            VillagerManager.Instance.FailToRescue();
            return;
        }

        int hour = (int)(remainTime / Constant.oneHour);
        int minute = (int)((remainTime - hour * Constant.oneHour) / Constant.oneMinute);

        timerGauge.fillAmount = remainTime / VillagerManager.Instance.sosTimeLimit;
        timerTMP.text = $"{hour:00}:{minute:00}";

        if (timerGauge.fillAmount <= 0.5f)
            profileBg.color = uiLib.miniOrderUI_maskColor_angry;
        else
            profileBg.color = uiLib.miniOrderUI_maskColor;
    }
}
