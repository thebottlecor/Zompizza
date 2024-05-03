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

    [Header("���â")]
    public float fadeTime = 0.5f;
    public CanvasGroup canvasGroup_resultPanel;
    public RectTransform rectTransform_resultPanel;

    Dictionary<Ingredient, int> resultDict;

    public TextMeshProUGUI resultText;
    public TextMeshProUGUI resultOkayText;

    public TextMeshProUGUI resultText_Detail;

    private void Start()
    {
        SlideQuantity(5f);
        SlideQuality(0f);

        HideUI_ResultPanel_Instant();
        UpdateText();
    }

    public void UpdateText()
    {
        TextManager tm = TextManager.Instance;

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

        costText.text = $"{TextManager.Instance.GetCommons("Costs")} : {cost}$";

        UpdateBtn();
    }
    public void UpdateBtn()
    {
        sendBtn.interactable = GM.Instance.gold >= cost;
        if (sendBtn.interactable) sendBtnText.text = TextManager.Instance.GetCommons("SendExploration");
        else sendBtnText.text = TextManager.Instance.GetCommons("SendDisable");
    }


    public void SendExploration()
    {
        if (GM.Instance.gold < cost) return;

        int notCompletedOrder = OrderManager.Instance.GetAcceptedOrderCount();
        if (notCompletedOrder > 0)
        {
            // ���� �Ϸ���� ���� �ֹ��� �ִµ� �������� �� ���ΰ�? ���

            // �̿Ϸ� �г�Ƽ
            GM.Instance.AddRating(Constant.delivery_Not_completed_rating * notCompletedOrder, GM.GetRatingSource.notComplete);
        }

        // ���� �ʾҴ� �ֹ� �г�Ƽ 
        OrderManager.Instance.RemoveAllOrders();

        GM.Instance.AddGold(-1 * cost, GM.GetGoldSource.explore);

        ExplorationResult();

        UIManager.Instance.UpdateIngredients();

        GM.Instance.NextDay();
    }

    public void ExplorationResult()
    {
        int maxQuantity = 10;

        int result_quantity = (int)(maxQuantity * quantity.Percent);

        resultDict = new Dictionary<Ingredient, int>();
        int count = result_quantity;
        while (count > 0)
        {
            Ingredient rand = (Ingredient)UnityEngine.Random.Range(0, GM.Instance.IngredientTypeCount);

            if (!resultDict.ContainsKey(rand))
                resultDict.Add(rand, 1);
            else
                resultDict[rand]++;

            count--;
        }

        foreach (var temp in resultDict)
        {
            GM.Instance.ingredients[temp.Key] += temp.Value;
        }

    }

    public void ShowResultPanel()
    {
        if (resultDict.Count > 0)
        {
            StringBuilder st = new StringBuilder();

            foreach (var temp in resultDict)
            {
                st.AppendFormat("<sprite={0}><size=90%>{2}</size> +{1}\n", (int)temp.Key + Constant.ingredientSpriteOffset, temp.Value, TextManager.Instance.GetIngredient(temp.Key));
            }

            resultText_Detail.text = st.ToString();

            OpenUI_ResultPanel();
        }
        else
        {
            // ������ �ƹ��ϵ� ����
        }
    }



    public void OpenUI_ResultPanel()
    {
        canvasGroup_resultPanel.alpha = 0f;
        canvasGroup_resultPanel.blocksRaycasts = true;
        canvasGroup_resultPanel.interactable = true;
        rectTransform_resultPanel.transform.localPosition = new Vector3(0f, 1000f, 0f);
        rectTransform_resultPanel.DOAnchorPos(new Vector2(0f, 0f), fadeTime, false).SetEase(Ease.OutElastic).SetUpdate(true);
        canvasGroup_resultPanel.DOFade(1f, fadeTime).SetUpdate(true);
    }

    public void HideUI_ResultPanel()
    {
        canvasGroup_resultPanel.alpha = 1f;
        canvasGroup_resultPanel.blocksRaycasts = false;
        canvasGroup_resultPanel.interactable = false;
        rectTransform_resultPanel.transform.localPosition = new Vector3(0f, 0f, 0f);
        float hideFast = fadeTime * 0.5f;
        rectTransform_resultPanel.DOAnchorPos(new Vector2(0f, -2000f), hideFast, false).SetEase(Ease.InOutQuint).SetUpdate(true);
        canvasGroup_resultPanel.DOFade(0f, hideFast).SetUpdate(true);
    }

    public void HideUI_ResultPanel_Instant()
    {
        canvasGroup_resultPanel.alpha = 0f;
        canvasGroup_resultPanel.blocksRaycasts = false;
        canvasGroup_resultPanel.interactable = false;
    }
}
