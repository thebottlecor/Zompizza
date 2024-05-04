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

    // �ִ� ���� ���� Ƚ��
    public int max = 1;
    public int cost; // �⺻ ���
    public float increaseCost; // ������ Ƚ������ �����ϴ� ���
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
