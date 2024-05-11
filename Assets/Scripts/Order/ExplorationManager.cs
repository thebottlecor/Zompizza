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
    private float qualityValue;

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
        quantity.Init(5f);
        SlideQuality(0f);

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
        //quality.categoryText.text = tm.GetCommons("Quality");

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
        qualityValue = value;

        SetCost();
    }

    public void SetCost()
    {
        float value = quantityValue * 100f;

        value += qualityValue * 100f;

        cost = (int)value;

        costText.text = $"{tm.GetCommons("Costs")} : {cost}$";

        UpdateBtn();
    }
    public void UpdateBtn()
    {
        sendBtn.interactable = GM.Instance.gold >= cost;
        if (sendBtn.interactable) sendBtnText.text = tm.GetCommons("SendExploration");
        else sendBtnText.text = tm.GetCommons("SendDisable");
    }


    public void SendExploration()
    {
        if (GM.Instance.gold < cost) return;

        TutorialManager.Instance.SendExploration();

        int notCompletedOrder = OrderManager.Instance.GetAcceptedOrderCount();
        if (notCompletedOrder > 0)
        {
            // 아직 완료되지 않은 주문이 있는데 다음날로 갈 것인가? 경고

            // 미완료 패널티
            GM.Instance.AddRating(Constant.delivery_Not_completed_rating * notCompletedOrder, GM.GetRatingSource.notComplete);
        }

        // 받지 않았던 주문 패널티 
        OrderManager.Instance.RemoveAllOrders();

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
        int maxQuantity = Constant.explorationQuantityMax;

        int result_quantity = (int)(maxQuantity * quantity.Percent);

        resultDict = new Dictionary<Ingredient, int>();
        int count = result_quantity;
        while (count > 0)
        {
            int rand = UnityEngine.Random.Range(0, OrderManager.Instance.ingredients.Count);
            Ingredient ingredient = OrderManager.Instance.ingredients[rand];

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
        canvasGroup_resultPanel.alpha = 0f;
        canvasGroup_resultPanel.blocksRaycasts = true;
        canvasGroup_resultPanel.interactable = true;
        rectTransform_resultPanel.transform.localPosition = new Vector3(0f, 1000f, 0f);
        rectTransform_resultPanel.DOAnchorPos(new Vector2(0f, 0f), fadeTime, false).SetEase(Ease.OutElastic).SetUpdate(true);
        canvasGroup_resultPanel.DOFade(1f, fadeTime).SetUpdate(true).OnComplete(() =>
        {
            canvasGroupLoading = false;
        });
    }

    public void HideUI_ResultPanel()
    {
        UIManager.Instance.UpdateIngredients();

        canvasGroupLoading = true;
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
        });
    }

    public void HideUI_ResultPanel_Instant()
    {
        canvasGroup_resultPanel.alpha = 0f;
        canvasGroup_resultPanel.blocksRaycasts = false;
        canvasGroup_resultPanel.interactable = false;
    }
}
