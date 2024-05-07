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
    [HideInInspector] public float remainTime;

    public TextMeshProUGUI timeText;
    private string[] dayStr = new string[2];
    public GameObject openImage;
    public GameObject closeImage;

    private float displaySpeed;
    public TextMeshProUGUI carSpeedText;
    public RectTransform speedNiddle;

    public PlayerController player;
    public Transform pizzeriaPos;

    private int displayGold;
    public TextMeshProUGUI[] goldText;
    private float displayRating;
    public TextMeshProUGUI[] ratingText;

    // 플레이어가 가진 자원
    public int gold;
    public float rating;
    public SerializableDictionary<Ingredient, int> ingredients;
    public int HasIngredient
    {
        get
        {
            int count = 0;
            foreach (var temp in ingredients)
            {
                count += temp.Value;
            }
            return count;
        }
    }
    public int IngredientTypeCount => ingredients.Count;


    public SerializableDictionary<GetGoldSource, int> dayOne_Gold;
    public SerializableDictionary<GetRatingSource, float> dayOne_Rating;

    public enum GetGoldSource
    {
        delivery,
        explore,
        zombie,
        upgrade,
    }
    public enum GetRatingSource
    {
        delivery,
        notComplete,
        notAccepted,
    }

    public Light globalLight;
    public Vector3 lightAngleX = new Vector3(140f, 120f, 160f);
    public Vector2 lightAngleY = new Vector2(-80f, 100f);
    public bool EndTime { get; private set; }

    public CanvasGroup darkCanvas;

    public GameObject accountObj;
    public TextMeshProUGUI[] accountText;
    public TextMeshProUGUI[] profitText;
    public TextMeshProUGUI[] profit_totalText;
    public Button nextDayBtn;
    public TextMeshProUGUI nextDayBtn_Text;

    public GameObject gameOverObj;
    public Button gameOverBtn_ToLobby;
    public TextMeshProUGUI[] gameOverText;
    public TextMeshProUGUI gameOverBtn_ToLobby_Text;
    public bool loading;

    public GameObject gameOverWarningObj;
    public RectTransform gameOverWarningRect;
    public TextMeshProUGUI gameOverWarningBtn_Text;
    public TextMeshProUGUI gameOverWarning_Text;
    public TextMeshProUGUI gameOverWarningDetail_Text;

    public Queue<int> warningQueue;

    // 패배 트리거
    public bool warning_gameOver;

    public static EventHandler<bool> EndTimeEvent; // true일시 마감
    private TextManager tm => TextManager.Instance;

    protected override void AddListeners()
    {
        OrderManager.OrderRemovedEvent += OnOrderRemoved;
    }

    protected override void RemoveListeners()
    {
        OrderManager.OrderRemovedEvent -= OnOrderRemoved;
    }

    private void OnOrderRemoved(object sender, int e)
    {
        if (e == 0)
        {
            openImage.SetActive(false);
            closeImage.SetActive(true);
        }
    }

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

        dayOne_Gold = new SerializableDictionary<GetGoldSource, int>();
        dayOne_Rating = new SerializableDictionary<GetRatingSource, float>();
        warningQueue = new Queue<int>();

        darkCanvas.alpha = 0f;
        darkCanvas.interactable = false;
        darkCanvas.blocksRaycasts = false;

        nextDayBtn.onClick.AddListener(() => { NextDay_Late(); });
        gameOverBtn_ToLobby.onClick.AddListener(() => { LoadingSceneManager.Instance.ToLobby(); });

        TextUpdate();

        day = 0;
        DayStringUpdate();
        ResearchManager.Instance.Init();
        UIManager.Instance.shopUI.Init();
        TutorialManager.Instance.Init();
        OrderManager.Instance.Init();
    }

    public void TextUpdate()
    {
        //accountText[1].text = $"<sprite={2}> {tm.GetCommons("Money")}";
        //accountText[2].text = $"<sprite={1}> {tm.GetCommons("Rating")}";

        StringBuilder st = new StringBuilder();
        st.Append(tm.GetCommons("Delivery")).AppendLine();
        st.Append(tm.GetCommons("Explore")).AppendLine();
        st.Append(tm.GetCommons("Zombie")).AppendLine();
        st.Append(tm.GetCommons("Upgrade")).AppendLine();

        profitText[0].text = st.ToString();

        StringBuilder st2 = new StringBuilder();
        st2.Append(tm.GetCommons("Delivery")).AppendLine();
        st2.Append(tm.GetCommons("NotCompleted")).AppendLine();
        st2.Append(tm.GetCommons("NotAccepted")).AppendLine();

        profitText[2].text = st2.ToString();

        profit_totalText[0].text = tm.GetCommons("Total");
        profit_totalText[2].text = tm.GetCommons("Total");

        gameOverText[0].text = tm.GetCommons("Gameover");
        gameOverText[1].text = tm.GetCommons("Gameover2");

        nextDayBtn_Text.text = $"> {tm.GetCommons("NextDay")} <";
        gameOverBtn_ToLobby_Text.text = $"> {tm.GetCommons("Menu")} <";

        gameOverWarningBtn_Text.text = tm.GetCommons("Close");
        gameOverWarning_Text.text = tm.GetCommons("Warning");
        gameOverWarningDetail_Text.text = string.Format(tm.GetCommons("GameoverWarning"), "<size=90%><sprite=1></size>");
    }

    private void Update()
    {
        if (loading) return;

        if (!TutorialManager.Instance.training)
            timer += Time.deltaTime;

        if (timer >= Constant.dayTime)
        {
            timer = Constant.dayTime;
        }

        remainTime = Constant.dayTime - timer;

        int hour = (int)(timer / Constant.oneHour);
        int minute = (int)((timer - hour * Constant.oneHour) / Constant.oneMinute);
        //int sec = (int)((timer - minute * Constant.oneMinute - hour * Constant.oneHour) / Constant.oneSec);

        hour += Constant.dayStartHour;
        if (hour >= Constant.dayEndHour)
        {
            hour = Constant.dayEndHour;
            minute = 0;
            //sec = 0;

            // 마감 시간 시간 정지
            if (!EndTime)
            {
                openImage.SetActive(false);
                closeImage.SetActive(true);

                if (EndTimeEvent != null)
                    EndTimeEvent(null, true);

                timeText.text = dayStr[1];
            }
            EndTime = true;
        }

        if (EndTime)
        {
            //timeText.text = dayStr[1];
        }
        else
        {
            timeText.text = dayStr[0] + $"{hour:00}:{minute:00}";
        }

        float timePercent = timer / Constant.dayTime;
        globalLight.color = DataManager.Instance.uiLib.timeLightGradient.Evaluate(timePercent);
        Vector3 lightAngle = globalLight.transform.localEulerAngles;
        lightAngle.y = (lightAngleY.y - lightAngleY.x) * timePercent + lightAngleY.x;

        if (hour < 12)
        {
            timePercent = timer / (Constant.oneHour * 6);
            lightAngle.x = (lightAngleX.y - lightAngleX.x) * timePercent + lightAngleX.x;
        }
        else
        {
            timePercent = timer / (Constant.oneHour * 12) - 0.5f;
            lightAngle.x = (lightAngleX.z - lightAngleX.y) * timePercent + lightAngleX.y;
        }

        globalLight.transform.localEulerAngles = lightAngle;
    }

    private void FixedUpdate()
    {
        CarSpeedUI(player.carSpeed, player.MaxSpeed);
    }

    public void DayStringUpdate()
    {
        dayStr[0] = string.Format(tm.GetCommons("Day"), day + 1) + "  ";
        dayStr[1] = string.Format(tm.GetCommons("Day"), day + 1) + "  <color=#A91111>18:00</color>"; // 영업 종료시
    }

    public void NextDay()
    {
        timer = 0f;
        accountText[0].text = string.Format(tm.GetCommons("Day"), day + 1);
        day++;
        DayStringUpdate();
        UIManager.Instance.shopUI.DayFirstReview();
        ZombiePooler.Instance.ZombieReset();

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
        // 머니
        {
            int total = 0;
            StringBuilder st = new StringBuilder();
            var list2 = System.Enum.GetValues(typeof(GetGoldSource));
            foreach (var temp in list2)
            {
                if (!dayOne_Gold.ContainsKey((GetGoldSource)temp))
                    dayOne_Gold.Add(new SerializableDictionary<GetGoldSource, int>.Pair { Key = (GetGoldSource)temp, Value = 0 });
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
            profitText[1].text = st.ToString();

            if (total >= 0)
            {
                if (total == 0)
                    profit_totalText[1].text = "0";
                else
                    profit_totalText[1].text = $"+{total}";
            }
            else
                profit_totalText[1].text = $"<color=#A91111>{total}</color>";
        }

        // 평점
        {
            float total = 0;
            StringBuilder st = new StringBuilder();
            var list3 = System.Enum.GetValues(typeof(GetRatingSource));
            foreach (var temp in list3)
            {
                if (!dayOne_Rating.ContainsKey((GetRatingSource)temp))
                    dayOne_Rating.Add(new SerializableDictionary<GetRatingSource, float>.Pair { Key = (GetRatingSource)temp, Value = 0 });
            }

            int count = dayOne_Rating.Count;
            foreach (var temp in dayOne_Rating)
            {
                float value = temp.Value;
                if (value >= 0)
                {
                    if (value == 0)
                        st.Append("0");
                    else
                        st.AppendFormat("+{0:0.#}", value);
                }
                else
                    st.AppendFormat("<color=#A91111>{0:0.#}</color>", value);
                total += value;
                count--;
                if (count > 0)
                    st.AppendLine();
            }
            profitText[3].text = st.ToString();

            if (total >= 0)
            {
                if (total == 0)
                    profit_totalText[3].text = "0";
                else
                    profit_totalText[3].text = $"+{total:0.#}";
            }
            else
                profit_totalText[3].text = $"<color=#A91111>{total:0.#}</color>";
        }

        accountText[1].text = $"<sprite={2}> {tm.GetCommons("Money")} ({gold})";
        if (rating > 0)
            accountText[2].text = $"<sprite={1}> {tm.GetCommons("Rating")} ({rating:0.#})";
        else
            accountText[2].text = $"<sprite={1}> {tm.GetCommons("Rating")} (<color=#A91111>{rating:0.#}</color>)";

    }

    public void NextDay_Late()
    {
        accountObj.SetActive(false);

        bool showWarning = false;
        if (rating <= 0f)
        {
            if (!warning_gameOver)
            {
                warning_gameOver = true;
                showWarning = true;
            }
            else
            {
                GameOver();
                return;
            }
        }
        else
            warning_gameOver = false;

        openImage.SetActive(true);
        closeImage.SetActive(false);

        if (EndTimeEvent != null)
            EndTimeEvent(null, false);
        EndTime = false;

        dayOne_Gold = new SerializableDictionary<GetGoldSource, int>();
        dayOne_Rating = new SerializableDictionary<GetRatingSource, float>();

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

            if (showWarning)
            {
                warningQueue.Enqueue(0);
            }
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
    public void ShowGameOverWarning(bool on)
    {
        gameOverWarningObj.gameObject.SetActive(on);

        if (!on)
        {
            ShowWarningQueue();
        }
        else
        {
            gameOverWarningRect.localScale = 0.01f * Vector3.one;
            gameOverWarningRect.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.OutElastic).SetUpdate(true);
            // 관리탭 강조하기
            UIManager.Instance.shopUI.SelectSubPanel(1);
        }
    }

    public void ShowWarningQueue()
    {
        if (warningQueue.Count > 0)
        {
            int idx = warningQueue.Dequeue();

            switch (idx)
            {
                case 0:
                    ShowGameOverWarning(true);
                    break;
                case 1:
                    break;
            }
        }
    }


    #region UI 표시
    public void CarSpeedUI(float carSpeed, float maxSpeed)
    {
        if (stop_control) return;

        float absoluteCarSpeed = Mathf.Abs(carSpeed);
        float vel = Mathf.FloorToInt(absoluteCarSpeed);

        displaySpeed = Mathf.Lerp(displaySpeed, vel, Time.fixedDeltaTime);
        carSpeedText.text = ((int)displaySpeed).ToString();

        float percent = vel / maxSpeed;
        if (percent > 1f) percent = 1f;
        else if (percent < 0f) percent = 0f;

        //speedNiddle.localEulerAngles = new Vector3(0f, 0f, -180f * percent + 90f);
        //speedNiddle.localEulerAngles = Vector3.Lerp(speedNiddle.localEulerAngles, new Vector3(0f, 0f, -180f * percent + 90f), Time.fixedDeltaTime);

        speedNiddle.rotation = Quaternion.Lerp(speedNiddle.rotation, Quaternion.Euler(0f, 0f, -180f * percent + 90f), Time.fixedDeltaTime);
    }

    public void AddGold(int value, GetGoldSource source)
    {
        //if (value > 0f) value = (int)(value * (1f + ResearchManager.Instance.globalEffect.goldGet));
        int target = gold + value;
        DOVirtual.Int(gold, target, 0.75f, (x) =>
        {
            displayGold = x;

            for (int i = 0; i < goldText.Length; i++)
                goldText[i].text = $"{displayGold}$";

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

        for (int i = 0; i < goldText.Length; i++)
            goldText[i].text = $"{displayGold}$";
    }
    public void AddRating(float value, GetRatingSource source)
    {
        //if (value > 0f) value *= (1f + ResearchManager.Instance.globalEffect.ratingGet);
        float target = rating + value;
        DOVirtual.Float(rating, target, 0.75f, (x) =>
        {
            displayRating = x;

            for (int i = 0; i < ratingText.Length; i++)
            {
                if (displayRating <= 0)
                    ratingText[i].text = $"<color=#A91111>{displayRating:0.#}</color>";
                else
                    ratingText[i].text = $"{displayRating:0.#}";
            }

        }).SetEase(Ease.OutCirc).SetUpdate(true);
        rating = target;

        if (!dayOne_Rating.ContainsKey(source))
            dayOne_Rating.Add(new SerializableDictionary<GetRatingSource, float>.Pair { Key = source, Value = value });
        else
            dayOne_Rating[source] += value;
    }
    public void SetRating(float value)
    {
        displayRating = value;
        rating = value;

        for (int i = 0; i < ratingText.Length; i++)
        {
            if (displayRating <= 0)
                ratingText[i].text = $"<color=#A91111>{displayRating:0.#}</color>";
            else
                ratingText[i].text = $"{displayRating:0.#}";
        }
    }
    #endregion

}
