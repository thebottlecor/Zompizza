using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HardModeTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public GameObject obj;

    public void OnPointerEnter(PointerEventData eventData)
    {
        obj.SetActive(true); 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        obj.SetActive(false);
    }
}
