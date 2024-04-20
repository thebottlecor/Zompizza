using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{

    public ShopUI shopUI;
    public UtilUI utilUI;


    public Button toLobbyButton;

    public void Start()
    {
        toLobbyButton.onClick.AddListener(() =>
        {
            LoadingSceneManager.Instance.ToLobby();
        });
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
