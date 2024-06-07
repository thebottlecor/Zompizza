using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using TMPro;

public class InputHelper : MonoBehaviour
{

    public static EventHandler<InputAction.CallbackContext> MoveEvent;
    public static EventHandler<InputAction.CallbackContext> SideBreakEvent;
    public static EventHandler<InputAction.CallbackContext> EscapeEvent;

    public static EventHandler<InputAction.CallbackContext> WorldmapEvent;
    public static EventHandler<InputAction.CallbackContext> WorldmapZoomEvent;
    public static EventHandler<InputAction.CallbackContext> WorldmapMoveEvent;

    public static EventHandler<InputAction.CallbackContext> EnterStoreEvent;

    public static EventHandler<InputAction.CallbackContext> TabMoveEvent;

    public static EventHandler<InputAction.CallbackContext> UIMoveEvent;
    public static EventHandler<InputAction.CallbackContext> OkayEvent;
    public static EventHandler<InputAction.CallbackContext> BackEvent;

    public GameObject disconnectedPanel;
    public TextMeshProUGUI panelTMP;
    public TextMeshProUGUI panelBtnTMP;
    public UINaviHelper uiNaviHelper;

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(null, context);
    }

    public void OnSideBreak(InputAction.CallbackContext context)
    {
        SideBreakEvent?.Invoke(null, context);
    }

    public void OnEscape(InputAction.CallbackContext context)
    {
        EscapeEvent?.Invoke(null, context);
    }

    public void OnWorldmap(InputAction.CallbackContext context)
    {
        WorldmapEvent?.Invoke(null, context);
    }

    public void OnWorldmapZoom(InputAction.CallbackContext context)
    {
        WorldmapZoomEvent?.Invoke(null, context);
    }

    public void OnWorldmapMove(InputAction.CallbackContext context)
    {
        WorldmapMoveEvent?.Invoke(null, context);
    }

    public void OnEnterStore(InputAction.CallbackContext context)
    {
        EnterStoreEvent?.Invoke(null, context);
    }

    public void OnTabMove(InputAction.CallbackContext context)
    {
        TabMoveEvent?.Invoke(null, context);
    }

    public void OnUIMove(InputAction.CallbackContext context)
    {
        UIMoveEvent?.Invoke(null, context);
    }
    public void OnOkay(InputAction.CallbackContext context)
    {
        OkayEvent?.Invoke(null, context);
    }
    public void OnBack(InputAction.CallbackContext context)
    {
        BackEvent?.Invoke(null, context);
    }

    public void PadDisconnected()
    {
        var uimanager = UIManager.Instance;
        if (uimanager != null)
        {
            if (!uimanager.shopUI.IsActive && !uimanager.utilUI.IsActive)
            {
                uimanager.utilUI.OpenSettings();
            }
        }
        var tm = TextManager.Instance;
        if (tm != null)
        {
            panelTMP.text = tm.GetCommons("ControllerDisconnected");
            panelBtnTMP.text = tm.GetCommons("ControllerDisconnected2");
        }
        disconnectedPanel.SetActive(true);

        // 패드 없음 - 비활성화
        uiNaviHelper.PadDisconnected();
        if (uiNaviHelper.ingame != null) uiNaviHelper.ingame.Toggle_AlwaysShow_PadUIs(false, uiNaviHelper.PadType);
    }
    public void PadConnected()
    {
        disconnectedPanel.SetActive(false);
        PadCheck();
    }
    public void InputChanged()
    {
        PadCheck();
    }
    public void PadCheck()
    {
        var pad = Gamepad.current;
        if (pad != null)
        {
            if (uiNaviHelper.ingame != null) uiNaviHelper.ingame.Toggle_AlwaysShow_PadUIs(true, uiNaviHelper.PadType);
            if (pad is UnityEngine.InputSystem.DualShock.DualShockGamepad)
            {
                uiNaviHelper.UIUpdate(0);
                uiNaviHelper.SetFirstSelect();
                return;
            }
            else
            {
                // 나머지는 Xbox UI로 통일
                uiNaviHelper.UIUpdate(1);
                uiNaviHelper.SetFirstSelect();
                return;
            }
        }

        // 패드 없음 - 비활성화
        uiNaviHelper.PadDisconnected();
        if (uiNaviHelper.ingame != null) uiNaviHelper.ingame.Toggle_AlwaysShow_PadUIs(false, uiNaviHelper.PadType);
    }
}

public enum PadKeyCode
{
    None,
    L1,
    L2,
    R1,
    R2,
    Start,
    Select,
    D_Up,
    D_Down,
    D_Left,
    D_Right,
    B_Up,
    B_Down,
    B_Left,
    B_Right,
    LeftStick,
    RightStick,
}
