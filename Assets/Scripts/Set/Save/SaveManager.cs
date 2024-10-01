using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System;
using System.IO;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;

[Serializable]
public struct SavePosition
{
    public float x;
    public float y;
    public float z;

    public SavePosition(Vector3 vec)
    {
        x = vec.x;
        y = vec.y;
        z = vec.z;
    }
    public static implicit operator Vector3(SavePosition c)
    {
        return new Vector3(c.x, c.y, c.z);
    }
    public static implicit operator SavePosition(Vector3 c)
    {
        return new SavePosition(c);
    }
}
[Serializable]
public struct Save2DArray<T>
{
    [Serializable]
    public struct SaveArray<M>
    {
        public M[] array;

        public static implicit operator M[](SaveArray<M> arrays)
        {
            M[] temp = new M[arrays.array.Length];
            for (int i = 0; i < arrays.array.Length; i++)
            {
                temp[i] = arrays.array[i];
            }
            return temp;
        }
    }
    public SaveArray<T>[] data;

    public Save2DArray(T[][] arrays)
    {
        data = new SaveArray<T>[arrays.GetLength(0)];
        for (int i = 0; i < data.Length; i++)
        {
            T[] temp = new T[arrays[i].Length];
            for (int j = 0; j < arrays[i].Length; j++)
            {
                temp[j] = arrays[i][j];
            }
            data[i] = new SaveArray<T> { array = temp };
        }
    }
    public static implicit operator Save2DArray<T>(T[][] arrays)
    {
        return new Save2DArray<T>(arrays);
    }
    public static implicit operator T[][](Save2DArray<T> arrays)
    {
        T[][] temp = new T[arrays.data.Length][];
        for (int i = 0; i < arrays.data.Length; i++)
        {
            temp[i] = new T[arrays.data[i].array.Length];
            for (int j = 0; j < arrays.data[i].array.Length; j++)
            {
                temp[i][j] = arrays.data[i].array[j];
            }
        }
        return temp;
    }
}

public class SaveManager : Singleton<SaveManager>
{

    public readonly int version = 100;


    public override void CallAfterAwake()
    {

    }
    public override void CallAfterStart(ConfigData config)
    {

    }

    public void SaveWithDay(int slotNum, int day)
    {
        //int currentSaveCount = JsonHelper.GetJsonFileCount(Application.persistentDataPath, $"Saves/Slot {slotNum}");
        //Debug.Log("currentSaveCount : " + currentSaveCount);
        Save(slotNum, $"save{day}");
    }

    public void Save(int slotNum, string fileName)
    {
        GameSaveData gameSaveData = new GameSaveData();

        gameSaveData.meta = new MetaSaveData {
            version = this.version,
            dateInfo = new DateInfo { year = DateTime.Now.Year, month = DateTime.Now.Month, day = DateTime.Now.Day, hour = DateTime.Now.Hour, minute = DateTime.Now.Minute },
        };

        gameSaveData.gm = new GMSaveData
        {
            data = GM.Instance.Save(),
            rocket = RocketManager.Instance.Save(),
            stat = StatManager.Instance.Save(),
        };

        gameSaveData.research = new ResearchSaveData
        {
            data = ResearchManager.Instance.Save(),
        };
        gameSaveData.reviews = new ReviewTotalData
        {
            data = UIManager.Instance.shopUI.Save(),
        };
        gameSaveData.villager = new VillagerData
        {
            data = VillagerManager.Instance.Save(),
        };

        //

        string jsonData = JsonHelper.ObjectToJson(gameSaveData);
        JsonHelper.CreateJsonFile(Application.persistentDataPath, fileName, $"Saves/Slot {slotNum}", jsonData, true);
    }

    public GameSaveData LoadSaveData(int slotNum, string saveName)
    {
        StringBuilder st = new StringBuilder().AppendFormat("{0}/Saves/{1}/{2}.json", Application.persistentDataPath, $"Slot {slotNum}", saveName);
        GameSaveData gameSaveData = JsonHelper.LoadJsonFile<GameSaveData>(st.ToString(), true);
        return gameSaveData;
    }

    public void DeleteSaveData(string saveName)
    {
        StringBuilder st = new StringBuilder().AppendFormat("{0}/Saves/{1}", Application.persistentDataPath, saveName);
        string path = st.ToString();

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    #region Player
    public void SavePlayer()
    {
        if (Lobby.Instance.SaveDataLoading) return;

        PlayerData player = new PlayerData();

        player.newGameCount = Lobby.Instance.newGameCount;

        string jsonData = JsonHelper.ObjectToJson(player);
        JsonHelper.CreateJsonFile(Application.persistentDataPath, "player", "Config", jsonData, false);
    }
    public PlayerData LoadPlayer()
    {
        StringBuilder st = new StringBuilder().AppendFormat("{0}/Config/player.json", Application.persistentDataPath);
        PlayerData player = JsonHelper.LoadJsonFile<PlayerData>(st.ToString(), false);
        return player;
    }
    #endregion

    #region Config
    public void SaveConfig()
    {
        if (Lobby.Instance.SaveDataLoading) return;

        ConfigData config = new ConfigData();

        config.lan = TextManager.Instance.language.ToString();

        var sm = SettingManager.Instance;
        config.volumeBGM = (int)sm.bgmSlider.value;
        config.volumeSFX = (int)sm.sfxSlider.value;

        config.fullScreen = sm.fullscreen;
        config.framerate = sm.framerateIdx;
        config.vsync = sm.vsync;
        config.resolution_width = sm.settingResolution.x;
        config.resolution_height = sm.settingResolution.y;

        string jsonData = JsonHelper.ObjectToJson(config);
        JsonHelper.CreateJsonFile(Application.persistentDataPath, "config", "Config", jsonData, false);
    }
    public ConfigData LoadConfig()
    {
        StringBuilder st = new StringBuilder().AppendFormat("{0}/Config/config.json", Application.persistentDataPath);
        ConfigData config = JsonHelper.LoadJsonFile<ConfigData>(st.ToString(), false);
        return config;
    }
    #endregion

    #region InGame UI
    public SaveSlotUI currentSaveSlot;
    public List<SaveSlotUI> saveSlots;
    public List<SaveFileUI> saveFiles;
    public TextMeshProUGUI saveCloseTMP;
    public TextMeshProUGUI saveDeleteTMP;
    public TextMeshProUGUI tutorialTMP;

    public float fadeTime = 1f;
    public CanvasGroup saveCanvasGroup;
    public RectTransform saveRectTransform;

    public GameObject[] saveUIs; // 0 슬롯 - 1 세이브 파일들
    public GameObject saveDeleteBtn;
    public Toggle tutorialToggle;

    public ScrollingUIEffect scrollEffect;

    public bool loading;
    public bool opened;

    public bool IsActive => loading || opened;
    private TextManager tm => TextManager.Instance;

    // 새 게임 세팅
    public EditorSettingLib settingLib;


    // 세이브 삭제
    public GameObject saveDeleteWarningObj;
    public TextMeshProUGUI[] saveDeleteWarningBtn_Text;
    public TextMeshProUGUI saveDeleteWarning_Text;
    public TextMeshProUGUI saveDeleteWarningDetail_Text;

    private void UpdateSaveTMP()
    {
        saveCloseTMP.text = tm.GetCommons("Close");
        saveDeleteTMP.text = tm.GetCommons("Delete");
        tutorialTMP.text = tm.GetCommons("Tutorial");
        saveDeleteWarningBtn_Text[0].text = tm.GetCommons("Delete");
        saveDeleteWarningBtn_Text[1].text = tm.GetCommons("Cancel");
        saveDeleteWarning_Text.text = tm.GetCommons("Warning");
        saveDeleteWarningDetail_Text.text = tm.GetCommons("DeleteConfirm");
    }

    private void UpdateCurrentSaveSlotUIs()
    {
        if (currentSaveSlot != null)
        {
            for (int i = 0; i < saveFiles.Count; i++)
            {
                saveFiles[i].gameObject.SetActive(false);
            }
            int max = Mathf.Min(saveFiles.Count, currentSaveSlot.saveIndexs.Count);
            for (int i = 0; i < max; i++)
            {
                saveFiles[i].gameObject.SetActive(true);
                saveFiles[i].saveFileTMP.text = string.Format(tm.GetCommons("Day"), currentSaveSlot.saveIndexs[i].index + 1);

                StringBuilder st = new StringBuilder();
                StringBuilder st2 = new StringBuilder();
                var date = currentSaveSlot.saveIndexs[i].date;
                var gm = currentSaveSlot.saveIndexs[i].gm;
                float rocketPrecent = (float)gm.rocket.currentStep / RocketManager.MaxStep;
                if (rocketPrecent <= 0f) rocketPrecent = 0.01f;
                st.AppendFormat("<sprite=8> {2:F0}% <sprite=2> {0} <sprite=1> {1}    ", gm.data.gold, gm.data.rating, rocketPrecent * 100f);
                st2.AppendFormat("{0}/{1:00}/{2:00} {3:00}:{4:00}", date.year, date.month, date.day, date.hour, date.minute);

                saveFiles[i].saveFileInfoTMP.text = st.ToString();
                saveFiles[i].saveFileDateTMP.text = st2.ToString();
            }
        }
    }
    public void SetCurrentSlot(SaveSlotUI slot)
    {
        if (slot.slotNum <= 0 || slot.slotNum > 3) return;

        if (slot.recentDays >= 0)
        {
            currentSaveSlot = slot;
            UpdateCurrentSaveSlotUIs();
            saveUIs[0].SetActive(false);
            tutorialToggle.gameObject.SetActive(false);
            saveUIs[1].SetActive(true);
            saveDeleteBtn.SetActive(true);
            UINaviHelper.Instance.SetFirstSelect();
        }
        else
        {
            Lobby.Instance.newGameCount++;
            SavePlayer();

            // 빈 슬롯 - 새 게임 시작
            Lobby.Instance.CloseAllPanel();
            Lobby.Instance.LobbyUISwitch(false);
            GameStartInfo gameStartInfo = new GameStartInfo
            {
                slotNum = slot.slotNum,
                saveName = string.Empty,
                tutorial = tutorialToggle.isOn,
            };
            LoadingSceneManager.Instance.LobbyStart(gameStartInfo, settingLib.sceneName);
        }
    }
    private void OpenSaveSlotMethod()
    {
        currentSaveSlot = null;
        UpdateSaveTMP();

        saveUIs[0].SetActive(true);
        tutorialToggle.gameObject.SetActive(true);
        saveUIs[1].SetActive(false);
        saveDeleteBtn.SetActive(false);

        for (int i = 0; i < saveSlots.Count; i++)
        {
            saveSlots[i].UpdateUI();
        }
    }

    public void OpenSaveSlots()
    {
        if (loading) return;

        if (opened)
        {
            HideSaveSlots();
            return;
        }

        scrollEffect.enabled = true;
        loading = true;

        // 창 열시 할 행동들
        Lobby.Instance.CloseAllPanel();
        OpenSaveSlotMethod();
        //UINaviHelper.Instance.inputHelper.GuidePadCheck();
        //

        saveCanvasGroup.alpha = 0f;
        saveRectTransform.transform.localPosition = new Vector3(0f, 1000f, 0f);
        saveRectTransform.DOAnchorPos(new Vector2(0f, 0f), fadeTime, false).SetEase(Ease.OutElastic).SetUpdate(true);
        saveCanvasGroup.DOFade(1f, fadeTime).SetUpdate(true).OnComplete(() =>
        {
            loading = false;
            opened = true;
            UINaviHelper.Instance.SetFirstSelect();
        });
    }
    public void HideSaveSlots()
    {
        if (!opened) return;
        if (loading) return;

        scrollEffect.enabled = false;
        opened = false;
        loading = true;

        // 창 닫을시 할 행동들
        ShowSaveDeleteWarning(false);
        //

        saveCanvasGroup.alpha = 1f;
        saveRectTransform.transform.localPosition = new Vector3(0f, 0f, 0f);
        saveRectTransform.DOAnchorPos(new Vector2(0f, -2000f), fadeTime, false).SetEase(Ease.InOutQuint).SetUpdate(true);
        saveCanvasGroup.DOFade(0f, fadeTime).SetUpdate(true).OnComplete(() =>
        {
            loading = false;
            UINaviHelper.Instance.SetFirstSelect();
        });
    }

    public void ZompizzaAutoSave(int day)
    {
        int slotNum = GM.Instance.slotNum;
        SaveWithDay(slotNum, day); // 좀피 슬롯식 세이브
    }

    public void ShowSaveDeleteWarning(bool on)
    {
        saveDeleteWarningObj.SetActive(on);
        UINaviHelper.Instance.SetFirstSelect();
    }
    public void SaveDelete()
    {
        ShowSaveDeleteWarning(false);

        if (currentSaveSlot == null) return;
        DeleteSaveData($"Slot {currentSaveSlot.slotNum}");
        OpenSaveSlotMethod();
        UINaviHelper.Instance.SetFirstSelect();
    }
#endregion
}

#region 게임 데이터
[Serializable]
public class GameSaveData
{
    public MetaSaveData meta;
    public GMSaveData gm;
    public ReviewTotalData reviews;
    public ResearchSaveData research;
    public AchievementData achieve;
    public VillagerData villager;
}

[Serializable]
public struct MetaSaveData
{
    public int version;
    public DateInfo dateInfo;
}
[Serializable]
public struct DateInfo : IComparable
{
    public int year;
    public int month;
    public int day;
    public int hour;
    public int minute;

    public int CompareTo(object obj)
    {
        var other = (DateInfo)obj;

        if (other.year > year)
            return 1;
        else if (other.year < year)
            return -1;
        else
        {
            if (other.month > month)
                return 1;
            else if (other.month < month)
                return -1;
            else
            {
                if (other.day > day)
                    return 1;
                else if (other.day < day)
                    return -1;
                else
                {
                    if (other.hour > hour)
                        return 1;
                    else if (other.hour < hour)
                        return -1;
                    else
                    {
                        if (other.minute > minute)
                            return 1;
                        else if (other.minute < minute)
                            return -1;
                        else
                            return 0;
                    }
                }
            }
        }
    }
}

[Serializable]
public struct GMSaveData
{
    public GM.SaveData data;
    public CustomDifficultyData customDifficulty;
    public TutorialData tutorial;
    public RocketManager.SaveData rocket;
    public StatManager.SaveData stat;
}
[Serializable]
public struct CustomDifficultyData
{
    public int resourceDensity;
    public int disasterIntensity;
    public int startPopulation;
    public int startResource;
}
[Serializable]
public struct TutorialData
{
    public int step;
}

[Serializable]
public struct ResearchSaveData
{
    public ResearchManager.SaveData data;
}
[Serializable]
public struct VillagerData
{
    public VillagerManager.SaveData data;
}

[Serializable]
public struct AchievementData
{
    public int removedNaturalObjects;
}

[Serializable]
public struct ReviewTotalData
{
    public List<ReviewData> data;
}
[Serializable]
public struct ReviewData
{
    public int day;
    public int customerIdx;
    public float time;
    public float hp;
    public int goal;
}
#endregion


#region 설정 데이터
[Serializable]
public class PlayerData
{
    public int newGameCount;
}

[Serializable]
public class ConfigData
{
    public string lan;

    public int volumeBGM = 10;
    public int volumeSFX = 10;

    public int fullScreen = 0;
    public int framerate = 0;
    public bool vsync = false;
    public int resolution_width = 1920;
    public int resolution_height = 1080;
}
#endregion