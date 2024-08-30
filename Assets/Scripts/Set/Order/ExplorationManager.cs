using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Text;

public class ExplorationManager : Singleton<ExplorationManager>
{
    public ExplorationSilder meats;
    public ExplorationSilder vegetables;
    public ExplorationSilder herbs;
    public ExplorationSilder quantity;
    public ExplorationSilder quality;

    public TextMeshProUGUI exploreText;
    public TextMeshProUGUI costText;

    public Button sendBtn;
    public TextMeshProUGUI sendBtnText;

    private int cost;

    private float quantityValue;
    private int qualityValue;

    [Header("결과창")]
    public float fadeTime = 0.5f;
    public CanvasGroup canvasGroup_resultPanel;
    public RectTransform rectTransform_resultPanel;
    public bool canvasGroupLoading;

    Dictionary<Ingredient, int> resultDict;

    public TextMeshProUGUI resultText;
    public TextMeshProUGUI resultOkayText;

    public TextMeshProUGUI resultText_Detail;

    private TextManager tm => TextManager.Instance;

    private void Start()
    {
        quantity.Init(5f, Constant.explorationQuantityMax, SlideQuantity);
        quality.Init(0, 0, SlideQuality);

        HideUI_ResultPanel_Instant();
        UpdateText();
    }

    public void UpdateText()
    {
        exploreText.text = tm.GetCommons("Explore");

        //meats.categoryText.text = tm.GetCommons("Meat");
        //vegetables.categoryText.text = tm.GetCommons("Vegetable");
        //herbs.categoryText.text = tm.GetCommons("Herb");
        quantity.categoryText.text = tm.GetCommons("Quantity");
        quality.categoryText.text = tm.GetCommons("Tier");

        resultText.text = tm.GetCommons("Result");
        resultOkayText.text = tm.GetCommons("Okay");
    }

    public void SlideQuantity(float value)
    {
        quantityValue = value;

        SetCost();
    }
    public void SlideQuality(float value)
    {
        qualityValue = (int)value;

        SetCost();
    }

    public void SetHighTierQuality()
    {
        UpgradeUpdate();
        quality.slider.SetValueWithoutNotify(quality.slider.maxValue);
        quality.UpdateUI();
        SlideQuality(quality.slider.maxValue);
    }

    private void UpgradeUpdate()
    {
        quantity.slider.maxValue = Constant.explorationQuantityMax + ResearchManager.Instance.globalEffect.explore_max_pay;
        quality.slider.maxValue = ResearchManager.Instance.globalEffect.tier;
    }

    public void SetCost()
    {
        float value = quantityValue * 100f;

        value *= (qualityValue + 1);

        float temp = value * (1f + ResearchManager.Instance.globalEffect.explore_cost);
        cost = (int)temp;

        costText.text = $"{tm.GetCommons("Costs")} : {cost}G";

        UpdateBtn();
    }
    private void UpdateBtn()
    {
        sendBtn.interactable = GM.Instance.gold >= cost;
        if (sendBtn.interactable) sendBtnText.text = tm.GetCommons("SendExploration");
        else sendBtnText.text = tm.GetCommons("SendDisable");
    }


    public void SendExploration()
    {
        if (GM.Instance.gold < cost) return;
        if (GM.Instance.loading) return;
        if (GM.Instance.midNight) return;
        if (UIManager.Instance.shopUI.loading) return;
        if (GameEventManager.Instance.eventPanel.activeSelf) return;
        if (UIManager.Instance.shopUI.sosWarningObj.activeSelf) return;

        TutorialManager.Instance.SendExploration();

        // 받지 않았던 주문 패널티 
        OrderManager.Instance.RemoveAllOrders();

        GM.Instance.CheckRaid_BeforeExploration();

        ExplorationResult();
        if (resultDict.Count > 0)
        {
            GM.Instance.AddGold(-1 * cost, GM.GetGoldSource.explore);
            AudioManager.Instance.PlaySFX(Sfx.carStart);
        }

        GM.Instance.NextDay();
    }

    private void ExplorationResult()
    {
        int maxQuantity = (int)quantity.slider.maxValue;

        int result_quantity = (int)(maxQuantity * quantity.Percent);
        float temp_bonus = result_quantity * (1f + ResearchManager.Instance.globalEffect.explore_get_bonus);
        float remain = temp_bonus - (int)temp_bonus;
        if (remain > 0f && UnityEngine.Random.Range(0f, 1f) < remain)
            result_quantity = (int)temp_bonus + 1;
        else
            result_quantity = (int)temp_bonus;

        List<Ingredient> ingredients = new List<Ingredient>();
        var ingLib = DataManager.Instance.ingredientLib;
        foreach (var temp in ingLib.ingredientTypes)
        {
            var key = (Ingredient)temp;
            // 임시유효성 검사
            if (ingLib.meats.ContainsKey(key) && ingLib.meats[key].valid && ingLib.meats[key].tier == qualityValue ||
                ingLib.vegetables.ContainsKey(key) && ingLib.vegetables[key].valid && ingLib.vegetables[key].tier == qualityValue ||
                ingLib.herbs.ContainsKey(key) && ingLib.herbs[key].valid && ingLib.herbs[key].tier == qualityValue)
            {
                Ingredient ingredient = (Ingredient)temp;
                ingredients.Add(ingredient);
            }
        }

        var gmInven = GM.Instance.ingredients;
        int minValue = int.MaxValue;
        for (int i = 0; i < ingredients.Count; i++)
        {
            if (gmInven[ingredients[i]] < minValue)
            {
                minValue = gmInven[ingredients[i]];
            }
        }
        int accpeted_Max_Value = (minValue < 15) ? 15 : ((minValue / 5) + 1) * 5; // 최소가 15개 미만인 경우, 15개 미만인 것들만 탐험에서 얻음
        List<Ingredient> adjusted_ingredients = new List<Ingredient>();
        for (int i = 0; i < ingredients.Count; i++)
        {
            if (gmInven[ingredients[i]] < accpeted_Max_Value)
            {
                adjusted_ingredients.Add(ingredients[i]);
            }
        }
        if (adjusted_ingredients.Count > 0)
        {
            ingredients = adjusted_ingredients;
        }

        resultDict = new Dictionary<Ingredient, int>();
        int count = result_quantity;
        while (count > 0)
        {
            int rand = UnityEngine.Random.Range(0, ingredients.Count);
            Ingredient ingredient = ingredients[rand];

            if (!resultDict.ContainsKey(ingredient))
                resultDict.Add(ingredient, 1);
            else
                resultDict[ingredient]++;

            count--;
        }

        foreach (var temp in resultDict)
        {
            GM.Instance.ingredients[temp.Key] += temp.Value;
        }
    }

    public bool ShowResultPanel()
    {
        if (resultDict.Count > 0)
        {
            int maxLine = 7;
            int exceedRow = (resultDict.Count - 1) / maxLine;
            int rowCount = 0;

            StringBuilder st = new StringBuilder();

            foreach (var temp in resultDict)
            {
                if (exceedRow > 0 && rowCount < exceedRow)
                {
                    st.AppendFormat("<sprite={0}><size=90%>{2}</size> +{1}  ", (int)temp.Key + Constant.ingredientSpriteOffset, temp.Value, tm.GetIngredient(temp.Key));
                    rowCount++;
                }
                else
                {
                    st.AppendFormat("<sprite={0}><size=90%>{2}</size> +{1}\n", (int)temp.Key + Constant.ingredientSpriteOffset, temp.Value, tm.GetIngredient(temp.Key));
                    rowCount = 0;
                }
            }

            resultText_Detail.text = st.ToString();

            OpenUI_ResultPanel();

            return true;
        }
        else
        {
            // 없으면 아무일도 없음 // 경고창은 정상적으로 뜸
            return false;
        }
    }



    public void OpenUI_ResultPanel()
    {
        canvasGroupLoading = true;
        UINaviHelper.Instance.SetFirstSelect();
        canvasGroup_resultPanel.alpha = 0f;
        canvasGroup_resultPanel.blocksRaycasts = true;
        canvasGroup_resultPanel.interactable = true;
        rectTransform_resultPanel.transform.localPosition = new Vector3(0f, 1000f, 0f);
        rectTransform_resultPanel.DOAnchorPos(new Vector2(0f, 0f), fadeTime, false).SetEase(Ease.OutElastic).SetUpdate(true);
        canvasGroup_resultPanel.DOFade(1f, fadeTime).SetUpdate(true).OnComplete(() =>
        {
            canvasGroupLoading = false;
            UINaviHelper.Instance.SetFirstSelect();
        });
    }

    public void HideUI_ResultPanel()
    {
        UIManager.Instance.UpdateIngredients();

        canvasGroupLoading = true;
        UINaviHelper.Instance.SetFirstSelect();
        canvasGroup_resultPanel.alpha = 1f;
        canvasGroup_resultPanel.blocksRaycasts = false;
        canvasGroup_resultPanel.interactable = false;
        rectTransform_resultPanel.transform.localPosition = new Vector3(0f, 0f, 0f);
        float hideFast = fadeTime * 0.5f;
        rectTransform_resultPanel.DOAnchorPos(new Vector2(0f, -2000f), hideFast, false).SetEase(Ease.InOutQuint).SetUpdate(true);
        canvasGroup_resultPanel.DOFade(0f, hideFast).SetUpdate(true).OnComplete(() =>
        {
            canvasGroupLoading = false;
            GM.Instance.ShowWarningQueue();
            UINaviHelper.Instance.SetFirstSelect();
        });
    }

    public void HideUI_ResultPanel_Instant()
    {
        canvasGroup_resultPanel.alpha = 0f;
        canvasGroup_resultPanel.blocksRaycasts = false;
        canvasGroup_resultPanel.interactable = false;
        UINaviHelper.Instance.SetFirstSelect();
    }
}
