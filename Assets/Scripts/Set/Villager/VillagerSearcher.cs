using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class VillagerSearcher : MonoBehaviour
{

    public GameObject talkObj;
    public GameObject[] tmpObj;
    public TextMeshProUGUI[] talkTMP;

    public HashSet<VillagerWay> villagers;

    private void Start()
    {
        villagers = new HashSet<VillagerWay>();

        talkTMP[0].text = TextManager.Instance.GetCommons("Talk") + $"({SettingManager.Instance.keyMappings[KeyMap.enterStore].GetName()})";
        talkTMP[1].text = TextManager.Instance.GetCommons("Talk");
    }

    public int GetVillagerIdx()
    {
        var list = VillagerManager.Instance.villagers;
        foreach (var temp in villagers)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == temp)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private void Update()
    {
        if (talkObj.activeSelf)
        {
            var pad = Gamepad.current;
            if (pad != null)
            {
                if (tmpObj[0].activeSelf)
                    tmpObj[0].SetActive(false);
                if (!tmpObj[1].activeSelf)
                    tmpObj[1].SetActive(true);
            }
            else
            {
                if (!tmpObj[0].activeSelf)
                    tmpObj[0].SetActive(true);
                if (tmpObj[1].activeSelf)
                    tmpObj[1].SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 16)
        {
            VillagerWay villager = other.GetComponent<VillagerWay>();
            if (villager != null)
            {
                if (!villagers.Contains(villager))
                {
                    villagers.Add(villager);
                    villager.interactionObj.SetActive(true);

                    talkObj.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 16)
        {
            VillagerWay villager = other.GetComponent<VillagerWay>();
            if (villager != null)
            {
                if (villagers.Contains(villager))
                {
                    villagers.Remove(villager);
                    villager.interactionObj.SetActive(false);

                    talkObj.SetActive(false);
                }
            }
        }
    }

    public void Clear()
    {
        talkObj.SetActive(false);
        foreach (var temp in villagers)
        {
            temp.interactionObj.SetActive(false);
        }
        villagers.Clear();
    }
}
