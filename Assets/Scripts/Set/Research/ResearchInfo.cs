using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ResearchEffect
{
    public float happiness;
    public float health;
    public float efficiency;
}
//[System.Serializable]
//public struct ResearchUnlockRecipe
//{
//    public BuildingType building;
//    public int recipeIdx;
//}

[CreateAssetMenu(fileName = "ResearchInfo", menuName = "Research/New ResearchInfo")]
public class ResearchInfo : ScriptableObject
{

    public int idx;
    public int tier;
    public int height;

    // 최대 연구 가능 횟수
    public int max = 1;
    public int cost; // 기본 비용
    public float increaseCost; // 연구한 횟수마다 증가하는 비용
    public List<int> needResearch;

    public bool invalid;

    public ResearchEffect effect;

    //public List<BuildingType> unlockBuildings;
    //public List<ResearchUnlockRecipe> unlockRecipes;

    public Sprite icon;

    public ResearchEffect AddEffect(ResearchEffect global)
    {
        ResearchEffect temp = new ResearchEffect
        {
            happiness = effect.happiness + global.happiness,
            health = effect.health + global.health,
            efficiency = effect.efficiency + global.efficiency,
        };
        return temp;
    }
}
