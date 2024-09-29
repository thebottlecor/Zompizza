using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveFileUI : MonoBehaviour
{

    public int index;

    public TextMeshProUGUI saveFileTMP;
    public TextMeshProUGUI saveFileInfoTMP;
    public TextMeshProUGUI saveFileDateTMP;


    public void LoadSaveFile()
    {
        var slot = SaveManager.Instance.currentSaveSlot;

        if (slot == null) return;

        int slotNum = slot.slotNum;

        if (slotNum <= 0 || slotNum > 3) return;

        int dayIndex = slot.saveIndexs[index].index;

        if (dayIndex < 0 || dayIndex > 30) return;

        string saveName = $"save{dayIndex}";

        if (!JsonHelper.CheckFile(Application.persistentDataPath, saveName, $"Saves/Slot {slotNum}")) return;

        Debug.Log("Load :: " + saveName);

        Lobby.Instance.CloseAllPanel();
        Lobby.Instance.LobbyUISwitch(false);
        GameStartInfo gameStartInfo = new GameStartInfo
        {
            slotNum = slot.slotNum,
            saveName = saveName,
        };
        LoadingSceneManager.Instance.LobbyStart(gameStartInfo, SaveManager.Instance.settingLib.sceneName);
    }
}
