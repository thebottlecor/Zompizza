using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Text;

public class GM : Singleton<GM>
{


    public bool stop_control;

    public int day;
    public float timer;

    public TextMeshProUGUI timeText;
    public GameObject openImage;
    public GameObject closeImage;

    private float displaySpeed;
    public TextMeshProUGUI carSpeedText;
    public RectTransform speedNiddle;

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


    public SerializableDictionary<GetGoldSource, int> dayOne_Gold;
    public SerializableDictionary<GetRatingSource, int> dayOne_Rating;

    public enum GetGoldSource
    {
        delivery,
        explore,
        zombie,
    }
    public enum GetRatingSource
    {
        delivery,
    }

    public Light globalLight;
    public Vector3 lightAngleX = new Vector3(140f, 120f, 160f);
    public Vector2 lightAngleY = new Vector2(-60f, 100f);
    private bool endTime;

    public CanvasGroup darkCanvas;

    public GameObject accountObj;
    public TextMeshProUGUI accountDayText;
    public TextMeshProUGUI profitText;
    public TextMeshProUGUI profitNumText;
    public TextMeshProUGUI profit_totalText;
    public TextMeshProUGUI profit_totalNumText;
    public Button nextDayBtn;
    public TextMeshProUGUI nextDayBtn_Text;

    public GameObject gameOverObj;
    public Button gameOverBtn_ToLobby;
    public TextMeshProUGUI gameOverBtn_ToLobby_Text;
    public bool loading;

    public static EventHandler<bool> EndTimeEvent; // true일시 마감
    private TextManager tm => TextManager.Instance;

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
        SetRating(10f);

        dayOne_Gold = new SerializableDictionary<GetGoldSource, int>();
        dayOne_Rating = new SerializableDictionary<GetRatingSource, int>();

        darkCanvas.alpha = 0f;
        darkCanvas.interactable = false;
        darkCanvas.blocksRaycasts = false;

        nextDayBtn.onClick.AddListener(() => { NextDay_Late(); });
        gameOverBtn_ToLobby.onClick.AddListener(() => { LoadingSceneManager.Instance.ToLobby(); });

        TextUpdate();

        day = 0;
        UIManager.Instance.shopUI.Init();
    }

    public void TextUpdate()
    {
        StringBuilder st = new StringBuilder();
        st.Append(tm.GetCommons("Delivery")).AppendLine();
        st.Append(tm.GetCommons("Explore")).AppendLine();
        st.Append(tm.GetCommons("Zombie")).AppendLine();

        profitText.text = st.ToString();

        profit_totalText.text = tm.GetCommons("Total");

        nextDayBtn_Text.text = $"> {tm.GetCommons("NextDay")} <";
        gameOverBtn_ToLobby_Text.text = $"> {tm.GetCommons("Menu")} <";
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
                openImage.SetActive(false);
                closeImage.SetActive(true);

                if (EndTimeEvent != null)
                    EndTimeEvent(null, true);
            }
            endTime = true;
        }
        else
        {
            if (endTime)
            {
                openImage.SetActive(true);
                closeImage.SetActive(false);

                if (EndTimeEvent != null)
                    EndTimeEvent(null, false);
            }
            endTime = false;
        }

        if (endTime)
            //timeText.text = $"Day {day} :: <color=#ff0000>{hour:00}:{minute:00}:{sec:00}</color>";
            timeText.text = string.Format(tm.GetCommons("Day"), day + 1) + $"  <color=#ff0000>{hour:00}:{minute:00}</color>";
        else
            //timeText.text = $"Day {day} :: {hour:00}:{minute:00}:{sec:00}";
            timeText.text = string.Format(tm.GetCommons("Day"), day + 1) + $"  {hour:00}:{minute:00}";

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

    private void FixedUpdate()
    {
        CarSpeedUI(player.carSpeed, player.maxSpeed);
    }

    public void NextDay()
    {
        timer = 0f;
        accountDayText.text = string.Format(tm.GetCommons("Day"), day + 1);
        day++;
        UIManager.Instance.shopUI.DayFirstReview();

        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetAutoKill(true);
        sequence.AppendCallback(() =>
        {
            accountObj.SetActive(false);
            darkCanvas.alpha = 0f;
            darkCanvas.interactable = false;
            darkCanvas.blocksRaycasts = true;
            loading = true;
        });
        sequence.Append(darkCanvas.DOFade(1f, 0.5f));
        sequence.AppendInterval(0.5f);
        sequence.AppendCallback(() =>
        {
            darkCanvas.interactable = true;
            UpdateAccountUI();
            accountObj.SetActive(true);
        });
    }
    public void UpdateAccountUI()
    {
        int total = 0;

        StringBuilder st = new StringBuilder();


        var list2 = System.Enum.GetValues(typeof(GetGoldSource));
        foreach (var temp in list2)
        {
            if (!dayOne_Gold.ContainsKey((GetGoldSource)temp))
                dayOne_Gold.Add(new SerializableDictionary<GetGoldSource, int>.Pair { Key = (GetGoldSource)temp, Value = 0 });
        }
        var list3 = System.Enum.GetValues(typeof(GetRatingSource));
        foreach (var temp in list3)
        {
            if (!dayOne_Rating.ContainsKey((GetRatingSource)temp))
                dayOne_Rating.Add(new SerializableDictionary<GetRatingSource, int>.Pair { Key = (GetRatingSource)temp, Value = 0 });
        }

        int count = dayOne_Gold.Count;
        foreach (var temp in dayOne_Gold)
        {
            int value = temp.Value;
            if (value >= 0)
            {
                if (value == 0)
                    st.Append("0");
                else
                    st.AppendFormat("+{0}", value);
            }
            else
                st.AppendFormat("<color=#A91111>{0}</color>", value);
            total += value;
            count--;
            if (count > 0)
                st.AppendLine();
        }
        profitNumText.text = st.ToString();

        if (total >= 0)
        {
            if (total == 0)
                profit_totalNumText.text = "0";
            else
                profit_totalNumText.text = $"+{total}";
        }
        else
            profit_totalNumText.text = $"<color=#A91111>{total}</color>";
    }

    public void NextDay_Late()
    {
        accountObj.SetActive(false);
        dayOne_Gold = new SerializableDictionary<GetGoldSource, int>();
        dayOne_Rating = new SerializableDictionary<GetRatingSource, int>();

        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetAutoKill(true);
        sequence.AppendCallback(() =>
        {
            AudioManager.Instance.PlaySFX(Sfx.nextDay);

            darkCanvas.blocksRaycasts = false;
            darkCanvas.interactable = false;
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
    public void CarSpeedUI(float carSpeed, float maxSpeed)
    {
        float absoluteCarSpeed = Mathf.Abs(carSpeed);
        float vel = Mathf.FloorToInt(absoluteCarSpeed);

        displaySpeed = Mathf.Lerp(displaySpeed, vel, Time.fixedDeltaTime);
        carSpeedText.text = $"{displaySpeed:F0}<size=75%>km/h</size>";

        float percent = vel / maxSpeed;
        if (percent > 1f) percent = 1f;
        else if (percent < 0f) percent = 0f;

        //speedNiddle.localEulerAngles = new Vector3(0f, 0f, -180f * percent + 90f);
        //speedNiddle.localEulerAngles = Vector3.Lerp(speedNiddle.localEulerAngles, new Vector3(0f, 0f, -180f * percent + 90f), Time.fixedDeltaTime);

        speedNiddle.rotation = Quaternion.Lerp(speedNiddle.rotation, Quaternion.Euler(0f, 0f, -180f * percent + 90f), Time.fixedDeltaTime);
    }

    public void AddGold(int value, GetGoldSource source)
    {
        int target = gold + value;
        DOVirtual.Int(gold, target, 0.75f, (x) =>
        {
            displayGold = x;
        }).SetEase(Ease.OutCirc).SetUpdate(true);
        gold = target;

        if (!dayOne_Gold.ContainsKey(source))
            dayOne_Gold.Add(new SerializableDictionary<GetGoldSource, int>.Pair { Key = source, Value = value });
        else
            dayOne_Gold[source] += value;
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
