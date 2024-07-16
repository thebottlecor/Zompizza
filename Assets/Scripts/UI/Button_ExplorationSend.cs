using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Button_ExplorationSend : Button
{
    // �е� ���� - Ȱ�� ��Ȱ�� ���°� ������ Ž�� ������ ��ư�� Ȱ���� ���¿����� ����������� ��Ȱ��(������)�̶� ������ ���� ����

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
