using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class ResearchManager : Singleton<ResearchManager>
{
    [Serializable]
    public struct SaveData
    {
        public SerializableDictionary<int, int> researchedCount;
    }
    public SaveData Save()
    {
        SaveData data = new();
        data.researchedCount = researchedCount;
        return data;
    }
    public void Load(SaveData data)
    {
        if (data.researchedCount == null || data.researchedCount.Count == 0) return;

        researchedCount = data.researchedCount;

        foreach (var info in ResearchInfo) // 새로 추가된 연구 데이터 삽입
        {
            if (!researchedCount.ContainsKey(info.Key))
            {
                researchedCount.Add(new SerializableDictionary<int, int>.Pair { Key = info.Key, Value = 0 });
            }
        }

        foreach (var research in researchedCount)
        {
            for (int i = 0; i < research.Value; i++)
            {
                globalEffect = ResearchInfo[research.Key].AddEffect(globalEffect);
            }
        }

        for (int i = 0; i < researchLines.Count; i++)
        {
            researchLines[i].InitVisual();
        }
        UpdatePizzeria();
        UIManager.Instance.UpdateIngredientsTier();
        UIManager.Instance.shopUI.UpdateHiddenUpgrade();
    }

    public ResearchEffect globalEffect;
    private Dictionary<int, ResearchInfo> ResearchInfo => DataManager.Instance.researches;

    private SerializableDictionary<int, int> researchedCount;
    public bool Researched(int idx) => researchedCount[idx] >= 1;
    public bool MaxResearched(int idx)
    {
        if (ResearchInfo[idx].max == 1)
            return Researched(idx);
        else
            return researchedCount[idx] >= ResearchInfo[idx].max;
    }
    public float GetResearchedPercent(int idx)
    {
        return (float)researchedCount[idx] / ResearchInfo[idx].max;
    }
    public int GetResearchCount(int idx) => researchedCount[idx];

    #region 창고
    //private Dictionary<BuildingType, List<int>> buildingNeeds;
    //public bool IsBuildingUnlocked(BuildingType building)
    //{
    //    if (GM.Instance.TEST_NotRequire_Research) return true;

    //    if (buildingNeeds.ContainsKey(building))
    //    {
    //        for (int i = 0; i < buildingNeeds[building].Count; i++)
    //        {
    //            if (!CheckValid(buildingNeeds[building][i]))
    //                return false;
    //            if (!Researched(buildingNeeds[building][i]))
    //                return false;
    //        }
    //        return true;
    //    }
    //    return true;
    //}
    //public int GetNeedBuildingUnlock(BuildingType building)
    //{
    //    if (buildingNeeds.ContainsKey(building)) // 하나만 리턴
    //    {
    //        return buildingNeeds[building][0];
    //    }
    //    return -1;
    //}
    //public bool IsBuildingValid(BuildingType building)
    //{
    //    if (buildingNeeds.ContainsKey(building))
    //    {
    //        for (int i = 0; i < buildingNeeds[building].Count; i++)
    //        {
    //            if (!CheckValid(buildingNeeds[building][i]))
    //                return false;
    //        }
    //        return true;
    //    }
    //    return true;
    //}
    //public bool IsBuildingUnlocked_ByNewResearch(BuildingType building, int researchIdx)
    //{
    //    if (buildingNeeds.ContainsKey(building))
    //    {
    //        bool newResearchRelated = false;
    //        for (int i = 0; i < buildingNeeds[building].Count; i++)
    //        {
    //            int idx = buildingNeeds[building][i];
    //            if (!Researched(idx))
    //                return false;
    //            else if (researchIdx == idx)
    //                newResearchRelated = true;
    //        }
    //        return newResearchRelated;
    //    }
    //    return false;
    //}
    //private Dictionary<BuildingType, Dictionary<int, List<int>>> recipeNeeds;
    //public bool IsRecipeUnlocked(BuildingType building, int recipeIdx)
    //{
    //    if (GM.Instance.TEST_NotRequire_Research) return true;
    //    if (recipeNeeds.ContainsKey(building) && recipeNeeds[building].ContainsKey(recipeIdx))
    //    {
    //        for (int i = 0; i < recipeNeeds[building][recipeIdx].Count; i++)
    //        {
    //            if (!Researched(recipeNeeds[building][recipeIdx][i]))
    //                return false;
    //        }
    //        return true;
    //    }
    //    return true;
    //}
    #endregion

    public static EventHandler<int> researchCompleteEvent;

    [Space(20f)]
    [SerializeField] private List<ResearchLine> researchLines;

    [Header("수동 업데이트 필요!")]
    public int[] tierResearches; // 수동으로 업데이트 필요!
    [SerializeField] private List<HiddenRecipeGoal> hiddenRecipeGoals;
    public int HiddenRecipeCount => hiddenRecipeGoals.Count;

    [Space(20f)]
    [SerializeField] private List<GameObject> tier_Add;
    [SerializeField] private List<GameObject> tier_Remove;
    [SerializeField] private List<GameObject> pizzeriaExpands_Add;
    [SerializeField] private List<GameObject> pizzeriaExpands_Remove;
    [SerializeField] private List<GameObject> raidDefenses_Add;
    [SerializeField] private List<GameObject> raidDefenses_Remove;

    public void Init()
    {
        globalEffect = new ResearchEffect();
        researchedCount = new SerializableDictionary<int, int>();
        foreach (var info in ResearchInfo)
        {
            researchedCount.Add(new SerializableDictionary<int, int>.Pair { Key = info.Key, Value = 0 });
        }

        for (int i = 0; i < researchLines.Count; i++)
        {
            researchLines[i].InitVisual();
        }
        UpdatePizzeria(); 
        UIManager.Instance.UpdateIngredientsTier();
    }

    //public override void CallAfterStart(GameSaveData gameSaveData)
    //{
    //    globalEffect = new ResearchEffect();
    //    if (gameSaveData == null)
    //    {
    //        researchedCount = new SerializableDictionary<int, int>();
    //        foreach (var info in ResearchInfo)
    //        {
    //            researchedCount.Add(new SerializableDictionary<int, int>.Pair { Key = info.Key, Value = 0 });
    //        }

    //        //int resCount = (int)ResourceType.LAST;
    //        //paidRes = new int[resCount];
    //    }
    //    else
    //    {
    //        GetSaveData(gameSaveData.research.data);
    //    }
    //    //CalcBuildingRequire();
    //    //CalcRecipeRequire();
    //}

    #region 창고 2
    //private void CalcBuildingRequire()
    //{
    //    buildingNeeds = new Dictionary<BuildingType, List<int>>();
    //    foreach (var info in ResearchInfo)
    //    {
    //        var list = GetUnlockBuildings(info.Key);
    //        for (int j = 0; j < list.Count; j++)
    //        {
    //            if (buildingNeeds.ContainsKey(list[j]))
    //            {
    //                buildingNeeds[list[j]].Add(info.Key);
    //            }
    //            else
    //            {
    //                List<int> needs = new List<int>();
    //                needs.Add(info.Key);
    //                buildingNeeds.Add(list[j], needs);
    //            }
    //        }
    //    }
    //}
    //private void CalcRecipeRequire()
    //{
    //    recipeNeeds = new Dictionary<BuildingType, Dictionary<int, List<int>>>();
    //    foreach (var info in ResearchInfo)
    //    {
    //        var list = GetUnlockRecipes(info.Key);
    //        for (int j = 0; j < list.Count; j++)
    //        {
    //            if (recipeNeeds.ContainsKey(list[j].building))
    //            {
    //                if (recipeNeeds[list[j].building].ContainsKey(list[j].recipeIdx))
    //                {
    //                    recipeNeeds[list[j].building][list[j].recipeIdx].Add(info.Key);
    //                }
    //                else
    //                {
    //                    List<int> needs = new List<int>();
    //                    needs.Add(info.Key);
    //                    recipeNeeds[list[j].building].Add(list[j].recipeIdx, needs);
    //                }
    //            }
    //            else
    //            {
    //                var dict = new Dictionary<int, List<int>>();
    //                List<int> needs = new List<int>();
    //                needs.Add(info.Key);
    //                dict.Add(list[j].recipeIdx, needs);
    //                recipeNeeds.Add(list[j].building, dict);
    //            }
    //        }
    //    }
    //}

    //public List<BuildingType> GetUnlockBuildings(int idx)
    //{
    //    return ResearchInfo[idx].unlockBuildings;
    //}
    //public List<ResearchUnlockRecipe> GetUnlockRecipes(int idx)
    //{
    //    return ResearchInfo[idx].unlockRecipes;
    //}
    #endregion

    public void UpdatePizzeria()
    {
        int changed = 0;
        int tier = globalEffect.tier;

        if (tier >= 1)
        {
            changed += SetVisbility(tier_Add[0], true);
            changed += SetVisbility(tier_Remove[0], false);
        }

        int expand = globalEffect.pizzeriaExpand;
        
        if (expand >= 1)
        {
            changed += SetVisbility(pizzeriaExpands_Add[0], true);
            changed += SetVisbility(pizzeriaExpands_Remove[0], false);
        }
        if (expand >= 2)
        {
            changed += SetVisbility(pizzeriaExpands_Add[1], true);
            changed += SetVisbility(pizzeriaExpands_Remove[1], false);
        }
        if (expand >= 3)
        {
            changed += SetVisbility(pizzeriaExpands_Add[2], true);
            changed += SetVisbility(pizzeriaExpands_Remove[2], false);
        }

        int raidDefense = globalEffect.raidDefense;

        if (raidDefense >= 1)
        {
            changed += SetVisbility(raidDefenses_Add[0], true);
            changed += SetVisbility(raidDefenses_Remove[0], false);
        }

        if (changed > 0)
        {
            var graph = AstarPath.active.data.graphs[1];
            AstarPath.active.Scan(graph);
        }
    }
    private int SetVisbility(GameObject obj, bool on)
    {
        if (on && !obj.activeSelf)
        {
            obj.SetActive(true); 
            return 1;
        }
        if (!on && obj.activeSelf)
        {
            obj.SetActive(false);
            return 1;
        }
        return 0;
    }

    public void ToggleAllHiddenRecipe(bool on)
    {
        if (on)
        {
            for (int i = 0; i < hiddenRecipeGoals.Count; i++)
            {
                hiddenRecipeGoals[i].Show();
            }
        }
        else
        {
            for (int i = 0; i < hiddenRecipeGoals.Count; i++)
            {
                hiddenRecipeGoals[i].Hide();
            }
        }
    }

    public bool CanResearced(int idx)
    {
        return ResearchInfo[idx].max > researchedCount[idx] && CheckneedResearch(idx);
    }

    public bool CheckCanUnlocked(int idx)
    {
        return CanResearced(idx) && CheckRating(idx) && PayResearchPoint(idx, false);
        // CanResearced(idx) && CheckRating(idx) && PayCost(idx, false) && PayResearchPoint(idx, false);
    }

    public bool ResearchUnlock(int idx)
    {
        // 실제 돈 지불이 들어가는 PayCost는 맨 마지막 조건므로
        //if (CanResearced(idx) && CheckRating(idx) && PayCost(idx, true))
        if (CanResearced(idx) && CheckRating(idx) && PayResearchPoint(idx, true))
        {
            globalEffect = ResearchInfo[idx].AddEffect(globalEffect);
            researchedCount[idx]++;

            if (researchCompleteEvent != null)
                researchCompleteEvent(null, idx);

            for (int i = 0; i < researchLines.Count; i++)
            {
                researchLines[i].InitVisual();
            }

            UpdatePizzeria();
            UIManager.Instance.UpdateIngredientsTier();

            return true;
        }
        return false;
    }
    public void AutoResearch_For_Tier() // 티어 자동 업그레이드
    {
        for (int i = 0; i < tierResearches.Length; i++)
        {
            if (ResearchUnlock(tierResearches[i]))
            {

            }
        }
    }

    public void ResearchUnlock_Force(int idx)
    {
        globalEffect = ResearchInfo[idx].AddEffect(globalEffect);
        researchedCount[idx]++;

        if (researchCompleteEvent != null)
            researchCompleteEvent(null, idx);

        UIManager.Instance.shopUI.UpdateHiddenUpgrade();
    }

    private bool CheckneedResearch(int idx)
    {
        var info = ResearchInfo[idx];
        if (info.invalid) return false;
        //if (GM.Instance.TEST_Free_Research) return true;

        if (info.tier > 0 && !Researched(DataManager.Instance.researchLib.pizzaRecipeUpgrades[info.tier - 1].idx)) return false;

        for (int i = 0; i < info.needResearch.Count; i++)
        {
            if (!Researched(info.needResearch[i].idx))
                return false;
        }
        return true;
    }

    public bool CheckValid(int idx)
    {
        return !ResearchInfo[idx].invalid;
    }

    private bool PayCost(int idx, bool pay = false)
    {
        //if (GM.Instance.TEST_Free_Research) return true;

        int gold = GM.Instance.gold;

        int needCost = GetCost(idx);

        if (gold < needCost)
            return false;

        if (pay)
        {
            // 실제 지불
            GM.Instance.AddGold(-1 * needCost, GM.GetGoldSource.upgrade);
        }
        return true;
    }
    public int GetCost(int idx)
    {
        return ResearchInfo[idx].Cost(researchedCount[idx]);
    }

    private bool CheckRating(int idx)
    {
        float rating = GM.Instance.rating;

        float needRating = GetRating_Require(idx);

        if (needRating > 0f && rating < needRating)
            return false;

        return true;
    }
    public float GetRating_Require(int idx)
    {
        return ResearchInfo[idx].Rating_Require(researchedCount[idx]);
    }

    private bool PayResearchPoint(int idx, bool pay = false)
    {
        //if (GM.Instance.TEST_Free_Research) return true;

        float rp = GM.Instance.researchPoint;

        float needCost = GetResearchPoint(idx);

        if (rp < needCost)
            return false;

        if (pay)
        {
            // 실제 지불
            GM.Instance.AddResearchPoint(-1 * needCost);
        }
        return true;
    }
    public float GetResearchPoint(int idx)
    {
        return ResearchInfo[idx].ResearchPoint(researchedCount[idx]);
    }
    public bool AllResearched()
    {
        bool result = true;
        foreach (var research in ResearchInfo)
        {
            if (research.Value.max == 1 && !Researched(research.Key))
            {
                result = false;
                break;
            }
        }
        return result;
    }

}
