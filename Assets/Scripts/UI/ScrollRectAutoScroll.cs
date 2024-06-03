using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


// https://gist.github.com/mandarinx/eae10c9e8d1a5534b7b19b74aeb2a665
// Unity3d ScrollRect Auto-Scroll, Dropdown Use: Places this component in the Template of Dropdown (with the ScrollRect component)


[RequireComponent(typeof(ScrollRect))]
public class ScrollRectAutoScroll : MonoBehaviour //, IPointerEnterHandler, IPointerExitHandler
{
    //public float scrollSpeed = 10f;
    //private bool mouseOver = false;

    private List<Selectable> m_Selectables = new List<Selectable>();
    [SerializeField] private ScrollRect m_ScrollRect;
    [SerializeField] private TMP_Dropdown dropdown;

    //private Vector2 m_NextScrollPosition = Vector2.up;

    //void OnEnable()
    //{
    //    if (m_ScrollRect)
    //    {
    //        m_ScrollRect.content.GetComponentsInChildren(m_Selectables);
    //    }
    //}
    //void Awake()
    //{
    //    m_ScrollRect = GetComponent<ScrollRect>();
    //}
    //void Start()
    //{
    //    if (m_ScrollRect)
    //    {
    //        m_ScrollRect.content.GetComponentsInChildren(m_Selectables);
    //    }
    //    ScrollToSelected(true);
    //}

    private void Start()
    {
        if (m_ScrollRect)
        {
            m_ScrollRect.content.GetComponentsInChildren(m_Selectables);
        }

        m_ScrollRect.normalizedPosition = new Vector2(0, 1 - (dropdown.value / (float)(dropdown.options.Count - 1)));
    }
    public void Move(int idx)
    {
        m_Selectables[idx].Select();
        m_ScrollRect.normalizedPosition = new Vector2(0, 1 - (idx / (float)(dropdown.options.Count - 1)));
    }

    //void Update()
    //{
    //   //Scroll via input.
    //   InputScroll();
    //    if (!mouseOver)
    //    {
    //        // Lerp scrolling code.
    //        m_ScrollRect.normalizedPosition = Vector2.Lerp(m_ScrollRect.normalizedPosition, m_NextScrollPosition, scrollSpeed * Time.deltaTime);
    //    }
    //    else
    //    {
    //        m_NextScrollPosition = m_ScrollRect.normalizedPosition;
    //    }
    //}
    //void InputScroll()
    //{
    //    if (m_Selectables.Count > 0)
    //    {
    //        if (Input.GetButtonDown("Horizontal") || Input.GetButtonDown("Vertical") || Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
    //        {
    //            ScrollToSelected(false);
    //        }
    //    }
    //}
    //void ScrollToSelected(bool quickScroll)
    //{
    //    int selectedIndex = -1;
    //    Selectable selectedElement = EventSystem.current.currentSelectedGameObject ? EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>() : null;

    //    if (selectedElement)
    //    {
    //        selectedIndex = m_Selectables.IndexOf(selectedElement);
    //    }
    //    if (selectedIndex > -1)
    //    {
    //        if (quickScroll)
    //        {
    //            m_ScrollRect.normalizedPosition = new Vector2(0, 1 - (selectedIndex / ((float)m_Selectables.Count - 1)));
    //            m_NextScrollPosition = m_ScrollRect.normalizedPosition;
    //        }
    //        else
    //        {
    //            m_NextScrollPosition = new Vector2(0, 1 - (selectedIndex / ((float)m_Selectables.Count - 1)));
    //        }
    //    }
    //}
    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    mouseOver = true;
    //}
    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    mouseOver = false;
    //    ScrollToSelected(false);
    //}
}
