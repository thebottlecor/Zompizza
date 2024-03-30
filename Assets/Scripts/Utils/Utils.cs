using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Utils
{
    public static bool MouseOverUI()
    {
        //bool isOver = EventSystem.current.IsPointerOverGameObject();
        //if (isOver)
        //{
        //    if (UIManager.Instance.runtimeMessageObj.isOver) return false;
        //}
        //return isOver;
        return EventSystem.current.IsPointerOverGameObject();
    }

    //public static int ModOperation(int value, int standard)
    //{
    //    int temp = (int)(value / (double)standard);
    //    return value - (temp * standard);
    //}

    public static double[] cc_tens = new double[] { 
        1d, 
        0.1d, 
        0.01d, 
        0.001d, 
        0.0001d,
        0.00001d,
        0.000001d,
        0.0000001d,
    };
    private static int[] tens = new int[] {
        1,
        10,
        100,
        1000,
        10000,
        100000,
        1000000,
        10000000,
    };
    public static int ModOperation_10(int value, int n)
    {
        int temp = (int)(value * cc_tens[n]);
        return value - (temp * tens[n]);
    }
}
