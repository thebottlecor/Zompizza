using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ResearchEffect
{
    public float goldGet;
    public float ratingGet;
    public int maxSpeed;
    public float damageReduce;
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
    //public int tier;
    //public int height;
    public int group;

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

    public int Cost(int level = 0)
    {
        int value = cost;

        if (level >= 1)
            value += (int)(level * increaseCost);

        return value;
    }

    public ResearchEffect AddEffect(ResearchEffect global)
    {

        ResearchEffect temp = new ResearchEffect
        {
            goldGet = effect.goldGet + global.goldGet,
            ratingGet = effect.ratingGet + global.ratingGet,
            maxSpeed = effect.maxSpeed + global.maxSpeed,
            damageReduce = effect.damageReduce + global.damageReduce,

        };
        return temp;
    }
}
