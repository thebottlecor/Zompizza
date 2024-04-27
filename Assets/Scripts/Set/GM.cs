using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GM : Singleton<GM>
{


    public bool stop_control;


    public const float dayTime = 30f;
    public const int dayStartHour = 8;
    public const int dayEndHour = 20;
    public const float oneHour = dayTime / (dayEndHour - dayStartHour);
    public const float oneMinute = oneHour / 60f;
    public const float oneSec = oneMinute / 60f;

    public int day;
    public float timer;

    public TextMeshProUGUI timeText;


    // 플레이어가 가진 자원
    public int gold;
    public SerializableDictionary<Ingredient, int> ingredients;

    private void Start()
    {
        ingredients = new SerializableDictionary<Ingredient, int>();
        var list = System.Enum.GetValues(typeof(Ingredient));
        foreach (var temp in list)
        {
            Ingredient key = (Ingredient)temp;
            ingredients.Add(new SerializableDictionary<Ingredient, int>.Pair(key, 1));
        }
    }

    private void Update()
    {

        timer += Time.deltaTime;

        if (timer >= dayTime)
        {
            timer = dayTime;
        }

        int hour = (int)(timer / oneHour);
        int minute = (int)((timer - hour * oneHour) / oneMinute);
        int sec = (int)((timer - minute * oneMinute - hour * oneHour) / oneSec);

        bool endtime = false;
        hour += dayStartHour;
        if (hour >= dayEndHour)
        {
            hour = dayEndHour;
            minute = 0;
            sec = 0;

            // 마감 시간 시간 정지
            endtime = true;
        }

        if (endtime)
            timeText.text = $"Day {day} :: <color=#ff0000>{hour:00}:{minute:00}:{sec:00}</color>";
        else
            timeText.text = $"Day {day} :: {hour:00}:{minute:00}:{sec:00}";
    }

}
