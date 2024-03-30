using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class ListHelper
{

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static List<T> ShuffleAndDeepCopy<T>(this IList<T> list)
    {
        var newList = list.DeepCopy();
        newList.Shuffle();
        return newList;
    }

    public static List<T> DeepCopy<T>(this IList<T> list)
    {
        List<T> tempList = new List<T>(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            tempList.Add(list[i]);
        }
        return tempList;
    }
}
