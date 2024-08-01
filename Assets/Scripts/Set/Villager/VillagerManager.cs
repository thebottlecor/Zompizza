using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MTAssets.EasyMinimapSystem;
using TMPro;
using UnityEngine.UI;

public class VillagerManager : Singleton<VillagerManager>
{

    public VillagerSearcher villagerSearcher;
    public VillagerWay[] villagers;
    public HelpGoal[] helpGoals;
    public VillagerManagerObj[] villagerManagerObjs;

    public int[] inventory;
    public VillagerInvenSlot[] invenSlots;


    public int currentSosIdx;
    public float sosTimeLimit;
    public float sosTimer;
    public VillagerSosMiniUI miniUI;

    public bool sosShowed; // 다음날 초기화

    public MinimapRenderer minimap;
    public MinimapRenderer worldmap;

    [Header("UI")]
    public TextMeshProUGUI villagersText;
    public ScrollRect villagerScroll;



    private void Start()
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

    public void CreateSOS()
    {
        // 복귀 후, 아무 주문이 없을 떄
        // 강제로 가게를 닫았을 떄

        int day = GM.Instance.day;

        if (day >= 9) return; // 데모는 10일 때 막음

        if ((day + 1) % 2 == 0 && !sosShowed) // 짝수날 마다
        {
            int recruited = GetRecruitedVillagerCount();
            int remained = GetRemainVillagerCount();

            if (recruited < Constant.maxVillager && remained > 0)
            {
                int who = RandomSosVillager();
                if (who >= 0)
                {
                    sosShowed = true;
                    UIManager.Instance.shopUI.ShowSosWarning(true);
                    AudioManager.Instance.PlaySFX_Villager(1, villagers[who].gender);

                    float dist = (helpGoals[who].transform.position - OrderManager.Instance.pizzeria.position).magnitude;
                    float km = dist * Constant.distanceScale; // 게임상 거리 200 = 1km
                    sosTimer = 0f;
                    sosTimeLimit = Constant.delivery_timeLimit_1km * 2f * km;

                    currentSosIdx = who;
                    miniUI.Init(who);
                    helpGoals[who].Show();
                }
            }
        }
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
}
