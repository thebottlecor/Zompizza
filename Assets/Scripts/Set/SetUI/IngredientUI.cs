using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientUI : MonoBehaviour
{

    public Image icon;
    public TextMeshProUGUI detail;
    public GameObject highlight;

    public Ingredient info;

    private bool valid;

    public void Init(Ingredient info)
    {
        this.info = info;
        icon.sprite = DataManager.Instance.uiLib.ingredients[info];
        highlight.SetActive(false);

        CheckValid();
    }

    public void CheckValid()
    {
        var ingLib = DataManager.Instance.ingredientLib;

        int tier = ResearchManager.Instance.globalEffect.tier;

        if (ingLib.meats.ContainsKey(info) && ingLib.meats[info].valid && ingLib.meats[info].tier <= tier ||
            ingLib.vegetables.ContainsKey(info) && ingLib.vegetables[info].valid && ingLib.vegetables[info].tier <= tier ||
            ingLib.herbs.ContainsKey(info) && ingLib.herbs[info].valid && ingLib.herbs[info].tier <= tier)
        {
            valid = true;
            icon.color = Color.white;
        }
        else
        {
            valid = false;
            icon.color = new Color(0.5f, 0.5f, 0.5f);
        }

        UpdateDetailUI();
    }
    
    public void UpdateDetailUI()
    {
        if (valid)
            detail.text = $"{TextManager.Instance.GetIngredient(info)}\nx{GM.Instance.ingredients[info]}";
        else
            detail.text = string.Empty;
    }

    public void ToggleHighlight(bool on)
    {
        highlight.SetActive(on);
    }

}
