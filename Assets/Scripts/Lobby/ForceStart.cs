using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ForceStart : MonoBehaviour
{

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
        GameStartInfo gameStartInfo = new GameStartInfo
        {
            cityName = "Pixel Colony",
            mapSeed = 0,
            saveName = string.Empty,

            tutorial = false,
        };
        LoadingSceneManager.Instance.LobbyStart(gameStartInfo);
    }
}
