using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;

public class UINaviHelper : Singleton<UINaviHelper>
{

    public UINavi current;
    public int PadType { get; private set; }

    public DataManager dataManager;

    [Header("UI 변환시 최우선 선택")]
    public UINavi title_first;
    public UINavi title_option_first;
    public UINavi title_language_first;
    public Func<bool> uiMoveCheckFunc;

    [Header("타이틀 - 옵션 저장값")]
    public UINavi[] title_option_left_buttons;
    public UINavi[] title_options_left; // 좌측 옵션들 (소리)
    public UINavi[] title_options_right; // 우측 옵션들
    public UINavi title_settings_close;
    private int currentDropdownItem;

    protected override void AddListeners()
    {
        InputHelper.UIMoveEvent += OnUIMove;
        InputHelper.OkayEvent += OnOkay;
        InputHelper.BackEvent += OnBack;

        InputHelper.WorldmapZoomEvent += OnSliderMove;
    }

    protected override void RemoveListeners()
    {
        InputHelper.UIMoveEvent -= OnUIMove;
        InputHelper.OkayEvent -= OnOkay;
        InputHelper.BackEvent -= OnBack;

        InputHelper.WorldmapZoomEvent -= OnSliderMove;
    }

    public void SetFirstSelect()
    {
        // 현재 활성화된 UI에 따라서 current를 능동적으로 골라야 함

        PadDisconnected();

        if (GM.Instance == null)
        {
            if (SettingManager.Instance != null && SettingManager.Instance.IsActive)
            {
                switch (SettingManager.Instance.activeSubPanel)
                {
                    case 0:
                        current = title_option_first;
                        break;
                    case 1:
                        current = title_language_first;
                        title_settings_close.ResetConnection();
                        title_settings_close.up = title_language_first;
                        title_settings_close.down = title_language_first;
                        break;
                }
                uiMoveCheckFunc = Check_Title_Setting;
            }
            else
            {
                current = title_first;
                uiMoveCheckFunc = Check_Title;
            }
        }

        if (current != null)
            current.Highlight(PadType, dataManager);
    }
    public void SetClose_TitleSettings_SubPanels(int idx)
    {
        title_settings_close.ResetConnection();

        switch (idx)
        {
            case 0: // 설정
                for (int i = 0; i < title_option_left_buttons.Length; i++)
                {
                    title_option_left_buttons[i].left = title_settings_close;
                    title_option_left_buttons[i].right = title_options_left[0]; // 좌측최상단 옵션에 연결
                }
                title_settings_close.left = title_options_right[0]; // 우측최상단 옵션에 연결
                title_settings_close.right = title_option_first;

                for (int i = 0; i < title_options_left.Length; i++)
                {
                    title_options_left[i].left = title_option_first;
                }
                for (int i = 0; i < title_options_right.Length; i++)
                {
                    title_options_right[i].right = title_settings_close;
                }
                break;
            case 1: // 키
                for (int i = 0; i < title_option_left_buttons.Length; i++)
                {
                    title_option_left_buttons[i].left = title_settings_close;
                    title_option_left_buttons[i].right = title_settings_close;
                }
                title_settings_close.left = title_option_first;
                title_settings_close.right = title_option_first;
                break;
        }
    }
    private bool Check_Title()
    {
        var set = SettingManager.Instance;
        return (GM.Instance == null) && (set != null && !set.opened && !set.loading);
    }
    private bool Check_Title_Setting()
    {
        var set = SettingManager.Instance;
        return (GM.Instance == null) && (set != null && set.opened && !set.loading);
    }
    private bool CheckFail()
    {
        return false;
    }
    public void UIUpdate(int padType)
    {
        switch (padType)
        {
            case 0:
                this.PadType = 0;
                // Ps UI
                break;
            default:
                this.PadType = padType;
                // XBox UI
                break;
        }
    }

    public void PadDisconnected()
    {
        uiMoveCheckFunc = CheckFail;
        if (current != null)
            current.DeHighlight();
        current = null;
    }

    private void OnOkay(object sender, InputAction.CallbackContext e)
    {
        if (!uiMoveCheckFunc()) return;
        if (current == null || !e.performed) return;

        //current.self.Select();

        switch (current.self)
        {
            case Button btn:
                btn.onClick.Invoke();
                break;
            case Toggle tog:
                tog.isOn = !tog.isOn;
                break;
            case TMP_Dropdown drop:
                if (drop.transform.childCount == 3)
                {
                    drop.Show();
                    currentDropdownItem = drop.value;
                }
                else if (drop.transform.childCount == 4)
                {
                    drop.value = currentDropdownItem;
                    drop.Hide();
                }
                break;
        }
    }

    private void OnBack(object sender, InputAction.CallbackContext e)
    {
        if (!uiMoveCheckFunc()) return;
        if (current == null || !e.performed) return;

        switch (current.self)
        {
            case TMP_Dropdown drop:
                if (drop.transform.childCount == 4)
                {
                    drop.Hide();
                    return;
                }
                break;
        }

        SettingManager.Instance.HideSettings();
        if (SettingManager.Instance != null)
            SettingManager.Instance.ButtonSound();
    }

    private void OnUIMove(object sender, InputAction.CallbackContext e)
    {
        if (!uiMoveCheckFunc()) return;
        if (current == null || !e.performed) return;

        Vector2 value = e.ReadValue<Vector2>();

        if (current.self is TMP_Dropdown)
        {
            TMP_Dropdown dropdown = current.self as TMP_Dropdown;
            if (dropdown.transform.childCount == 4)
            {
                if (value.y > 0)
                    currentDropdownItem--;
                else if (value.y < 0)
                    currentDropdownItem++;
                if (currentDropdownItem < 0)
                    currentDropdownItem = 0;
                if (currentDropdownItem >= dropdown.options.Count)
                    currentDropdownItem = dropdown.options.Count - 1;
                dropdown.GetComponentInChildren<ScrollRectAutoScroll>().Move(currentDropdownItem);
                return;
            }
        }

        if (value.y > 0)
        {
            NewCurrent(current.up, 0);
        }
        else if (value.y < 0)
        {
            NewCurrent(current.down, 1);
        }
        else if (value.x < 0)
        {
            NewCurrent(current.left, 2);
        }
        else if (value.x > 0)
        {
            NewCurrent(current.right, 3);
        }
    }

    private void NewCurrent(UINavi navi, int dir)
    {
        if (navi != null)
        {
            if (current != null)
                current.DeHighlight();

            while (!navi.gameObject.activeSelf)
            {
                switch (dir)
                {
                    case 0:
                        navi = navi.up;
                        break;
                    case 1:
                        navi = navi.down;
                        break;
                    case 2:
                        navi = navi.left;
                        break;
                    case 3:
                        navi = navi.right;
                        break;
                }
                if (navi == null)
                {
                    Debug.LogWarning(current + " >> " + dir + " 패드 UI 이동 오류");
                    SetFirstSelect();
                    return;
                }
            }

            current = navi;
            current.Highlight(PadType, dataManager);
        }
    }


    private void OnSliderMove(object sender, InputAction.CallbackContext e)
    {
        if (!uiMoveCheckFunc()) return;
        if (current == null || !e.performed) return;

        float value = e.ReadValue<float>();

        if (current.self is Slider)
        {
            if (value > 0)
                (current.self as Slider).value -= 1;
            else if (value < 0)
                (current.self as Slider).value += 1;
        }
    }
}
