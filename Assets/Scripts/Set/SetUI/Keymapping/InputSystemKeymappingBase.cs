using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputSystemKeymappingBase", menuName = "Keymapping/InputSystem")]
public class InputSystemKeymappingBase : ScriptableObject
{
    /// <summary>
    /// Reference to the action that is to be rebound.
    /// </summary>
    public InputActionReference actionReference
    {
        get => m_Action;
        set
        {
            m_Action = value;
            //UpdateActionLabel();
            //UpdateBindingDisplay();
        }
    }

    /// <summary>
    /// ID (in string form) of the binding that is to be rebound on the action.
    /// </summary>
    /// <seealso cref="InputBinding.id"/>
    public string bindingId
    {
        get => m_BindingId;
        set
        {
            m_BindingId = value;
            //UpdateBindingDisplay();
        }
    }

    [Tooltip("Reference to action that is to be rebound from the UI.")]
    [SerializeField]
    private InputActionReference m_Action;
    [SerializeField]
    private string m_BindingId;

#if UNITY_EDITOR
    protected void OnValidate()
    {
        UpdateBindingDisplay();
    }
#endif

    public void UpdateBindingDisplay()
    {
        var displayString = string.Empty;
        var deviceLayoutName = default(string);
        var controlPath = default(string);

        // Get display string from action.
        var action = m_Action?.action;
        if (action != null)
        {
            var bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == m_BindingId);
            if (bindingIndex != -1)
                displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath);
        }

        //// Set on label (if any).
        //if (m_BindingText != null)
        //    m_BindingText.text = displayString;

        //// Give listeners a chance to configure UI in response.
        //m_UpdateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
    }

    public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
    {
        bindingIndex = -1;

        action = m_Action?.action;
        if (action == null)
            return false;

        if (string.IsNullOrEmpty(m_BindingId))
            return false;

        // Look up binding index.
        var bindingId = new Guid(m_BindingId);
        bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
        if (bindingIndex == -1)
        {
            Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
            return false;
        }

        return true;
    }
}
