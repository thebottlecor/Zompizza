using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VillagerUI : EventListener
{

    [Header("가게 UI")]
    public List<PanelButtonPair> panelButtonPairs;
    public int activeSubPanel = 1;
    public float fadeTime = 1f;
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;

    [HideInInspector] public bool loading;
    [HideInInspector] public bool opened;

    public bool IsActive => loading || opened;

    public ScrollingUIEffect scrollEffect;

    [Header("텍스트")]
    private int currentVillagerIdx;
    public Slider expSlider;
    public TextMeshProUGUI[] fixedTexts;
    public TextMeshProUGUI[] infoTexts;
    private string[] fixedStr;
    public TextMeshProUGUI villagerName;
    public Image villagerProfile;

    [Header("주기")]
    public GameObject needsParentObj;
    public Button giveBtn;
    public GameObject[] giveEffects;
    public TextMeshProUGUI needsText;
    public Image needsIcon;

    [Header("주문탭")]
    public GameObject expelWarningObj;
    public TextMeshProUGUI[] exeplWarningBtn_Text;
    public TextMeshProUGUI expelWarning_Text;
    public TextMeshProUGUI expelWarningDetail_Text;


    private TextManager tm => TextManager.Instance;
    private UIManager um => UIManager.Instance;

    public void UpdateTexts()
    {
        fixedTexts[0].text = tm.GetCommons("Give");
        fixedTexts[1].text = tm.GetCommons("Expel");
        fixedTexts[2].text = tm.GetCommons("Close");
        fixedTexts[3].text = tm.GetCommons("Traits");
        fixedTexts[4].text = tm.GetCommons("Inventory");

        fixedStr = new string[3];
        fixedStr[0] = tm.GetCommons("Relations");
        fixedStr[1] = tm.GetCommons("Condition");
        fixedStr[2] = tm.GetCommons("Income");

        expelWarning_Text.text = tm.GetCommons("Warning");
        expelWarningDetail_Text.text = tm.GetCommons("ExpelWarning");
        exeplWarningBtn_Text[0].text = tm.GetCommons("Expel");
        exeplWarningBtn_Text[1].text = tm.GetCommons("Cancel");
    }
    public void UpdateUI(int villagerIdx)
    {
        villagerName.text = tm.GetVillagerName(villagerIdx);
        villagerProfile.sprite = DataManager.Instance.uiLib.villagerProfile[villagerIdx];

        var villager = VillagerManager.Instance.villagers[villagerIdx];

        float sliderValue = villager.relationExp > 0.01f ? villager.relationExp : 0.01f;
        expSlider.value = sliderValue;

        infoTexts[0].text = $"{fixedStr[0]} <size=75%> {villager.relations + 1}";
        switch (villager.condition)
        {
            case 0:
                infoTexts[1].text = $"{fixedStr[1]}\n<size=75%> <size=100%><sprite=\"emoji\" index={villager.condition + 6}> <size=50%><color=#851111> {fixedStr[2]} -100%";
                break;
            case 1:
                infoTexts[1].text = $"{fixedStr[1]}\n<size=75%> <size=100%><sprite=\"emoji\" index={villager.condition + 6}> <size=50%><color=#851111> {fixedStr[2]} -50%";
                break;
            case 2:
                infoTexts[1].text = $"{fixedStr[1]}\n<size=75%> <size=100%><sprite=\"emoji\" index={villager.condition + 6}> <size=50%><color=#1D1D1D> {fixedStr[2]} +0%";
                break;
            case 3:
                infoTexts[1].text = $"{fixedStr[1]}\n<size=75%> <size=100%><sprite=\"emoji\" index={villager.condition + 6}> <size=50%><color=#1B8411> {fixedStr[2]} +50%";
                break;
            case 4:
                infoTexts[1].text = $"{fixedStr[1]}\n<size=75%> <size=100%><sprite=\"emoji\" index={villager.condition + 6}> <size=50%><color=#1B8411> {fixedStr[2]} +100%";
                break;
        }
        infoTexts[2].text = $"{fixedStr[2]}\n<size=75%> <sprite=2> +{villager.Income()}";

        if (villager.currentNeeds >= 0)
        {
            needsText.text = villager.gender == 0 ? tm.GetCommons("Needs1") : tm.GetCommons("Needs2");
            needsIcon.sprite = DataManager.Instance.uiLib.villagerItems[villager.currentNeeds];
            needsParentObj.SetActive(true);
            giveBtn.gameObject.SetActive(true);
        }
        else
        {
            needsParentObj.SetActive(false);
            giveBtn.gameObject.SetActive(false);
        }
        UINaviHelper.Instance.SetFirstSelect();
    }
    public void Give()
    {
        bool success = VillagerManager.Instance.Give(currentVillagerIdx);
        if (success)
        {
            UpdateUI(currentVillagerIdx);
            for (int i = 0; i <giveEffects.Length; i++)
                giveEffects[i].SetActive(true);
        }
    }

    protected override void AddListeners()
    {
        //InputHelper.TabMoveEvent += OnTabMove;
    }
    protected override void RemoveListeners()
    {
        //InputHelper.TabMoveEvent -= OnTabMove;
    }
    /*
    private void OnTabMove(object sender, InputAction.CallbackContext e)
    {
        /// <summary>
        /// <see cref="UIManager.OnESC"/>
        /// <ser cref="UINaviHelper.SetFirstSelect"/>
        /// </summary>

        if (um.isDirecting || um.changingResolution) return;
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
        if (GameEventManager.Instance.eventPanel.activeSelf) return;
        if (DialogueManager.Instance.eventPanel.activeSelf) return;
        if (TutorialManager.Instance.blackScreen.activeSelf) return;

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
    */

    public void OpenUI(int idx)
    {
        if (um.isDirecting || um.changingResolution) return;
        if (GM.Instance.loading) return;

        if (loading) return;

        if (um.utilUI.IsActive)
        {
            um.utilUI.HideUI_Replace();
        }
        if (um.shopUI.IsActive)
        {
            um.shopUI.HideUI_Replace();
        }

        if (opened)
        {
            HideUI();
            return;
        }

        scrollEffect.enabled = true;
        um.OffAll_Ingredient_Highlight();
        GM.Instance.stop_control = true;
        Time.timeScale = 0f;
        loading = true;

        um.ToggleDrivingInfo(false);

        um.orderIndicator.SetActive(false);
        um.padUIs.SetActive(false);
        OrderManager.Instance.fastTravelBtnParnet.SetActive(false);
        ExplorationManager.Instance.HideUI_ResultPanel_Instant();

        for (int i = 0; i < giveEffects.Length; i++)
            giveEffects[i].SetActive(false);
        currentVillagerIdx = idx;
        UpdateUI(currentVillagerIdx);
        SelectSubPanel(0);

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
        //for (int i = 0; i < panelButtonPairs.Count; i++)
        //{
        //    panelButtonPairs[i].button.Hide();
        //}

        DOTween.Kill(rectTransform);
        DOTween.Kill(canvasGroup);
        scrollEffect.enabled = false;
        loading = false;
        opened = false;

        rectTransform.anchoredPosition = new Vector2(0f, -2000f);
        canvasGroup.alpha = 0f;

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

        //for (int i = 0; i < panelButtonPairs.Count; i++)
        //{
        //    panelButtonPairs[i].button.Hide();
        //}

        TutorialManager.Instance.ShopWindowHide();

        if (instant)
        {
            rectTransform.transform.localPosition = new Vector3(0f, -2000f, 0f);
            canvasGroup.alpha = 0f;
            loading = false;
            TutorialManager.Instance.ManMove_TalkEnd();
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
                TutorialManager.Instance.ManMove_TalkEnd();
                UINaviHelper.Instance.SetFirstSelect();
            });
        }
    }

    public void SelectSubPanel(int idx)
    {
        for (int i = 0; i < panelButtonPairs.Count; i++)
        {
            panelButtonPairs[i].panel.SetActive(false);
            //panelButtonPairs[i].button.SetHighlight(false);
        }

        panelButtonPairs[idx].panel.SetActive(true);
        //panelButtonPairs[idx].button.SetHighlight(true);

        activeSubPanel = idx;

        UINaviHelper.Instance.SetFirstSelect();
    }

    public void ShowExpelWarning(bool on)
    {
        expelWarningObj.SetActive(on);
        UINaviHelper.Instance.SetFirstSelect();
    }
    public void Expel() // 직접 추방
    {
        var expel = VillagerManager.Instance.villagers[currentVillagerIdx];
        expel.Expel();

        VillagerManager.Instance.villagerSearcher.Clear();

        ShowExpelWarning(false);
        HideUI();
        AudioManager.Instance.PlaySFX(Sfx.close);
        UINaviHelper.Instance.SetFirstSelect();
    }
}
