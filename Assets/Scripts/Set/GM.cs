using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

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
    public TextMeshProUGUI carSpeedText;

    public PlayerController player;

    private int displayGold;
    public TextMeshProUGUI[] goldText;
    private float displayRating;
    public TextMeshProUGUI[] ratingText;

    // 플레이어가 가진 자원
    public int gold;
    public float rating;
    public SerializableDictionary<Ingredient, int> ingredients;


    public Light globalLight;
    public Vector3 lightAngleX = new Vector3(140f, 120f, 160f);
    public Vector2 lightAngleY = new Vector2(-60f, 100f);
    private bool endTime;

    public static EventHandler<bool> EndTimeEvent; // true일시 마감

    private void Start()
    {
        ingredients = new SerializableDictionary<Ingredient, int>();
        var list = System.Enum.GetValues(typeof(Ingredient));
        foreach (var temp in list)
        {
            Ingredient key = (Ingredient)temp;
            ingredients.Add(new SerializableDictionary<Ingredient, int>.Pair(key, 1));
        }

        UIManager.Instance.Init();

        SetGold(1000);
        SetRating(5f);
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

        hour += dayStartHour;
        if (hour >= dayEndHour)
        {
            hour = dayEndHour;
            minute = 0;
            sec = 0;

            // 마감 시간 시간 정지
            if (!endTime)
            {
                if (EndTimeEvent != null)
                    EndTimeEvent(null, true);
            }
            endTime = true;
        }
        else
        {
            if (endTime)
            {
                if (EndTimeEvent != null)
                    EndTimeEvent(null, false);
            }
            endTime = false;
        }

        if (endTime)
            //timeText.text = $"Day {day} :: <color=#ff0000>{hour:00}:{minute:00}:{sec:00}</color>";
            timeText.text = $"Day {day}\n<color=#ff0000>{hour:00}:{minute:00}</color>";
        else
            //timeText.text = $"Day {day} :: {hour:00}:{minute:00}:{sec:00}";
            timeText.text = $"Day {day}\n{hour:00}:{minute:00}";

        CarSpeedUI(player.carSpeed);

        for (int i = 0; i < goldText.Length; i++)
            goldText[i].text = $"{displayGold}$";
        for (int i = 0; i < ratingText.Length; i++)
            ratingText[i].text = $"{displayRating:0.#}";

        float timePercent = timer / dayTime;
        globalLight.color = DataManager.Instance.uiLib.timeLightGradient.Evaluate(timePercent);
        Vector3 lightAngle = globalLight.transform.localEulerAngles;
        lightAngle.y = (lightAngleY.y - lightAngleY.x) * timePercent + lightAngleY.x;

        if (hour < 12)
        {
            timePercent = timer / (oneHour * 4);
            lightAngle.x = (lightAngleX.y - lightAngleX.x) * timePercent + lightAngleX.x;
        }
        else
        {
            timePercent = timer / (oneHour * 8) - 0.5f;
            lightAngle.x = (lightAngleX.z - lightAngleX.y) * timePercent + lightAngleX.y;
        }

        globalLight.transform.localEulerAngles = lightAngle;
    }

    public void CarSpeedUI(float carSpeed)
    {
        float absoluteCarSpeed = Mathf.Abs(carSpeed);
        carSpeedText.text = $"{Mathf.RoundToInt(absoluteCarSpeed)}<size=75%>km/h</size>";
    }

    public void AddGold(int value)
    {
        int target = gold + value;
        DOVirtual.Int(gold, target, 0.5f, (x) =>
        {
            displayGold = x;
        }).SetEase(Ease.OutCirc).SetUpdate(true);
        gold = target; 
    }
    public void SetGold(int value)
    {
        displayGold = value;
        gold = value;
    }
    public void AddRating(float value)
    {
        float target = rating + value;
        DOVirtual.Float(rating, target, 0.5f, (x) =>
        {
            displayRating = x;
        }).SetEase(Ease.OutCirc).SetUpdate(true);
        rating = target;
    }
    public void SetRating(float value)
    {
        displayRating = value;
        rating = value;
    }

}
