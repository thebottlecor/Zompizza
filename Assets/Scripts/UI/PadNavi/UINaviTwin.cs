using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class UINaviTwin : UINavi
{

    public PadKeyCode padKey2;
    public Image padUI2;

    public IngredientUI ing;

    public override void Highlight(int padType, DataManager data)
    {
        base.Highlight(padType, data);

        if (padUI2 != null)
        {
            if (padKey2 != PadKeyCode.None)
            {
                switch (padType)
                {
                    case 0:
                        padUI2.sprite = data.uiLib.padKeyUIsPS[padKey2];
                        break;
                    default:
                        padUI2.sprite = data.uiLib.padKeyUIsXbox[padKey2];
                        break;
                }
            }
            padUI2.gameObject.SetActive(true);
        }

        ing.ToggleHighlight(true);
    }
    public override void DeHighlight()
    {
        base.DeHighlight();

        if (padUI2 != null)
            padUI2.gameObject.SetActive(false);

        ing.ToggleHighlight(false);
    }
}
