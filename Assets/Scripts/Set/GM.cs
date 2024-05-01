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
    public int IngredientCount => ingredients.Count;

    public Light globalLight;
    public Vector3 lightAngleX = new Vector3(140f, 120f, 160f);
    public Vector2 lightAngleY = new Vector2(-60f, 100f);
    private bool endTime;

    public CanvasGroup darkCanvas;
    public GameObject gameOverObj;
    public Button gameOverBtn_ToLobby;
    public TextMeshProUGUI gameOverBtn_ToLobby_Text;
    public bool loading;

    public static EventHandler<bool> EndTimeEvent; // true일시 마감

    private void Start()
    {
        ingredients = new SerializableDictionary<Ingredient, int>();
        var list = System.Enum.GetValues(typeof(Ingredient));
        foreach (var temp in list)
        {
            Ingredient key = (Ingredient)temp;
            ingredients.Add(new SerializableDictionary<Ingredient, int>.Pair(key, 10));
        }

        UIManager.Instance.Init();

        SetGold(1000);
        SetRating(5f);

        darkCanvas.alpha = 0f;
        darkCanvas.interactable = false;
        darkCanvas.blocksRaycasts = false;
        gameOverBtn_ToLobby.onClick.AddListener(() => { LoadingSceneManager.Instance.ToLobby(); });

        TextUpdate();
    }

    public void TextUpdate()
    {
        gameOverBtn_ToLobby_Text.text = $"> {TextManager.Instance.GetCommons("Menu")} <";
    }

    private void Update()
    {
        if (loading) return;

        timer += Time.deltaTime;

        if (timer >= Constant.dayTime)
        {
            timer = Constant.dayTime;
        }

        int hour = (int)(timer / Constant.oneHour);
        int minute = (int)((timer - hour * Constant.oneHour) / Constant.oneMinute);
        int sec = (int)((timer - minute * Constant.oneMinute - hour * Constant.oneHour) / Constant.oneSec);

        hour += Constant.dayStartHour;
        if (hour >= Constant.dayEndHour)
        {
            hour = Constant.dayEndHour;
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

        float timePercent = timer / Constant.dayTime;
        globalLight.color = DataManager.Instance.uiLib.timeLightGradient.Evaluate(timePercent);
        Vector3 lightAngle = globalLight.transform.localEulerAngles;
        lightAngle.y = (lightAngleY.y - lightAngleY.x) * timePercent + lightAngleY.x;

        if (hour < 12)
        {
            timePercent = timer / (Constant.oneHour * 4);
            lightAngle.x = (lightAngleX.y - lightAngleX.x) * timePercent + lightAngleX.x;
        }
        else
        {
            timePercent = timer / (Constant.oneHour * 8) - 0.5f;
            lightAngle.x = (lightAngleX.z - lightAngleX.y) * timePercent + lightAngleX.y;
        }

        globalLight.transform.localEulerAngles = lightAngle;
    }

    public void NextDay()
    {
        timer = 0f;
        day++;

        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetAutoKill(true);
        sequence.AppendCallback(() =>
        {
            darkCanvas.alpha = 0f;
            darkCanvas.blocksRaycasts = true;
            loading = true;
        });
        sequence.Append(darkCanvas.DOFade(1f, 0.5f));
        sequence.AppendInterval(0.75f);
        sequence.AppendCallback(() =>
        {
            AudioManager.Instance.PlaySFX(Sfx.nextDay);

            darkCanvas.blocksRaycasts = false;
            darkCanvas.alpha = 0f;

            loading = false;
            OrderManager.Instance.NewOrder();
            ExplorationManager.Instance.ShowResultPanel();
        });
        sequence.Append(darkCanvas.DOFade(0f, 0.5f));
    }
    public void GameOver()
    {
        darkCanvas.alpha = 1f;
        darkCanvas.blocksRaycasts = true;
        darkCanvas.interactable = true;
        loading = true;

        gameOverObj.SetActive(true);
    }


    #region UI 표시
    public void CarSpeedUI(float carSpeed)
    {
        float absoluteCarSpeed = Mathf.Abs(carSpeed);
        carSpeedText.text = $"{Mathf.RoundToInt(absoluteCarSpeed)}<size=75%>km/h</size>";
    }

    public void AddGold(int value)
    {
        int target = gold + value;
        DOVirtual.Int(gold, target, 0.75f, (x) =>
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
        DOVirtual.Float(rating, target, 0.75f, (x) =>
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
    #endregion

}
