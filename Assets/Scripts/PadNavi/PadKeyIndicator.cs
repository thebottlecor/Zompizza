using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PadKeyIndicator : MonoBehaviour
{
    public TextMeshProUGUI tmp;
    public Image icon;

    public PadKeyCode padKey;

    public void UIUpdate(int padType)
    {
        if (tmp != null)
        {
            var tm = TextManager.Instance;
            switch (padKey)
            {
                case PadKeyCode.B_Up:
                    tmp.text = tm.GetCommons("Menu");
                    break;
                case PadKeyCode.B_Down:
                    tmp.text = tm.GetKeyMaps(KeyMap.enterStore);
                    break;
                case PadKeyCode.B_Left:
                    tmp.text = tm.GetKeyMaps(KeyMap.worldMap);
                    break;
                case PadKeyCode.B_Right:
                    tmp.text = tm.GetKeyMaps(KeyMap.carBreak);
                    break;
                case PadKeyCode.L2:
                    tmp.text = tm.GetKeyMaps(KeyMap.carBackward);
                    break;
                case PadKeyCode.R2:
                    tmp.text = tm.GetKeyMaps(KeyMap.carForward);
                    break;
                case PadKeyCode.LeftStick:
                    tmp.text = $"{tm.GetKeyMaps(KeyMap.carLeft)} / {tm.GetKeyMaps(KeyMap.carRight)}";
                    break;
                default:
                    tmp.text = string.Empty;
                    break;
            }
        }

        var data = DataManager.Instance;

        if (padKey != PadKeyCode.None)
        {
            icon.sprite = padType switch
            {
                0 => data.uiLib.padKeyUIsPS[padKey],
                _ => data.uiLib.padKeyUIsXbox[padKey],
            };
            icon.gameObject.SetActive(true);
        }
        else
        {
            icon.gameObject.SetActive(false);
        }
    }
}
