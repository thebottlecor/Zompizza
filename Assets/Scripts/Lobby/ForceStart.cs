using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ForceStart : MonoBehaviour
{

    public EditorSettingLib editorSettingLib;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void FirstLoad()
    {
#if UNITY_EDITOR

        EditorSettingLib editorSettingLib = (EditorSettingLib)AssetDatabase.LoadAssetAtPath("Assets/Prefabs_Lib/00 EditorSettingLib.asset", typeof(EditorSettingLib));

        if (editorSettingLib.forceStart)
        {
            string sceneName = SceneManager.GetActiveScene().name;

            if (!sceneName.Equals("lobby"))
            {
                SceneManager.LoadScene("editor_lobby");
            }
        }
#endif
    }


    private void Start()
    {
        Lobby.Instance.LobbyUISwitch(false);

        GameStartInfo gameStartInfo = new GameStartInfo
        {
            pizzeriaName = "Zompizza",
            slotNum = 1,
            saveName = string.Empty,
            tutorial = true,
        };
        Debug.Log("·ÎµùµÇ´Â ¾À : " + editorSettingLib.sceneName);
        LoadingSceneManager.Instance.LobbyStart(gameStartInfo, editorSettingLib.sceneName);
    }
}
