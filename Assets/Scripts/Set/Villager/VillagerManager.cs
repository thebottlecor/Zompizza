using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerManager : Singleton<VillagerManager>
{

    public VillagerSearcher villagerSearcher;
    public VillagerWay[] villagers;

    public int[] inventory;
    public VillagerInvenSlot[] invenSlots;

    private void Start()
    {
        inventory = new int[10];
         
        // µð¹ö±ë
        for (int i = 0; i < inventory.Length; i++)
        {
            inventory[i] = UnityEngine.Random.Range(1, 5);
        }
        inventory[0] = 0;

        for (int i = 0; i < invenSlots.Length; i++)
        {
            invenSlots[i].Init(i);
        }
    }

    public void NextDay()
    {
        for (int i = 0; i < villagers.Length; i++)
        {
            villagers[i].CalcNeedsMet();
        }
    }

    public bool Give(int currentVillager)
    {
        if (currentVillager < 0) return false;

        int currentNeeds = villagers[currentVillager].currentNeeds;

        if (currentNeeds < 0) return false;

        if (GiveItem(currentVillager, villagers[currentVillager].currentNeeds))
        {
            // ±â»Ý ¿¬Ãâ
            AudioManager.Instance.PlaySFX_Villager(0, villagers[currentVillager].gender);
            AudioManager.Instance.PlaySFX(Sfx.pizzaComplete);
            return true;
        }
        else
        {
            AudioManager.Instance.PlaySFX(Sfx.deny);
            return false;
        }
    }

    private bool GiveItem(int villagerIdx, int itemIdx)
    {
        if (inventory[itemIdx] <= 0) return false;

        inventory[itemIdx] -= 1;

        villagers[villagerIdx].AddCondition(1);
        //villagers[villagerIdx].AddExp(0.05f);
        villagers[villagerIdx].SetNeeds(-1);

        UpdateSlots();

        return true;
    }
    public void UpdateSlots()
    {
        for (int i = 0; i < invenSlots.Length; i++)
        {
            invenSlots[i].UpdateStack();
        }
    }

    public void SetMidNight(bool on)
    {
        for (int i = 0; i < villagers.Length; i++)
        {
            villagers[i].MidNight(on);
        }
    }

    public void GetIncome()
    {
        int villager_income = 0;
        for (int i = 0; i < villagers.Length; i++)
        {
            if (villagers[i].recruited)
            {
                villager_income += villagers[i].Income();
            }
        }
        GM.Instance.AddGold(villager_income, GM.GetGoldSource.villager);
    }
}
