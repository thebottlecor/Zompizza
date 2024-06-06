using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResearchUI : MonoBehaviour
{

    public int idx;
    public Image icon;
    public Image level_highlight;

    private Vector2 initSizeDelta;

    public void Init(int idx)
    {
        GetComponent<Button>().onClick.AddListener(() => UIManager.Instance.ButtonSound());

        this.idx = idx;

        icon.sprite = DataManager.Instance.researches[idx].icon;

        initSizeDelta = (transform as RectTransform).sizeDelta;

        UpdateUI();
    }

    public void UpdateUI()
    {
        float value = ResearchManager.Instance.GetResearchedPercent(idx);
        level_highlight.fillAmount = value;
        if (value > 0f)
            gameObject.SetActive(true);

        if (UIManager.Instance.shopUI.currentSelectUpgrade == idx)
        {
            (transform as RectTransform).sizeDelta = 1.25f * initSizeDelta;
        }
        else
        {
            (transform as RectTransform).sizeDelta = initSizeDelta;
        }
    }

    public void BtnClick()
    {
        UIManager.Instance.shopUI.SelectUpgrade(idx);
    }
}
