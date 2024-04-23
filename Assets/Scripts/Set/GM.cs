using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GM : Singleton<GM>
{


    public bool stop_control;


    public const float dayTime = 360f;
    public const int dayStartHour = 8;
    public const float oneHour = dayTime / 24f;
    public const float oneMinute = oneHour / 60f;
    public const float oneSec = oneMinute / 60f;

    public int day;
    public float timer;

    public TextMeshProUGUI timeText;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= dayTime)
        {
            timer = 0;
            day++;
        }

        int hour = (int)(timer / oneHour);
        int minute = (int)((timer - hour * oneHour) / oneMinute);
        int sec = (int)((timer - minute * oneMinute - hour * oneHour) / oneSec);

        hour += dayStartHour;
        if (hour >= 24) hour -= 24;

        timeText.text = $"Day {day} :: {hour:00}:{minute:00}:{sec:00}";
    }

}
