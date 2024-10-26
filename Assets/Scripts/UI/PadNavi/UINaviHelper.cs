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

    [Header("타이틀 - 세이브 저장값")]
    public UINavi[] title_saveSlots;
    public UINavi[] title_saveFiles;
    public UINavi title_save_delete_sure_first;
    public UINavi title_save_delete;
    public UINavi title_save_tutorial;
    public UINavi title_save_close;

    [Header("인게임 - 옵션 저장값")]
    [HideInInspector] public UINaviHelper_Ingame ingame;

    [Header("항상 보여지는 인디케이터")]
    public PadKeyIndicator[] alwaysShow_PadUIs;
    public PadKeyIndicator[] keyGuide_PadUIs;
    public TextMeshProUGUI[] virtualCursorTMP;

    protected override void AddListeners()
    {
        InputHelper.UIMoveEvent += OnUIMove;
        InputHelper.OkayEvent += OnOkay;
        InputHelper.BackEvent += OnBack;
        InputHelper.TabMoveEvent += OnTabMove;

        InputHelper.WorldmapMoveEvent += OnScrollMove;
        InputHelper.WorldmapZoomEvent += OnSliderMove;
    }

    protected override void RemoveListeners()
    {
        InputHelper.UIMoveEvent -= OnUIMove;
        InputHelper.OkayEvent -= OnOkay;
        InputHelper.BackEvent -= OnBack;
        InputHelper.TabMoveEvent -= OnTabMove;

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

        if (LoadingSceneManager.Instance == null || LoadingSceneManager.Instance.IsSceneLoading || LoadingSceneManager.Instance.logueLoading)
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
            else if (SaveManager.Instance != null && SaveManager.Instance.IsActive)
            {
                var save = SaveManager.Instance;
                if (save.saveUIs[0].activeSelf)
                {
                    current = title_saveSlots[0];
                    title_save_close.ResetConnection();
                    title_save_tutorial.ResetConnection();
                    title_save_delete.ResetConnection();
                    title_save_close.left = title_save_tutorial;
                    title_save_close.right = title_save_tutorial;
                    title_save_tutorial.right = title_save_close;
                    title_save_tutorial.left = title_save_close;
                    title_save_close.up = title_saveSlots[0];
                    title_save_tutorial.up = title_saveSlots[0];
                }
                else if (save.saveUIs[1].activeSelf)
                {
                    if (save.saveDeleteWarningObj.activeSelf)
                    {
                        current = title_save_delete_sure_first;
                    }
                    else
                    {
                        current = title_saveFiles[0];
                    }
                    title_save_close.ResetConnection();
                    title_save_delete.ResetConnection();
                    title_save_tutorial.ResetConnection();
                    title_save_close.left = title_save_delete;
                    title_save_close.right = title_save_delete;
                    title_save_delete.right = title_save_close;
                    title_save_delete.left = title_save_close;
                    title_save_delete.down = title_saveFiles[0];
                    title_save_close.down = title_saveFiles[0];
                    UINavi lastOne = title_saveFiles[0];
                    for (int i = title_saveFiles.Length - 1; i >= 0; i--)
                    {
                        if (title_saveFiles[i].gameObject.activeSelf)
                        {
                            lastOne = title_saveFiles[i];
                            break;
                        }
                    }
                    title_save_delete.up = lastOne;
                    title_save_close.up = lastOne;
                }
                uiMoveCheckFunc = Check_Title_Save;
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
            var villager = UIManager.Instance.villagerUI;
            var exploration = ExplorationManager.Instance;

            if (UIManager.Instance != null)
            {
                if (TutorialManager.Instance.blackScreen.activeSelf)
                {

                }
                else if (util.IsActive)
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
                                else if (RocketManager.Instance.panel.activeSelf)
                                {
                                    if (ingame.rocket_first[0].gameObject.activeSelf)
                                    {
                                        current = ingame.rocket_first[0];
                                        ingame.rocket_first[0].ResetConnection();
                                        if (ingame.rocket_first[1].gameObject.activeSelf)
                                        {
                                            ingame.rocket_first[1].ResetConnection();
                                            ingame.rocket_first[0].left = ingame.rocket_first[1];
                                            ingame.rocket_first[0].right = ingame.rocket_first[1];
                                            ingame.rocket_first[1].left = ingame.rocket_first[0];
                                            ingame.rocket_first[1].right = ingame.rocket_first[0];
                                        }
                                        else if (ingame.rocket_first[2].transform.parent.gameObject.activeSelf)
                                        {
                                            ingame.rocket_first[2].ResetConnection();
                                            ingame.rocket_first[0].left = ingame.rocket_first[2];
                                            ingame.rocket_first[0].right = ingame.rocket_first[2];
                                            ingame.rocket_first[2].left = ingame.rocket_first[0];
                                            ingame.rocket_first[2].right = ingame.rocket_first[0];
                                        }
                                    }
                                    else if (ingame.rocket_first[1].gameObject.activeSelf)
                                    {
                                        ingame.rocket_first[1].ResetConnection();
                                        current = ingame.rocket_first[1];
                                    }
                                    else if (ingame.rocket_first[2].transform.parent.gameObject.activeSelf)
                                    {
                                        ingame.rocket_first[2].ResetConnection();
                                        current = ingame.rocket_first[2];
                                    }
                                    else
                                    {
                                        // null
                                    }
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
                            {
                                current = ingame.shopCloseWarning_first;
                            }
                            else if (shop.sosWarningObj.activeSelf)
                            {
                                current = ingame.sosWarning_first;
                            }
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
                else if (villager.IsActive)
                {
                    if (villager.expelWarningObj.activeSelf)
                    {
                        current = ingame.expel_first;
                    }
                    else
                    {
                        current = ingame.Villager_Reconnection();
                    }
                    uiMoveCheckFunc = Check_Ingame_Villager;
                }
                else
                {
                    if (DialogueManager.Instance.eventPanel.activeSelf)
                    {
                        current = ingame.Dialogue_Reconnection();
                        uiMoveCheckFunc = Check_Dialouge;
                    }
                    else
                    {
                        if (gm.darkCanvas.blocksRaycasts)
                        {
                            if (gm.congratulationsObj.activeSelf)
                            {
                                current = ingame.gameWin_first;
                                uiMoveCheckFunc = Check_Dialouge;
                            }
                        }
                    }
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
    private bool Check_Title_Save()
    {
        if (inputHelper.disconnectedPanel.activeSelf) return false;
        var set = SaveManager.Instance;
        return (GM.Instance == null) && (set != null && set.opened && !set.loading);
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
    private bool Check_Ingame_Villager()
    {
        if (inputHelper.disconnectedPanel.activeSelf) return false;
        var set = UIManager.Instance;
        return (ingame != null) && (set != null && set.villagerUI.opened && !set.villagerUI.loading);
    }
    private bool Check_Dialouge()
    {
        if (inputHelper.disconnectedPanel.activeSelf) return false;
        return true;
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

    private void OnTabMove(object sender, InputAction.CallbackContext e)
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
                    float value = e.ReadValue<float>();
                    if (value > 0)
                    {
                        OrderManager.Instance.FastTravelAction();
                    }
                }
            }
        }
    }

    private void OnOkay(object sender, InputAction.CallbackContext e)
    {
        if (inputHelper.disconnectedPanel.activeSelf)
        {
            inputHelper.PadConnected();
            return;
        }

        if (!e.performed) return;
        //if (ingame != null)
        //{
        //    if (current == null)
        //    {
        //        if (OrderManager.Instance.fastTravleBtn.gameObject.activeInHierarchy)
        //        {
        //            OrderManager.Instance.FastTravelAction();
        //        }
        //        else
        //            return;
        //    }
        //}
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

        if (current == null) return;

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

        var set = SettingManager.Instance;
        if (set != null && set.IsActive)
        {
            set.HideSettings();
            set.ButtonSound();
        }
        var save = SaveManager.Instance;
        if (save != null && save.IsActive)
        {
            if (save.saveDeleteWarningObj.activeSelf)
                save.ShowSaveDeleteWarning(false);
            else
                save.HideSaveSlots();
            if (set != null) set.ButtonSound();
        }
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

        sliderDir = 0f;

        if (!uiMoveCheckFunc()) return;
        if (current == null || !e.performed) return;

        Vector2 input = e.ReadValue<Vector2>();

        if (input != null && e.performed)
        {
            //scrollMoveDir = new Vector3(input.x, 0f, input.y).normalized;
            scrollMoveDir = new Vector3(0f, 0f, input.y).normalized;
            virtualCursorDir = new Vector3(input.x, input.y).normalized;

            if (input.x > 0)
                sliderDir = 1f;
            else if (input.x < 0)
                sliderDir = -1f;
        }
    }

    Vector3 scrollMoveDir;
    Vector2 virtualCursorDir;
    float sliderDir;
    float sliderGauge;

    public void UpdateTexts()
    {
        virtualCursorTMP[0].text = TextManager.Instance.GetCommons("VirtualCursor0");
        virtualCursorTMP[1].text = TextManager.Instance.GetCommons("VirtualCursor3");
        virtualCursorTMP[2].text = TextManager.Instance.GetCommons("VirtualCursor4");
        virtualCursorTMP[3].text = TextManager.Instance.GetCommons("VirtualCursor6");
    }

    private void Update()
    {
        if (current != null)
        {
            if (current.self is Scrollbar)
            {
                Scrollbar scrollbar = current.self as Scrollbar;

                scrollbar.value += 2f * Time.unscaledDeltaTime * scrollMoveDir.z;
            }
            else if (current.self is Slider)
            {
                Slider slider = current.self as Slider;

                if (sliderDir == 0f) sliderGauge = 0f;
                else sliderGauge += Time.unscaledDeltaTime;

                if (sliderGauge > 0.075f)
                {
                    sliderGauge = 0f;
                    slider.value += sliderDir;
                }
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
        //if (!uiMoveCheckFunc()) return;
        //if (current == null || !e.performed) return;

        //float value = e.ReadValue<float>();

        //if (current.self is Slider)
        //{
        //    if (value > 0)
        //        (current.self as Slider).value -= 1;
        //    else if (value < 0)
        //        (current.self as Slider).value += 1;
        //}
    }

    public void Toggle_AlwaysShow_PadUIs(bool on)
    {
        if (on)
        {
            for (int i = 0; i < alwaysShow_PadUIs.Length; i++)
            {
                alwaysShow_PadUIs[i].UIUpdate(PadType);
                alwaysShow_PadUIs[i].gameObject.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < alwaysShow_PadUIs.Length; i++)
            {
                alwaysShow_PadUIs[i].gameObject.SetActive(false);
            }
        }
    }

    public void Toggle_Guide_PadUIs(bool on)
    {
        if (on)
        {
            for (int i = 0; i < keyGuide_PadUIs.Length; i++)
            {
                keyGuide_PadUIs[i].UIUpdate(PadType);
                keyGuide_PadUIs[i].gameObject.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < keyGuide_PadUIs.Length; i++)
            {
                keyGuide_PadUIs[i].gameObject.SetActive(false);
            }
        }
    }
}
