using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UtilUI : EventListener
{

    [Header("유틸 UI")]
    public List<PanelButtonPair> panelButtonPairs;
    public int activeSubPanel = 1;
    public float fadeTime = 1f;
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;

    public bool loading;
    public bool opened;

    public bool IsActive => loading || opened;

    public ScrollingUIEffect scrollEffect;

    public TextManager tm => TextManager.Instance;
    public TextMeshProUGUI[] buttonTexts;

    public void UpdateTexts()
    {
        buttonTexts[0].text = tm.GetCommons("Worldmap");
        buttonTexts[1].text = tm.GetCommons("Menu");

        buttonTexts[2].text = tm.GetCommons("Back");

    }

    protected override void AddListeners()
    {

    }

    protected override void RemoveListeners()
    {

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
        if (UIManager.Instance.isDirecting || UIManager.Instance.changingResolution) return;
        if (GM.Instance.loading) return;

        if (loading) return;

        if (UIManager.Instance.shopUI.IsActive)
        {
            UIManager.Instance.shopUI.HideUI_Replace();
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

        UIManager.Instance.orderMiniUIParent.SetActive(false);
        UIManager.Instance.speedInfo.SetActive(false);
        UIManager.Instance.timeInfo.SetActive(false);
        WorldMapManager.Instance.CloseMinimap();
        WorldMapManager.Instance.OpenFullscreenMap();

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

        rectTransform.anchoredPosition = new Vector2(0f, -2000f);
        canvasGroup.alpha = 0f;
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
        WorldMapManager.Instance.CloseFullscreenMap();
        WorldMapManager.Instance.OpenMinimap();

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
        });
    }

    public void SelectSubPanel(int idx)
    {
        if (idx == 0) // 월드맵
        {
            //WorldMapManager.Instance.OpenFullscreenMap();

            WorldMapManager.Instance.FirstFocus();
        }
        else
        {
            //WorldMapManager.Instance.CloseFullscreenMap();
        }

        for (int i = 0; i < panelButtonPairs.Count; i++)
        {
            panelButtonPairs[i].panel.SetActive(false);
            panelButtonPairs[i].button.SetHighlight(false);
        }

        panelButtonPairs[idx].panel.SetActive(true);
        //if (idx != 0)
        //    panelButtonPairs[idx].panel.SetActive(true);
        //else
        //    StartCoroutine(DelayShow(idx));
        panelButtonPairs[idx].button.SetHighlight(true);

        activeSubPanel = idx;
    }
    private IEnumerator DelayShow(int idx)
    {
        yield return CoroutineHelper.WaitForSecondsRealtime(0.01f);
        panelButtonPairs[idx].panel.SetActive(true);
    }
}
