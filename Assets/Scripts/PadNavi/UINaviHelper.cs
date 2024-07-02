using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class UINaviHelper : Singleton<UINaviHelper>
{

    public UINavi current;
    public int PadType { get; private set; }

    public InputHelper inputHelper;
    public DataManager dataManager;

    [Header("UI 변환시 최우선 선택")]
    public UINavi title_first;
    public UINavi title_option_first;
    [HideInInspector] public UINavi title_language_first;
    public Func<bool> uiMoveCheckFunc;

    [Header("타이틀 - 옵션 저장값")]
    public UINavi[] title_option_left_buttons;
    public UINavi[] title_options_left; // 좌측 옵션들 (소리)
    public UINavi[] title_options_right; // 우측 옵션들
    public UINavi title_settings_close;
    private int currentDropdownItem;

    [Header("인게임 - 옵션 저장값")]
    [HideInInspector] public UINaviHelper_Ingame ingame;

    protected override void AddListeners()
    {
        InputHelper.UIMoveEvent += OnUIMove;
        InputHelper.OkayEvent += OnOkay;
        InputHelper.BackEvent += OnBack;

        InputHelper.WorldmapMoveEvent += OnScrollMove;
        InputHelper.WorldmapZoomEvent += OnSliderMove;
    }

    protected override void RemoveListeners()
    {
        InputHelper.UIMoveEvent -= OnUIMove;
        InputHelper.OkayEvent -= OnOkay;
        InputHelper.BackEvent -= OnBack;

        InputHelper.WorldmapMoveEvent -= OnScrollMove;
        InputHelper.WorldmapZoomEvent -= OnSliderMove;
    }

    public void SetFirstSelect()
    {
        // 현재 활성화된 UI에 따라서 current를 능동적으로 골라야 함
        /// <see cref = "UIManager.OnESC"/>
        /// <see cref="ShopUI.OnTabMove"/>

        PadDisconnected();

        var pad = Gamepad.current;
        if (pad == null) return;

        if (LoadingSceneManager.Instance == null || LoadingSceneManager.Instance.IsSceneLoading)
        {
            return;
        }

        var gm = GM.Instance;

        if (gm == null)
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
                    case 2:
                        current = title_settings_close;
                        title_settings_close.ResetConnection();
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
        else if (ingame != null)
        {
            ingame.virtualCursor.gameObject.SetActive(false);
            var util = UIManager.Instance.utilUI;
            var shop = UIManager.Instance.shopUI;
            var exploration = ExplorationManager.Instance;

            if (UIManager.Instance != null)
            {
                if (util.IsActive)
                {
                    switch (util.activeSubPanel)
                    {
                        case 0:
                            current = ingame.Utils_Map_Reconnection();
                            break;
                        case 1:
                            current = title_option_first;
                            break;
                    }
                    uiMoveCheckFunc = Check_Ingame_Utils;
                }
                else if (shop.IsActive)
                {
                    switch (shop.activeSubPanel)
                    {
                        case 0:
                            if (gm.darkCanvas.blocksRaycasts)
                            {
                                if (gm.gameOverObj.activeSelf)
                                {
                                    current = ingame.gameOver_first;
                                }
                                else if (gm.congratulationsObj.activeSelf)
                                {
                                    current = ingame.gameWin_first;
                                }
                                else if (gm.accountObj.activeSelf)
                                {
                                    current = ingame.nextDay_first;
                                }
                                else
                                {
                                    // null
                                }
                            }
                            else if (exploration.canvasGroupLoading)
                            {
                                // null
                            }
                            else if (exploration.canvasGroup_resultPanel.alpha >= 0.99f)
                            {
                                current = ingame.explore_first;
                            }
                            else if (gm.raidObj.activeSelf)
                            {
                                current = ingame.raid_first;
                            }
                            else if (RivalManager.Instance.rankingObj.activeSelf)
                            {
                                current = ingame.ranking_first;
                            }
                            else if (GameEventManager.Instance.eventPanel.activeSelf)
                            {
                                current = ingame.GameEvent_Reconnection();
                            }
                            else if (shop.shopCloseWarningObj.activeSelf)
                                current = ingame.shopCloseWarning_first;
                            else
                                current = ingame.Shop_Orders_Reconnection();
                            break;
                        case 1:
                            if (gm.gameOverWarningObj.activeSelf)
                                current = ingame.gameOverWarning_first;
                            else if (gm.raidObj.activeSelf)
                            {
                                current = ingame.raid_first;
                            }
                            else if (RivalManager.Instance.rankingObj.activeSelf)
                            {
                                current = ingame.ranking_first;
                            }
                            else if (GameEventManager.Instance.eventPanel.activeSelf)
                            {
                                current = ingame.GameEvent_Reconnection();
                            }
                            else
                            {
                                current = ingame.Shop_Management_Reconnection();
                            }
                            break;
                        case 2:
                            current = ingame.Shop_Upgrade_Reconnection();
                            break;
                        case 3:
                            current = ingame.Shop_Vehicle_Reconnection();
                            break;
                    }
                    uiMoveCheckFunc = Check_Ingame_Shops;
                }
                else
                {

                }
            }
        }

        if (current != null)
            current.Highlight(PadType, dataManager);
    }
    public void SetClose_SubPanels(int idx)
    {
        var close = title_settings_close;

        if (UIManager.Instance != null && UIManager.Instance.utilUI.IsActive)
        {
            close = ingame.utils_close;
        }

        close.ResetConnection();

        switch (idx)
        {
            case 0: // 설정
                for (int i = 0; i < title_option_left_buttons.Length; i++)
                {
                    title_option_left_buttons[i].left = close;
                    title_option_left_buttons[i].right = title_options_left[0]; // 좌측최상단 옵션에 연결
                }
                close.left = title_options_right[0]; // 우측최상단 옵션에 연결
                close.right = title_option_first;

                for (int i = 0; i < title_options_left.Length; i++)
                {
                    title_options_left[i].left = title_option_first;
                }
                for (int i = 0; i < title_options_right.Length; i++)
                {
                    title_options_right[i].right = close;
                }
                break;
            case 1: // 키
                for (int i = 0; i < title_option_left_buttons.Length; i++)
                {
                    title_option_left_buttons[i].left = close;
                    title_option_left_buttons[i].right = close;
                }
                close.left = title_option_first;
                close.right = title_option_first;
                break;
        }
    }
    private bool Check_Title()
    {
        if (inputHelper.disconnectedPanel.activeSelf) return false;
        var set = SettingManager.Instance;
        return (GM.Instance == null) && (set != null && !set.opened && !set.loading);
    }
    private bool Check_Title_Setting()
    {
        if (inputHelper.disconnectedPanel.activeSelf) return false;
        var set = SettingManager.Instance;
        return (GM.Instance == null) && (set != null && set.opened && !set.loading);
    }
    private bool Check_Ingame_Utils()
    {
        if (inputHelper.disconnectedPanel.activeSelf) return false;
        var set = UIManager.Instance;
        return (ingame != null) && (set != null && set.utilUI.opened && !set.utilUI.loading);
    }
    private bool Check_Ingame_Shops()
    {
        if (inputHelper.disconnectedPanel.activeSelf) return false;
        var set = UIManager.Instance;
        return (ingame != null) && (set != null && set.shopUI.opened && !set.shopUI.loading);
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
        if (inputHelper.disconnectedPanel.activeSelf)
        {
            inputHelper.PadConnected();
            return;
        }

        if (!e.performed) return;
        if (ingame != null)
        {
            if (current == null)
            {
                if (OrderManager.Instance.fastTravleBtn.gameObject.activeInHierarchy)
                {
                    OrderManager.Instance.FastTravelAction();
                }
                else
                    return;
            }
        }
        if (!uiMoveCheckFunc()) return;

        if (ingame != null && ingame.virtualCursor.gameObject.activeSelf)
        {
            var uis = UIManager.Instance.shopUI.researchUIs;
            ResearchUI selected = null;
            foreach (var temp in uis)
            {
                if (!temp.Value.gameObject.activeSelf) continue;

                float dist = (ingame.virtualCursor.transform.position - temp.Value.icon.rectTransform.position).magnitude;

                if (dist <= 0.375f)
                {
                    selected = temp.Value;
                    break;
                }
            }
            if (selected != null && selected.idx != UIManager.Instance.shopUI.currentSelectUpgrade)
            {
                UIManager.Instance.shopUI.SelectUpgrade(selected.idx);
                AudioManager.Instance.PlaySFX(Sfx.buttons);
                return;
            }
        }

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
        if (inputHelper.disconnectedPanel.activeSelf)
        {
            inputHelper.PadConnected();
            return;
        }

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

        if (UIManager.Instance != null)
            UIManager.Instance.OnESC(null, e);

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
                if (navi == current)
                    break;
                if (navi == null)
                {
                    Debug.LogWarning(current + " >> " + dir + " 패드 UI 이동 오류");
                    SetFirstSelect();
                    return;
                }
            }

            current = navi;
            current.Highlight(PadType, dataManager);

            if (current != null && current.focusRect != null)
            {
                UIManager.Instance.shopUI.SnapTo(current.focusRect);
            }
        }
    }

    private void OnScrollMove(object sender, InputAction.CallbackContext e)
    {
        scrollMoveDir = Vector3.zero;
        virtualCursorDir = Vector2.zero;

        if (!uiMoveCheckFunc()) return;
        if (current == null || !e.performed) return;

        Vector2 input = e.ReadValue<Vector2>();

        if (input != null && e.performed)
        {
            //scrollMoveDir = new Vector3(input.x, 0f, input.y).normalized;
            scrollMoveDir = new Vector3(0f, 0f, input.y).normalized;
            virtualCursorDir = new Vector3(input.x, input.y).normalized;
        }
    }

    Vector3 scrollMoveDir;
    Vector2 virtualCursorDir;

    private void Update()
    {
        if (current != null)
        {
            if (current.self is Scrollbar)
            {
                Scrollbar scrollbar = current.self as Scrollbar;

                scrollbar.value += 2f * Time.unscaledDeltaTime * scrollMoveDir.z;
            }
        }
        if (ingame != null)
        {
            if (ingame.virtualCursor.gameObject.activeSelf)
            {
                ingame.virtualCursor.anchoredPosition += 200f * Time.unscaledDeltaTime * virtualCursorDir;
            }
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
