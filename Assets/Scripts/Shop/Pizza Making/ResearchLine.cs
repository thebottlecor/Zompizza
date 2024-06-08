using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class ResearchLine : MonoBehaviour
{

    public RectTransform selfRect;
    public RectTransform targetRect;

    public ResearchInfo targetResearch;

    public UILineRenderer lineRenderer;


    private void Start()
    {
        InitPos();

        this.gameObject.SetActive(ResearchManager.Instance.CheckValid(targetResearch.idx));
    }

    public void InitPos()
    {
        (transform as RectTransform).anchoredPosition = selfRect.anchoredPosition;
        lineRenderer.Points[0] = Vector2.zero;
        Vector2 offset = targetRect.anchoredPosition - selfRect.anchoredPosition;
        lineRenderer.Points[1] = offset;
    }

    public void InitVisual()
    {
        if (ResearchManager.Instance.Researched(targetResearch.idx))
        {
            // 연구됨
            lineRenderer.LineThickness = DataManager.Instance.uiLib.researchLineInfos[1].thickness;
            lineRenderer.sprite = DataManager.Instance.uiLib.researchLineInfos[1].sprite;
            lineRenderer.material = UIManager.Instance.shopUI.maskedUIHelders[1].dummy.material;
        }
        else if (ResearchManager.Instance.CanResearced(targetResearch.idx))
        {
            // 연구가능
            lineRenderer.LineThickness = DataManager.Instance.uiLib.researchLineInfos[0].thickness;
            lineRenderer.sprite = DataManager.Instance.uiLib.researchLineInfos[0].sprite;
            lineRenderer.material = UIManager.Instance.shopUI.maskedUIHelders[0].dummy.material;
        }
        else
        {
            // 연구 불가
            lineRenderer.LineThickness = DataManager.Instance.uiLib.researchLineInfos[2].thickness;
            lineRenderer.sprite = DataManager.Instance.uiLib.researchLineInfos[2].sprite;
            lineRenderer.material = UIManager.Instance.shopUI.maskedUIHelders[2].dummy.material;
        }
    }
}
