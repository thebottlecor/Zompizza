using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

    [Header("주문탭")]
    public Image[] pizzaBoys;
    public Image[] pizzaBoxes;

    public ScrollRect orderScroll;
    public RectTransform contentPanel;

    public Button shopCloseBtn;
    public TextMeshProUGUI shopCloseBtn_Text;
    public GameObject shopCloseWarningObj;
    public TextMeshProUGUI[] shopCloseWarningBtn_Text;
    public TextMeshProUGUI shopCloseWarning_Text;
    public TextMeshProUGUI shopCloseWarningDetail_Text;

    [Header("야간 모드 - 탐험")]
    public GameObject orderPanel;
    public GameObject makingPanel;
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
    public ScrollRect reviewScroll;
    public List<ReviewDayObject> reviewDayObjects;
    public List<Review> reviewObjects;

    [Header("관리탭 - 대출")]
    public TextMeshProUGUI loanText;
    public RepaySilder repaySlider;

    [Header("관리탭 - 라이벌")]
    public TextMeshProUGUI rivalText;
    public TextMeshProUGUI rivalRatingText;

    [Header("업그레이드탭")]
    public TextMeshProUGUI upgradeText;
    public int currentSelectUpgrade;
    public GameObject upgradeUI_Source;
    public TextMeshProUGUI[] upgradeUI_GroupText;
    public Transform[] upgradeUI_Parent;
    public TextMeshProUGUI upgradeDetailNameText;
    public TextMeshProUGUI upgradeDetailText;
    public Dictionary<int, ResearchUI> researchUIs;
    public Button upgrade_UnlockBtn;
    public TextMeshProUGUI upgrade_UnlockBtnText;
    public UpgradeDirection upgradeDirection;
    public SerializableDictionary<ResearchInfo, RectTransform> upgradePositions;
    public List<MaskedUIHelper> maskedUIHelders;

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

        //buttonTexts[4].text = tm.GetCommons("Back");
        buttonTexts[4].text = tm.GetCommons("Close");

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

        shopCloseBtn_Text.text = tm.GetCommons("ShopClose");
        shopCloseWarning_Text.text = tm.GetCommons("Warning");
        shopCloseWarningDetail_Text.text = tm.GetCommons("ShopCloseWarning");
        shopCloseWarningBtn_Text[0].text = tm.GetCommons("Close");
        shopCloseWarningBtn_Text[1].text = tm.GetCommons("Cancel");

        loanText.text = tm.GetCommons("Loan");

        rivalText.text = tm.GetCommons("Rival");
    }

    protected override void AddListeners()
    {
        ShopEnter.PlayerArriveEvent += OnPlayerArriveShop;
        ShopExiter.PlayerExitEvent += OnPlayerExitShop;
        PizzaDirection.PizzaCompleteEvent += OnPizzaCompleted;
        OrderManager.OrderRemovedEvent += OnOrderRemoved;
        GM.EndTimeEvent += OnEndtime;
        InputHelper.TabMoveEvent += OnTabMove;
    }

    protected override void RemoveListeners()
    {
        ShopEnter.PlayerArriveEvent -= OnPlayerArriveShop;
        ShopExiter.PlayerExitEvent -= OnPlayerExitShop;
        PizzaDirection.PizzaCompleteEvent -= OnPizzaCompleted;
        OrderManager.OrderRemovedEvent -= OnOrderRemoved;
        GM.EndTimeEvent -= OnEndtime;
        InputHelper.TabMoveEvent -= OnTabMove;
    }

    private void OnTabMove(object sender, InputAction.CallbackContext e)
    {
        /// <summary>
        /// <see cref="UIManager.OnESC"/>
        /// <ser cref="UINaviHelper.SetFirstSelect"/>
        /// </summary>

        if (!opened || loading) return;

        var exploration = ExplorationManager.Instance;
        if (exploration.canvasGroupLoading) return;
        if (exploration.canvasGroup_resultPanel.alpha >= 0.99f) return;
        if (GM.Instance.gameOverWarningObj.activeSelf) return;
        if (shopCloseWarningObj.activeSelf) return;

        float value = e.ReadValue<float>();

        if (e.performed)
        {
            if (value > 0)
            {
                activeSubPanel++;
                if (activeSubPanel >= panelButtonPairs.Count)
                    activeSubPanel = 0;
                SelectSubPanel(activeSubPanel);
                SettingManager.Instance.ButtonSound();
            }
            else if (value < 0)
            {
                activeSubPanel--;
                if (activeSubPanel < 0)
                    activeSubPanel = panelButtonPairs.Count - 1;
                SelectSubPanel(activeSubPanel);
                SettingManager.Instance.ButtonSound();
            }
        }
    }

    private void OnPlayerArriveShop(object sender, EventArgs e)
    {
        if (GM.Instance.rating >= Constant.winRating)
        {
            if (!GM.Instance.CongratulationTriggered)
            {
                GM.Instance.Congratulation(true);
                AudioManager.Instance.PlaySFX(Sfx.complete);
                upgradeDirection.Show();
            }
        }

        ShowOrder();
        playerStay = true;
    }

    private void OnPlayerExitShop(object sender, EventArgs e)
    {
        if (playerStay)
        {
            playerStay = false;
        }
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

            shopCloseBtn.gameObject.SetActive(false);
        }
        else
        {
            orderPanel.SetActive(true);
            explorePanel.SetActive(false);

            buttonTexts[0].text = tm.GetCommons("Order");

            pizzaBoys[0].gameObject.SetActive(false);
            pizzaBoys[1].gameObject.SetActive(true);
            pizzaBoys[2].gameObject.SetActive(false);

            shopCloseBtn.gameObject.SetActive(true);
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
        if (UIManager.Instance.isDirecting || UIManager.Instance.changingResolution) return;
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
        UIManager.Instance.padUIs.SetActive(false);
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

        UINaviHelper.Instance.SetFirstSelect();
    }

    public void HideUI()
    {
        //if (UIManager.Instance.isDirecting) OrderManager.Instance.pizzaDirection.StopSequence();
        if (UIManager.Instance.isDirecting || UIManager.Instance.changingResolution) return;
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
        UIManager.Instance.padUIs.SetActive(true);
        ExplorationManager.Instance.HideUI_ResultPanel_Instant();
        WorldMapManager.Instance.OpenMinimap();

        for (int i = 0; i < panelButtonPairs.Count; i++)
        {
            panelButtonPairs[i].button.Hide();
        }

        TutorialManager.Instance.ShopWindowHide();

        canvasGroup.alpha = 1f;
        rectTransform.transform.localPosition = new Vector3(0f, 0f, 0f);
        float hideFast = fadeTime * 0.5f;
        rectTransform.DOAnchorPos(new Vector2(0f, -2000f), hideFast, false).SetEase(Ease.InOutQuint).SetUpdate(true);
        canvasGroup.DOFade(0f, hideFast).SetUpdate(true).OnComplete(() =>
        {
            loading = false;
            TutorialManager.Instance.ShopWindowHideComplete();
            UINaviHelper.Instance.SetFirstSelect();
        });
    }

    public void SelectSubPanel(int idx)
    {
        if (idx == 0)
        {
            SnapTo(null);
            UpdatePizzaBox(null);
            if (orderPanel.activeSelf)
            {
                UIManager.Instance.OrderUIBtnUpdate();
            }
            if (explorePanel.activeSelf)
            {
                ExplorationManager.Instance.SetCost();
            }

            shopCloseBtn.gameObject.SetActive(!endTime);
        }
        else
        {
            if (UIManager.Instance.isDirecting) 
                OrderManager.Instance.pizzaDirection.StopSequence();
            ExplorationManager.Instance.HideUI_ResultPanel_Instant();

            switch (idx)
            {
                case 1:
                    StatManager.Instance.UpdateText();
                    reviewScroll.verticalNormalizedPosition = 1f;
                    break;
                case 2:
                    //SelectUpgrade(-1);
                    SelectUpgrade(GetMainResearch().idx);
                    break;
            }

            shopCloseBtn.gameObject.SetActive(false);
        }

        for (int i = 0; i < panelButtonPairs.Count; i++)
        {
            panelButtonPairs[i].panel.SetActive(false);
            panelButtonPairs[i].button.SetHighlight(false);
        }

        panelButtonPairs[idx].panel.SetActive(true);
        panelButtonPairs[idx].button.SetHighlight(true);

        activeSubPanel = idx;

        UINaviHelper.Instance.SetFirstSelect();
    }

    //연출
    public void SnapTo(RectTransform target)
    {
        if (target != null)
        {
            float value = Mathf.Abs(target.anchoredPosition.y) / (orderScroll.content.rect.height - target.sizeDelta.y);
            orderScroll.verticalNormalizedPosition = 1f - value;
        }
        else
            orderScroll.verticalNormalizedPosition = 1f;
    }

    private void UpdatePizzaBox(OrderInfo info)
    {
        int count = OrderManager.Instance.GetCurrentPizzaBox();
        GM.Instance.player.UpdateBox(count);

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

    #region 상점 닫기
    public void ShowShopCloseWarning(bool on)
    {
        shopCloseWarningObj.SetActive(on);
        UINaviHelper.Instance.SetFirstSelect();
    }
    public void Force_CloseShop()
    {
        ShowShopCloseWarning(false);
        AudioManager.Instance.PlaySFX(Sfx.close);
        OrderManager.Instance.RemoveAllOrders();
        UINaviHelper.Instance.SetFirstSelect();
        ExplorationManager.Instance.SetCost();
    }

    #endregion

    #region 업그레이드
    ResearchManager rm => ResearchManager.Instance;
    public void CreateUpgradeUI()
    {
        var infos = DataManager.Instance.researches;
        researchUIs = new Dictionary<int, ResearchUI>();

        foreach (var temp in infos)
        {
            var obj = Instantiate(upgradeUI_Source, upgradeUI_Parent[temp.Value.group]);
            obj.name = infos[temp.Key].name;
            (obj.transform as RectTransform).anchoredPosition = upgradePositions[temp.Value].anchoredPosition;
            (obj.transform as RectTransform).sizeDelta = upgradePositions[temp.Value].sizeDelta;
            ResearchUI researchUI = obj.GetComponent<ResearchUI>();
            researchUI.Init(temp.Key);
            researchUIs.Add(temp.Key, researchUI);

            if (temp.Value.invalid)
                researchUI.gameObject.SetActive(false);

            if (temp.Value.hidden && !rm.Researched(temp.Key))
                researchUI.gameObject.SetActive(false);
        }
    }
    public void UpdateHiddenUpgrade()
    {
        var infos = DataManager.Instance.researches;
        foreach (var temp in researchUIs)
        {
            if (infos[temp.Key].hidden && !infos[temp.Key].invalid)
            {
                temp.Value.gameObject.SetActive(rm.Researched(temp.Key));
            }
            temp.Value.UpdateUI();
        }
    }
    public ResearchUI GetMainResearch()
    {
        ResearchUI first = null;
        var infos = DataManager.Instance.researches;
        foreach (var ui in researchUIs)
        {
            if (infos[ui.Key].main && ui.Value.gameObject.activeSelf)
            {
                first = ui.Value;
                break;
            }    
        }
        //SelectUpgrade(first.idx);
        return first;
    }
    public ResearchUI GetCurrentResearchUI()
    {
        return researchUIs[currentSelectUpgrade];
    }

    public void SelectUpgrade(int idx)
    {
        int prev = currentSelectUpgrade;
        currentSelectUpgrade = idx;
        if (prev != -1)
            researchUIs[prev].UpdateUI();

        if (idx == -1)
        {
            upgradeDetailNameText.text = string.Empty;
            upgradeDetailText.text = string.Empty;
            upgrade_UnlockBtn.gameObject.SetActive(false);
            return;
        }

        researchUIs[currentSelectUpgrade].UpdateUI();

        bool canResearch = !rm.MaxResearched(idx);

        var info = DataManager.Instance.researches[idx];

        StringBuilder st2 = new StringBuilder();
        if (info.max > 1)
            st2.AppendFormat("<b>{0}</b> ({1}/{2})", tm.GetResearch(idx), rm.GetResearchCount(idx), info.max);
        else
            st2.AppendFormat("<b>{0}</b>", tm.GetResearch(idx));
        upgradeDetailNameText.text = st2.ToString();

        StringBuilder st = new StringBuilder();
        if (canResearch)
            st.AppendFormat("{0} : {1}$", tm.GetCommons("Costs"), rm.GetCost(idx));
        st.AppendLine();
        st.AppendLine();
        st.AppendFormat("<b>{0}</b>", tm.GetCommons("Effect"));
        st.AppendLine();

        st.Append("<size=90%>");

        info.effect.ShowgoldGet(st);
        info.effect.ShowratingGet(st);

        info.effect.Showexplore_max_pay(st);
        info.effect.Showexplore_cost(st);
        info.effect.Showexplore_get_bonus(st);

        info.effect.Showtier(st);

        info.effect.Showmeat_tier(st);
        info.effect.Showvegetable_tier(st);
        info.effect.Showherb_tier(st);
        info.effect.Showproduction_tier(st);
        info.effect.Showproduction_bonus(st);

        info.effect.ShowpizzeriaExpand(st);

        info.effect.ShowraidDefense(st);

        info.effect.Showcustomer_timelimit(st);
        info.effect.Showcustomer_max_tier(st);
        info.effect.Showcustomer_max_amount(st);
        info.effect.Showcustomer_max_type(st);

        info.effect.ShowmaxSpeed(st);
        info.effect.ShowdamageReduce(st);
        info.effect.Showacceleration(st);

        upgradeDetailText.text = st.ToString();

        upgrade_UnlockBtn.gameObject.SetActive(canResearch);
        //upgrade_UnlockBtn.enabled = canResearch;

        UINaviHelper.Instance.SetFirstSelect();
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
