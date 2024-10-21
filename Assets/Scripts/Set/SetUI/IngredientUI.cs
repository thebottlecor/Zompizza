using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientUI : MonoBehaviour
{

    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI stackText;
    public GameObject highlight;

    public GameObject[] buttons;

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
        {
            nameText.text = $"{TextManager.Instance.GetIngredient(info)}";
            UpdateStack_ComboMode();
        }
        else
        {
            nameText.text = string.Empty;
            stackText.text = string.Empty;

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].SetActive(false);
            }
        }
    }

    public void ToggleHighlight(bool on)
    {
        highlight.SetActive(on);
    }

    public void UpdateStack_ComboMode()
    {
        if (OrderManager.Instance.comboMode)
        {
            var inputs = OrderManager.Instance.ovenMiniGame.inputs;
            if (inputs.ContainsKey(info))
                stackText.text = $"({inputs[info]}/{GM.Instance.ingredients[info]})";
            else
                stackText.text = $"(0/{GM.Instance.ingredients[info]})";

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].SetActive(true);
            }
        }
        else
        {
            stackText.text = $"x{GM.Instance.ingredients[info]}";

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].SetActive(false);
            }
        }
    }

    public void Input()
    {
        if (!OrderManager.Instance.comboMode) return;
        if (!valid)
        {
            AudioManager.Instance.PlaySFX(Sfx.deny);
            return;
        }

        var oven = OrderManager.Instance.ovenMiniGame;
        var inputs = oven.inputs;
        if (!inputs.ContainsKey(info))
            inputs.Add(new SerializableDictionary<Ingredient, int>.Pair(info, 0));

        if (GM.Instance.ingredients[info] > inputs[info] && !oven.IsMaxInput)
        {
            if (inputs.ContainsKey(info))
                inputs[info] += 1;
            AudioManager.Instance.PlaySFX(Sfx.buttons);
        }
        else
            AudioManager.Instance.PlaySFX(Sfx.deny);

        oven.UpdateValueTexts();
        UpdateStack_ComboMode();
    }
    public void Cancel()
    {
        if (!OrderManager.Instance.comboMode) return;
        if (!valid)
        {
            AudioManager.Instance.PlaySFX(Sfx.deny);
            return;
        }

        var inputs = OrderManager.Instance.ovenMiniGame.inputs;
        if (inputs.ContainsKey(info))
        {
            if (inputs[info] > 0)
                inputs[info] -= 1;
        }
        else
            inputs.Add(new SerializableDictionary<Ingredient, int>.Pair(info, 0));
        AudioManager.Instance.PlaySFX(Sfx.inputFieldEnd);

        OrderManager.Instance.ovenMiniGame.UpdateValueTexts();
        UpdateStack_ComboMode();
    }

}
