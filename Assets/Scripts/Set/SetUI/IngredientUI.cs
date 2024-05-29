using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

        var ingLib = DataManager.Instance.ingredientLib;
        // 임시유효성 검사
        if (ingLib.meats.ContainsKey(info) && ingLib.meats[info] ||
            ingLib.vegetables.ContainsKey(info) && ingLib.vegetables[info] ||
            ingLib.herbs.ContainsKey(info) && ingLib.herbs[info])
        {
            valid = true;
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
