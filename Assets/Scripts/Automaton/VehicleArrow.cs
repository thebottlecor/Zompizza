using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VehicleArrow : MonoBehaviour
{

    public RectTransform rect;

    [HideInInspector] public Vector2 initPos;
    public Vector2 speed;

    public int turningCount = 100;
    private int count;

    private void Start()
    {
        initPos = rect.anchoredPosition;
    }


    void Update()
    {
        count++;

        if (count >= turningCount)
        {
            rect.anchoredPosition = initPos + speed * (turningCount * 2 - count);

            if (count >= turningCount * 2)
            {
                count = 0;
            }
        }
        else
        {
            rect.anchoredPosition = initPos + speed * count;
        }
    }
}
