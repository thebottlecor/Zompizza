using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultipleSpriteButton : Button
{
    public List<Graphic> others = new List<Graphic>();

    public bool playHighlightSound;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        var targetColor =
            state == SelectionState.Disabled ? colors.disabledColor :
            state == SelectionState.Highlighted ? colors.highlightedColor :
            state == SelectionState.Normal ? colors.normalColor :
            state == SelectionState.Pressed ? colors.pressedColor :
            state == SelectionState.Selected ? colors.selectedColor : Color.white;

        if (targetGraphic != null)
            targetGraphic.CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);
        for (int i = 0; i < others.Count; i++)
        {
            others[i].CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);
        }

        if (playHighlightSound)
        {
            switch (state)
            {
                case SelectionState.Highlighted:
                    SettingManager.Instance.ButtonHighlightSound();
                    break;
            }
        }
    }
}
