using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UtilUI : EventListener
{

    [Header("유틸 UI")]
    public List<PanelButtonPair> panelButtonPairs;
    public int activeSubPanel = 1;
    public float fadeTime = 1f;
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;

    [HideInInspector] public bool loading;
    [HideInInspector] public bool opened;

    public bool IsActive => loading || opened;

    public ScrollingUIEffect scrollEffect;

    public TextMeshProUGUI[] buttonTexts;

    private UIManager um => UIManager.Instance;
    private TextManager tm => TextManager.Instance;

    public void UpdateTexts()
    {
        buttonTexts[0].text = tm.GetCommons("Worldmap");
        buttonTexts[1].text = tm.GetCommons("Menu");

        //buttonTexts[2].text = tm.GetCommons("Back");
        buttonTexts[2].text = tm.GetCommons("Close");
    }

    protected override void AddListeners()
    {
        InputHelper.TabMoveEvent += OnTabMove;
    }

    protected override void RemoveListeners()
    {
        InputHelper.TabMoveEvent -= OnTabMove;
    }

    private void OnTabMove(object sender, InputAction.CallbackContext e)
    {
        if (!opened || loading) return;

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

    public void OpenWorldMap()
    {
        // 버그 해결법 - 세팅창과 월드맵을 번갈아 연다
        activeSubPanel = 0;
        OpenUI();
        StartCoroutine(DelayOpenWorldMap());
    }
    private IEnumerator DelayOpenWorldMap()
    {
        yield return CoroutineHelper.WaitForSecondsRealtime(fadeTime);
        SelectSubPanel(1);
        SelectSubPanel(0);

    }

    public void OpenSettings()
    {
        activeSubPanel = 1;
        OpenUI();
    }

    public void OpenUI()
    {
        if (um.isDirecting || um.changingResolution) return;
        if (GM.Instance.loading) return;

        if (loading) return;

        if (um.shopUI.IsActive)
        {
            um.shopUI.HideUI_Replace();
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
        GM.Instance.stop_control = true;
        Time.timeScale = 0f;
        loading = true;

        um.ToggleDrivingInfo(false);

        um.orderIndicator.SetActive(false);
        um.padUIs.SetActive(false);
        OrderManager.Instance.fastTravelBtnParnet.SetActive(false);
        WorldMapManager.Instance.OpenFullscreenMap();
        UINaviHelper.Instance.inputHelper.GuidePadCheck();

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
        WorldMapManager.Instance.CloseFullscreenMap();
        SettingManager.Instance.EndKeyBinding();

        rectTransform.anchoredPosition = new Vector2(0f, -2000f);
        canvasGroup.alpha = 0f;

        UINaviHelper.Instance.SetFirstSelect();
    }

    public void HideUI()
    {
        //if (um.isDirecting) OrderManager.Instance.pizzaDirection.StopSequence();
        if (um.isDirecting || um.changingResolution) return;
        if (GM.Instance.loading) return;

        if (!opened) return;
        if (loading) return;

        scrollEffect.enabled = false;
        opened = false;
        GM.Instance.stop_control = false;
        Time.timeScale = 1f;
        loading = true;

        um.ToggleDrivingInfo(true);

        um.orderIndicator.SetActive(true);
        um.padUIs.SetActive(true);
        OrderManager.Instance.fastTravelBtnParnet.SetActive(true);
        WorldMapManager.Instance.CloseFullscreenMap();
        SettingManager.Instance.EndKeyBinding();

        for (int i = 0; i < panelButtonPairs.Count; i++)
        {
            panelButtonPairs[i].button.Hide();
        }

        TutorialManager.Instance.ShopWindowHide();

        canvasGroup.alpha = 1f;
        rectTransform.transform.localPosition = new Vector3(0f, 0f, 0f);
        rectTransform.DOAnchorPos(new Vector2(0f, -2000f), fadeTime, false).SetEase(Ease.InOutQuint).SetUpdate(true);
        canvasGroup.DOFade(0f, fadeTime).SetUpdate(true).OnComplete(() =>
        {
            loading = false;
            TutorialManager.Instance.ShopWindowHideComplete();
            UINaviHelper.Instance.SetFirstSelect();
        });
    }

    public void SelectSubPanel(int idx)
    {
        SettingManager.Instance.EndKeyBinding();

        if (idx == 0) // 월드맵
        {
            //WorldMapManager.Instance.OpenFullscreenMap();

            WorldMapManager.Instance.FirstFocus();
        }
        else
        {
            //WorldMapManager.Instance.CloseFullscreenMap();

            SettingManager.Instance.ShowSubSettingPanel(0);
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
}
