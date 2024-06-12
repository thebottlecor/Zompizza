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

    public int combo;

    public float RatingDailyChange { get; private set; }
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
        //loan,
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

    [Header("정산서")]
    public GameObject accountObj;
    public TextMeshProUGUI[] accountText;
    public TextMeshProUGUI[] profitText;
    public TextMeshProUGUI[] profit_totalText;
    public Button nextDayBtn;
    public TextMeshProUGUI nextDayBtn_Text;

    [Header("게임 오버")]
    public GameObject gameOverObj;
    public Button gameOverBtn_ToLobby;
    public TextMeshProUGUI[] gameOverText;
    public TextMeshProUGUI gameOverBtn_ToLobby_Text;
    public bool loading;

    [Header("평점 획득 축하")] // 원래는 빚 갚음
    public GameObject congratulationsObj;
    public Button congratulationBtn_ToLobby;
    public TextMeshProUGUI[] congratulationsText;
    public TextMeshProUGUI congratulationsBtn_Text;
    public bool CongratulationTriggered { get; private set; }

    [Header("평점 게임 오버 경고")]
    public GameObject gameOverWarningObj;
    public RectTransform gameOverWarningRect;
    public TextMeshProUGUI gameOverWarningBtn_Text;
    public TextMeshProUGUI gameOverWarning_Text;
    public TextMeshProUGUI gameOverWarningDetail_Text;
    // 패배 트리거
    public bool warning_gameOver;

    [Header("습격받은 결과")]
    public GameObject raidObj;
    public RectTransform raidRect;
    public TextMeshProUGUI raidBtn_Text;
    public TextMeshProUGUI raid_Text;
    public TextMeshProUGUI raidDetail_Text;
    public List<int> tenDays_RaidRecords;

    public Queue<int> warningQueue;

    [Header("차량 변경")]
    public int currentVehicle;
    public PlayerControllerData[] controllerData;
    public bool[] unlockedVehicles;
    public int[] costVehicles;
    public float[] ratingVehicles;

    [Header("선물 상자")]
    public GiftGoal[] giftGoals;

    [Space(10f)]

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

            if (day == 5) GameEventManager.Instance.SetEvent(1); // 6일차 가게 닫은 후 이장 이벤트
        }
    }

    private void Start()
    {
        ingredients = new SerializableDictionary<Ingredient, int>();
        foreach (var temp in DataManager.Instance.ingredientLib.ingredientTypes)
        {
            Ingredient key = (Ingredient)temp;
            ingredients.Add(new SerializableDictionary<Ingredient, int>.Pair(key, 0));
        }
        int initAmount = 10;
        ingredients[Ingredient.meat1] = initAmount;
        ingredients[Ingredient.meat2] = initAmount;
        ingredients[Ingredient.vegetable1] = initAmount;
        ingredients[Ingredient.vegetable2] = initAmount;
        ingredients[Ingredient.herb1] = initAmount;
        ingredients[Ingredient.herb2] = initAmount;

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
        congratulationBtn_ToLobby.onClick.AddListener(() => { LoadingSceneManager.Instance.ToLobby(); });

        tenDays_RaidRecords = new List<int>();

        unlockedVehicles = new bool[controllerData.Length];

        TextUpdate();

        day = 0;
        DayStringUpdate();
        ResearchManager.Instance.Init();
        //LoanManager.Instance.Init();
        UIManager.Instance.shopUI.Init();
        TutorialManager.Instance.Init();
        OrderManager.Instance.Init();

        // 저장 불러오기시 주의
        RandomGiftBox();
        ResearchManager.Instance.ToggleAllHiddenRecipe(true);

        InitPlayer();
    }

    public void TextUpdate()
    {
        //accountText[1].text = $"<sprite={2}> {tm.GetCommons("Money")}";
        //accountText[2].text = $"<sprite={1}> {tm.GetCommons("Rating")}";

        StringBuilder st = new StringBuilder();
        st.Append(tm.GetCommons("Delivery")).AppendLine();
        st.Append(tm.GetCommons("Explore")).AppendLine();
        st.Append(tm.GetCommons("Zombie")).AppendLine();
        st.Append(tm.GetCommons("Upgrade"));
        //.AppendLine();
        //st.Append(tm.GetCommons("Loan")).AppendLine();

        profitText[0].text = st.ToString();

        StringBuilder st2 = new StringBuilder();
        st2.Append(tm.GetCommons("Delivery")).AppendLine();
        st2.Append(tm.GetCommons("NotCompleted")).AppendLine();
        st2.Append(tm.GetCommons("NotAccepted")).AppendLine();

        profitText[2].text = st2.ToString();

        profit_totalText[0].text = tm.GetCommons("Total");
        profit_totalText[2].text = tm.GetCommons("Total");

        gameOverText[0].text = tm.GetCommons("Gameover");

        nextDayBtn_Text.text = $"> {tm.GetCommons("NextDay")} <";
        gameOverBtn_ToLobby_Text.text = $"> {tm.GetCommons("Menu")} <";

        gameOverWarningBtn_Text.text = tm.GetCommons("Close");
        gameOverWarning_Text.text = tm.GetCommons("Warning");

        raid_Text.text = tm.GetCommons("Raid");
        raidBtn_Text.text = tm.GetCommons("RaidClose");

        congratulationsText[0].text = tm.GetCommons("Congratulations");
        congratulationsText[1].text = tm.GetCommons("CompleteRating");
        //congratulationsBtn_Text.text = tm.GetCommons("Resume");
        congratulationsBtn_Text.text = $"> {tm.GetCommons("Menu")} <";
    }

    private void Update()
    {
        if (loading) return;

        var tuto = TutorialManager.Instance;
        if (!tuto.training && !tuto.debug_fixTime && !tuto.debug_fixTime_Noon)
            timer += Time.deltaTime;

        if (tuto.debug_fixTime_Noon)
            timer = Constant.oneHour * 6;

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

                if (day == 5) GameEventManager.Instance.SetEvent(1); // 6일차 가게 닫은 후 이장 이벤트

                if (EndTimeEvent != null)
                    EndTimeEvent(null, true);

                timeText.text = dayStr[1];

                HideAllGiftBox();
                ResearchManager.Instance.ToggleAllHiddenRecipe(false);
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

    private void DayStringUpdate()
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
        player.ShakeOffAllZombies();
        ZombiePooler.Instance.ZombieReset();
        //LoanManager.Instance.PayInterest();

        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetAutoKill(true);
        sequence.AppendCallback(() =>
        {
            accountObj.SetActive(false);
            darkCanvas.alpha = 0f;
            darkCanvas.interactable = false;
            darkCanvas.blocksRaycasts = true;
            loading = true;
            UINaviHelper.Instance.SetFirstSelect();
        });
        sequence.Append(darkCanvas.DOFade(1f, 0.5f));
        sequence.AppendInterval(0.5f);
        sequence.AppendCallback(() =>
        {
            darkCanvas.interactable = true;
            UpdateAccountUI();
            accountObj.SetActive(true);
            UINaviHelper.Instance.SetFirstSelect();

            if (day >= 10)
            {
                if (!CongratulationTriggered)
                {
                    Congratulation(true);
                    AudioManager.Instance.PlaySFX(Sfx.complete);
                    UIManager.Instance.shopUI.upgradeDirection.Show();
                }
            }
        });
    }
    private void UpdateAccountUI()
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

            RatingDailyChange = total;
        }

        accountText[1].text = $"<sprite={2}> {tm.GetCommons("Money")} ({gold})";
        //if (rating >= RivalManager.Instance.rating)
        if (rating > 0)
            accountText[2].text = $"<sprite={1}> {tm.GetCommons("Rating")} ({rating:0.#})";
        else
            accountText[2].text = $"<sprite={1}> {tm.GetCommons("Rating")} (<color=#A91111>{rating:0.#}</color>)";

    }

    private void NextDay_Late()
    {
        accountObj.SetActive(false);
        UINaviHelper.Instance.SetFirstSelect();

        //int loanWarning = LoanManager.Instance.NextDayLate();
        int loanWarning = -1;

        bool showWarning = false;
        //if (rating < RivalManager.Instance.rating)
        if (rating <= 0)
        {
            if (!warning_gameOver)
            {
                warning_gameOver = true;
                showWarning = true;
            }
            else
            {
                gameOverText[1].text = tm.GetCommons("Gameover2");
                GameOver();
                return;
            }
        }
        else
            warning_gameOver = false;

        if (loanWarning == 0)
        {
            gameOverText[1].text = tm.GetCommons("Gameover3");
            GameOver();
            return;
        }

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
            bool hasResult = ExplorationManager.Instance.ShowResultPanel();

            if (showWarning)
            {
                warningQueue.Enqueue(0);
            }
            if (loanWarning == 1)
            {
                warningQueue.Enqueue(1);
            }

            if (!hasResult)
            {
                ShowWarningQueue();
            }

            TutorialManager.Instance.NextDay();
            StatManager.Instance.NextDay();
            GameEventManager.Instance.NextDay();
            RandomGiftBox();
            ResearchManager.Instance.ToggleAllHiddenRecipe(true);
            //RivalManager.Instance.NextDay();
            //ShowRatingText();
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
        UINaviHelper.Instance.SetFirstSelect();
    }
    public void Congratulation(bool on)
    {
        if (on)
        {
            if (CongratulationTriggered) return;
            CongratulationTriggered = true;

            darkCanvas.alpha = 1f;
            darkCanvas.blocksRaycasts = true;
            darkCanvas.interactable = true;
            loading = true;
        }
        else
        {
            darkCanvas.alpha = 0f;
            darkCanvas.blocksRaycasts = false;
            darkCanvas.interactable = false;
            loading = false;
        }
        congratulationsObj.SetActive(on);
        UINaviHelper.Instance.SetFirstSelect();
    }
    public void ShowGameOverWarning(bool on)
    {
        //gameOverWarningDetail_Text.text = string.Format(tm.GetCommons("GameoverWarning"), "<size=90%><sprite=1></size>", $"<color=#760048>{RivalManager.Instance.rating:0.#}</color>");
        gameOverWarningDetail_Text.text = string.Format(tm.GetCommons("GameoverWarning"), "<size=90%><sprite=1></size>", $"<color=#760048>0</color>");

        gameOverWarningObj.SetActive(on);

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
        UINaviHelper.Instance.SetFirstSelect();
    }

    public void ShowWarningQueue()
    {
        if (warningQueue.Count > 0)
        {
            int idx = warningQueue.Dequeue();

            switch (idx)
            {
                case 0: // 평점 위험
                    ShowGameOverWarning(true);
                    break;
                case 1: // 대출 위험
                    //LoanManager.Instance.ShowLoanWarning(true);
                    //TutorialManager.Instance.NextDay();
                    break;
                case 2: // 습격
                    ShowRaidPanel();
                    break;
            }
        }
        else
        {
            UINaviHelper.Instance.SetFirstSelect();
            if (day == 2) GameEventManager.Instance.SetEvent(0); // 3일차 아침 고아원 원장 이벤트
            if (day == 8) GameEventManager.Instance.SetEvent(2); // 9일차 아침 고양이 이벤트
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
            ShowGoldText();

        }).SetEase(Ease.OutCirc).SetUpdate(true);
        gold = target;

        if (!dayOne_Gold.ContainsKey(source))
            dayOne_Gold.Add(new SerializableDictionary<GetGoldSource, int>.Pair { Key = source, Value = value });
        else
            dayOne_Gold[source] += value;

        if (value > 0)
            StatManager.Instance.totalEarn += value;
    }

    private void ShowGoldText()
    {
        for (int i = 0; i < goldText.Length; i++)
            goldText[i].text = $"{displayGold}$";
    }

    public void SetGold(int value)
    {
        displayGold = value;
        gold = value;
        ShowGoldText();
    }
    public void AddRating(float value, GetRatingSource source)
    {
        //if (value > 0f) value *= (1f + ResearchManager.Instance.globalEffect.ratingGet);
        float target = rating + value;
        DOVirtual.Float(rating, target, 0.75f, (x) =>
        {
            displayRating = x;
            ShowRatingText();
        }).SetEase(Ease.OutCirc).SetUpdate(true);
        rating = target;

        if (!dayOne_Rating.ContainsKey(source))
            dayOne_Rating.Add(new SerializableDictionary<GetRatingSource, float>.Pair { Key = source, Value = value });
        else
            dayOne_Rating[source] += value;

        StatManager.Instance.totalRating += value;
    }

    private void ShowRatingText()
    {
        //float rivalRating = RivalManager.Instance.rating;

        for (int i = 0; i < ratingText.Length; i++)
        {
            //if (displayRating <= 0)
            //    ratingText[i].text = $"<color=#A91111>{displayRating:0.#}</color> / {Constant.winRating:F0}";
            //else
            //    ratingText[i].text = $"{displayRating:0.#} / {Constant.winRating:F0}";

            if (displayRating <= 0)
                ratingText[i].text = $"<color=#A91111>{displayRating:0.#}</color>";
            else
                ratingText[i].text = $"{displayRating:0.#}";

            //if (displayRating >= rivalRating)
            //    ratingText[i].text = $"{displayRating:0.#} / <sprite=6> {rivalRating:0.#}";
            //else
            //    ratingText[i].text = $"<color=#A91111>{displayRating:0.#}</color> / <sprite=6> {rivalRating:0.#}";
        }

        //if (rating >= rivalRating)
        //    UIManager.Instance.shopUI.rivalRatingText.text = $"<sprite=1> {rivalRating:0.#}";
        //else
        //    UIManager.Instance.shopUI.rivalRatingText.text = $"<sprite=1> <color=#A91111>{rivalRating:0.#}</color>";
    }

    public void SetRating(float value)
    {
        displayRating = value;
        rating = value;
        ShowRatingText();
    }
    #endregion

    #region 습격
    public void CheckRaid_BeforeExploration() 
    {
        if (day % 10 == 0)
        {
            tenDays_RaidRecords = new List<int>();
        }

        // 10일 간격으로, 최대 5번의 습격만 발생할 수 있음
        int tenDays_RaidCount = 0;
        for (int i = 0; i < tenDays_RaidRecords.Count; i++)
        {
            tenDays_RaidCount += tenDays_RaidRecords[i];
        }
        if (tenDays_RaidCount >= 5)
            return;

        if (CheckRaid())
        {
            warningQueue.Enqueue(2);
            tenDays_RaidRecords.Add(1);
            AudioManager.Instance.PlaySFX(Sfx.raid);
        }
        else
        {
            // 습격 왔을 땐, 빼먹지 않음
            CatSteal();
        }
    }
    private bool CheckRaid()
    {
        int hasRes = HasIngredient;

        // 가진 자원수가 30 넘을 때, 50% 확률로 습격 발생, 습격 관련 업그레이드에 따라서 30%~0% 만큼 자원을 빼앗김, 2 티어부터 발생
        if (hasRes > 30 && ResearchManager.Instance.globalEffect.tier >= 1 && UnityEngine.Random.Range(0, 2) == 1)
        {
            float percent = 0.3f;
            switch (ResearchManager.Instance.globalEffect.raidDefense)
            {
                case 1:
                    percent = 0.2f;
                    break;
                case 2:
                    percent = 0.1f;
                    break;
                case 3:
                    percent = 0f;
                    break;
            }
            if (GameEventManager.Instance.hasCat) percent -= 0.05f;
            if (percent < 0) percent = 0f;

            int count = Mathf.CeilToInt(hasRes * percent);
            if (count >= hasRes) count = hasRes - 1;
            if (count <= 0)
                return false;

            var resultDict = new Dictionary<Ingredient, int>();
            List<Ingredient> tempIngredients = new List<Ingredient>();
            foreach (var temp in ingredients)
            {
                if (temp.Value > 0)
                {
                    tempIngredients.Add(temp.Key);
                }
            }

            while (count > 0)
            {
                int rand = UnityEngine.Random.Range(0, tempIngredients.Count);
                Ingredient ingredient = tempIngredients[rand];

                if (ingredients[ingredient] > 0)
                {
                    ingredients[ingredient]--;

                    if (!resultDict.ContainsKey(ingredient))
                        resultDict.Add(ingredient, 1);
                    else
                        resultDict[ingredient]++;

                    count--;
                }
            }

            int rowCount = 0;

            StringBuilder st = new StringBuilder();

            foreach (var temp in resultDict)
            {
                if (rowCount < 2)
                {
                    st.AppendFormat("<sprite={0}><size=90%>{2}</size> -{1} ", (int)temp.Key + Constant.ingredientSpriteOffset, temp.Value, tm.GetIngredient(temp.Key));
                    rowCount++;
                }
                else
                {
                    st.AppendFormat("<sprite={0}><size=90%>{2}</size> -{1}\n", (int)temp.Key + Constant.ingredientSpriteOffset, temp.Value, tm.GetIngredient(temp.Key));
                    rowCount = 0;
                }
            }

            raidDetail_Text.text = st.ToString();

            return true;
        }

        return false;
    }
    public void ShowRaidPanel()
    {
        raidObj.SetActive(true);

        raidRect.localScale = 0.01f * Vector3.one;
        raidRect.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.OutElastic).SetUpdate(true);

        UINaviHelper.Instance.SetFirstSelect();
    }
    public void HideRaidPanel()
    {
        raidObj.SetActive(false);
        ShowWarningQueue();
    }

    public void CatSteal() // 고양이 들일시 50% 확률로 매일 1개씩 재료 빼먹음
    {
        if (GameEventManager.Instance.hasCat)
        {
            if (UnityEngine.Random.Range(0, 2) == 1)
            {
                if (IngredientSteal(1))
                {
                    AudioManager.Instance.PlaySFX(Sfx.cat);
                }
            }
        }
    }

    public bool IngredientSteal(int count)
    {
        int hasRes = HasIngredient;
        if (hasRes <= 0) return false;
        if (count >= hasRes) count = hasRes - 1;
        if (count <= 0) return false;

        List<Ingredient> tempIngredients = new List<Ingredient>();
        foreach (var temp in ingredients)
        {
            if (temp.Value > 0)
            {
                tempIngredients.Add(temp.Key);
            }
        }

        while (count > 0)
        {
            int rand = UnityEngine.Random.Range(0, tempIngredients.Count);
            Ingredient ingredient = tempIngredients[rand];

            if (ingredients[ingredient] > 0)
            {
                ingredients[ingredient]--;
                count--;
            }
        }
        return true;
    }
    public bool RandomIngredientGet(int count)
    {
        if (count <= 0) return false;

        while (count > 0)
        {
            Ingredient ingredient = OrderManager.Instance.GetRandomIngredient_HighTier();
            ingredients[ingredient]++;
            count--;
        }
        return true;
    }
    public Ingredient RandomIngredientGet()
    {
        Ingredient ingredient = OrderManager.Instance.GetRandomIngredient_HighTier();
        ingredients[ingredient]++;
        return ingredient;
    }
    #endregion

    #region 차량 변경
    public void InitPlayer()
    {
        currentVehicle = 0;
        controllerData[0].SetData(player);
        unlockedVehicles[0] = true;
    }

    public void ChangeVehicle(int idx)
    {
        if (currentVehicle >= 0)
            controllerData[currentVehicle].gameObject.SetActive(false);
        currentVehicle = idx;
        controllerData[idx].SetData(player);
    }
    
    public bool BuyVehicle(int idx)
    {
        //if (!unlockedVehicles[idx] && gold >= costVehicles[idx])
        //{
        //    // 성공 연출
        //    AudioManager.Instance.PlaySFX(Sfx.complete);
        //    AddGold(-1 * costVehicles[idx], GetGoldSource.upgrade);
        //    unlockedVehicles[idx] = true;
        //    return true;
        //}
        if (!unlockedVehicles[idx] && rating >= ratingVehicles[idx])
        {
            // 성공 연출
            AudioManager.Instance.PlaySFX(Sfx.complete);
            unlockedVehicles[idx] = true;
            return true;
        }
        else
        {
            AudioManager.Instance.PlaySFX(Sfx.deny);
            return false;
        }
    }
    #endregion

    #region 선물 상자
    public void RandomGiftBox() // 해가 뜬 후 최대 5개의 박스 활성화
    {
        HideAllGiftBox();

        giftGoals.Shuffle();

        for (int i = 0; i < 5; i++)
        {
            giftGoals[i].Show();
        }
    }
    public void HideAllGiftBox() // 해가 진 후 모든 박스 비활성화
    {
        for (int i = 0; i < giftGoals.Length; i++)
        {
            giftGoals[i].Hide();
        }
    }
    #endregion
}
