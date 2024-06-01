using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


#region 세이브 데이터
public enum KeyMap
{
    escape,

    carForward,
    carBackward,
    carLeft,
    carRight,
    carBreak,

    worldMap,
    worldMapZoomIn,
    worldMapZoomOut,

    enterStore,

    LAST,
}

[Serializable]
public class KeySaveData
{
    public SerializableDictionary<KeyMap, KeyCode> keyCodes = new()
    {
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.escape, Value = KeyCode.Escape },

        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.carForward, Value = KeyCode.W },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.carBackward, Value = KeyCode.S },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.carLeft, Value = KeyCode.A },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.carRight, Value = KeyCode.D },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.carBreak, Value = KeyCode.Space },

        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.worldMap, Value = KeyCode.M },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.worldMapZoomIn, Value = KeyCode.PageDown },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.worldMapZoomOut, Value = KeyCode.PageUp },

        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.enterStore, Value = KeyCode.Return },
    };
}
[Serializable]
public class KeyMapping
{
    public KeyCode key;
    public KeyMap keyMap;

    public bool Getkey()
    {
        if (SettingManager.Instance.DisableControl) return false;
        //if (SteamHelper.Instance != null && SteamHelper.Instance.SteamOverlayIsOn) return false;

        if (key != KeyCode.None)
            return Input.GetKey(key);
        else
            return false;
    }

    public bool GetkeyDown()
    {
        if (SettingManager.Instance.DisableControl) return false;
        //if (SteamHelper.Instance != null && SteamHelper.Instance.SteamOverlayIsOn) return false;

        if (key != KeyCode.None)
            return Input.GetKeyDown(key);
        else
            return false;
    }

    public bool GetkeyUp()
    {
        if (SettingManager.Instance.DisableControl) return false;
        //if (SteamHelper.Instance != null && SteamHelper.Instance.SteamOverlayIsOn) return false;

        if (key != KeyCode.None)
            return Input.GetKeyUp(key);
        else
            return false;
    }

    public string GetName()
    {
        if (TextManager.Instance.HasKeyCode(key))
        {
            return TextManager.Instance.GetKeyCodes(key);
        }
        else
            return key.ToString();
    }
}
#endregion

public class SettingManager : Singleton<SettingManager>
{

    [HideInInspector] public SerializableDictionary<KeyMap, KeyMapping> keyMappings;
    private KeySaveData keySave;
    [HideInInspector] public KeyMap currentActiveKeyNum = KeyMap.LAST;
    [SerializeField] private GameObject keyObject_Prefab;
    [SerializeField] private GameObject keyObject_Reset_Prefab;
    [SerializeField] private Transform keyObject_Parent;
    private SerializableDictionary<KeyMap, KeyObject> keyObjects;
    private Dictionary<KeyCode, byte> bannedSettingKey_ForKeyboard;
    private TextMeshProUGUI restoreDefaultsTMP;

    // Keysetting에서 prefab 생성 순서
    private readonly List<KeyMap> keyOrder = new List<KeyMap>
    {
        KeyMap.escape,

        KeyMap.carForward,
        KeyMap.carBackward,
        KeyMap.carLeft,
        KeyMap.carRight,
        KeyMap.carBreak,

        KeyMap.worldMap,
        KeyMap.worldMapZoomIn,
        KeyMap.worldMapZoomOut,

        KeyMap.enterStore,
    };
    // keyOrder 순서에 맞춰서 InputSystemKeymappingBase 데이터 넣기
    [SerializeField] private List<InputSystemKeymappingBase> inputSystemKeymappings;

    public bool DisableControl => WindowOutOfFocus || currentActiveKeyNum != KeyMap.LAST;

    public bool WindowOutOfFocus { get; private set; }

    private void OnApplicationPause(bool pause)
    {
        if (pause == true)
        {
            // 이탈
            WindowOutOfFocus = true;

        }
        else
        {
            // 복귀
            WindowOutOfFocus = false;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        CallAfterAwake();
    }

    public override void CallAfterAwake()
    {
        InitResolutionUI();
    }
    public override void CallAfterStart(ConfigData config)
    {
        InitScreenModeUI();
        InitFramerateUI();
        KeyInit(true);
        SettingInit(config);
    }

    #region 키 초기 세팅
    public void KeySave()
    {
        foreach (var item in keyMappings)
        {
            keySave.keyCodes[item.Key] = item.Value.key;
        }

        UpdateKeyMapWithInputSystem();

        string jsonData = JsonHelper.ObjectToJson(keySave);
        JsonHelper.CreateJsonFile(Application.persistentDataPath, "keys", "Config", jsonData, false);
    }
    public void KeyLoad()
    {
        StringBuilder st = new StringBuilder().AppendFormat("{0}/Config/keys.json", Application.persistentDataPath);
        keySave = JsonHelper.LoadJsonFile<KeySaveData>(st.ToString(), false);
        if (keySave == null)
        {
            keySave = new KeySaveData();

            if (Application.systemLanguage == SystemLanguage.French)
            {
                keySave.keyCodes[KeyMap.carForward] = KeyCode.Z;
                keySave.keyCodes[KeyMap.carLeft] = KeyCode.Q;
            }
            else if (Application.systemLanguage == SystemLanguage.German)
            {

            }
        }
    }
    public void KeyReset()
    {
        //SoundHelper.Instance.Play_ButtonClick_0();
        StringBuilder st = new StringBuilder().AppendFormat("{0}/Config/keys.json", Application.persistentDataPath);
        JsonHelper.DeleteJsonFile(st.ToString());
        KeyLoad();
        KeyInit(false);
        //LanguageChangeApply();
    }

    // Main 에서 초기화 통제
    public void KeyInit(bool createObj)
    {
        KeyLoad();
        KeyInit_SetOrder();

        currentActiveKeyNum = KeyMap.LAST;
        if (createObj)
        {
            CreateKeyObject();
        }
        else
        {
            KeySetting_TextUpdate();
        }

        KeySave();
    }

    private void KeySetting_TextUpdate()
    {
        if (keyObjects == null) return;
        foreach (var item in keyObjects)
        {
            item.Value.UpdateName();
        }
    }
    #endregion

    #region 키 설정
    private void CreateKeyObject()
    {
        keyObjects = new SerializableDictionary<KeyMap, KeyObject>();

        for (int i = 0; i < keyOrder.Count; i++)
        {
            KeyObject keyObject = Instantiate(keyObject_Prefab, keyObject_Parent).GetComponent<KeyObject>();
            keyObject.Init(keyOrder[i]);
            keyObject.newInput.CopyData(inputSystemKeymappings[i]);
            keyObjects.Add(new SerializableDictionary<KeyMap, KeyObject>.Pair { Key = keyOrder[i], Value = keyObject });
        }

        // 공백 만들기
        int keyCount = 4 * 6;
        keyCount -= keyOrder.Count;
        //for (int i = 1; i < keyCount; i++)
        //    Instantiate(DataManager.Instance.uiLibrary.uiLayoutDummy, keyObject_Parent);

        Button resetButton = Instantiate(keyObject_Reset_Prefab, keyObject_Parent).GetComponent<Button>();
        resetButton.onClick.AddListener(() => {
            KeyReset();
            AudioManager.Instance.PlaySFX(Sfx.buttons);
        });
        restoreDefaultsTMP = resetButton.GetComponentInChildren<TextMeshProUGUI>();
        restoreDefaultsTMP.text = tm.GetCommons("RestoreDefaults");
    }

    private void Update()
    {
        if (LoadingSceneManager.Instance.IsSceneLoading) return;

        if (currentActiveKeyNum != KeyMap.LAST)
        {
            foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (!bannedSettingKey_ForKeyboard.ContainsKey(kcode) && (int)kcode < 323) // 323부터는 Mouse와 Joystick
                {
                    if (Input.GetKeyUp(kcode))
                    {
                        foreach (var item in keyMappings)
                        {
                            if (item.Key != currentActiveKeyNum && item.Value.key == kcode)
                            {
                                item.Value.key = keyMappings[currentActiveKeyNum].key;
                                keyObjects[item.Key].UpdateName();
                                break;
                            }
                        }
                        keyMappings[currentActiveKeyNum].key = kcode;
                        keyObjects[currentActiveKeyNum].UpdateName();
                        KeySave();
                        //LanguageChangeApply();
                        //AudioManager.Instance.PlaySFX(Sfx.inputFieldEnd);
                        currentActiveKeyNum = KeyMap.LAST;
                        break;
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1))
                    {
                        keyObjects[currentActiveKeyNum].UpdateName();
                        currentActiveKeyNum = KeyMap.LAST;
                        //AudioManager.Instance.PlaySFX(Sfx.smallButtons);
                        break;
                    }
                }
            }
        }
    }

    public void UpdateKeyMapWithInputSystem()
    {
        foreach (var keyobj in keyObjects)
        {
            var obj = keyobj.Value;
            if (obj.newInput.ResolveActionAndBinding(out var action, out var bindingIndex))
            {
                InputBinding inputBinding = action.bindings[bindingIndex];
                var keyCode = keyMappings[keyobj.Key].key;
                if (tm.HasInputSystems(keyCode))
                    inputBinding.overridePath = $"<Keyboard>/{tm.GetInputSystems(keyCode)}";
                else
                    inputBinding.overridePath = $"<Keyboard>/{keyCode}";
                action.ApplyBindingOverride(bindingIndex, inputBinding);
            }
        }
    }

private void KeyInit_SetOrder()
    {
        keyMappings = new SerializableDictionary<KeyMap, KeyMapping>();

        for (int i = 0; i < (int)KeyMap.LAST; i++)
        {
            KeyMap keyMap = (KeyMap)i;
            KeyMapping keyMapping = new()
            {
                key = keySave.keyCodes[keyMap],
                keyMap = keyMap,
            };
            keyMappings.Add(new SerializableDictionary<KeyMap, KeyMapping>.Pair { Key = keyMap, Value = keyMapping });
        }

        bannedSettingKey_ForKeyboard = new Dictionary<KeyCode, byte>();
        //bannedSettingKey.Add(KeyCode.LeftShift, 0);
        //bannedSettingKey.Add(KeyCode.LeftControl, 0);
        //bannedSettingKey_ForKeyboard.Add(KeyCode.Mouse0, 0);
        //bannedSettingKey_ForKeyboard.Add(KeyCode.Mouse1, 0);
        //bannedSettingKey_ForKeyboard.Add(KeyCode.Mouse2, 0);
        //bannedSettingKey_ForKeyboard.Add(KeyCode.Mouse3, 0);
        //bannedSettingKey_ForKeyboard.Add(KeyCode.Mouse4, 0);
        //bannedSettingKey_ForKeyboard.Add(KeyCode.Mouse5, 0);
        //bannedSettingKey_ForKeyboard.Add(KeyCode.Mouse6, 0);
    }
    #endregion

    #region 설정 (통합)
    [Header("Settings")]
    [SerializeField] private TextMeshProUGUI[] subSettingPanelTMP;
    [SerializeField] private GameObject[] subSettingsPanels;
    [SerializeField] private TextMeshProUGUI settingPanelCloseTMP;

    public GameObject[] ingameObjects;
    public RectTransform ingameMovingPanel;
    public RectTransform movingPanelParent;

    public List<PanelButtonPair> panelButtonPairs;
    public int activeSubPanel = 0;
    public float fadeTime = 1f;
    public CanvasGroup settingCanvasGroup;
    public RectTransform settingRectTransform;

    public ScrollingUIEffect scrollEffect;

    public bool loading;
    public bool opened;

    public bool IsActive => loading || opened;

    private TextManager tm => TextManager.Instance;

    public void ReturnToParent()
    {
        ingameMovingPanel.SetParent(movingPanelParent);
        ingameMovingPanel.SetSiblingIndex(1);
        ingameMovingPanel.offsetMin = new Vector2(0f, 0f);
        ingameMovingPanel.offsetMax = new Vector2(0f, 0f);
        ingameMovingPanel.localScale = Vector3.one;
    }

    public void LobbySwitch(bool lobby)
    {
        if (lobby)
        {
            for (int i = 0; i < ingameObjects.Length; i++)
            {
                ingameObjects[i].SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < ingameObjects.Length; i++)
            {
                ingameObjects[i].SetActive(true);
            }
        }
    }

    public void OpenPanel(int num)
    {
        activeSubPanel = num;
        OpenSettings();
        AudioManager.Instance.PlaySFX(Sfx.buttons);
    }

    public void ButtonSound()
    {
        AudioManager.Instance.PlaySFX(Sfx.buttons);
    }
    public void ButtonHighlightSound() // 큰 탭 관련 (월드맵, 설정, 주문, 관리 등등)
    {
        if (loading || (UIManager.Instance != null && (UIManager.Instance.shopUI.loading || UIManager.Instance.utilUI.loading))) return;

        AudioManager.Instance.PlaySFX(Sfx.btnHighlight2, volume: 0.75f);
    }
    public void ButtonHighlightSound2()
    {
        AudioManager.Instance.PlaySFX(Sfx.btnHighlight, volume: 0.75f);
    }

    public void OpenSettings()
    {
        if (loading) return;

        if (opened)
        {
            HideSettings();
            return;
        }

        scrollEffect.enabled = true;
        loading = true;

        SelectSubPanel(activeSubPanel);
        ShowSubSettingPanel(0);

        settingCanvasGroup.alpha = 0f;
        settingRectTransform.transform.localPosition = new Vector3(0f, 1000f, 0f);
        settingRectTransform.DOAnchorPos(new Vector2(0f, 0f), fadeTime, false).SetEase(Ease.OutElastic).SetUpdate(true);
        settingCanvasGroup.DOFade(1f, fadeTime).SetUpdate(true).OnComplete(() =>
        {
            loading = false;
            opened = true;
        });
    }
    public void HideSettings()
    {
        if (!opened) return;
        if (loading) return;

        scrollEffect.enabled = false;
        opened = false;
        loading = true;

        for (int i = 0; i < panelButtonPairs.Count; i++)
        {
            panelButtonPairs[i].button.Hide();
        }

        settingCanvasGroup.alpha = 1f;
        settingRectTransform.transform.localPosition = new Vector3(0f, 0f, 0f);
        settingRectTransform.DOAnchorPos(new Vector2(0f, -2000f), fadeTime, false).SetEase(Ease.InOutQuint).SetUpdate(true);
        settingCanvasGroup.DOFade(0f, fadeTime).SetUpdate(true).OnComplete(() =>
        {
            loading = false;
        });
    }

    public void ShowSubSettingPanel(int idx)
    {
        for (int i = 0; i < subSettingsPanels.Length; i++)
        {
            subSettingsPanels[i].SetActive(false);
        }

        subSettingsPanels[idx].SetActive(true);
    }

    public void SelectSubPanel(int idx)
    {
        for (int i = 0; i < panelButtonPairs.Count; i++)
        {
            panelButtonPairs[i].panel.SetActive(false);
            panelButtonPairs[i].button.SetHighlight(false);
        }

        panelButtonPairs[idx].panel.SetActive(true);
        panelButtonPairs[idx].button.SetHighlight(true);

        activeSubPanel = idx;
    }

    public void UpdateTexts()
    {
        if (restoreDefaultsTMP != null)
            restoreDefaultsTMP.text = tm.GetCommons("RestoreDefaults");

        sfxSliderTMP.text = tm.GetCommons("SFXvolume");
        bgmSliderTMP.text = tm.GetCommons("BGMvolume");
        resolutionTMP.text = tm.GetCommons("Resoultion");
        fullScreenTMP.text = tm.GetCommons("Display");
        framerateTMP.text = tm.GetCommons("LockFramerate");
        vsyncTMP.text = tm.GetCommons("VSync");

        subSettingPanelTMP[0].text = tm.GetCommons("Settings");
        subSettingPanelTMP[1].text = tm.GetCommons("KeySettings");
        subSettingPanelTMP[2].text = tm.GetCommons("Save");
        subSettingPanelTMP[3].text = tm.GetCommons("Load");
        subSettingPanelTMP[4].text = tm.GetCommons("Main Menu");
        subSettingPanelTMP[5].text = tm.GetCommons("Quit");
        subSettingPanelTMP[6].text = tm.GetCommons("Unstuck");
        subSettingPanelTMP[7].text = tm.GetCommons("SendFeedback");

        cameraSpeedTMP.text = string.Format(tm.defaultCultureInfo, tm.GetCommons("CameraSpeed"), cameraSpeed);
        invertZoomTMP.text = tm.GetCommons("InvertZoom");
        edgeScrollingTMP.text = tm.GetCommons("EdgeScrolling");
        autosaveTMP.text = tm.GetCommons("Autosave");

        settingPanelCloseTMP.text = tm.GetCommons("Back");

        KeySetting_TextUpdate();
        UpdateScreenModeUI();
        UpdateFramerateUI();
    }
    #endregion

    #region 볼륨 설정    
    [Header("Settings 세부 UI")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI bgmSliderTMP;
    [SerializeField] private TextMeshProUGUI sfxSliderTMP;

    public void SetBGMVolume(float value)
    {
        AudioManager.Instance.SetBGMVolume(value);
        SaveManager.Instance.SaveConfig();
    }

    public void SetSFXVolume(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
        SaveManager.Instance.SaveConfig();
    }
    #endregion

    #region 카메라 설정
    public float cameraSpeed { get; private set; }
    public Slider cameraSpeedSlider;
    public bool invertZoom { get; private set; }
    [SerializeField] private Toggle invertZoomToggle;
    public bool edgeScrolling { get; private set; }
    [SerializeField] private Toggle edgeScrollingToggle;
    public bool autosave { get; private set; }
    [SerializeField] private Toggle autosaveToggle;

    [SerializeField] private TextMeshProUGUI cameraSpeedTMP;
    [SerializeField] private TextMeshProUGUI invertZoomTMP;
    [SerializeField] private TextMeshProUGUI edgeScrollingTMP;
    [SerializeField] private TextMeshProUGUI autosaveTMP;

    public void SetCameraSpeed(float value)
    {
        cameraSpeed = value;
        cameraSpeedTMP.text = string.Format(tm.defaultCultureInfo, tm.GetCommons("CameraSpeed"), cameraSpeed);
        SaveManager.Instance.SaveConfig();
    }

    public void SetInvertZoom(bool on)
    {
        invertZoom = on;
        SaveManager.Instance.SaveConfig();
    }
    public void SetEdgeScrolling(bool on)
    {
        edgeScrolling = on;
        SaveManager.Instance.SaveConfig();
    }
    public void SetAutosave(bool on)
    {
        autosave = on;
        SaveManager.Instance.SaveConfig();
    }
    #endregion

    #region 해상도 설정
    [Header("설정")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    public int fullscreen { get; private set; } // 드랍다운의 인덱스 값을 가짐
    public bool vsync { get; private set; }
    [SerializeField] private TMP_Dropdown fullScreenDropdown;
    [SerializeField] private TMP_Dropdown framerateDropdown;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private TextMeshProUGUI resolutionTMP;
    [SerializeField] private TextMeshProUGUI fullScreenTMP;
    [SerializeField] private TextMeshProUGUI framerateTMP;
    [SerializeField] private TextMeshProUGUI vsyncTMP;
    private List<Resolution> possibleResolution;
    public Vector2Int settingResolution { get; private set; }

    private List<FullScreenMode> fullScreenModesOptions;

    public int framerateIdx { get; private set; } // 드랍다운의 인덱스 값을 가짐
    private readonly List<int> framerateList = new List<int> { -1, 244, 240, 165, 120, 95, 90, 75, 60, 55, 45, 30, 24 };

    public static EventHandler ResolutionChangedEvent;

    private void SettingInit(ConfigData config)
    {
        if (config != null)
        {
            cameraSpeedSlider.value = Mathf.Clamp(config.cameraSpeed, 0.5f, 1.5f);
            invertZoomToggle.isOn = config.invertZoom;
            edgeScrollingToggle.isOn = config.edgeScrolling;
            autosaveToggle.isOn = config.autosave;

            fullScreenDropdown.value = config.fullScreen;
            SetFullScreen(config.fullScreen);

            framerateDropdown.value = config.framerate;
            SetFramerate(config.framerate);

            vsyncToggle.isOn = config.vsync;
            SetVSync(config.vsync);

            int findResolution = -1;
            for (int i = 0; i < possibleResolution.Count; i++)
            {
                if (possibleResolution[i].width == config.resolution_width && possibleResolution[i].height == config.resolution_height)
                {
                    findResolution = i;
                    break;
                }
            }
            if (findResolution >= 0)
            {
                resolutionDropdown.value = findResolution;
                SetResolution(findResolution);
            }
            else
            {
                resolutionDropdown.value = possibleResolution.Count - 1;
                SetResolution(possibleResolution.Count - 1);
            }

            bgmSlider.value = Mathf.Clamp(config.volumeBGM, 0, 10);
            sfxSlider.value = Mathf.Clamp(config.volumeSFX, 0, 10);
            AudioManager.Instance.SetBGMVolume(bgmSlider.value);
            AudioManager.Instance.SetSFXVolume(sfxSlider.value);
        }
        else
        {
            cameraSpeedSlider.value = 1f;
            invertZoomToggle.isOn = false;
            edgeScrollingToggle.isOn = true;
            autosaveToggle.isOn = true;

            fullScreenDropdown.value = 0;
            SetFullScreen(0);

            framerateDropdown.value = 0;
            SetFramerate(0);

            vsyncToggle.isOn = false;
            SetVSync(false);

            resolutionDropdown.value = possibleResolution.Count - 1;
            SetResolution(possibleResolution.Count - 1);

            bgmSlider.value = 5;
            sfxSlider.value = 5;
            AudioManager.Instance.SetBGMVolume(bgmSlider.value);
            AudioManager.Instance.SetSFXVolume(sfxSlider.value);
        }
    }

    private void InitResolutionUI()
    {
        var temp_possibleResolution = Screen.resolutions;
        possibleResolution = new List<Resolution>(temp_possibleResolution.Length);
        for (int i = 0; i < temp_possibleResolution.Length; i++)
        {
            bool has = false;
            for (int n = 0; n < possibleResolution.Count; n++)
            {
                if (temp_possibleResolution[i].width == possibleResolution[n].width && temp_possibleResolution[i].height == possibleResolution[n].height)
                {
                    has = true;
                    break;
                }    
            }
            if (!has)
                possibleResolution.Add(temp_possibleResolution[i]);
        }

        resolutionDropdown.ClearOptions();

        for (int i = 0; i < possibleResolution.Count; i++)
        {
            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
            optionData.text = string.Format("{0}x{1}", possibleResolution[i].width, possibleResolution[i].height);
            resolutionDropdown.options.Add(optionData);
        }
    }
    private void InitScreenModeUI()
    {
        fullScreenDropdown.ClearOptions();

#if UNITY_STANDALONE_WIN

        fullScreenModesOptions = new List<FullScreenMode> { FullScreenMode.FullScreenWindow, FullScreenMode.ExclusiveFullScreen, FullScreenMode.Windowed };
#else
        fullScreenModesOptions = new List<FullScreenMode> { FullScreenMode.FullScreenWindow, FullScreenMode.Windowed };
#endif

        for (int i = 0; i < fullScreenModesOptions.Count; i++)
        {
            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
            optionData.text = tm.GetCommons($"FullScreen{(int)fullScreenModesOptions[i]}");
            fullScreenDropdown.options.Add(optionData);
        }
    }
    private void UpdateScreenModeUI()
    {
        for (int i = 0; i < fullScreenDropdown.options.Count; i++)
        {
            fullScreenDropdown.options[i].text = tm.GetCommons($"FullScreen{(int)fullScreenModesOptions[i]}");
        }
        fullScreenDropdown.RefreshShownValue();
    }
    private void InitFramerateUI()
    {
        framerateDropdown.ClearOptions();

        for (int i = 0; i < framerateList.Count; i++)
        {
            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
            optionData.text = framerateList[i].ToString();
            framerateDropdown.options.Add(optionData);
        }
        UpdateFramerateUI();
    }
    private void UpdateFramerateUI()
    {
        if (framerateDropdown.options.Count >= 1)
        {
            framerateDropdown.options[0].text = tm.GetCommons("Uncapped");
            framerateDropdown.RefreshShownValue();
        }
    }

    public void SetFullScreen(int mode)
    {
        if (mode >= fullScreenModesOptions.Count)
            mode = 0;
        fullscreen = mode;
        Screen.SetResolution(settingResolution.x, settingResolution.y, fullScreenModesOptions[fullscreen]);
        SaveManager.Instance.SaveConfig();
        fullScreenDropdown.RefreshShownValue();
    }
    public void SetVSync(bool on)
    {
        vsync = on;
        QualitySettings.vSyncCount = on ? 1 : 0;
        SaveManager.Instance.SaveConfig();
    }
    public void SetResolution(int idx)
    {
        settingResolution = new Vector2Int(possibleResolution[idx].width, possibleResolution[idx].height);
        Screen.SetResolution(possibleResolution[idx].width, possibleResolution[idx].height, fullScreenModesOptions[fullscreen]);
        SaveManager.Instance.SaveConfig();
        resolutionDropdown.RefreshShownValue();

        if (ResolutionChangedEvent != null)
            ResolutionChangedEvent(null, null);


        float ratio = (float)settingResolution.y / settingResolution.x;
        float modify = (ratio > 0.5625f) ? 0f : 1f;

        var canvasScaler = FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < canvasScaler.Length; i++)
        {
            canvasScaler[i].matchWidthOrHeight = modify;
        }

        //StartCoroutine(CanvasUpdate());
    }
    private IEnumerator CanvasUpdate()
    {
        yield return null;

        float ratio = (float)settingResolution.y / settingResolution.x;
        float modify = (ratio > 0.5625f) ? 0f : 1f;

        var canvasScaler = FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < canvasScaler.Length; i++)
        {
            canvasScaler[i].matchWidthOrHeight = modify;
        }
    }

    public void SetFramerate(int idx)
    {
        framerateIdx = idx;
        Application.targetFrameRate = framerateList[idx];
        SaveManager.Instance.SaveConfig();
        framerateDropdown.RefreshShownValue();
    }
#endregion
}
