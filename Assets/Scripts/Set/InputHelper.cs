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
        if (SteamWishlist.SteamOverlayActivated) return;

        MoveEvent?.Invoke(null, context);
    }

    public void OnSideBreak(InputAction.CallbackContext context)
    {
        if (SteamWishlist.SteamOverlayActivated) return;

        SideBreakEvent?.Invoke(null, context);
    }

    public void OnEscape(InputAction.CallbackContext context)
    {
        if (SteamWishlist.SteamOverlayActivated) return;

        EscapeEvent?.Invoke(null, context);
    }

    public void OnWorldmap(InputAction.CallbackContext context)
    {
        if (SteamWishlist.SteamOverlayActivated) return;

        WorldmapEvent?.Invoke(null, context);
    }

    public void OnWorldmapZoom(InputAction.CallbackContext context)
    {
        if (SteamWishlist.SteamOverlayActivated) return;

        WorldmapZoomEvent?.Invoke(null, context);
    }

    public void OnWorldmapMove(InputAction.CallbackContext context)
    {
        if (SteamWishlist.SteamOverlayActivated) return;

        WorldmapMoveEvent?.Invoke(null, context);
    }

    public void OnEnterStore(InputAction.CallbackContext context)
    {
        if (SteamWishlist.SteamOverlayActivated) return;

        EnterStoreEvent?.Invoke(null, context);
    }

    public void OnTabMove(InputAction.CallbackContext context)
    {
        if (SteamWishlist.SteamOverlayActivated) return;

        TabMoveEvent?.Invoke(null, context);
    }

    public void OnUIMove(InputAction.CallbackContext context)
    {
        if (SteamWishlist.SteamOverlayActivated) return;

        UIMoveEvent?.Invoke(null, context);
    }
    public void OnOkay(InputAction.CallbackContext context)
    {
        if (SteamWishlist.SteamOverlayActivated) return;

        OkayEvent?.Invoke(null, context);
    }
    public void OnBack(InputAction.CallbackContext context)
    {
        if (SteamWishlist.SteamOverlayActivated) return;

        BackEvent?.Invoke(null, context);
    }

    public void PadDisconnected()
    {
        var um = UIManager.Instance;
        if (um != null)
        {
            if (um.Panels_Inactive)
            {
                um.utilUI.OpenSettings();
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
        uiNaviHelper.Toggle_AlwaysShow_PadUIs(false);
        if (uiNaviHelper.ingame != null) uiNaviHelper.ingame.Toggle_AlwaysShow_PadUIs(false, uiNaviHelper.PadType);
        if (TutorialManager.Instance != null) TutorialManager.Instance.PadCheck();
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
        if (TutorialManager.Instance != null) TutorialManager.Instance.PadCheck();
        if (pad != null)
        {
            if (pad is UnityEngine.InputSystem.DualShock.DualShockGamepad)
            {
                uiNaviHelper.UIUpdate(0);
            }
            else
            {
                // 나머지는 Xbox UI로 통일
                uiNaviHelper.UIUpdate(1);
            }

            uiNaviHelper.Toggle_AlwaysShow_PadUIs(true);
            if (uiNaviHelper.ingame != null) uiNaviHelper.ingame.Toggle_AlwaysShow_PadUIs(true, uiNaviHelper.PadType);

            uiNaviHelper.SetFirstSelect();
            return;
        }

        // 패드 없음 - 비활성화
        uiNaviHelper.PadDisconnected();
        uiNaviHelper.Toggle_AlwaysShow_PadUIs(false);
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
    D_Pad,
}
