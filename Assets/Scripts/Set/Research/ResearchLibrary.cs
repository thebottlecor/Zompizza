using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ResearchLibrary", menuName = "Library/Research")]
public class ResearchLibrary : ScriptableObject
{
    // 2티어, 3티어, 4티어, 5티어 담당 피자 레시피 업그레이드 관련 정보
    public List<ResearchInfo> pizzaRecipeUpgrades;

    [SerializeField]
    private List<ResearchInfo> researchInfos = null;

    public Dictionary<int, ResearchInfo> GetHashMap()
    {
        Dictionary<int, ResearchInfo> hashMap = new Dictionary<int, ResearchInfo>();
        foreach (var research in researchInfos)
        {
            if (hashMap.ContainsKey(research.idx))
                continue;

            hashMap.Add(research.idx, research);
        }
        return hashMap;
    }
}
