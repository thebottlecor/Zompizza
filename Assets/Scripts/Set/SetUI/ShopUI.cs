using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct PanelButtonPair
{
    public GameObject panel;
    public ShopButton button;
}

public class ShopUI : EventListener
{

    [Header("가게 UI")]
    public List<PanelButtonPair> panelButtonPairs;
    public int activeSubPanel = 1;
    public float fadeTime = 1f;
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;

    public GameObject shopUIOpenButton;

    public bool loading;
    public bool opened;

    public bool IsActive => loading || opened;

    public bool playerStay;

    public ScrollingUIEffect scrollEffect;

    public TextManager tm => TextManager.Instance;
    public TextMeshProUGUI[] buttonTexts;

    public Image[] pizzaBoys;
    public Image[] pizzaBoxes;

    public ScrollRect scrollRect;
    public RectTransform contentPanel;

    [Header("야간 모드")]
    public GameObject orderPanel;
    public GameObject explorePanel;
    private bool endTime;

    public TextMeshProUGUI orderText;
    public TextMeshProUGUI ingredientText;
    public TextMeshProUGUI[] ingredientsSub;

    [Header("관리탭")]
    public TextMeshProUGUI reviewsText;
    public TextMeshProUGUI achievementText;
    public TextMeshProUGUI statsText;

    public GameObject reviewObject_Day_Source;
    public GameObject reviewObject_Source;
    public Transform reviewObject_Parent;
    public List<ReviewDayObject> reviewDayObjects;
    public List<Review> reviewObjects;

    [Header("업그레이드탭")]
    public TextMeshProUGUI upgradeText;
    public int currentSelectUpgrade;
    public GameObject upgradeUI_Source;
    public TextMeshProUGUI[] upgradeUI_GroupText;
    public Transform[] upgradeUI_Parent;
    public TextMeshProUGUI upgradeDetailText;
    public Dictionary<int, ResearchUI> researchUIs;
    public Button upgrade_UnlockBtn;
    public TextMeshProUGUI upgrade_UnlockBtnText;
    public UpgradeDirection upgradeDirection;

    public void Init()
    {
        reviewDayObjects = new List<ReviewDayObject>();
        reviewObjects = new List<Review>();

        DayFirstReview();
        CreateUpgradeUI();
    }

    public void UpdateTexts()
    {
        buttonTexts[0].text = tm.GetCommons("Order");
        buttonTexts[1].text = tm.GetCommons("Management");
        buttonTexts[2].text = tm.GetCommons("Upgrade");
        buttonTexts[3].text = tm.GetCommons("News");

        buttonTexts[4].text = tm.GetCommons("Back");

        orderText.text = tm.GetCommons("Order");
        ingredientText.text = tm.GetCommons("Ingredient");
        ingredientsSub[0].text = tm.GetCommons("Meat");
        ingredientsSub[1].text = tm.GetCommons("Vegetable");
        ingredientsSub[2].text = tm.GetCommons("Herb");

        reviewsText.text = tm.GetCommons("Reviews");
        achievementText.text = tm.GetCommons("Achievement");
        statsText.text = tm.GetCommons("Stats");

        upgradeText.text = tm.GetCommons("Upgrade");
        upgrade_UnlockBtnText.text = tm.GetCommons("Upgrade");

        upgradeUI_GroupText[0].text = tm.GetCommons("Shop");
        upgradeUI_GroupText[1].text = tm.GetCommons("Vehicle");
    }

    protected override void AddListeners()
    {
        ShopEnter.PlayerArriveEvent += OnPlayerArriveShop;
        ShopEnter.PlayerExitEvent += OnPlayerExitShop;
        PizzaDirection.PizzaCompleteEvent += OnPizzaCompleted;
        OrderManager.OrderRemovedEvent += OnOrderRemoved;
        GM.EndTimeEvent += OnEndtime;
    }

    protected override void RemoveListeners()
    {
        ShopEnter.PlayerArriveEvent -= OnPlayerArriveShop;
        ShopEnter.PlayerExitEvent -= OnPlayerExitShop;
        PizzaDirection.PizzaCompleteEvent -= OnPizzaCompleted;
        OrderManager.OrderRemovedEvent -= OnOrderRemoved;
        GM.EndTimeEvent -= OnEndtime;
    }

    private void OnPlayerArriveShop(object sender, EventArgs e)
    {
        ShowOrder();
        playerStay = true;
    }

    private void OnPlayerExitShop(object sender, EventArgs e)
    {
        playerStay = false;
    }

    private void OnPizzaCompleted(object sender, OrderInfo e) // OrderInfo <= 제외할 정보
    {
        UpdatePizzaBox(e);
    }
    private void OnOrderRemoved(object sender, int e)
    {
        if (e == 0)
        {
            OnEndtime(null, true);
        }

        UpdatePizzaBox(null);
    }

    private void OnEndtime(object sender, bool e)
    {
        endTime = e;

        if (e) // 마감
        {
            orderPanel.SetActive(false);
            explorePanel.SetActive(true);

            buttonTexts[0].text = tm.GetCommons("Explore");

            pizzaBoys[0].gameObject.SetActive(false);
            pizzaBoys[1].gameObject.SetActive(false);
            pizzaBoys[2].gameObject.SetActive(true);
        }
        else
        {
            orderPanel.SetActive(true);
            explorePanel.SetActive(false);

            buttonTexts[0].text = tm.GetCommons("Order");

            pizzaBoys[0].gameObject.SetActive(false);
            pizzaBoys[1].gameObject.SetActive(true);
            pizzaBoys[2].gameObject.SetActive(false);

        }
    }

    private void Start()
    {
        orderPanel.SetActive(true);
        explorePanel.SetActive(false);
    }

    private void Update()
    {
        bool buttonOn = false;
        if (!IsActive && !UIManager.Instance.utilUI.IsActive && playerStay)
        {
            buttonOn = true;
        }
        if ((buttonOn && !shopUIOpenButton.activeSelf) || !buttonOn && shopUIOpenButton.activeSelf)
        {
            shopUIOpenButton.SetActive(buttonOn);
        }
    }

    public void ShowOrder()
    {
        activeSubPanel = 0;
        OpenUI();
    }
    public void ShowOrder(int customerIdx)
    {
        activeSubPanel = 0;
        SnapTo(UIManager.Instance.orderUIObjects[customerIdx].transform as RectTransform);
        OpenUI();
    }

    public void OpenUI()
    {
        if (UIManager.Instance.isDirecting) return;
        if (GM.Instance.loading) return;

        if (loading) return;

        if (UIManager.Instance.utilUI.IsActive)
        {
            UIManager.Instance.utilUI.HideUI_Replace();
        }

        if (opened)
        {
            HideUI();
            return;
        }

        scrollEffect.enabled = true;
        UIManager.Instance.OffAll_Ingredient_Highlight();
        GM.Instance.stop_control = true;
        Time.timeScale = 0f;
        loading = true;

        UIManager.Instance.orderMiniUIParent.SetActive(false);
        UIManager.Instance.speedInfo.SetActive(false);
        UIManager.Instance.timeInfo.SetActive(false);
        ExplorationManager.Instance.HideUI_ResultPanel_Instant();
        WorldMapManager.Instance.CloseMinimap();

        SelectSubPanel(activeSubPanel);

        canvasGroup.alpha = 0f;
        rectTransform.transform.localPosition = new Vector3(0f, 1000f, 0f);
        rectTransform.DOAnchorPos(new Vector2(0f, 0f), fadeTime, false).SetEase(Ease.OutElastic).SetUpdate(true);
        canvasGroup.DOFade(1f, fadeTime).SetUpdate(true).OnComplete(() =>
        {
            loading = false;
            opened = true;
        });
    }

    public void HideUI_Replace()
    {
        for (int i = 0; i < panelButtonPairs.Count; i++)
        {
            panelButtonPairs[i].button.Hide();
        }

        DOTween.Kill(rectTransform);
        DOTween.Kill(canvasGroup);
        scrollEffect.enabled = false;
        loading = false;
        opened = false;

        rectTransform.anchoredPosition = new Vector2(0f, -2000f);
        canvasGroup.alpha = 0f;
    }

    public void HideUI()
    {
        //if (UIManager.Instance.isDirecting) OrderManager.Instance.pizzaDirection.StopSequence();
        if (UIManager.Instance.isDirecting) return;
        if (GM.Instance.loading) return;

        if (!opened) return;
        if (loading) return;

        scrollEffect.enabled = false;
        opened = false;
        GM.Instance.stop_control = false;
        Time.timeScale = 1f;
        loading = true;

        UIManager.Instance.orderMiniUIParent.SetActive(true);
        UIManager.Instance.speedInfo.SetActive(true);
        UIManager.Instance.timeInfo.SetActive(true);
        ExplorationManager.Instance.HideUI_ResultPanel_Instant();
        WorldMapManager.Instance.OpenMinimap();

        for (int i = 0; i < panelButtonPairs.Count; i++)
        {
            panelButtonPairs[i].button.Hide();
        }

        canvasGroup.alpha = 1f;
        rectTransform.transform.localPosition = new Vector3(0f, 0f, 0f);
        float hideFast = fadeTime * 0.5f;
        rectTransform.DOAnchorPos(new Vector2(0f, -2000f), hideFast, false).SetEase(Ease.InOutQuint).SetUpdate(true);
        canvasGroup.DOFade(0f, hideFast).SetUpdate(true).OnComplete(() =>
        {
            loading = false;
        });
    }

    public void SelectSubPanel(int idx)
    {
        if (idx == 0)
        {
            UpdatePizzaBox(null);
            ExplorationManager.Instance.UpdateBtn();
        }
        else
        {
            if (UIManager.Instance.isDirecting) 
                OrderManager.Instance.pizzaDirection.StopSequence();
            ExplorationManager.Instance.HideUI_ResultPanel_Instant();

            if (idx == 2)
            {
                SelectUpgrade(-1);
            }
        }

        for (int i = 0; i < panelButtonPairs.Count; i++)
        {
            panelButtonPairs[i].panel.SetActive(false);
            panelButtonPairs[i].button.SetHighlight(false);
        }

        panelButtonPairs[idx].panel.SetActive(true);
        panelButtonPairs[idx].button.SetHighlight(true);

        activeSubPanel = idx;
    }

    //연출
    public void SnapTo(RectTransform target)
    {
        float value = Mathf.Abs(target.anchoredPosition.y) / scrollRect.content.rect.height;

        scrollRect.verticalNormalizedPosition = 1f - value;
    }

    private void UpdatePizzaBox(OrderInfo info)
    {
        int count = OrderManager.Instance.GetCurrentPizzaBox();

        if (info != null)
            count -= info.pizzas.Count;

        if (endTime)
        {
            pizzaBoys[0].gameObject.SetActive(count != 0);
            pizzaBoys[1].gameObject.SetActive(false);
            pizzaBoys[2].gameObject.SetActive(count == 0);
        }
        else
        {
            pizzaBoys[0].gameObject.SetActive(count != 0);
            pizzaBoys[1].gameObject.SetActive(count == 0);
            pizzaBoys[2].gameObject.SetActive(false);
        }

        for (int i = 0; i < pizzaBoxes.Length; i++)
        {
            pizzaBoxes[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < pizzaBoxes.Length; i++)
        {
            if (count <= 0)
                break;
            if (!pizzaBoxes[i].gameObject.activeSelf)
            {
                pizzaBoxes[i].gameObject.SetActive(true);
                count--;
            }
        }
    }


    public void DayFirstReview()
    {
        var obj = Instantiate(reviewObject_Day_Source, reviewObject_Parent);
        ReviewDayObject reviewDay = obj.GetComponent<ReviewDayObject>();
        reviewDay.Init(GM.Instance.day);

        reviewDayObjects.Add(reviewDay);
        reviewDay.transform.SetAsFirstSibling();
    }

    public void AddReview(OrderInfo info, float time, float hp)
    {
        int day = GM.Instance.day;

        var obj = Instantiate(reviewObject_Source, reviewObject_Parent);
        ReviewObject reviewObj = obj.GetComponent<ReviewObject>();
        reviewObj.Init(day, info.customerIdx, time, hp);

        reviewObjects.Add(reviewObj);

        reviewObj.transform.SetAsFirstSibling();
        reviewDayObjects[day].transform.SetAsFirstSibling();
    }

    public void OrderTextUpdate()
    {
        OrderManager.Instance.GetDeliveringCount();
        int current = OrderManager.Instance.currentAcceptance;
        int max = OrderManager.Instance.MaxAccpetance;

        if (current >= max)
            orderText.text = tm.GetCommons("Order") + $" <color=#A91111><size=75%>({current}/{max})</color>";
        else
            orderText.text = tm.GetCommons("Order") + $" <size=75%>({current}/{max})";

    }

    #region 업그레이드
    ResearchManager rm => ResearchManager.Instance;
    public void CreateUpgradeUI()
    {
        var infos = DataManager.Instance.researches;
        researchUIs = new Dictionary<int, ResearchUI>();

        foreach (var temp in infos)
        {
            var obj = Instantiate(upgradeUI_Source, upgradeUI_Parent[temp.Value.group]);
            ResearchUI researchUI = obj.GetComponent<ResearchUI>();
            researchUI.Init(temp.Key);
            researchUIs.Add(temp.Key, researchUI);
        }
    }

    public void SelectUpgrade(int idx)
    {
        currentSelectUpgrade = idx;

        if (idx == -1)
        {
            upgradeDetailText.text = string.Empty;
            upgrade_UnlockBtn.gameObject.SetActive(false);
            return;
        }

        var info = DataManager.Instance.researches[idx];

        StringBuilder st = new StringBuilder();
        st.AppendFormat("<size=120%><b>{0}</b> ({1}/{2})</size>", tm.GetResearch(idx), rm.GetResearchCount(idx), info.max);
        st.AppendLine();
        st.AppendFormat("{0} : {1}$", tm.GetCommons("Costs"), rm.GetCost(idx));
        st.AppendLine();
        st.AppendLine();
        st.AppendFormat("<b>{0}</b>", tm.GetCommons("Effect"));
        st.AppendLine();

        float goldGet = info.effect.goldGet;
        float ratingGet = info.effect.ratingGet;
        int maxSpeed = info.effect.maxSpeed;
        float defense = info.effect.damageReduce;

        st.Append("<size=90%>");
        if (goldGet != 0f)
        {
            string sub;
            if (goldGet > 0f)
                sub = string.Format(tm.defaultCultureInfo, "+{0:P0}", goldGet);
            else
                sub = string.Format(tm.defaultCultureInfo, "{0:P0}", goldGet);
            st.AppendFormat(tm.GetCommons("UpgradeEffect0"), "<sprite=2>", sub);
            st.AppendLine();
        }
        if (ratingGet != 0f)
        {
            string sub;
            if (ratingGet > 0f)
                sub = string.Format(tm.defaultCultureInfo, "+{0:P0}", ratingGet);
            else
                sub = string.Format(tm.defaultCultureInfo, "{0:P0}", ratingGet);
            st.AppendFormat(tm.GetCommons("UpgradeEffect1"), "<sprite=1>", sub);
            st.AppendLine();
        }
        if (maxSpeed != 0)
        {
            string sub;
            if (maxSpeed > 0f)
                sub = string.Format("+{0}km/h", maxSpeed);
            else
                sub = string.Format("{0}km/h", maxSpeed);
            st.AppendFormat(tm.GetCommons("UpgradeEffect2"), sub);
            st.AppendLine();
        }
        if (defense != 0)
        {
            string sub;
            if (defense > 0f)
                sub = string.Format(tm.defaultCultureInfo, "+{0:P0}", defense);
            else
                sub = string.Format(tm.defaultCultureInfo, "{0:P0}", defense);
            st.AppendFormat(tm.GetCommons("UpgradeEffect3"), sub);
            st.AppendLine();
        }

        upgradeDetailText.text = st.ToString();

        upgrade_UnlockBtn.gameObject.SetActive(true);
        upgrade_UnlockBtn.enabled = !rm.MaxResearched(idx);
    }

    public void ClickUpgrade()
    {
        if (currentSelectUpgrade < 0) return;

        bool result = rm.ResearchUnlock(currentSelectUpgrade);

        if (result)
        {
            // 성공 연출
            AudioManager.Instance.PlaySFX(Sfx.complete);
            upgradeDirection.Show();

            researchUIs[currentSelectUpgrade].UpdateUI();

            SelectUpgrade(currentSelectUpgrade);
        }
        else
        {
            AudioManager.Instance.PlaySFX(Sfx.deny);
        }
    }
    #endregion
}
