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

    public void Init(Ingredient info)
    {
        this.info = info;
        icon.sprite = DataManager.Instance.uiLib.ingredients[info];
        highlight.SetActive(false);
        UpdateDetailUI();
    }
    
    public void UpdateDetailUI()
    {
        detail.text = $"{TextManager.Instance.GetIngredient(info)}\nx{GM.Instance.ingredients[info]}";
    }

    public void ToggleHighlight(bool on)
    {
        highlight.SetActive(on);
    }

}
