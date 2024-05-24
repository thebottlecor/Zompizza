using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RivalManager : Singleton<RivalManager>
{

    public float rating;

    public float[] randomDailyAddRatings;

    public int currentIdx;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        rating = 0f;
        currentIdx = 0;
        CreateRandomRating();
    }

    private void CreateRandomRating()
    {
        randomDailyAddRatings = new float[10];
        float[] values = new float[5] { 4f, 4.5f, 5f, 5.5f, 6f };
        for (int i = 0; i < 2; i++)
        {
            randomDailyAddRatings[i] = values[0];
        }
        for (int i = 2; i < 4; i++)
        {
            randomDailyAddRatings[i] = values[1];
        }
        for (int i = 4; i < 6; i++)
        {
            randomDailyAddRatings[i] = values[2];
        }
        for (int i = 6; i < 8; i++)
        {
            randomDailyAddRatings[i] = values[3];
        }
        for (int i = 8; i < 10; i++)
        {
            randomDailyAddRatings[i] = values[4];
        }
        randomDailyAddRatings.Shuffle();
    }

    public void NextDay()
    {
        rating += randomDailyAddRatings[currentIdx];
        currentIdx++;
        if (currentIdx >= randomDailyAddRatings.Length)
        {
            currentIdx = 0;
            randomDailyAddRatings.Shuffle();
        }    
    }
}
