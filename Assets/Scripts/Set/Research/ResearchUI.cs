using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResearchUI : MonoBehaviour
{

    public Image bg;

    public int idx;
    public Image icon;
    public Image level_highlight;

    public TextMeshProUGUI romanNumText;

    private Vector2 initSizeDelta;
    private Vector3 initRomanScale;

    public void Init(int idx)
    {
        GetComponent<Button>().onClick.AddListener(() => UIManager.Instance.ButtonSound());

        this.idx = idx;

        var info = DataManager.Instance.researches[idx];
        icon.sprite = info.icon;

        int roman = info.romanNum;
        if (roman > 0)
        {
            switch (roman)
            {
                case 1:
                    romanNumText.text = "II";
                    break;
                case 2:
                    romanNumText.text = "III";
                    break;
                case 3:
                    romanNumText.text = "IV";
                    break;
                case 4:
                    romanNumText.text = "V";
                    break;
            }

            romanNumText.gameObject.SetActive(true);
        }
        else
            romanNumText.gameObject.SetActive(false);

        initSizeDelta = (transform as RectTransform).sizeDelta;

        if (initSizeDelta.x >= 96f)
            romanNumText.rectTransform.localScale = Vector3.one * 1.25f;

        initRomanScale = romanNumText.rectTransform.localScale;

        UpdateUI();
    }

    public void UpdateUI()
    {
        float value = ResearchManager.Instance.GetResearchedPercent(idx);
        level_highlight.fillAmount = value;
        if (value > 0f)
            gameObject.SetActive(true);

        bool tierOkay = ResearchManager.Instance.CheckCanUnlocked_ByTier(idx);
        bool researchable = false;
        if (tierOkay)
            researchable = ResearchManager.Instance.CheckCanUnlocked(idx);

        if (UIManager.Instance.shopUI.currentSelectUpgrade == idx)
        {
            (transform as RectTransform).sizeDelta = 1.25f * initSizeDelta;
            romanNumText.rectTransform.localScale = initRomanScale * 1.25f;

            if (researchable)
                bg.color = DataManager.Instance.uiLib.button_HighlightColor;
            else
            {
                if (!tierOkay)
                    bg.color = DataManager.Instance.uiLib.button_Upgrade_needTier;
                else
                    bg.color = DataManager.Instance.uiLib.button_Upgrade_needRes;
            }
        }
        else
        {
            (transform as RectTransform).sizeDelta = initSizeDelta;
            romanNumText.rectTransform.localScale = initRomanScale;

            if (researchable)
                bg.color = DataManager.Instance.uiLib.button_Upgrade;
            else
            {
                if (!tierOkay)
                    bg.color = DataManager.Instance.uiLib.button_Upgrade_needTier;
                else
                    bg.color = DataManager.Instance.uiLib.button_Upgrade_needRes;
            }
        }
    }

    public void BtnClick()
    {
        UIManager.Instance.shopUI.SelectUpgrade(idx);
    }
}
