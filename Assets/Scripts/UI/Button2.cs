using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Button2 : Button
{

    protected override void DoStateTransition(SelectionState state, bool instant)
    {

        base.DoStateTransition(state, instant);

        switch (state)
        {
            case SelectionState.Highlighted:
                UIManager.Instance.ButtonHighlightSound();
                break;
        }
    }
}
