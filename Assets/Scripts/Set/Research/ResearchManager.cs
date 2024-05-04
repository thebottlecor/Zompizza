using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchManager : Singleton<ResearchManager>
{
    [Serializable]
    public struct SaveData
    {
        public SerializableDictionary<int, int> researchedCount;
    }
    public SaveData SetSaveData()
    {
        SaveData data = new();
        data.researchedCount = researchedCount;
        return data;
    }
    public void GetSaveData(SaveData data)
    {
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
    }

    public ResearchEffect globalEffect;
    private Dictionary<int, ResearchInfo> ResearchInfo => DataManager.Instance.researches;

    private SerializableDictionary<int, int> researchedCount;
    public bool Researched(int idx) => researchedCount[idx] >= 1;
    public bool Researched2(int idx)
    {
        if (ResearchInfo[idx].max == 1)
            return Researched(idx);
        else
            return researchedCount[idx] >= ResearchInfo[idx].max;
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

    private void Start()
    {
        globalEffect = new ResearchEffect();
        researchedCount = new SerializableDictionary<int, int>();
        foreach (var info in ResearchInfo)
        {
            researchedCount.Add(new SerializableDictionary<int, int>.Pair { Key = info.Key, Value = 0 });
        }
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

    public bool ResearchUnlock(int idx)
    {
        if (ResearchInfo[idx].max > researchedCount[idx] && CheckneedResearch(idx) && PayCost(idx))
        {
            globalEffect = ResearchInfo[idx].AddEffect(globalEffect);
            researchedCount[idx]++;

            if (researchCompleteEvent != null)
                researchCompleteEvent(null, idx);
            return true;
        }
        return false;
    }

    private bool CheckneedResearch(int idx)
    {
        if (ResearchInfo[idx].invalid) return false;
        //if (GM.Instance.TEST_Free_Research) return true;

        for (int i = 0; i < ResearchInfo[idx].needResearch.Count; i++)
        {
            if (!Researched(ResearchInfo[idx].needResearch[i]))
                return false;
        }
        return true;
    }

    public bool CheckValid(int idx)
    {
        return !ResearchInfo[idx].invalid;
    }

    private bool PayCost(int idx)
    {
        //if (GM.Instance.TEST_Free_Research) return true;

        int gold = GM.Instance.gold;
        var info = ResearchInfo[idx];

        int needCost;

        if (info.max > 1)
        {
            needCost = info.cost + (int)(info.increaseCost * researchedCount[idx]);
        }
        else
        {
            needCost = info.cost;
        }

        if (gold < needCost)
            return false;

        // 실제 지불
        GM.Instance.AddGold(-1 * needCost, GM.GetGoldSource.upgrade);
        return true;
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
