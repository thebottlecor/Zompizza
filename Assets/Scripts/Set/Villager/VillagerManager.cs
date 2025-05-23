using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MTAssets.EasyMinimapSystem;
using TMPro;
using UnityEngine.UI;
using System;

public class VillagerManager : Singleton<VillagerManager>
{
    [Serializable]
    public struct SaveData
    {
        public int[] inventory;
        public List<VillagerWay.SaveData> villagers;
    }

    public SaveData Save()
    {
        List<VillagerWay.SaveData> villagerData = new List<VillagerWay.SaveData>();
        for (int i = 0; i < villagers.Length; i++)
        {
            villagerData.Add(villagers[i].Save());
        }

        SaveData data = new SaveData
        {
            inventory = this.inventory,
            villagers = villagerData,
        };
        return data;
    }

    public void Load(SaveData data)
    {
        if (data.villagers != null)
        {
            int max = Mathf.Min(data.villagers.Count, villagers.Length);
            for (int i = 0; i < max; i++)
            {
                villagers[i].Load(data.villagers[i]);
            }
        }
        if (data.inventory != null)
        {
            if (data.inventory.Length < inventory.Length)
            {
                for (int i = 0; i < data.inventory.Length; i++)
                {
                    inventory[i] = data.inventory[i];
                }
            }
            else
                inventory = data.inventory;
        }
        UpdateSlots();
    }

    public VillagerSearcher villagerSearcher;
    public VillagerWay[] villagers;
    public HelpGoal[] helpGoals;
    public VillagerManagerObj[] villagerManagerObjs;

    public int[] inventory;
    public VillagerInvenSlot[] invenSlots;


    public int currentSosIdx = -1;
    public float sosTimeLimit;
    public float sosTimer;
    public VillagerSosMiniUI miniUI;

    public bool sosShowed; // 다음날 초기화
    public bool incomeReceived; // 다음날 초기화

    public MinimapRenderer minimap;
    public MinimapRenderer worldmap;

    [Header("UI")]
    public TextMeshProUGUI villagersText;
    public ScrollRect villagerScroll;



    public void Init(bool saveLoad, GameSaveData data)
    {
        inventory = new int[10];
         
        //// 디버깅
        //for (int i = 0; i < inventory.Length; i++)
        //{
        //    inventory[i] = UnityEngine.Random.Range(1, 5);
        //}
        //inventory[0] = 0;

        for (int i = 0; i < invenSlots.Length; i++)
        {
            invenSlots[i].Init(i);
        }

        for (int i = 0; i < helpGoals.Length; i++)
        {
            minimap.minimapItemsToHightlight.Add(helpGoals[i].minimapItem);
            worldmap.minimapItemsToHightlight.Add(helpGoals[i].minimapItem);
        }

        for (int i = 0; i < villagerManagerObjs.Length; i++)
        {
            villagerManagerObjs[i].Init(i);
        }

        ResetStat();

        for (int i = 0; i < villagers.Length; i++)
        {
            villagers[i].Init();
        }

        if (saveLoad)
        {
            Load(data.villager.data);
        }
    }

    private void Update()
    {
        if (currentSosIdx >= 0)
        {
            sosTimer += Time.deltaTime;
            miniUI.UpdateTimer();
        }
    }

    public void NextDay()
    {
        if (currentSosIdx >= 0)
        {
            var villager = villagers[currentSosIdx];
            villager.Expel();
            AudioManager.Instance.PlaySFX_Villager(2, villager.gender);
            helpGoals[currentSosIdx].Hide();
            miniUI.Hide();
        }
    }

    public void NextDay_Late()
    {
        for (int i = 0; i < villagers.Length; i++)
        {
            villagers[i].CalcNeedsMet();
        }
        for (int i = 0; i < helpGoals.Length; i++)
        {
            helpGoals[i].Hide();
        }
        ResetStat();
    }

    private void ResetStat()
    {
        sosShowed = false;
        incomeReceived = false;
        currentSosIdx = -1;
        miniUI.Hide();
    }

    public bool Give(int currentVillager)
    {
        if (currentVillager < 0) return false;

        int currentNeeds = villagers[currentVillager].currentNeeds;

        if (currentNeeds < 0) return false;

        if (GiveItem(currentVillager, villagers[currentVillager].currentNeeds))
        {
            // 기쁨 연출
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

    public int RandomItemGet()
    {
        int rand = UnityEngine.Random.Range(0, inventory.Length);
        inventory[rand] += 1;
        UpdateSlots();
        return rand;
    }
    public int ItemGet(int idx)
    {
        inventory[idx] += 1;
        UpdateSlots();
        return idx;
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
        if (on)
        {
            int chatMax = 24 + 1;
            HashSet<int> chatHashset = new HashSet<int>();
            for (int i = 0; i < villagers.Length; i++)
            {
                if (villagers[i].recruited && !villagers[i].expelled)
                {
                    int chatIdx;
                    while (true)
                    {
                        int randChat = UnityEngine.Random.Range(0, chatMax);
                        if (!chatHashset.Contains(randChat))
                        {
                            chatHashset.Add(randChat);
                            chatIdx = randChat;
                            break;
                        }
                    }
                    villagers[i].chatIdx = chatIdx;
                }
            }
        }
    }

    public void GetIncome()
    {
        if (incomeReceived) return;

        incomeReceived = true;

        int villager_income = 0;
        for (int i = 0; i < villagers.Length; i++)
        {
            if (villagers[i].recruited)
            {
                villager_income += villagers[i].Income();
            }
        }

        if (villager_income > 0)
        {
            AudioManager.Instance.PlaySFX(Sfx.money);
        }
        GM.Instance.AddGold(villager_income, GM.GetGoldSource.villager);
    }

    public int GetRemainVillagerCount()
    {
        int value = 0;

        for (int i= 0; i < villagers.Length; i++)
        {
            if (villagers[i].recruited || villagers[i].expelled)
            {
                value++;
            }
        }
        value = villagers.Length - value;
        return value;
    }
    public int GetRecruitedVillagerCount()
    {
        int value = 0;
        for (int i = 0; i < villagers.Length; i++)
        {
            if (villagers[i].recruited && !villagers[i].expelled)
            {
                value++;
            }
        }
        return value;
    }
    public int RandomSosVillager()
    {
        List<VillagerWay> randList = new List<VillagerWay>();

        for (int i = 0; i < villagers.Length; i++)
        {
            if (!villagers[i].recruited && !villagers[i].expelled)
            {
                randList.Add(villagers[i]);
            }
        }
        randList.Shuffle();

        if (randList.Count > 0)
            return randList[0].idx;
        else
            return -1;
    }

    public bool CreateSOS(bool fromShop = true)
    {
        // 복귀 후, 아무 주문이 없을 떄
        // 강제로 가게를 닫았을 떄

        int day = GM.Instance.day; // 짝수날 마다 => 4의 배수마다 (09.11) SOS

        bool sosDay = day == 3 || day == 7 || day == 10 || day == 15 || day == 19 || day == 22 || day == 27;

        if (sosDay && !sosShowed) // 짝수날 마다 => 4의 배수마다 (09.11)
        {
            int recruited = GetRecruitedVillagerCount();
            int remained = GetRemainVillagerCount();

            if (recruited < Constant.maxVillager && remained > 0)
            {
                int who = RandomSosVillager();
                if (who >= 0)
                {
                    sosShowed = true;

                    AudioManager.Instance.PlaySFX_Villager(1, villagers[who].gender);

                    if (fromShop)
                        UIManager.Instance.shopUI.ShowSosWarning(true);
                    else
                    {
                        UIManager.Instance.utilUI.ShowSosWarning(true);
                    }
                    //float dist = (helpGoals[who].transform.position - OrderManager.Instance.pizzeria.position).magnitude;
                    float dist = (helpGoals[who].transform.position - GM.Instance.player.transform.position).magnitude;

                    float km = dist * Constant.distanceScale; // 게임상 거리 200 = 1km
                    sosTimer = 0f;
                    sosTimeLimit = Constant.sos_timeLimit_1km * km + Constant.delivery_timeLimit_base * 1.5f;

                    currentSosIdx = who;
                    miniUI.Init(who);
                    helpGoals[who].Show();

                    return true;
                }
            }
        }
        return false;
    }

    public void Rescue(int idx)
    {
        // 주민 구출
        var villager = villagers[idx];
        AudioManager.Instance.PlaySFX_Villager(0, villager.gender);
        AudioManager.Instance.PlaySFX(Sfx.complete);
        villager.Recruit();

        currentSosIdx = -1;

        //miniUI.Hide();
        miniUI.RescuedMode();

        int count = GetRecruitedVillagerCount();
        if (SteamHelper.Instance != null && count >= Constant.maxVillager) SteamHelper.Instance.AchieveVillagers();

        OrderManager.Instance.FastTravelShow(); // 주민 구출 후
    }

    public void FailToRescue()
    {
        var villager = villagers[currentSosIdx];
        villager.Expel();
        // 비명 소리
        AudioManager.Instance.PlaySFX_Villager(2, villager.gender);
        helpGoals[currentSosIdx].Hide();

        currentSosIdx = -1;
        miniUI.Hide();
    }

    public void UpdateUIs()
    {
        villagersText.text = $"{TextManager.Instance.GetCommons("Villager")} <color=#ffffff>({GetRecruitedVillagerCount()}/{Constant.maxVillager})"; 
        villagerScroll.verticalNormalizedPosition = 1f;


        for (int i = 0; i < villagerManagerObjs.Length; i++)
        {
            villagerManagerObjs[i].UpdateUI();
        }
    }

    public int GetNeededThings()
    {
        List<int> needthings = new List<int>();

        for (int i = 0; i < villagers.Length; i++)
        {
            if (villagers[i].recruited && !villagers[i].expelled)
            {
                int needs = villagers[i].currentNeeds;
                if (needs > -1) // 원하는 것이 있음
                {
                    if (inventory[needs] == 0) // 그것을 가지고 있지 않음
                    {
                        needthings.Add(needs);
                    }
                }
            }
        }

        if (needthings != null && needthings.Count > 0)
        {
            needthings.Shuffle();
            return needthings[0];
        }
        return -1;
    }
}
