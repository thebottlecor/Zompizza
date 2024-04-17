using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using UnityEngine.Networking;
using UnityEditor;

[CreateAssetMenu(fileName = "EditorLibrary", menuName = "Library/Editor")]
public class EditorLibrary : ScriptableObject
{

    private readonly string key = "16pXeNndcsfn7cLcezK_ZD_yeoKqB-rrjp6SZKh6SshY";
    private readonly string[] sheetName = new string[] { "recipe", "research", "common", "keymap", "keycode", "trader", "name1" };

    private string Get_URL(int idx)
    {
        return $"https://docs.google.com/spreadsheets/d/{key}/gviz/tq?tqx=out:csv&sheet={sheetName[idx]}";
    }

    public void LoadAllData()
    {
        for (int i = 0; i < sheetName.Length; i++)
        {
            EditorCoroutineUtility.StartCoroutine(DataUpdate(i), this);
        }
    }

    IEnumerator DataUpdate(int idx)
    {
        string url = Get_URL(idx);
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;

        CSVReader.WriteCSV(data, Application.streamingAssetsPath + $"/TextManager - {sheetName[idx]}.csv");

        Debug.Log($"TextManager - {sheetName[idx]}.csv download!");

        AssetDatabase.Refresh();
    }
}
