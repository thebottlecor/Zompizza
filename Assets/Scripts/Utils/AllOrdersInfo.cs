using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AllOrdersInfo : MonoBehaviour
{

    public OrderGoal[] goals;
    public Transform pizzeriaPos;

    [ContextMenu("Ãø·®")]
    public void Scan()
    {
        goals = FindObjectsOfType<OrderGoal>(true);

        goals = goals.OrderBy(x => x.GetDist(pizzeriaPos)).ToArray();

        for (int i =0; i < goals.Length; i++)
        {
            goals[i].ShowDist2(pizzeriaPos);
        }
    }
}
