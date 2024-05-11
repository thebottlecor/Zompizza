using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{

    public ShopUI shopUI;
    public UtilUI utilUI;

    public RectTransform movingSettingsPanelParent;

    public bool isDirecting;

    public List<OrderUIObject> orderUIObjects;

    [Header("������ ��� ����")]
    public GameObject orderMiniUI_Source;
    public int maxOrderMiniUI = 20;
    public Transform orderMiniUIContexts;
    public List<OrderMiniUI> orderMiniUIs;
    public GameObject orderMiniUIParent;

    public GameObject speedInfo;
    public GameObject timeInfo;

    [Header("���â")]
    public GameObject ingredient_Source;
    public Transform[] ingredient_Parents;
    public List<IngredientUI> ingredientUIs;
    public Dictionary<Ingredient, IngredientUI> ingredientUIPairs;

    private SerializableDictionary<KeyMap, KeyMapping> HotKey => SettingManager.Instance.keyMappings;

    public void Init()
    {
        var panel = SettingManager.Instance.ingameMovingPanel;
        panel.SetParent(movingSettingsPanelParent);
        panel.gameObject.SetActive(true);

        panel.offsetMin = new Vector2(0f, 0f);
        panel.offsetMax = new Vector2(0f, 0f);

        shopUI.UpdateTexts();
        utilUI.UpdateTexts();

        // ������ ��� ����
        orderMiniUIs = new List<OrderMiniUI>(maxOrderMiniUI);
        for (int i = 0; i < maxOrderMiniUI; i++)
        {
            var obj = Instantiate(orderMiniUI_Source, orderMiniUIContexts);
            OrderMiniUI miniUI = obj.GetComponent<OrderMiniUI>();
            orderMiniUIs.Add(miniUI);
            obj.SetActive(false);
        }
        // ���â
        var ingredients = Enum.GetValues(typeof(Ingredient));
        ingredientUIPairs = new Dictionary<Ingredient, IngredientUI>();
        foreach(var temp in ingredients)
        {
            Ingredient ingredient = (Ingredient)temp;
            int parentIdx = 0;
            if (DataManager.Instance.ingredientLib.vegetables.ContainsKey(ingredient))
            {
                parentIdx = 1;
            }
            else if (DataManager.Instance.ingredientLib.herbs.ContainsKey(ingredient))
            {
                parentIdx = 2;
            }
            var obj = Instantiate(ingredient_Source, ingredient_Parents[parentIdx]);
            IngredientUI ingredientUI = obj.GetComponent<IngredientUI>();
            ingredientUI.Init(ingredient);
            ingredientUIPairs.Add(ingredient, ingredientUI);
            ingredientUIs.Add(ingredientUI);
        }
    }

    public void UpdateIngredients()
    {
        for (int i = 0; i < ingredientUIs.Count; i++)
        {
            ingredientUIs[i].UpdateDetailUI();
        }
    }
    public void OffAll_Ingredient_Highlight()
    {
        for (int i = 0; i < ingredientUIs.Count; i++)
        {
            ingredientUIs[i].ToggleHighlight(false);
        }
    }

    public void ButtonSound()
    {
        AudioManager.Instance.PlaySFX(Sfx.buttons);
    }
    public void ButtonHighlightSound()
    {
        AudioManager.Instance.PlaySFX(Sfx.btnHighlight, volume: 0.75f);
    }

    private void Update()
    {
        if (isDirecting) return;
        if (GM.Instance.loading) return;

        if (HotKey[KeyMap.escape].GetkeyDown())
        {
            if (ExplorationManager.Instance.canvasGroupLoading)
                return;
            else if (ExplorationManager.Instance.canvasGroup_resultPanel.alpha >= 0.99f)
            {
                ExplorationManager.Instance.HideUI_ResultPanel();
                return;
            }

            if (GM.Instance.gameOverWarningObj.activeSelf)
            {
                GM.Instance.ShowGameOverWarning(false);
                return;
            }
            //if (LoanManager.Instance.loanWarningObj.activeSelf)
            //{
            //    LoanManager.Instance.ShowLoanWarning(false);
            //    return;
            //}
            if (shopUI.shopCloseWarningObj.activeSelf)
            {
                shopUI.ShowShopCloseWarning(false);
                return;
            }

            if (utilUI.IsActive)
            {
                utilUI.HideUI();
            }
            else if (shopUI.IsActive)
            {
                shopUI.HideUI();
            }
            else
            {
                utilUI.OpenSettings();
            }
        }
        else if (HotKey[KeyMap.worldMap].GetkeyDown())
        {
            if (!shopUI.IsActive && !utilUI.IsActive)
            {
                utilUI.OpenWorldMap();
            }
        }
    }


    #region �ֹ� UI
    public void OrderUIUpdate()
    {
        var list = OrderManager.Instance.orderList;
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].accepted)
            {
                orderUIObjects[list[i].customerIdx].UIUpdate(list[i]);
                orderUIObjects[list[i].customerIdx].gameObject.SetActive(true);
                orderUIObjects[list[i].customerIdx].ButtonUpdate();
            }
        }
    }
    public void OrderUIBtnUpdate()
    {
        var list = OrderManager.Instance.orderList;
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].accepted)
            {
                orderUIObjects[list[i].customerIdx].ButtonUpdate();
            }
        }
    }
    public void OrderUIReset()
    {
        for (int i = 0; i < orderUIObjects.Count; i++)
        {
            orderUIObjects[i].OrderReset();
        }
    }
    #endregion
}
