using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Button_ExplorationSend : Button
{
    // 패드 사용시 - 활성 비활성 상태가 가능한 탐험 보내기 버튼이 활성된 상태에서도 비쥬얼적으론 비활성(반투명)이라서 강제로 색상 고정

    [SerializeField] private Color baseColor;
    [SerializeField] private Image img;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        if (state != SelectionState.Disabled)
        {
            img.color = baseColor;
        }

        switch (state)
        {
            case SelectionState.Highlighted:
                SettingManager.Instance.ButtonHighlightSound();
                break;
        }
    }
}
