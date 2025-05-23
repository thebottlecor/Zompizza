using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Text;

public class UIManager : Singleton<UIManager>
{

    public ShopUI shopUI;
    public UtilUI utilUI;
    public VillagerUI villagerUI;

    public RectTransform movingSettingsPanelParent;

    public bool isDirecting;
    public bool changingResolution;

    public GameObject uiCam;
    public GameObject minimapCam;
    public GameObject worldmapCam;

    public List<OrderUIObject> orderUIObjects;

    [Header("운행중 배달 정보")]
    public GameObject orderMiniUI_Source;
    public Transform orderMiniUIContexts;
    public List<OrderMiniUI> orderMiniUIs;
    public GameObject orderMiniUIParent;

    public GameObject speedInfo;
    public GameObject talkInfo;
    public GameObject timeInfo;
    public GameObject padUIs;
    public GameObject installUIs;
    public GameObject orderIndicator;

    [Header("재료창")]
    public GameObject ingredient_Source;
    public Transform[] ingredient_Parents;
    public List<IngredientUI> ingredientUIs;
    public Dictionary<Ingredient, IngredientUI> ingredientUIPairs;

    [Header("티어업")]
    public int tierUpMilestone;
    public int vehicleMilestone;
    public GameObject[] vehicleImages;
    public GameObject tierUpEffect;
    public GameObject vehicleUpEffect;
    public GameObject tierUpPanel;
    public TextMeshProUGUI tierUpTMP;

    private TextManager tm => TextManager.Instance;

    public void Init()
    {
        var panel = SettingManager.Instance.ingameMovingPanel;
        panel.SetParent(movingSettingsPanelParent);
        panel.gameObject.SetActive(true);

        panel.offsetMin = new Vector2(0f, 0f);
        panel.offsetMax = new Vector2(0f, 0f);
        panel.localScale = Vector3.one;

        shopUI.UpdateTexts();
        utilUI.UpdateTexts();
        villagerUI.UpdateTexts();

        // 운행중 배달 정보
        orderMiniUIs = new List<OrderMiniUI>(Constant.maxOrderDaily);
        for (int i = 0; i < Constant.maxOrderDaily; i++)
        {
            var obj = Instantiate(orderMiniUI_Source, orderMiniUIContexts);
            OrderMiniUI miniUI = obj.GetComponent<OrderMiniUI>();
            orderMiniUIs.Add(miniUI);
            obj.SetActive(false);
        }
        // 재료창
        var ingredients = Enum.GetValues(typeof(Ingredient));
        ingredientUIPairs = new Dictionary<Ingredient, IngredientUI>();
        var ingredientLib = DataManager.Instance.ingredientLib;

        List<List<UINavi>> navis = new List<List<UINavi>>();

        foreach (var temp in ingredients)
        {
            Ingredient ingredient = (Ingredient)temp;
            int parentIdx = -1;
            int tier = 0;
            if (ingredientLib.meats.ContainsKey(ingredient) && ingredientLib.meats[ingredient].valid)
            {
                parentIdx = 0;
                tier = ingredientLib.meats[ingredient].tier;
            }
            else if (ingredientLib.vegetables.ContainsKey(ingredient) && ingredientLib.vegetables[ingredient].valid)
            {
                parentIdx = 1;
                tier = ingredientLib.vegetables[ingredient].tier;
            }
            else if (ingredientLib.herbs.ContainsKey(ingredient) && ingredientLib.herbs[ingredient].valid)
            {
                parentIdx = 2;
                tier = ingredientLib.herbs[ingredient].tier;
            }
            if (parentIdx > -1)
            {
                var obj = Instantiate(ingredient_Source, ingredient_Parents[parentIdx]);
#if UNITY_EDITOR
                obj.name = ingredient.ToString();
#endif
                IngredientUI ingredientUI = obj.GetComponent<IngredientUI>();
                ingredientUI.Init(ingredient);
                ingredientUIPairs.Add(ingredient, ingredientUI);
                ingredientUIs.Add(ingredientUI);

                if (navis.Count == tier)
                    navis.Add(new List<UINavi>());

                var navi = obj.GetComponent<UINavi>();
                navis[tier].Add(navi);
                (navi as UINaviTwin).ing = ingredientUI;
            }
        }
        for (int i = 0; i < navis.Count; i++)
        {
            for (int n = 0; n < navis[i].Count; n++)
            {
                var self = navis[i][n];

                if (n == 0)
                {
                    self.left = UINaviHelper.Instance.ingame.shops_orders_making[0];
                }

                if (n > 0)
                    self.left = navis[i][n - 1];
                if (n < navis[i].Count - 1)
                    self.right = navis[i][n + 1];

                if (n == navis[i].Count - 1)
                {
                    self.right = navis[i][0];
                }

                if (i == 0)
                {
                    self.up = navis[navis.Count - 1][n];
                }

                if (i > 0)
                    self.up = navis[i - 1][n];
                if (i < navis.Count - 1)
                    self.down = navis[i + 1][n];

                if (i == navis.Count - 1)
                {
                    self.down = navis[0][n];
                }
            }
        }
        UINaviHelper.Instance.ingame.shops_orders_making[0].right = navis[0][0];
        UINaviHelper.Instance.ingame.shops_orders_making[1].left = navis[0][navis[0].Count - 1];

        float ratio = (float)SettingManager.Instance.settingResolution.y / SettingManager.Instance.settingResolution.x;
        float modify = (ratio > 0.5625f) ? 0f : 1f;

        var canvasScaler = FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < canvasScaler.Length; i++)
        {
            canvasScaler[i].matchWidthOrHeight = modify;
        }
    }

    public void ToggleDrivingInfo(bool on)
    {
        if (on)
        {
            Cursor.visible = false;

            uiCam.SetActive(false);
            minimapCam.SetActive(true);
            worldmapCam.SetActive(false);

            if (TutorialManager.Instance.training && TutorialManager.Instance.step <= 1) return;

            WorldMapManager.Instance.OpenMinimap();
            orderMiniUIParent.SetActive(true);
            timeInfo.SetActive(true);
            talkInfo.SetActive(true);

            if (GM.Instance.midNight) return;

            speedInfo.SetActive(true);
            if (!GM.Instance.pizzeriaStay)
                installUIs.SetActive(true);
        }
        else
        {
            var pad = Gamepad.current;
            Cursor.visible = pad == null;

            uiCam.SetActive(true);
            minimapCam.SetActive(false);
            worldmapCam.SetActive(true);
            WorldMapManager.Instance.CloseMinimap();
            orderMiniUIParent.SetActive(false);
            timeInfo.SetActive(false);
            talkInfo.SetActive(false);
            speedInfo.SetActive(false);
            installUIs.SetActive(false);
        }
    }

    public void UpdateIngredientsTier()
    {
        for (int i = 0; i < ingredientUIs.Count; i++)
        {
            ingredientUIs[i].CheckValid();
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

    protected override void AddListeners()
    {
        InputHelper.EscapeEvent += OnESC;
        InputHelper.WorldmapEvent += OnWorldmap;
        InputHelper.TabMoveEvent += OnTabMove;
        InputHelper.FastTravelEvent += OnFastTravel;
        InputHelper.EnterStoreEvent += OnShopEnter;
    }
    protected override void RemoveListeners()
    {
        InputHelper.EscapeEvent -= OnESC;
        InputHelper.WorldmapEvent -= OnWorldmap;
        InputHelper.TabMoveEvent -= OnTabMove;
        InputHelper.FastTravelEvent -= OnFastTravel;
        InputHelper.EnterStoreEvent -= OnShopEnter;
    }

    private bool Interacting(InputAction.CallbackContext e)
    {
        if (!e.performed) return false;
        if (isDirecting || changingResolution) return false;
        if (GM.Instance.loading) return false;
        if (LoadingSceneManager.Instance.logueLoading) return false;
        if (GameEventManager.Instance.eventPanel.activeSelf) return false;
        if (DialogueManager.Instance.eventPanel.activeSelf) return false;
        if (TutorialManager.Instance.blackScreen.activeSelf) return false;
        return true;
    }

    public bool Panels_Inactive => !shopUI.IsActive && !utilUI.IsActive && !villagerUI.IsActive;
    public bool Panels_Loading => shopUI.loading || utilUI.loading || villagerUI.loading;

    private void OnShopEnter(object sender, InputAction.CallbackContext e)
    {
        if (!Interacting(e)) return;

        var vs = VillagerManager.Instance.villagerSearcher;

        if (shopUI.shopUIOpenButton.activeSelf)
        {
            if (Panels_Inactive)
            {
                shopUI.ShowOrder();
            }
        }
        else if (vs.talkObj.activeSelf) // 주민과 대화
        {
            if (Panels_Inactive)
            {
                int idx = vs.GetVillagerIdx();
                if (idx >= 0)
                    villagerUI.OpenUI(idx);
            }
        }
        else if (GM.Instance.installJumpEnable)
        {
            if (!GM.Instance.pizzeriaStay && installUIs.activeSelf)
            {
                GM.Instance.EnableJumpRampInstall(false);
                GM.Instance.InstallJumpRamp();
                TutorialManager.Instance.JumpRampInstalled();
            }
        }
    }
    private void OnFastTravel(object sender, InputAction.CallbackContext e)
    {
        if (!Interacting(e)) return;

        if (OrderManager.Instance.fastTravleBtn.gameObject.activeInHierarchy)
        {
            OrderManager.Instance.FastTravelAction();
        }
    }

    private void OnWorldmap(object sender, InputAction.CallbackContext e)
    {
        if (!Interacting(e)) return;

        if (Panels_Inactive)
        {
            utilUI.OpenWorldMap();
        }
    }

    private void OnTabMove(object sender, InputAction.CallbackContext e) // 주행중 카메라 시점 변경
    {
        if (!Interacting(e)) return;

        if (Panels_Inactive)
        {
            float value = e.ReadValue<float>();

            if (value > 0)
            {

            }
            else if (value < 0)
            {
                GM.Instance.player.cam.ChangeMode();
            }
        }
    }

    public void OnESC(object sender, InputAction.CallbackContext e)
    {
        if (!Interacting(e)) return;

        // ShopUI => TabMove 쪽도 동기화시키기!
        /// <summary>
        /// <see cref="ShopUI.OnTabMove"/>
        /// <ser cref="UINaviHelper.SetFirstSelect"/>
        /// </summary>

        if (UINaviHelper.Instance.inputHelper.disconnectedPanel.activeSelf)
        {
            UINaviHelper.Instance.inputHelper.PadConnected();
            return;
        }

        if (GM.Instance.darkCanvas.blocksRaycasts) return;

        var exploration = ExplorationManager.Instance;
        if (exploration.canvasGroupLoading)
            return;
        else if (exploration.canvasGroup_resultPanel.alpha >= 0.99f)
        {
            exploration.HideUI_ResultPanel();
            return;
        }

        if (GM.Instance.raidObj.activeSelf)
        {
            GM.Instance.HideRaidPanel();
            return;
        }
        if (RivalManager.Instance.rankingObj.activeSelf)
        {
            RivalManager.Instance.HideRankingPanel();
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
        if (utilUI.sosWarningObj_maps.activeSelf)
        {
            utilUI.ShowSosWarning(false);
            return;
        }
        if (shopUI.sosWarningObj.activeSelf)
        {
            shopUI.ShowSosWarning(false);
        }
        if (villagerUI.expelWarningObj.activeSelf)
        {
            villagerUI.ShowExpelWarning(false);
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
        else if (villagerUI.IsActive)
        {
            villagerUI.HideUI();
        }
        else
        {
            utilUI.OpenSettings();
        }
    }


    #region 주문 UI
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


    public void VehicleUnlock()
    {
        int newVehicle = GM.Instance.Auto_ButVehicle();
        int totalVehicle = 0;
        var array = GM.Instance.unlockedVehicles;
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i]) totalVehicle++;
        }

        if (SteamHelper.Instance != null && totalVehicle >= array.Length - 1) SteamHelper.Instance.AchieveVehicle();
        if (newVehicle < 0) return;

        int currentVehicle = GM.Instance.currentVehicle;
        var vehicleInfos = GM.Instance.controllerData;
        if (vehicleInfos[newVehicle].maxLoad >= vehicleInfos[currentVehicle].maxLoad)
        {
            GM.Instance.ChangeVehicle(newVehicle);
        }

        if (vehicleMilestone < totalVehicle)
        {
            for (int i = 0; i < vehicleImages.Length; i++)
            {
                vehicleImages[i].SetActive(false);
            }
            vehicleImages[newVehicle - 1].SetActive(true);

            vehicleMilestone = totalVehicle;
            vehicleUpEffect.transform.position = new Vector3(-100f, 100f, 0f);
            vehicleUpEffect.SetActive(true);

            StringBuilder st = new StringBuilder();
            st.Append("<color=#E66D4C>").Append(tm.GetVehicles(newVehicle)).Append("</color>");
            st.AppendLine();
            st.Append("<size=50%>").AppendFormat(tm.GetCommons("NewVehicle"));

            tierUpTMP.text = st.ToString();
            tierUpPanel.SetActive(true);

            AudioManager.Instance.PlaySFX(GameEventManager.Instance.audioClips[2]);
            AudioManager.Instance.PlaySFX(Sfx.pizzaComplete);

            StartCoroutine(TierUpEffectHide());
        }
    }

    public void TierUp()
    {
        ResearchManager.Instance.AutoResearch_For_Tier();

        int tier = ResearchManager.Instance.globalEffect.tier;

        if (SteamHelper.Instance != null) SteamHelper.Instance.AchieveTier(tier);

        if (tierUpMilestone < tier)
        {
            ExplorationManager.Instance.SetHighTierQuality();

            tierUpMilestone = tier;
            if (vehicleUpEffect.activeSelf)
            {
                vehicleUpEffect.transform.position = new Vector3(-102f, 100f, 0f);
                tierUpEffect.transform.position = new Vector3(-98f, 100f, 0f);
            }
            else
            {
                tierUpEffect.transform.position = new Vector3(-100f, 100f, 0f);
            }
            tierUpEffect.SetActive(true);

            StringBuilder st = new StringBuilder();
            st.Append("<color=#E66D4C>").Append(tm.GetCommons("Tier3")).Append("</color>");
            st.AppendLine();
            st.Append("<size=50%>").AppendFormat(tm.GetCommons("UpgradeEffect5"), tier + 1);

            tierUpTMP.text = st.ToString();
            tierUpPanel.SetActive(true);

            AudioManager.Instance.PlaySFX(GameEventManager.Instance.audioClips[2]);
            AudioManager.Instance.PlaySFX(Sfx.pizzaComplete);

            StartCoroutine(TierUpEffectHide());
        }
    }
    private IEnumerator TierUpEffectHide()
    {
        yield return CoroutineHelper.WaitForSecondsRealtime(3f);
        tierUpEffect.SetActive(false);
        vehicleUpEffect.SetActive(false);
        tierUpPanel.SetActive(false);
    }
}
