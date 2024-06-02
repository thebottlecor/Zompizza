using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

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

}
