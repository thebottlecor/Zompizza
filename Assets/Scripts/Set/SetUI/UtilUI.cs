using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UtilUI : EventListener
{

    [Header("À¯Æ¿ UI")]
    public List<PanelButtonPair> panelButtonPairs;
    public int activeSubPanel = 1;
    public float fadeTime = 1f;
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;

    public bool loading;
    public bool opened;

    public bool IsActive => loading || opened;

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
        activeSubPanel = 0;
        OpenUI();
    }

    public void OpenSettings()
    {
        activeSubPanel = 1;
        OpenUI();
    }

    public void OpenUI()
    {
        if (UIManager.Instance.isDirecting) return;

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

        GM.Instance.stop_control = true;
        Time.timeScale = 0f;
        loading = true;

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
        loading = false;
        opened = false;
        WorldMapManager.Instance.CloseFullscreenMap();

        rectTransform.anchoredPosition = new Vector2(0f, -2000f);
        canvasGroup.alpha = 0f;
    }

    public void HideUI()
    {
        if (UIManager.Instance.isDirecting) return;

        if (!opened) return;
        if (loading) return;

        opened = false;
        GM.Instance.stop_control = false;
        Time.timeScale = 1f;
        loading = true;

        WorldMapManager.Instance.CloseFullscreenMap();
        WorldMapManager.Instance.OpenMinimap();

        for (int i = 0; i < panelButtonPairs.Count; i++)
        {
            panelButtonPairs[i].button.Hide();
        }

        canvasGroup.alpha = 1f;
        rectTransform.transform.localPosition = new Vector3(0f, 0f, 0f);
        rectTransform.DOAnchorPos(new Vector2(0f, -2000f), fadeTime, false).SetEase(Ease.InOutQuint).SetUpdate(true);
        canvasGroup.DOFade(0f, fadeTime).SetUpdate(true).OnComplete(() =>
        {
            loading = false;
        });
    }

    public void SelectSubPanel(int idx)
    {
        if (idx == 0) // ¿ùµå¸Ê
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
        panelButtonPairs[idx].button.SetHighlight(true);

        activeSubPanel = idx;
    }
}
