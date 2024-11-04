using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Lobby : Singleton<Lobby>
{

    public GameObject lobbyUIObjects;

    public LanguagePanel languagePanel;

    public bool SaveDataLoading { get; private set; }

    public int newGameCount;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this.gameObject);

        LobbyUISwitch(true);
        CallAfterAwake();
    }

    public override void CallAfterAwake()
    {
        //SettingManager.Instance.CallAfterAwake();

        //var cursorLib = DataManager.Instance.cursorLibrary;
        //Cursor.SetCursor(cursorLib.normal, Vector2.zero, CursorMode.Auto);
    }

    private void Start()
    {
        SaveDataLoading = true;
        var config = SaveManager.Instance.LoadConfig();
        CallAfterStart(config);
        var player = SaveManager.Instance.LoadPlayer();
        if (player != null)
        {
            newGameCount = player.newGameCount;
        }
    }

    public override void CallAfterStart(ConfigData config)
    {
        TextManager.Instance.CallAfterStart(config);
        SettingManager.Instance.CallAfterStart(config);

        languagePanel.Init();

        //SaveManager.Instance.CallAfterStart(config);

        SaveDataLoading = false;
        // 초기화 완료 후 설정 저장
        SaveManager.Instance.SaveConfig();

        StartCoroutine(UINaviStart());
    }

    protected override void AddListeners()
    {
        LoadingSceneManager.SceneLoadCompletedEvent += OnLoadingComplete;

        InputHelper.EscapeEvent += OnESC;
        InputHelper.BackEvent += OnBack;
    }
    protected override void RemoveListeners()
    {
        LoadingSceneManager.SceneLoadCompletedEvent -= OnLoadingComplete;

        InputHelper.EscapeEvent -= OnESC;
        InputHelper.BackEvent -= OnBack;
    }

    private void OnLoadingComplete(object sender, string e)
    {
        if (e.Equals("lobby"))
        {
            // 로비 귀환시 UI 복구
            LobbyUISwitch(true);
        }
    }
    public void LobbyUISwitch(bool on)
    {
        LoadingSceneManager.Instance.logueLoading = false;
        AudioManager.Instance.ToggleMute(false);

        if (on)
        {
            var pad = Gamepad.current;
            Cursor.visible = pad == null;
        }

        lobbyUIObjects.SetActive(on);
        SettingManager.Instance.LobbySwitch(on);
        StartCoroutine(UINaviStart());
    }
    private IEnumerator UINaviStart()
    {
        yield return null;
        yield return null;
        UINaviHelper.Instance.SetFirstSelect();
    }

    private void OnESC(object sender, InputAction.CallbackContext e)
    {
        if (GM.Instance != null) return; // 인게임 씬에서는 단축키 막음
        if (!lobbyUIObjects.activeSelf) return;
        if (LoadingSceneManager.Instance.logueLoading) return; // 프롤로그나 에필로그 중에는 단축키 막음
        if (LoadingSceneManager.Instance.IsSceneLoading) return; // 로딩 중 단축키 막음

        if (e.performed)
        {
            if (UINaviHelper.Instance.inputHelper.disconnectedPanel.activeSelf)
            {
                UINaviHelper.Instance.inputHelper.PadConnected();
                return;
            }

            if (SaveManager.Instance.saveDeleteWarningObj.activeSelf)
            {
                SaveManager.Instance.ShowSaveDeleteWarning(false);
            }
            else if (SaveManager.Instance.IsActive)
            {
                SaveManager.Instance.HideSaveSlots();
            }
            else if (SettingManager.Instance.IsActive)
            {
                SettingManager.Instance.HideSettings();
            }
            else
            {
                SettingManager.Instance.OpenSettings();
            }
        }
    }
    private void OnBack(object sender, InputAction.CallbackContext e)
    {
        if (GM.Instance != null) return; // 인게임 씬에서는 단축키 막음

        if (e.performed)
        {

        }
    }
    private void Update()
    {
        if (GM.Instance != null) return; // 인게임 씬에서는 단축키 막음
        if (!lobbyUIObjects.activeSelf) return;
        if (LoadingSceneManager.Instance.logueLoading) return; // 프롤로그나 에필로그 중에는 단축키 막음
        if (LoadingSceneManager.Instance.IsSceneLoading) return; // 로딩 중 단축키 막음

        if (Input.anyKeyDown)
        {
            var pad = Gamepad.current;
            Cursor.visible = pad == null;
        }
    }

    public void CloseAllPanel()
    {
        SaveManager.Instance.HideSaveSlots();
        SettingManager.Instance.HideSettings();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    //public void LoadGameStart(string saveName)
    //{
    //    CloseAllPanel();
    //    LobbyUISwitch(false);
    //    GameStartInfo gameStartInfo = new GameStartInfo
    //    {
    //        slotNum = -1,
    //        saveName = saveName,
    //    };
    //    LoadingSceneManager.Instance.LobbyStart(gameStartInfo, settingLib.sceneName);
    //}

    //public void TEMP_GameSTART() // 현재 데모 버전 시작 버튼
    //{
    //    CloseAllPanel();
    //    LobbyUISwitch(false);
    //    GameStartInfo gameStartInfo = new GameStartInfo
    //    {
    //        slotNum = -1,
    //        tutorial = true,
    //    };
    //    LoadingSceneManager.Instance.LobbyStart(gameStartInfo, settingLib.sceneName);
    //}

    public void MenuButtonSound()
    {
        AudioManager.Instance.PlaySFX(Sfx.buttons); // 로비 UI 버튼들
    }

    [Header("Fixed Texts")]
    [SerializeField] private TextMeshProUGUI newGameTMP;
    [SerializeField] private TextMeshProUGUI settingsTMP;
    [SerializeField] private TextMeshProUGUI exitTMP;
    [SerializeField] private TextMeshProUGUI versionTMP;
    [SerializeField] private TextMeshProUGUI creditPanelTMP;
    [SerializeField] private TextMeshProUGUI creditBtnTMP;

    [SerializeField] private TextMeshProUGUI settings_titleTMP;
    [SerializeField] private TextMeshProUGUI wishlistNowTMP;

    public void UpdateTexts()
    {
        var tm = TextManager.Instance;
        newGameTMP.text = tm.GetCommons("Start");
        settingsTMP.text = tm.GetCommons("Menu");
        exitTMP.text = tm.GetCommons("Exit");
        versionTMP.text = string.Format("v{0:0.00}", (SaveManager.Instance.version / 100f));
        creditPanelTMP.text = tm.GetCommons("Credits");
        creditBtnTMP.text = tm.GetCommons("Credits");

        settings_titleTMP.text = tm.GetCommons("Settings");
        wishlistNowTMP.text = tm.GetCommons("WishlistNow2");
    }

    /*
    #region NewGame
    [Header("New Game")]
    public Canvas newGameCanvas;

    public TMP_InputField cityNameInputField;
    public TMP_InputField mapseedInputField;

    public TMP_Dropdown resourceDensityDropdown;
    public TMP_Dropdown disasterIntensityDropdown;
    public TMP_Dropdown startPopulationDropdown;
    public TMP_Dropdown startResourcesDropdown;

    public void OpenNewGameCanvas()
    {
        cityNameInputField.text = TextManager.Instance.GetSurvivorName(0);
        mapseedInputField.text = UnityEngine.Random.Range(0, int.MaxValue).ToString();

        ResetDropdown(resourceDensityDropdown, 1);
        ResetDropdown(disasterIntensityDropdown, 1);
        ResetDropdown(startPopulationDropdown, 1);
        ResetDropdown(startResourcesDropdown, 1);

        SetTutorialForce(newGameCount == 0);

        CloseAllPanel();
        newGameCanvas.gameObject.SetActive(true);

        void ResetDropdown(TMP_Dropdown dropdown, int init)
        {
            dropdown.SetValueWithoutNotify(init);
            dropdown.captionText.text = dropdown.options[init].text;
        }
    }

    public void StartCityName(string str)
    {
        AudioManager.Instance.PlaySFX(Sfx.inputFieldStart);
    }
    public void EditingCityName(string str)
    {
        if (cityNameInputField.textComponent.preferredWidth > 220f)
        {
            cityNameInputField.text = str.Substring(0, str.Length - 1);
        }
    }
    public void EndCityName(string str)
    {
        cityNameInputField.text = str;

        while (cityNameInputField.textComponent.preferredWidth > 220f)
        {
            cityNameInputField.text = cityNameInputField.text.Substring(0, cityNameInputField.text.Length - 1);
        }

        AudioManager.Instance.PlaySFX(Sfx.inputFieldEnd);
    }

    public void StartEditMapSeed(string str)
    {
        AudioManager.Instance.PlaySFX(Sfx.inputFieldStart);
    }
    public void EndEditMapSeed(string str)
    {
        int.TryParse(str, out int mapseed);
        if (mapseed < 0) mapseed = UnityEngine.Random.Range(0, int.MaxValue);
        mapseedInputField.text = mapseed.ToString();
        AudioManager.Instance.PlaySFX(Sfx.inputFieldEnd);
    }

    public void MapSeedRerollButton()
    {
        int random = UnityEngine.Random.Range(0, int.MaxValue);
        mapseedInputField.text = random.ToString();
    }

    public void NewGameStart()
    {
        CloseAllPanel();
        LobbyUISwitch(false);
        // 플레이어가 시작 전에 설정할 수 있는 정보

        int.TryParse(mapseedInputField.text, out int mapseed);

        newGameCount++;
        SaveManager.Instance.SavePlayer();

        GameStartInfo gameStartInfo = new GameStartInfo
        {
            pizzeriaName = cityNameInputField.text,

            resourceDensity = resourceDensityDropdown.value - 1,
            disasterIntensity = disasterIntensityDropdown.value - 1,
            startPopulation = startPopulationDropdown.value - 1,
            startResource = startResourcesDropdown.value - 1,

            tutorial = tutorialToggle.isOn,

            slotNum = -1,
        };
        LoadingSceneManager.Instance.LobbyStart(gameStartInfo, SaveManager.Instance.settingLib.sceneName);
    }

    [SerializeField] private Toggle tutorialToggle;
    [SerializeField] private TextMeshProUGUI tutorialTMP;

    public void SetTutorialForce(bool on)
    {
        tutorialToggle.isOn = on;
    }
    #endregion
    */
}

public struct GameStartInfo
{
    // NewGame (플레이어 설정)
    public string pizzeriaName;

    public int resourceDensity;
    public int disasterIntensity;
    public int startPopulation;
    public int startResource;

    public bool tutorial;

    // LoadGame (세이브 파일 불러오기)
    public int slotNum;
    public string saveName;
}
