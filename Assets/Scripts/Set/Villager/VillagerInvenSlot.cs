using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VillagerInvenSlot : MonoBehaviour
{

    public int idx;

    public Image icon;
    public TextMeshProUGUI stackTMP;

    public void Init(int idx)
    {
        this.idx = idx;
        icon.sprite = DataManager.Instance.uiLib.villagerItems[idx];
        UpdateStack();
    }

    public void UpdateStack()
    {
        stackTMP.text = $"x{VillagerManager.Instance.inventory[idx]}";
    }

}
