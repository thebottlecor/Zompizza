using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("°¡°Ô UI")]
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

    public TextManager tm => TextManager.Instance;
    public TextMeshProUGUI[] buttonTexts;

    public Image[] pizzaBoys;
    public Image[] pizzaBoxes;

    public ScrollRect scrollRect;
    public RectTransform contentPanel;

    public void UpdateTexts()
    {
        buttonTexts[0].text = tm.GetCommons("Order");
        buttonTexts[1].text = tm.GetCommons("Management");
        buttonTexts[2].text = tm.GetCommons("Shop");
        buttonTexts[3].text = tm.GetCommons("News");

        buttonTexts[4].text = tm.GetCommons("Back");

    }

    protected override void AddListeners()
    {
        ShopEnter.PlayerArriveEvent += OnPlayerArriveShop;
        ShopEnter.PlayerExitEvent += OnPlayerExitShop;
        PizzaDirection.PizzaCompleteEvent += OnPizzaCompleted;
    }

    protected override void RemoveListeners()
    {
        ShopEnter.PlayerArriveEvent -= OnPlayerArriveShop;
        ShopEnter.PlayerExitEvent -= OnPlayerExitShop;
        PizzaDirection.PizzaCompleteEvent -= OnPizzaCompleted;
    }

    private void OnPlayerArriveShop(object sender, EventArgs e)
    {
        OpenUI();
        playerStay = true;
    }

    private void OnPlayerExitShop(object sender, EventArgs e)
    {
        playerStay = false;
    }

    private void OnPizzaCompleted(object sender, OrderInfo e)
    {
        UpdatePizzaBox();
    }

    public void ShowOrder(int customerIdx)
    {
        activeSubPanel = 0;
        SnapTo(UIManager.Instance.orderUIObjects[customerIdx].transform as RectTransform);
        OpenUI();
    }

    public void SnapTo(RectTransform target)
    {
        //Canvas.ForceUpdateCanvases();

        //float value = target.anchoredPosition.y / (scrollRect.content.rect.height - (scrollRect.transform as RectTransform).rect.height);
        //Debug.Log(target.anchoredPosition.y + " > " + (scrollRect.content.rect.height - (scrollRect.transform as RectTransform).rect.height) + " >> " + value);

        //float value = (Mathf.Abs(target.anchoredPosition.y) - target.sizeDelta.y) / scrollRect.content.rect.height;
        //Debug.Log((Mathf.Abs(target.anchoredPosition.y) + target.sizeDelta.y) + " > " + scrollRect.content.rect.height + " >> " + value);

        float value = Mathf.Abs(target.anchoredPosition.y) / scrollRect.content.rect.height;

        scrollRect.verticalNormalizedPosition = 1f - value;
    }

    private void UpdatePizzaBox()
    {
        int count = OrderManager.Instance.GetCurrentPizzaBox();

        pizzaBoys[0].gameObject.SetActive(count != 0);
        pizzaBoys[1].gameObject.SetActive(count == 0);

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


    public void OpenUI()
    {
        if (UIManager.Instance.isDirecting) return;

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

        GM.Instance.stop_control = true;
        Time.timeScale = 0f;
        loading = true;

        UIManager.Instance.orderMiniUIParent.SetActive(false);
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
        loading = false;
        opened = false;

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

        UIManager.Instance.orderMiniUIParent.SetActive(true);
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
            UpdatePizzaBox();
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
