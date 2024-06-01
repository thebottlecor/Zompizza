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
    public static EventHandler<InputAction.CallbackContext> EnterStoreEvent;

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

    public void OnEnterStore(InputAction.CallbackContext context)
    {
        EnterStoreEvent?.Invoke(null, context);
    }

}
