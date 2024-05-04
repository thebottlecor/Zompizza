using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ResearchLibrary", menuName = "Library/Research")]
public class ResearchLibrary : ScriptableObject
{
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
