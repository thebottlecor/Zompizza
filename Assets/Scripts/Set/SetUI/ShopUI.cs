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
    public TextMeshProUGUI shopUIOpenTmp;

    [HideInInspector] public bool loading;
    [HideInInspector] public bool opened;

    public bool IsActive => loading || opened;

    public bool playerStay;

    public ScrollingUIEffect scrollEffect;

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

    [Header("SOS")]
    public GameObject sosWarningObj;
    public RectTransform sosWarningRect;
    public TextMeshProUGUI[] sosWarningBtn_Text;
    public TextMeshProUGUI[] sosWarning_Text;
    public TextMeshProUGUI[] sosWarningDetail_Text;

    [Header("야간 모드 - 탐험")]
    public GameObject orderPanel;
    public GameObject makingPanel;
    public GameObject explorePanel;
    private bool endTime;

    public TextMeshProUGUI orderText;
    public TextMeshProUGUI moreOrderGuideText;
    public TextMeshProUGUI ingredientText;
    public TextMeshProUGUI[] ingredientsSub;

    [Header("야간 모드 - 한밤중")]
    public GameObject midnightPanel;
    public TextMeshProUGUI midnightStartText;

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
    public int currentSelectUpgrade;
    public GameObject upgradeUI_Source;
    public Transform[] upgradeUI_Parent;
    public TextMeshProUGUI upgradeDetailNameText;
    public TextMeshProUGUI upgradeDetailText;
    public Dictionary<int, ResearchUI> researchUIs;
    public Button upgrade_UnlockBtn;
    public TextMeshProUGUI upgrade_UnlockBtnText;
    public UpgradeDirection upgradeDirection;
    public SerializableDictionary<ResearchInfo, RectTransform> upgradePositions;
    public List<MaskedUIHelper> maskedUIHelders;
    public GameObject researchableNotification;

    [Header("차량탭")]
    public GameObject upgradePanel_Vehicle;
    public TextMeshProUGUI upgradeDetailNameText_Vehicle;
    public TextMeshProUGUI upgradeDetailText_Vehicle;
    public Button upgrade_UnlockBtn_Vehicle;
    public TextMeshProUGUI upgrade_UnlockBtnText_Vehicle;

    public GameObject infoPanel_Vehicle;
    public int currentViewVehicle;
    public TextMeshProUGUI ownedVehiclesText;
    public Button selectVehicleBtn;
    private int selectVehicleBtnMode;
    public GameObject canBuyVehicleHighlight;
    public TextMeshProUGUI selectVehicleBtnText;
    public Vehicle3DShowcase vehicleShowcase;

    public TextMeshProUGUI vehicleInfo_NameText;
    public TextMeshProUGUI vehicleInfo_DetailText;

    public int[] vehicleShowcaseOrders;
    public int ShowingVehicles => vehicleShowcaseOrders[currentViewVehicle];

    private TextManager tm => TextManager.Instance;
    private UIManager um => UIManager.Instance;
    private UILibrary uiLib => DataManager.Instance.uiLib;

    public void Init(bool saveLoad, GameSaveData data)
    {
        reviewDayObjects = new List<ReviewDayObject>();
        reviewObjects = new List<Review>();

        if (!saveLoad)
            DayFirstReview(0);
        else
            Load(data.reviews.data);
        CreateUpgradeUI();
    }

    public void UpdateTexts()
    {
        buttonTexts[0].text = tm.GetCommons("Order");
        buttonTexts[1].text = tm.GetCommons("Management");
        //buttonTexts[2].text = tm.GetCommons("Upgrade");
        buttonTexts[2].text = tm.GetCommons("Research");
        //buttonTexts[3].text = tm.GetCommons("News");
        //buttonTexts[3].text = tm.GetCommons("Vehicle");
        buttonTexts[3].text = tm.GetCommons("Garage");

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

        upgrade_UnlockBtnText.text = tm.GetCommons("Upgrade");
        upgrade_UnlockBtnText_Vehicle.text = tm.GetCommons("Upgrade");

        shopCloseBtn_Text.text = tm.GetCommons("ShopClose");
        shopCloseWarning_Text.text = tm.GetCommons("Warning");
        shopCloseWarningDetail_Text.text = tm.GetCommons("ShopCloseWarning");
        shopCloseWarningBtn_Text[0].text = tm.GetCommons("ShopClose");
        shopCloseWarningBtn_Text[1].text = tm.GetCommons("Cancel");

        sosWarning_Text[0].text = tm.GetCommons("Sos");
        sosWarning_Text[1].text = tm.GetCommons("Sos");
        sosWarningDetail_Text[0].text = tm.GetCommons("Sos2");
        sosWarningDetail_Text[1].text = tm.GetCommons("Sos2");
        sosWarningBtn_Text[0].text = tm.GetCommons("Sos3");
        sosWarningBtn_Text[1].text = tm.GetCommons("Sos3");

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
         
        ovenDir = 0f;

        if (um.changingResolution) return;
        if (GM.Instance.loading) return;

        if (!opened || loading) return;

        var exploration = ExplorationManager.Instance;
        if (exploration.canvasGroupLoading) return;
        if (exploration.canvasGroup_resultPanel.alpha >= 0.99f) return;
        if (GM.Instance.gameOverWarningObj.activeSelf) return;
        if (GM.Instance.raidObj.activeSelf) return;
        if (RivalManager.Instance.rankingObj.activeSelf) return;
        if (GM.Instance.darkCanvas.blocksRaycasts) return;
        if (shopCloseWarningObj.activeSelf) return;
        if (sosWarningObj.activeSelf) return;
        if (GameEventManager.Instance.eventPanel.activeSelf) return;
        if (DialogueManager.Instance.eventPanel.activeSelf) return;
        if (TutorialManager.Instance.blackScreen.activeSelf) return;

        float value = e.ReadValue<float>();

        if (um.isDirecting)
        {
            if (makingPanel.activeSelf && OrderManager.Instance.ovenMiniGame.inputPanel.activeSelf)
            {      
                var navi = UINaviHelper.Instance.current;
                if (navi != null && navi is UINaviTwin)
                {
                    if (e.performed)
                    {
                        if (value > 0)
                            ovenDir = 1f;
                        else if (value < 0)
                            ovenDir = -1f;

                        var ing = (navi as UINaviTwin).ing;
                        if (value > 0)
                            ing.Input();
                        else if (value < 0)
                            ing.Cancel();
                    }
                }
            }
            return;
        }

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
        ShowOrder();
        um.VehicleUnlock();
        um.TierUp();
        ToggleResearchNotification(true);
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
            if (GM.Instance.day == RocketManager.Countdown - 1) // 마지막 날엔 탐색 X
            {
                if (!midnightPanel.activeSelf)
                {
                    ForceMidnightUIUpdate(true);
                    GM.Instance.lastLaunch = true;
                    VillagerManager.Instance.GetIncome();
                }
            }
            else
            {
                if (!explorePanel.activeSelf)
                {
                    orderPanel.SetActive(false);
                    OrderManager.Instance.bonusObj.SetActive(false);
                    explorePanel.SetActive(true);
                    midnightPanel.SetActive(false);
                    ExplorationManager.Instance.SetHighTierQuality();

                    buttonTexts[0].text = tm.GetCommons("Explore");

                    pizzaBoys[0].gameObject.SetActive(false);
                    pizzaBoys[1].gameObject.SetActive(false);
                    pizzaBoys[2].gameObject.SetActive(true);

                    shopCloseBtn.gameObject.SetActive(false);

                    VillagerManager.Instance.GetIncome();
                    GameEventManager.Instance.OfferingDaily();
                }
            }
        }
        else
        {
            if (!orderPanel.activeSelf)
            {
                orderPanel.SetActive(true);
                explorePanel.SetActive(false);
                midnightPanel.SetActive(false);

                buttonTexts[0].text = tm.GetCommons("Order");

                pizzaBoys[0].gameObject.SetActive(false);
                pizzaBoys[1].gameObject.SetActive(true);
                pizzaBoys[2].gameObject.SetActive(false);

                shopCloseBtn.gameObject.SetActive(true);
            }
        }
    }
    public void ForceMidnightUIUpdate(bool lastDay)
    {
        orderPanel.SetActive(false);
        OrderManager.Instance.bonusObj.SetActive(false);
        explorePanel.SetActive(false); 
        midnightPanel.SetActive(true);

        if (lastDay)
        {
            buttonTexts[0].text = tm.GetCommons("Launch");
            midnightStartText.text = tm.GetCommons("SpaceshipProject");
        }
        else
        {
            buttonTexts[0].text = tm.GetCommons("ShopOpen");
            midnightStartText.text = tm.GetCommons("ShopOpen");
        }

        pizzaBoys[0].gameObject.SetActive(false);
        pizzaBoys[1].gameObject.SetActive(false);
        pizzaBoys[2].gameObject.SetActive(false);

        shopCloseBtn.gameObject.SetActive(false);
    }

    private void Start()
    {
        orderPanel.SetActive(true);
        explorePanel.SetActive(false);

        // 강제 연구탭 틀어놓아서 애니메이션 정상화시키기
        for (int i = 0; i < panelButtonPairs.Count; i++)
        {
            panelButtonPairs[i].panel.SetActive(false);
            panelButtonPairs[i].button.SetHighlight(false);
        }
        panelButtonPairs[2].panel.SetActive(true);
        panelButtonPairs[2].button.SetHighlight(true);
    }

    private void Update()
    {
        bool buttonOn = false;
        if (!IsActive && !um.utilUI.IsActive && !um.villagerUI.IsActive && playerStay)
        {
            buttonOn = true;
        }
        if ((buttonOn && !shopUIOpenButton.activeSelf) || !buttonOn && shopUIOpenButton.activeSelf)
        {
            if (buttonOn)
            {
                var pad = Gamepad.current;
                shopUIOpenTmp.text = $"{tm.GetKeyMaps(KeyMap.enterStore)} ({SettingManager.Instance.keyMappings[KeyMap.enterStore].GetName()})";
                shopUIOpenTmp.gameObject.SetActive(pad == null);
            }
            shopUIOpenButton.SetActive(buttonOn);
        }

        if (um.isDirecting)
        {
            if (makingPanel.activeSelf && OrderManager.Instance.ovenMiniGame.inputPanel.activeSelf)
            {
                var navi = UINaviHelper.Instance.current;
                if (navi != null && navi is UINaviTwin)
                {
                    if (ovenDir == 0f)
                    {
                        sliderGauge = 0f;
                        //sliderGauge2 = 0f;
                    }
                    else
                    {
                        sliderGauge += Time.unscaledDeltaTime;
                        //sliderGauge2 += Time.unscaledDeltaTime * 4f;
                    }

                    //float value = ovenDir;
                    //if (sliderGauge2 > 1f)
                    //{
                    //    value *= (int)sliderGauge2 + 1f;
                    //}
                    //int value2 = Mathf.Min(2, (int)Mathf.Abs(value));

                    if (sliderGauge > 0.15f)
                    {
                        sliderGauge = 0f;

                        var ing = (navi as UINaviTwin).ing;
                        if (ovenDir > 0)
                            ing.Input();
                        else if (ovenDir < 0)
                            ing.Cancel();
                    }
                }
            }
        }
    }

    private float ovenDir;
    private float sliderGauge;
    //private float sliderGauge2;

    public void ShowOrder()
    {
        activeSubPanel = 0;
        OpenUI();
    }
    public void ShowOrder(int customerIdx)
    {
        activeSubPanel = 0;
        OpenUI();
        SnapTo(um.orderUIObjects[customerIdx].transform as RectTransform);
    }

    public void OpenUI()
    {
        if (um.isDirecting || um.changingResolution) return;
        if (GM.Instance.loading) return;

        if (loading) return;

        if (um.utilUI.IsActive)
        {
            um.utilUI.HideUI_Replace();
        }
        if (um.villagerUI.IsActive)
        {
            um.villagerUI.HideUI_Replace();
        }

        if (opened)
        {
            HideUI();
            return;
        }

        scrollEffect.enabled = true;
        um.OffAll_Ingredient_Highlight();
        OrderManager.Instance.ovenMiniGame.HideHighlight();
        GM.Instance.stop_control = true;
        Time.timeScale = 0f;
        loading = true;

        um.ToggleDrivingInfo(false);

        um.orderIndicator.SetActive(false);
        um.padUIs.SetActive(false);
        OrderManager.Instance.fastTravelBtnParnet.SetActive(false);
        ExplorationManager.Instance.HideUI_ResultPanel_Instant();

        SelectSubPanel(activeSubPanel);

        canvasGroup.alpha = 0f;
        rectTransform.transform.localPosition = new Vector3(0f, 1000f, 0f);
        rectTransform.DOAnchorPos(new Vector2(0f, 0f), fadeTime, false).SetEase(Ease.OutElastic).SetUpdate(true);
        canvasGroup.DOFade(1f, fadeTime).SetUpdate(true).OnComplete(() =>
        {
            WinCheck();
        });
    }

    private void WinCheck()
    {
        loading = false;
        opened = true;

        if (GM.Instance.closeImage.activeSelf && !GM.Instance.midNight)
        {
            bool triggered = false;
            int day = GM.Instance.day; // 가게 닫은 후 이벤트들
            switch (day)
            {
                case 5:
                    triggered = GameEventManager.Instance.SetEvent(1); // 6일차 가게 닫은 후 이장 이벤트
                    break;
                case 11:
                    triggered = GameEventManager.Instance.SetEvent(3); // 12일차 가게 닫은 후 치킨 닌자 이벤트
                    break;
                case 17:
                    triggered = GameEventManager.Instance.SetEvent(5); // 18일차 가게 닫은 후 이장 이벤트
                    break;
                case 23:
                    triggered = GameEventManager.Instance.SetEvent(7); // 24일차 가게 닫은 후 치킨 닌자 이벤트
                    break;
            }

            if (!triggered && !GM.Instance.midNight)
            {
                VillagerManager.Instance.CreateSOS();
            }
        }

        //if (GM.Instance.rating >= Constant.winRating)
        //{
        //    if (!GM.Instance.CongratulationTriggered)
        //    {
        //        GM.Instance.Congratulation(true);
        //        AudioManager.Instance.PlaySFX(Sfx.complete);
        //        upgradeDirection.Show();
        //    }
        //}
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

        HideAllVehicle();

        UINaviHelper.Instance.SetFirstSelect();
    }

    public void HideUI(bool instant = false)
    {
        //if (um.isDirecting) OrderManager.Instance.pizzaDirection.StopSequence();
        if (um.isDirecting || um.changingResolution) return;
        if (GM.Instance.loading) return;

        if (!opened) return;
        if (loading) return;

        HideUI_Main(instant);
    }

    public void HideForEnding()
    {
        scrollEffect.enabled = false;
        opened = false;
        Time.timeScale = 1f;
        HideAllVehicle();
        for (int i = 0; i < panelButtonPairs.Count; i++)
        {
            panelButtonPairs[i].button.Hide();
        }
        rectTransform.transform.localPosition = new Vector3(0f, -2000f, 0f);
        canvasGroup.alpha = 0f;
        loading = false;
    }

    private void HideUI_Main(bool instant)
    {
        scrollEffect.enabled = false;
        opened = false;
        GM.Instance.stop_control = false;
        Time.timeScale = 1f;
        loading = true;

        um.ToggleDrivingInfo(true);

        um.orderIndicator.SetActive(true);
        um.padUIs.SetActive(true);
        OrderManager.Instance.fastTravelBtnParnet.SetActive(true);
        ExplorationManager.Instance.HideUI_ResultPanel_Instant();
        HideAllVehicle();

        for (int i = 0; i < panelButtonPairs.Count; i++)
        {
            panelButtonPairs[i].button.Hide();
        }

        TutorialManager.Instance.ShopWindowHide();

        if (instant)
        {
            rectTransform.transform.localPosition = new Vector3(0f, -2000f, 0f);
            canvasGroup.alpha = 0f;
            loading = false;
            TutorialManager.Instance.ShopWindowHideComplete();
            UINaviHelper.Instance.SetFirstSelect();
        }
        else
        {
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
    }

    public void SelectSubPanel(int idx)
    {
        if (um.isDirecting) return;

        if (idx != 3)
        {
            HideAllVehicle();
        }

        if (idx == 0)
        {
            SnapTo(null);
            UpdatePizzaBox(null);
            if (orderPanel.activeSelf)
            {
                um.OrderUIBtnUpdate();
            }
            if (explorePanel.activeSelf)
            {
                ExplorationManager.Instance.SetHighTierQuality();
                //ExplorationManager.Instance.SetCost();
            }

            shopCloseBtn.gameObject.SetActive(!endTime);
        }
        else
        {
            //if (um.isDirecting) 
            //    OrderManager.Instance.pizzaDirection.StopSequence();
            ExplorationManager.Instance.HideUI_ResultPanel_Instant();

            switch (idx)
            {
                case 1:
                    StatManager.Instance.UpdateText();
                    VillagerManager.Instance.UpdateUIs();
                    RocketManager.Instance.UpdateShopUI();
                    reviewScroll.verticalNormalizedPosition = 1f;
                    break;
                case 2:
                    var pad = Gamepad.current;
                    if (pad == null)
                        SelectUpgrade(-1);
                    else
                        SelectUpgrade(GetMainResearch().idx);
                    ToggleResearchNotification(false);
                    break;
                case 3:
                    ResetVehicleUI();
                    //SelectUpgrade(GetMainResearch_Vehicle().idx);
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
            count -= 1;

        if (GM.Instance.midNight)
        {
            pizzaBoys[0].gameObject.SetActive(false);
            pizzaBoys[1].gameObject.SetActive(false);
            pizzaBoys[2].gameObject.SetActive(false);
        }
        else if (endTime)
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


    public void DayFirstReview(int day)
    {
        var obj = Instantiate(reviewObject_Day_Source, reviewObject_Parent);
        ReviewDayObject reviewDay = obj.GetComponent<ReviewDayObject>();
        reviewDay.Init(day);

        reviewDayObjects.Add(reviewDay);
        reviewDay.transform.SetAsFirstSibling();
    }

    [ContextMenu("날짜 건너뛰기 조정")]
    public void TEST()
    {
        int testDay = GM.Instance.day;
        for (int i = 0; i <= testDay; i++)
        {
            if (reviewDayObjects.Count <= i)
            {
                DayFirstReview(i);
            }
        }
    }

    public void AddReview(int day, int customerIdx, int goal, float time, float hp)
    {
        var obj = Instantiate(reviewObject_Source, reviewObject_Parent);
        ReviewObject reviewObj = obj.GetComponent<ReviewObject>();
        float rating = reviewObj.Init(day, customerIdx, time, hp, goal);

        reviewObjects.Add(reviewObj);

        reviewObj.transform.SetAsFirstSibling();
        reviewDayObjects[day].transform.SetAsFirstSibling();

        var cInfo = OrderManager.Instance.customersInfos[goal];
        cInfo.totalOrder++;
        cInfo.totalRating += rating;
        OrderManager.Instance.customersInfos[goal] = cInfo;
    }
    private void Load(List<ReviewData> datas)
    {
        int currentDay = GM.Instance.day;
        if (datas.Count == 0)
        {
            for (int i = 0; i <= currentDay; i++)
            {
                DayFirstReview(i);
            }
        }
        else
        {
            int day = -1;
            for (int i = 0; i < datas.Count; i++)
            {
                int dataDay = datas[i].day;

                while (dataDay > reviewDayObjects.Count) // 이전의 날들에서 배달을 아예 안했을 경우
                {
                    int differ = dataDay - reviewDayObjects.Count;
                    DayFirstReview(dataDay - differ);
                }

                if (dataDay > day)
                {
                    DayFirstReview(dataDay);
                    day = dataDay;
                }

                AddReviewByLoad(datas[i]);
            }
            while (currentDay > day)
            {
                day++;
                DayFirstReview(day);
            }
        }
    }
    private void AddReviewByLoad(ReviewData data)
    {
        AddReview(data.day, data.customerIdx, data.goal, data.time, data.hp);
    }
    public List<ReviewData> Save()
    {
        List<ReviewData> data = new List<ReviewData>();
        for (int i = 0; i < reviewObjects.Count; i++)
        {
            data.Add((reviewObjects[i] as ReviewObject).GetReviewData());
        }
        return data;
    }

    public void OrderLoadCountTextUpdate()
    {
        OrderManager.Instance.GetDeliveringCount();
        int current = OrderManager.Instance.currentAcceptance;
        int max = OrderManager.Instance.MaxAccpetance;

        if (current >= max)
        {
            orderText.text = tm.GetCommons("Order") + $" <color=#A91111><size=75%>({current}/{max})</color>";
            moreOrderGuideText.gameObject.SetActive(false);
        }
        else
        {
            orderText.text = tm.GetCommons("Order") + $" <size=75%>({current}/{max})";
            moreOrderGuideText.text = string.Format(tm.GetCommons("MoreGuide"), max - current);
            moreOrderGuideText.gameObject.SetActive(true);
        }
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
        ExplorationManager.Instance.SetHighTierQuality();
        //ExplorationManager.Instance.SetCost();
        OrderManager.Instance.ovenMiniGame.HideHighlight();

        if (!GM.Instance.midNight)
        {
            bool triggered = false;
            int day = GM.Instance.day; // 가게 "강제로" 닫은 후 이벤트들
            switch (day)
            {
                case 5:
                    triggered = GameEventManager.Instance.SetEvent(1); // 6일차 가게 "강제로" 닫은 후 이장 이벤트
                    break;
                case 11:
                    triggered = GameEventManager.Instance.SetEvent(3); // 12일차 가게 "강제로" 닫은 후 치킨 닌자 이벤트
                    break;
                case 17:
                    triggered = GameEventManager.Instance.SetEvent(5); // 18일차 가게 "강제로" 닫은 후 이장 이벤트
                    break;
                case 23:
                    triggered = GameEventManager.Instance.SetEvent(7); // 24일차 가게 "강제로" 닫은 후 치킨 닌자 이벤트
                    break;
            }
            if (!triggered)
            {
                VillagerManager.Instance.CreateSOS();
            }
        }
    }
    #endregion

    #region 구조 신호 경고
    public void ShowSosWarning(bool on)
    {
        sosWarningObj.SetActive(on);

        if (!on)
        {
            HideUI();
        }
        else
        {
            sosWarningRect.localScale = 0.01f * Vector3.one;
            sosWarningRect.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.OutElastic).SetUpdate(true);
        }

        UINaviHelper.Instance.SetFirstSelect();
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
            var obj = Instantiate(upgradeUI_Source, upgradeUI_Parent[(int)temp.Value.group]);
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

            if (temp.Value.group == ResearchInfo.ResearchGroup.vehicle)
            {
                if (UINaviHelper.Instance != null)
                {
                    UINaviHelper.Instance.ingame.shops_vehicles.Add(obj.GetComponent<UINavi>());
                }
            }
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
    public ResearchUI GetMainResearch_Vehicle()
    {
        ResearchUI first = null;
        var infos = DataManager.Instance.researches;
        foreach (var ui in researchUIs)
        {
            if (infos[ui.Key].main && infos[ui.Key].group == ResearchInfo.ResearchGroup.vehicle && ui.Value.gameObject.activeSelf)
            {
                first = ui.Value;
                break;
            }
        }
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

        //var info = DataManager.Instance.researches[idx];

        TextMeshProUGUI nameText = upgradeDetailNameText;
        TextMeshProUGUI detailText = upgradeDetailText;
        Button unlockBtn = upgrade_UnlockBtn;

        //bool vehicle = info.group == ResearchInfo.ResearchGroup.vehicle;
        //if (vehicle)
        //{
        //    nameText = upgradeDetailNameText_Vehicle;
        //    detailText = upgradeDetailText_Vehicle;
        //    unlockBtn = upgrade_UnlockBtn_Vehicle;

        //    upgradePanel_Vehicle.SetActive(true);
        //    infoPanel_Vehicle.SetActive(false);
        //}

        if (idx == -1)
        {
            nameText.text = string.Empty;
            detailText.text = string.Empty;
            unlockBtn.gameObject.SetActive(false);
            return;
        }

        var info = DataManager.Instance.researches[idx];

        researchUIs[currentSelectUpgrade].UpdateUI();

        bool canResearch = !rm.MaxResearched(idx);

        StringBuilder st2 = new StringBuilder();
        if (info.hidden)
            st2.Append("<color=#320D5C>");
        if (info.max > 1)
            //st2.AppendFormat("<b>{0}</b> ({1}/{2})", tm.GetResearch(idx), rm.GetResearchCount(idx), info.max);
            st2.AppendFormat("<b>{0}</b>\n({1}/{2})", tm.GetResearch(idx), rm.GetResearchCount(idx), info.max);
        else
            st2.AppendFormat("<b>{0}</b>", tm.GetResearch(idx));
        if (info.hidden)
            st2.Append("</color>");
        nameText.text = st2.ToString();

        StringBuilder st = new StringBuilder();
        //if (!info.hidden && !vehicle)
        if (!info.hidden)
        {
            switch (info.tier)
            {
                case 1:
                    st.Append("<color=#950F14>");
                    st.AppendFormat(tm.GetCommons("Tier2"), info.tier + 1);
                    break;
                case 2:
                    st.Append("<color=#2E0E07>");
                    st.AppendFormat(tm.GetCommons("Tier2"), info.tier + 1);
                    break;
                case 3:
                    //st.Append("<color=#140A06>");
                    st.Append("<color=#1A3141>");
                    st.AppendFormat(tm.GetCommons("Tier2"), info.tier + 1);
                    break;
                case 4:
                    st.Append("<color=#573100>");
                    st.AppendFormat(tm.GetCommons("Tier2"), info.tier + 1);
                    break;
                default:
                    st.Append("<color=#092214>");
                    st.AppendFormat(tm.GetCommons("Tier2"), info.tier + 1);
                    break;
            }
            st.Append("</color>");
            st.AppendLine();
        }
        if (canResearch)
        {
            float rating = rm.GetRating_Require(idx);
            if (rating > 0)
            {
                if (GM.Instance.rating >= rating)
                {
                    st.AppendFormat(tm.GetCommons("RatingNeed"), "<sprite=1>", rating);
                }
                else
                {
                    st.AppendFormat(tm.GetCommons("RatingNeed"), "<sprite=1>", "<color=#A91111>" + rating + "</color>");
                }
                st.AppendLine();
            }

            //int cost = rm.GetCost(idx);
            //if (cost > 0)
            //{
            //    if (GM.Instance.gold >= cost)
            //    {
            //        st.AppendFormat("<sprite=2> {0} : {1}G", tm.GetCommons("Costs"), cost);
            //    }
            //    else
            //    {
            //        st.AppendFormat("<sprite=2> {0} : <color=#A91111>{1}G</color>", tm.GetCommons("Costs"), cost);
            //    }
            //}

            float rp = rm.GetResearchPoint(idx);
            if (rp > 0)
            {
                if (GM.Instance.researchPoint >= rp)
                {
                    st.AppendFormat("{0} <sprite=7> {1:0.#}", tm.GetCommons("Costs"), rp);
                }
                else
                {
                    st.AppendFormat("{0} <sprite=7> <color=#A91111>{1:0.#}</color>", tm.GetCommons("Costs"), rp);
                }
            }
        }
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

        info.effect.Showvillager_bonus(st);

        info.effect.ShowpizzeriaExpand(st);

        info.effect.ShowraidDefense(st);

        info.effect.Showcustomer_timelimit(st);
        info.effect.Showcustomer_max_tier(st);
        info.effect.Showorder_max(st);
        info.effect.Showcustomer_max_amount(st);

        info.effect.ShowmaxSpeed(st);
        info.effect.ShowdamageReduce(st);
        info.effect.Showacceleration(st);

        detailText.text = st.ToString();

        unlockBtn.gameObject.SetActive(canResearch);
        if (canResearch)
        {
            if (ResearchManager.Instance.CheckCanUnlocked(idx))
            {
                unlockBtn.GetComponent<Image>().color = uiLib.button_MainColor;
            }
            else
            {
                unlockBtn.GetComponent<Image>().color = uiLib.button_inactiveColor;
            }
        }
        //upgrade_UnlockBtn.enabled = canResearch;

        //if (vehicle)
        //    UINaviHelper.Instance.ingame.Shop_Vehicle_Reconnection();
        //else
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

            UIManager.Instance.shopUI.UpdateResearchUI();

            SelectUpgrade(currentSelectUpgrade);
        }
        else
        {
            AudioManager.Instance.PlaySFX(Sfx.deny);
        }
    }
    public void UpdateResearchUI()
    {
        foreach (var temp in researchUIs)
        {
            temp.Value.UpdateUI();
        }
    }

    public void ToggleResearchNotification(bool on)
    {
        if (on)
        {
            if (SomethingResearchable())
            {
                researchableNotification.SetActive(true);
            }
            else
                researchableNotification.SetActive(false);

            UIManager.Instance.shopUI.UpdateResearchUI();
        }
        else
            researchableNotification.SetActive(false);
    }

    public bool SomethingResearchable()
    {
        var infos = DataManager.Instance.researches;
        foreach (var ui in researchUIs)
        {
            if (ui.Value.gameObject.activeSelf && ResearchManager.Instance.CheckCanUnlocked(infos[ui.Key].idx))
            {
                return true;
            }
        }
        return false;
    }
    public ResearchUI GetMainResearch()
    {
        ResearchUI first = null;
        var infos = DataManager.Instance.researches;
        foreach (var ui in researchUIs)
        {
            if (infos[ui.Key].main && infos[ui.Key].group == ResearchInfo.ResearchGroup.upgrade && ui.Value.gameObject.activeSelf)
            {
                first = ui.Value;
                break;
            }
        }
        //SelectUpgrade(first.idx);
        return first;
    }
    #endregion

    #region 차량 변경
    public void ResetVehicleUI()
    {
        UpdateOwnedVehicles();

        int temp = 0;
        for (int i = 0; i < vehicleShowcaseOrders.Length; i++)
        {
            if (vehicleShowcaseOrders[i] == GM.Instance.currentVehicle)
                temp = i;
        }
        currentViewVehicle = temp;
        UpdateVehicleUI(ShowingVehicles);

        UINaviHelper.Instance.SetFirstSelect();
    }
    private void UpdateOwnedVehicles()
    {
        int count = 0;
        var array = GM.Instance.unlockedVehicles;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i]) count++;
        }
        ownedVehiclesText.text = $"{tm.GetCommons("OwnedVehicles")} {count} / {array.Length}";
    }
    private void UpdateVehicleUI(int idx)
    {
        HideAllVehicle();

        vehicleShowcase.vehicleModels[idx].gameObject.SetActive(true);
        vehicleShowcase.ResetAngle();

        ShowVehicleStat();
    }
    public void HideAllVehicle()
    {
        for (int i = 0; i < vehicleShowcase.vehicleModels.Length; i++)
        {
            vehicleShowcase.vehicleModels[i].gameObject.SetActive(false);
        }
    }
    private void ShowVehicleStat()
    {
        int current = ShowingVehicles;

        if (upgradePanel_Vehicle.activeSelf)
        {
            upgradePanel_Vehicle.SetActive(false);
            infoPanel_Vehicle.SetActive(true);

            UINaviHelper.Instance.ingame.Shop_Vehicle_Reconnection();
        }

        //SelectUpgrade(0);

        canBuyVehicleHighlight.SetActive(false);
        if (GM.Instance.currentVehicle == current)
        {
            selectVehicleBtnText.text = tm.GetCommons("SelectVehicles2");
            //selectVehicleBtn.image.color = new Color(0.1753262f, 0.6415094f, 0.09985761f);
            selectVehicleBtn.image.color = uiLib.button_greenColor;
            selectVehicleBtnMode = 0;
        }
        else
        {
            if (GM.Instance.unlockedVehicles[current])
            {
                selectVehicleBtnText.text = tm.GetCommons("SelectVehicles");
                //selectVehicleBtn.image.color = new Color(0.5283019f, 0.5283019f, 0.5283019f);
                selectVehicleBtn.image.color = uiLib.button_MainColor;
                selectVehicleBtnMode = 1;
            }
            else
            {
                //selectVehicleBtnText.text = $"{tm.GetCommons("BuyVehicles")} ({GM.Instance.costVehicles[current]}$)";

                if (GM.Instance.CanBuyVehicle(current))
                {
                    //selectVehicleBtn.image.color = new Color(0.8113208f, 0.5289791f, 0.1033286f);
                    selectVehicleBtn.image.color = uiLib.button_MainColor;
                    canBuyVehicleHighlight.SetActive(true);
                }
                else
                {
                    //selectVehicleBtn.image.color = new Color(0.8113208f, 0.5289791f, 0.1033286f);
                    selectVehicleBtn.image.color = uiLib.button_inactiveColor;
                }

                selectVehicleBtnText.text = string.Format(tm.GetCommons("RatingNeed"), "<sprite=1>", GM.Instance.ratingVehicles[current]);
                selectVehicleBtnMode = 2;
            }
        }

        vehicleInfo_NameText.text = tm.GetVehicles(current);

        var info = GM.Instance.controllerData[current];

        StringBuilder st = new StringBuilder();

        st.AppendLine();
        st.AppendLine();
        st.AppendFormat("{0} <size=95%>{1}</size>", tm.GetCommons("VehicleStat0"), info.maxLoad);
        st.AppendLine();
        int speed = (info.statStar % 100000000) / 1000000;
        int durability = (info.statStar % 1000000) / 10000;
        int handling = (info.statStar % 10000) / 100;
        int weight = info.statStar % 100;

        ShowStatStar(st, tm.GetCommons("VehicleStat1"), speed);
        ShowStatStar(st, tm.GetCommons("VehicleStat2"), durability);
        ShowStatStar(st, tm.GetCommons("VehicleStat3"), handling);
        ShowStatStar(st, tm.GetCommons("VehicleStat4"), weight);

        vehicleInfo_DetailText.text = st.ToString();

        void ShowStatStar(StringBuilder st, string stat, int value)
        {
            st.AppendFormat("{0} ", stat);
            int full = value / 10;
            for (int i = 0; i < full; i++)
            {
                st.Append("<size=90%><sprite=1> ");
            }
            float half = value / 10f - full;
            if (half > 0) st.Append("<sprite=4>");
            st.Append("</size>");
            st.AppendLine();
        }
    }
    public void SelectVehicleBtnAction()
    {
        switch (selectVehicleBtnMode)
        {
            case 0:
                break;
            case 1:
                GM.Instance.ChangeVehicle(ShowingVehicles);
                break;
            case 2:
                GM.Instance.BuyVehicle(ShowingVehicles);
                UpdateOwnedVehicles();
                break;
        }
        UpdateVehicleUI(ShowingVehicles);
    }
    public void ChangeViewVehicle(bool right)
    {
        if (right)
        {
            currentViewVehicle++;
            if (currentViewVehicle >= vehicleShowcase.vehicleModels.Length)
                currentViewVehicle = 0;
        }
        else
        {
            currentViewVehicle--;
            if (currentViewVehicle < 0)
                currentViewVehicle = vehicleShowcase.vehicleModels.Length - 1;
        }
        UpdateVehicleUI(ShowingVehicles);
    }
    #endregion
}
