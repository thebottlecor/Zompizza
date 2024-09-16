using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System;
using System.IO;
using TMPro;
using UnityEngine.UI;
using System.Linq;

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

    public readonly int version = 53;


    public override void CallAfterAwake()
    {

    }
    public override void CallAfterStart(ConfigData config)
    {
        InitSaveUI();
    }

    public void Save(string saveName)
    {
        GameSaveData gameSaveData = new GameSaveData();

        gameSaveData.meta = new MetaSaveData {
            version = this.version,
            dateInfo = new DateInfo { year = DateTime.Now.Year, month = DateTime.Now.Month, day = DateTime.Now.Day, hour = DateTime.Now.Hour, minute = DateTime.Now.Minute },
        };

        gameSaveData.gm = new GMSaveData
        {
            data = GM.Instance.Save(),
        };

        //

        string jsonData = JsonHelper.ObjectToJson(gameSaveData);
        JsonHelper.CreateJsonFile(Application.persistentDataPath, "save", $"Saves/{saveName}", jsonData, false);
    }

    private IEnumerator SaveCoroutine2(string saveName)
    {
        GameSaveData gameSaveData = new GameSaveData();

        gameSaveData.meta = new MetaSaveData
        {
            version = this.version,
            dateInfo = new DateInfo { year = DateTime.Now.Year, month = DateTime.Now.Month, day = DateTime.Now.Day, hour = DateTime.Now.Hour, minute = DateTime.Now.Minute },
        };

        yield return null;

        string jsonData = JsonHelper.ObjectToJson(gameSaveData);
        JsonHelper.CreateJsonFile(Application.persistentDataPath, "save", $"Saves/{saveName}", jsonData, false);
    }


    private IEnumerator SaveCoroutine(string saveName)
    {
        GameSaveData gameSaveData = new GameSaveData();

        gameSaveData.meta = new MetaSaveData
        {
            version = this.version,
            dateInfo = new DateInfo { year = DateTime.Now.Year, month = DateTime.Now.Month, day = DateTime.Now.Day, hour = DateTime.Now.Hour, minute = DateTime.Now.Minute },
        };

        yield return null;

        string jsonData = JsonHelper.ObjectToJson(gameSaveData);
        JsonHelper.CreateJsonFile(Application.persistentDataPath, "save", $"Saves/{saveName}", jsonData, false);
    }

    public GameSaveData LoadSaveData(string saveName)
    {
        StringBuilder st = new StringBuilder().AppendFormat("{0}/Saves/{1}/save.json", Application.persistentDataPath, saveName);
        GameSaveData gameSaveData = JsonHelper.LoadJsonFile<GameSaveData>(st.ToString(), false);
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

        config.language = TextManager.Instance.language.ToString();

        var sm = SettingManager.Instance;
        config.volumeBGM = (int)sm.bgmSlider.value;
        config.volumeSFX = (int)sm.sfxSlider.value;

        config.fullScreen = sm.fullscreen;
        config.framerate = sm.framerateIdx;
        config.vsync = sm.vsync;
        config.resolution_width = sm.settingResolution.x;
        config.resolution_height = sm.settingResolution.y;

        config.cameraSpeed = sm.cameraSpeed;
        config.invertZoom = sm.invertZoom;
        config.edgeScrolling = sm.edgeScrolling;
        config.autosave = sm.autosave;

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
    public Canvas saveCanvas;
    public ConfirmPanel manualSave_confirmPanel;
    public GameObject saveListObject_source;
    public GameObject newSaveObject_source;
    public RectTransform saveListObject_parent;
    private List<SaveListObject> saveListObjects;

    public bool SavePanel_LoadMode { get; private set; }
    private NewSaveObject newSaveButton;

    private void InitSaveUI()
    {
        saveListObjects = new List<SaveListObject>();
        newSaveButton = Instantiate(newSaveObject_source, saveListObject_parent).GetComponent<NewSaveObject>();
        newSaveButton.Init(() => { NewSave(); });

        string path = string.Format("{0}/Saves", Application.persistentDataPath);

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        var allDirectories = Directory.GetDirectories(path);
        for (int i = 0; i < allDirectories.Length; i++)
        {
            string saveFileName = string.Empty;

            // 리눅스 경로 조심
#if UNITY_STANDALONE_LINUX
            var temp = allDirectories[i].Split("/");
            saveFileName = temp[temp.Length - 1];
#else
            var temp = allDirectories[i].Split("\\");
            saveFileName = temp[temp.Length - 1];
#endif
            var data = LoadSaveData(saveFileName);
            if (data != null)
            {
                var obj = Instantiate(saveListObject_source, saveListObject_parent).GetComponent<SaveListObject>();
                var dateTime = new DateTime(data.meta.dateInfo.year, data.meta.dateInfo.month, data.meta.dateInfo.day, data.meta.dateInfo.hour, data.meta.dateInfo.minute, 0);
                if (saveFileName.Equals("Autosave"))
                    obj.Init(saveFileName, dateTime, () => {  });
                else
                    obj.Init(saveFileName, dateTime, () => { OverwriteSave(saveFileName); });
                saveListObjects.Add(obj);
            }
        }
        SaveListSort();
    }

    private void SaveListSort()
    {
        saveListObjects = saveListObjects.OrderByDescending(x => x.SaveDate).ToList();
        int autosaveIdx = -1;
        for (int i = 0; i < saveListObjects.Count; i++)
        {
            if (saveListObjects[i].SaveName.Equals("Autosave")) autosaveIdx = i;
            saveListObjects[i].transform.SetSiblingIndex(i + 1);
        }
        if (autosaveIdx >= 0)
            saveListObjects[autosaveIdx].transform.SetSiblingIndex(1);
    }

    public void OpenSaveUI(bool loadMode)
    {
        //Lobby.Instance.CloseAllPanel();
        SavePanel_LoadMode = loadMode;
        newSaveButton.gameObject.SetActive(!loadMode);
        UpdateTexts();
        saveCanvas.gameObject.SetActive(true);
    }

    public void HideSaveUI()
    {
        saveCanvas.gameObject.SetActive(false);
    }

    public void ZompizzaAutoSave()
    {
        // 테스트

        //string slotName = "Slot 1";
        //Debug.Log("Save Slot 1");
        //Save(slotName);
    }

    public void NewSave()
    {
        //var gm = GM.Instance;
        //string uniqueIdx = $"{version}{gm.mapSeed}{gm.cityName}{DateTime.Now.Day}{gm.year}{DateTime.Now.Hour}{gm.totalStorage}{DateTime.Now.Minute}{gm.totalRes}{DateTime.Now.Second}";

        string uniqueIdx = DateTime.Now.ToString();

        uniqueIdx = JsonHelper.SaveFileNameEncrypt(uniqueIdx);
        Save(uniqueIdx);

        var obj = Instantiate(saveListObject_source, saveListObject_parent).GetComponent<SaveListObject>();
        obj.Init(uniqueIdx, DateTime.Now, () => { OverwriteSave(uniqueIdx); });
        saveListObjects.Add(obj);
        SaveListSort();
    }

    public IEnumerator AutoSave()
    {
        string uniqueIdx = "Autosave";
        yield return SaveCoroutine2(uniqueIdx);

        bool hasAutosave = false;
        for (int i = 0; i < saveListObjects.Count; i++)
        {
            if (saveListObjects[i].SaveName == uniqueIdx)
            {
                saveListObjects[i].Init(uniqueIdx, DateTime.Now, () => {  });
                hasAutosave = true;
                break;
            }
        }
        if (!hasAutosave)
        {
            var obj = Instantiate(saveListObject_source, saveListObject_parent).GetComponent<SaveListObject>();
            obj.Init(uniqueIdx, DateTime.Now, () => {  });
            saveListObjects.Add(obj);
        }
        SaveListSort();
    }

    public void OverwriteSave(string existOtherSaveName)
    {
        manualSave_confirmPanel.Init(TextManager.Instance.GetCommons("SaveConfirm"), TextManager.Instance.GetCommons("Accept"), () =>
        {
            Save(existOtherSaveName);
            for (int i = 0; i < saveListObjects.Count; i++)
            {
                if (saveListObjects[i].SaveName == existOtherSaveName)
                {
                    saveListObjects[i].Init(existOtherSaveName, DateTime.Now, () => { OverwriteSave(existOtherSaveName); });
                    break;
                }
            }
            SaveListSort();
        });
    }

    public void DeleteSaveUI(SaveListObject saveListObject)
    {
        manualSave_confirmPanel.Init(TextManager.Instance.GetCommons("DeleteConfirm"), TextManager.Instance.GetCommons("Accept"), () =>
        {
            DeleteSaveData(saveListObject.SaveName);
            for (int i = saveListObjects.Count - 1; i >= 0; i--)
            {
                if (saveListObjects[i] == saveListObject)
                {
                    saveListObjects.RemoveAt(i);
                    break;
                }
            }
            Destroy(saveListObject.gameObject);
            SaveListSort();
        });
    }

    [SerializeField] private TextMeshProUGUI savePanelTMP;

    public void UpdateTexts()
    {
        var tm = TextManager.Instance;

        if (SavePanel_LoadMode)
            savePanelTMP.text = tm.GetCommons("Load");
        else
            savePanelTMP.text = tm.GetCommons("Save");

        if (newSaveButton != null)
            newSaveButton.UIUpdate();
    }
#endregion
}

#region 게임 데이터
[Serializable]
public class GameSaveData
{
    public MetaSaveData meta;
    public GMSaveData gm;
    public ResearchSaveData research;
    public AchievementData achieve;
}

[Serializable]
public struct MetaSaveData
{
    public int version;
    public DateInfo dateInfo;
}
[Serializable]
public struct DateInfo
{
    public int year;
    public int month;
    public int day;
    public int hour;
    public int minute;
}

[Serializable]
public struct GMSaveData
{
    public GM.SaveData data;
    public CustomDifficultyData customDifficulty;
    public TutorialData tutorial;
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
    //public ResearchManager.SaveData data;
}

[Serializable]
public struct AchievementData
{
    public int removedNaturalObjects;
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
    public string language;

    public int volumeBGM = 10;
    public int volumeSFX = 10;

    public int fullScreen = 0;
    public int framerate = 0;
    public bool vsync = false;
    public int resolution_width = 1920;
    public int resolution_height = 1080;

    public float cameraSpeed = 1f;
    public bool invertZoom = false;
    public bool edgeScrolling = true;
    public bool autosave = true;
}
#endregion