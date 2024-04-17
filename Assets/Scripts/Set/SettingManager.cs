using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


#region ���̺� ������
public enum KeyMap
{
    cameraMove_Up,
    cameraMove_Down,
    cameraMove_Left,
    cameraMove_Right,
    cameraZoom_In,
    cameraZoom_Out,
    time_Pause,
    time_Speed_1,
    time_Speed_2,
    time_Speed_3,
    escape,
    toggle_UI_Display,
    demolish_Building,
    jobPriority_Up,
    jobPriority_Down,
    pause,
    upgrade,

    //help,
    trade,
    research,
    stockManagement,

    remove,

    LAST,
}

[Serializable]
public class KeySaveData
{
    public SerializableDictionary<KeyMap, KeyCode> keyCodes = new()
    {
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.cameraMove_Up, Value = KeyCode.W },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.cameraMove_Down, Value = KeyCode.S },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.cameraMove_Left, Value = KeyCode.A },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.cameraMove_Right, Value = KeyCode.D },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.cameraZoom_In, Value = KeyCode.PageDown },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.cameraZoom_Out, Value = KeyCode.PageUp },

        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.time_Pause, Value = KeyCode.Space },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.time_Speed_1, Value = KeyCode.Alpha1 },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.time_Speed_2, Value = KeyCode.Alpha2 },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.time_Speed_3, Value = KeyCode.Alpha3 },

        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.escape, Value = KeyCode.Escape },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.toggle_UI_Display, Value = KeyCode.I },

        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.demolish_Building, Value = KeyCode.X },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.jobPriority_Up, Value = KeyCode.J },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.jobPriority_Down, Value = KeyCode.H },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.pause, Value = KeyCode.F },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.upgrade, Value = KeyCode.U },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.remove, Value = KeyCode.Z },

        //new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.help, Value = KeyCode.Home },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.trade, Value = KeyCode.T },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.research, Value = KeyCode.R },
        new SerializableDictionary<KeyMap, KeyCode>.Pair { Key = KeyMap.stockManagement, Value = KeyCode.N },
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
        {
            return Input.GetKeyDown(key);
        }
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
    private Dictionary<KeyCode, byte> bannedSettingKey;
    private TextMeshProUGUI restoreDefaultsTMP;

    // Keysetting���� prefab ���� ����
    private readonly List<KeyMap> keyOrder = new List<KeyMap>
    {
        KeyMap.cameraMove_Up,
        KeyMap.cameraMove_Down,
        KeyMap.cameraMove_Left,
        KeyMap.cameraMove_Right,
        KeyMap.cameraZoom_In,
        KeyMap.cameraZoom_Out,

        KeyMap.time_Pause,
        KeyMap.time_Speed_1,
        KeyMap.time_Speed_2,
        KeyMap.time_Speed_3,

        KeyMap.escape,
        KeyMap.toggle_UI_Display,
        KeyMap.demolish_Building,

        KeyMap.jobPriority_Up,
        KeyMap.jobPriority_Down,
        KeyMap.pause,
        KeyMap.upgrade,
        KeyMap.remove,

        //KeyMap.help,
        KeyMap.trade,
        KeyMap.research,
        KeyMap.stockManagement,
    };

    public bool DisableControl => WindowOutOfFocus || currentActiveKeyNum != KeyMap.LAST;

    public bool WindowOutOfFocus { get; private set; }

    private void OnApplicationPause(bool pause)
    {
        if (pause == true)
        {
            // ��Ż
            WindowOutOfFocus = true;

        }
        else
        {
            // ����
            WindowOutOfFocus = false;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        InitResolutionUI();
        fullscreen = true;
        edgeScrolling = true;
        invertZoom = false;
        autosave = true;
        cameraSpeed = 1f;
    }

    public override void CallAfterAwake()
    {

    }
    public override void CallAfterStart(ConfigData config)
    {
        KeyInit(true);
        SettingInit(config);
    }

    #region Ű �ʱ� ����
    public void KeySave()
    {
        foreach (var item in keyMappings)
        {
            keySave.keyCodes[item.Key] = item.Value.key;
        }

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
                keySave.keyCodes[KeyMap.cameraMove_Up] = KeyCode.Z;
                keySave.keyCodes[KeyMap.cameraMove_Left] = KeyCode.Q;
                keySave.keyCodes[KeyMap.remove] = KeyCode.W;
                keySave.keyCodes[KeyMap.stockManagement] = KeyCode.N;
            }
            else if (Application.systemLanguage == SystemLanguage.German)
            {
                keySave.keyCodes[KeyMap.remove] = KeyCode.Y;
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

    // Main ���� �ʱ�ȭ ����
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

    #region Ű ����
    private void CreateKeyObject()
    {
        keyObjects = new SerializableDictionary<KeyMap, KeyObject>();

        for (int i = 0; i < keyOrder.Count; i++)
        {
            KeyObject keyObject = Instantiate(keyObject_Prefab, keyObject_Parent).GetComponent<KeyObject>();
            keyObject.Init(keyOrder[i]);
            keyObjects.Add(new SerializableDictionary<KeyMap, KeyObject>.Pair { Key = keyOrder[i], Value = keyObject });
        }

        // ���� �����
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
        restoreDefaultsTMP.text = TextManager.Instance.GetCommons("RestoreDefaults");
    }

    private void Update()
    {
        if (LoadingSceneManager.Instance.IsSceneLoading) return;

        if (currentActiveKeyNum != KeyMap.LAST)
        {
            foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (!bannedSettingKey.ContainsKey(kcode))
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
                        AudioManager.Instance.PlaySFX(Sfx.inputFieldEnd);
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
                        AudioManager.Instance.PlaySFX(Sfx.smallButtons);
                        break;
                    }
                }
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

        bannedSettingKey = new Dictionary<KeyCode, byte>();
        bannedSettingKey.Add(KeyCode.LeftShift, 0);
        bannedSettingKey.Add(KeyCode.LeftControl, 0);
        bannedSettingKey.Add(KeyCode.Mouse0, 0);
        bannedSettingKey.Add(KeyCode.Mouse1, 0);
        bannedSettingKey.Add(KeyCode.Mouse2, 0);
        bannedSettingKey.Add(KeyCode.Mouse3, 0);
        bannedSettingKey.Add(KeyCode.Mouse4, 0);
        bannedSettingKey.Add(KeyCode.Mouse5, 0);
        bannedSettingKey.Add(KeyCode.Mouse6, 0);
    }
    #endregion

    #region (����) ����
    [Header("Settings")]
    public Canvas settingCanvas;
    [SerializeField] private TextMeshProUGUI settingsTMP;
    [SerializeField] private TextMeshProUGUI[] subSettingPanelTMP;

    [SerializeField] private GameObject[] subSettingsPanels;

    public void OpenSettings()
    {
        //Lobby.Instance.CloseAllPanel();
        settingCanvas.gameObject.SetActive(true);
        ShowSubSettingPanel(0);
    }
    public void HideSettings()
    {
        settingCanvas.gameObject.SetActive(false);
    }

    public void ShowSubSettingPanel(int idx)
    {
        for (int i = 0; i < subSettingsPanels.Length; i++)
        {
            subSettingsPanels[i].SetActive(false);
            subSettingPanelTMP[i].color = Color.white;
        }

        subSettingsPanels[idx].SetActive(true);
        //subSettingPanelTMP[idx].color = DataManager.Instance.uiLibrary.settingPanelHighlight;
    }

    public void UpdateTexts()
    {
        var tm = TextManager.Instance;
        if (restoreDefaultsTMP != null)
            restoreDefaultsTMP.text = tm.GetCommons("RestoreDefaults");

        sfxSliderTMP.text = tm.GetCommons("SFXvolume");
        bgmSliderTMP.text = tm.GetCommons("BGMvolume");
        resolutionTMP.text = tm.GetCommons("Resoultion");
        fullScreenTMP.text = tm.GetCommons("FullScreen");

        settingsTMP.text = tm.GetCommons("Settings");
        subSettingPanelTMP[0].text = tm.GetCommons("CommonSettings");
        subSettingPanelTMP[1].text = tm.GetCommons("KeySettings");

        cameraSpeedTMP.text = string.Format(tm.defaultCultureInfo, tm.GetCommons("CameraSpeed"), cameraSpeed);
        invertZoomTMP.text = tm.GetCommons("InvertZoom");
        edgeScrollingTMP.text = tm.GetCommons("EdgeScrolling");
        autosaveTMP.text = tm.GetCommons("Autosave");

        KeySetting_TextUpdate();
    }
    #endregion

    #region ���� ����    
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

    #region ī�޶� ����
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
        cameraSpeedTMP.text = string.Format(TextManager.Instance.GetCommons("CameraSpeed"), cameraSpeed);
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

    #region �ػ� ����
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    public bool fullscreen { get; private set; }
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private TextMeshProUGUI resolutionTMP;
    [SerializeField] private TextMeshProUGUI fullScreenTMP;
    private List<Resolution> possibleResolution;
    public Vector2Int settingResolution { get; private set; }

    private void SettingInit(ConfigData config)
    {
        if (config != null)
        {
            cameraSpeedSlider.value = Mathf.Clamp(config.cameraSpeed, 0.5f, 1.5f);
            invertZoomToggle.isOn = config.invertZoom;
            edgeScrollingToggle.isOn = config.edgeScrolling;
            autosaveToggle.isOn = config.autosave;

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

            fullScreenToggle.isOn = config.fullScreen;

            bgmSlider.value = Mathf.Clamp(config.volumeBGM, 0.001f, 1f);
            sfxSlider.value = Mathf.Clamp(config.volumeSFX, 0.001f, 1f);
        }
        else
        {
            cameraSpeedSlider.value = 1f;
            invertZoomToggle.isOn = false;
            edgeScrollingToggle.isOn = true;
            autosaveToggle.isOn = true;

            resolutionDropdown.value = possibleResolution.Count - 1;
            SetResolution(possibleResolution.Count - 1);

            fullScreenToggle.isOn = true;

            bgmSlider.value = 1f;
            sfxSlider.value = 1f;
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

    public void SetFullScreen(bool on)
    {
        fullscreen = on;
        Screen.SetResolution(settingResolution.x, settingResolution.y, fullscreen);
        SaveManager.Instance.SaveConfig();
    }
    public void SetResolution(int idx)
    {
        settingResolution = new Vector2Int(possibleResolution[idx].width, possibleResolution[idx].height);
        Screen.SetResolution(possibleResolution[idx].width, possibleResolution[idx].height, fullscreen);
        SaveManager.Instance.SaveConfig();
    }
    #endregion
}
