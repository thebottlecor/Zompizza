using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class UINaviOther : UINavi
{

    public Image padUI2;

    public override void Highlight(int padType, DataManager data)
    {
        base.Highlight(padType, data);

        if (padUI2 != null)
            padUI2.gameObject.SetActive(true);
    }

    public override void DeHighlight()
    {
        base.DeHighlight();

        if (padUI2 != null)
            padUI2.gameObject.SetActive(false);
    }
}
