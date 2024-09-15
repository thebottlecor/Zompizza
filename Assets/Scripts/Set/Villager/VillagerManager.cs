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

    public bool sosShowed; // ������ �ʱ�ȭ
    public bool incomeReceived; // ������ �ʱ�ȭ

    public MinimapRenderer minimap;
    public MinimapRenderer worldmap;

    [Header("UI")]
    public TextMeshProUGUI villagersText;
    public ScrollRect villagerScroll;



    private void Start()
    {
        inventory = new int[10];
         
        //// �����
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
            // ��� ����
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

    public void CreateSOS()
    {
        // ���� ��, �ƹ� �ֹ��� ���� ��
        // ������ ���Ը� �ݾ��� ��

        int day = GM.Instance.day; // ¦���� ���� => 4�� ������� (09.11) SOS

        if (day >= 9) return; // ����� 10�� �� ����

        if ((day + 1) % 4 == 0 && !sosShowed) // ¦���� ���� => 4�� ������� (09.11)
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
                    float km = dist * Constant.distanceScale; // ���ӻ� �Ÿ� 200 = 1km
                    sosTimer = 0f;
                    sosTimeLimit = Constant.sos_timeLimit_1km * km + Constant.delivery_timeLimit_base;

                    currentSosIdx = who;
                    miniUI.Init(who);
                    helpGoals[who].Show();
                }
            }
        }
    }

    public void Rescue(int idx)
    {
        // �ֹ� ����
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
        // ��� �Ҹ�
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
                if (needs > -1) // ���ϴ� ���� ����
                {
                    if (inventory[needs] == 0) // �װ��� ������ ���� ����
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
