using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyObject : MonoBehaviour
{
    public TextMeshProUGUI keyFunctionName;
    public TextMeshProUGUI currentKeyShow;
    public Button button;
    public GameObject highlight;

    public KeyObjectNew newInput;

    private KeyMap keyMap;

    public void Init(KeyMap _keyMap)
    {
        keyMap = _keyMap;
        button.onClick.AddListener(() => { ActiveThisKey(); });
        UpdateName();
    }

    private void ActiveThisKey()
    {
        SettingManager.Instance.currentActiveKeyNum = keyMap;
        //AudioManager.Instance.PlaySFX(Sfx.inputFieldStart);
        highlight.SetActive(true);
    }

    public void UpdateName()
    {
        keyFunctionName.text = TextManager.Instance.GetKeyMaps(keyMap);

        string name = SettingManager.Instance.keyMappings[keyMap].GetName();
        currentKeyShow.text = name;
        highlight.SetActive(false);
    }
}
