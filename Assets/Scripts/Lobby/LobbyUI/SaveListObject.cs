using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveListObject : MonoBehaviour
{

    public Image image;
    public TextMeshProUGUI tmp;
    public Button button;

    public GameObject autosaveMark;

    public string SaveName { get; private set; }
    public DateTime SaveDate { get; private set; }

    public void Init(string saveName, DateTime saveDate, Action action)
    {
        this.SaveName = saveName;
        this.SaveDate = saveDate;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (SaveManager.Instance.SavePanel_LoadMode)
                Load();
            else
                action();

        });

        UIUpdate();
    }

    public void UIUpdate()
    {
        if (SaveName.Equals(string.Empty)) return;

        autosaveMark.SetActive(SaveName.Equals("Autosave"));

        var saveData = SaveManager.Instance.LoadSaveData(SaveName);
        if (saveData != null)
        {
            var tm = TextManager.Instance;
            StringBuilder st = new StringBuilder();
            st.AppendFormat("{0}/{1:00}/{2:00} {3:00}:{4:00}", saveData.meta.dateInfo.year, saveData.meta.dateInfo.month, saveData.meta.dateInfo.day, saveData.meta.dateInfo.hour, saveData.meta.dateInfo.minute);
            tmp.text = st.ToString();
        }
        else
        {
            tmp.text = string.Empty;
            
        }
    }

    public void Load() // Load Panel에서 클릭시
    {
        if (SaveName.Equals(string.Empty)) return;

        Lobby.Instance.LoadGameStart(SaveName);
    }

    public void Overwrite() // Save Panel에서 클릭시
    {
        if (SaveName.Equals(string.Empty)) return;

        SaveManager.Instance.OverwriteSave(SaveName);
    }

    public void Delete() // Trash Icon 클릭시
    {
        if (SaveName.Equals(string.Empty)) return;

        SaveManager.Instance.DeleteSaveUI(this);
    }
}
