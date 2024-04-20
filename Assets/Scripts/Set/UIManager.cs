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

    private SerializableDictionary<KeyMap, KeyMapping> HotKey => SettingManager.Instance.keyMappings;

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

    private void Update()
    {
        if (HotKey[KeyMap.escape].GetkeyDown())
        {
            if (utilUI.IsActive)
            {
                utilUI.HideUI();
            }
            else if (shopUI.IsActive)
            {
                shopUI.HideUI();
            }
            else
            {
                utilUI.OpenSettings();
            }
        }
        else if (HotKey[KeyMap.worldMap].GetkeyDown())
        {
            if (!shopUI.IsActive && !utilUI.IsActive)
            {
                utilUI.OpenWorldMap();
            }
        }
    }
}
