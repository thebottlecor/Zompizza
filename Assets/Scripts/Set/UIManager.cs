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

    public RectTransform movingSettingsPanelParent;

    private SerializableDictionary<KeyMap, KeyMapping> HotKey => SettingManager.Instance.keyMappings;

    private void Start()
    {
        var panel = SettingManager.Instance.ingameMovingPanel;
        panel.SetParent(movingSettingsPanelParent);
        panel.gameObject.SetActive(true);

        panel.offsetMin = new Vector2(0f, 0f);
        panel.offsetMax = new Vector2(0f, 0f);

        shopUI.UpdateTexts();
        utilUI.UpdateTexts();
    }

    public void ButtonSound()
    {
        AudioManager.Instance.PlaySFX(Sfx.buttons);
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
