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

    public void Init(int idx)
    {
        GetComponent<Button>().onClick.AddListener(() => UIManager.Instance.ButtonSound());

        this.idx = idx;

        icon.sprite = DataManager.Instance.researches[idx].icon;

        UpdateUI();
    }

    public void UpdateUI()
    {
        level_highlight.fillAmount = ResearchManager.Instance.GetResearchedPercent(idx);
    }

    public void BtnClick()
    {
        UIManager.Instance.shopUI.SelectUpgrade(idx);
    }
}
