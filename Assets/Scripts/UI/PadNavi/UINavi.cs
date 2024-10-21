using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class UINavi : MonoBehaviour
{
    public Selectable self;

    public UINavi up;
    public UINavi down;
    public UINavi left;
    public UINavi right;

    public PadKeyCode padKey;
    public Image padUI;
    public Animator anim;

    public RectTransform focusRect;

    public virtual void Highlight(int padType, DataManager data)
    {
        if (anim != null && this.gameObject.activeInHierarchy)
        {
            anim.Play("Selected");
            if (SettingManager.Instance != null)
                SettingManager.Instance.ButtonHighlightSound2();
        }
        if (padUI != null)
        {
            if (padKey != PadKeyCode.None)
            {
                switch (padType)
                {
                    case 0:
                        padUI.sprite = data.uiLib.padKeyUIsPS[padKey];
                        break;
                    default:
                        padUI.sprite = data.uiLib.padKeyUIsXbox[padKey];
                        break;
                }
            }
            padUI.gameObject.SetActive(true);
        }
    }
    public virtual void DeHighlight()
    {
        if (anim != null && this.gameObject.activeInHierarchy)
            anim.Play("Normal");
        if (padUI != null)
            padUI.gameObject.SetActive(false);
        if (self is TMP_Dropdown)
        {
            if (self.transform.childCount == 4)
                (self as TMP_Dropdown).Hide();
        }
    }

    public void ResetConnection()
    {
        up = null;
        down = null;
        left = null;
        right = null;
    }
}
