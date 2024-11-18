using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class SaveSlotUI : MonoBehaviour
{

    public int slotNum;
    public TextMeshProUGUI slotTMP;
    public TextMeshProUGUI hardModeTMP;
    public TextMeshProUGUI slotInfoTMP;

    private List<string> saveNames;
    public int recentDays;
    public bool hardMode;
    public List<SaveIndexInfo> saveIndexs;

    public struct SaveIndexInfo
    {
        public int index;
        public DateInfo date;
        public GMSaveData gm;
    }

    public void SelectSlot()
    {
        SaveManager.Instance.SetCurrentSlot(this);
    }

    public void UpdateUI()
    {
        GetRecentSave();

        var tm = TextManager.Instance;

        slotTMP.text = $"{tm.GetCommons("Slot")} {slotNum}";
        if (recentDays >= 0)
        {
            if (hardMode)
            {
                hardModeTMP.text = tm.GetCommons("HardMode");
                hardModeTMP.gameObject.SetActive(true);
            }
            else
                hardModeTMP.gameObject.SetActive(false);

            slotInfoTMP.text = string.Format(tm.GetCommons("Day"), recentDays + 1);
        }
        else
        {
            slotInfoTMP.text = tm.GetCommons("Empty");
            hardModeTMP.gameObject.SetActive(false);
        }
    }

    private void GetRecentSave()
    {
        saveNames = new List<string>();
        recentDays = -1;
        hardMode = false;
        saveIndexs = new List<SaveIndexInfo>();

        JsonHelper.GetJsonFileNames(Application.persistentDataPath, $"Saves/Slot {slotNum}", ref saveNames);

        if (saveNames == null || saveNames.Count == 0) return;

        var sortedWords = from word in saveNames
                          orderby word descending
                          select word;

        foreach (var temp in sortedWords)
        {
            string result = Regex.Replace(temp, @"\D", ""); // 정규표현식 [^0-9]는 0에서 9사이의 숫자가 아닌 문자를 의미하며, replacement는 빈문자열이므로, 숫자가 아닌 문자열을 모두 제거

            int index = -1;
            if (int.TryParse(result, out index))
            {
                // 불러오기 성공
                var saveData = SaveManager.Instance.LoadSaveData(slotNum, $"save{index}");
                saveIndexs.Add(new SaveIndexInfo { index = index, date = saveData.meta.dateInfo, gm = saveData.gm });
            }
            else
            {
                Debug.LogWarning("Incorrect Save Format");
            }
        }

        if (saveIndexs == null || saveIndexs.Count == 0) return;

        saveIndexs = saveIndexs.OrderBy(x => x.date).ToList();
        hardMode = saveIndexs[0].gm.data.hardMode;
        recentDays = saveIndexs[0].index;
    }
}
