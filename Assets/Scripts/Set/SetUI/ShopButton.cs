using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopButton : MonoBehaviour //, IPointerEnterHandler, IPointerExitHandler
{

    public Image buttonImage;
    public ScrollingUIEffect scrollingUI;

    private void Start()
    {
        scrollingUI.enabled = false;
    }

    public void SetHighlight(bool on)
    {
        if (on)
        {
            buttonImage.sprite = DataManager.Instance.uiLibrary.shopButtonHighlighted;
            scrollingUI.enabled = true;
        }
        else
        {
            buttonImage.sprite = DataManager.Instance.uiLibrary.shopButton;
            scrollingUI.enabled = false;
        }
    }

    public void Hide()
    {
        scrollingUI.enabled = false;
    }

    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    scrollingUI.enabled = true;
    //}

    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    scrollingUI.enabled = false;
    //}
}
