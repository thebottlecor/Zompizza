using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Text;
using UnityEngine.InputSystem;

public class GM : Singleton<GM>
{
    [Serializable]
    public struct SaveData
    {
        public int day;
        public float timer;
        public int gold;
        public float rating;
        public float researchPoint;

        public SerializableDictionary<Ingredient, int> ingredients;

        public int currentVehicle;
        public bool[] unlockedVehicles;

        public int vehicleMilestone;
        public int tierUpMilestone;

        public List<SavePosition> installJumpPostions;
    }
    public SaveData Save()
    {
        SaveData data = new()
        {
            day = this.day,
            timer = this.timer,

            gold = this.gold,
            rating = this.rating,
            researchPoint = this.researchPoint,

            ingredients = this.ingredients,

            currentVehicle = this.currentVehicle,
            unlockedVehicles = this.unlockedVehicles,

            vehicleMilestone = UIManager.Instance.vehicleMilestone,
            tierUpMilestone = UIManager.Instance.tierUpMilestone,

            installJumpPostions = this.installJumpPostions,
        };

        return data;
    }
    public void Load(SaveData data)
    {
        day = data.day;
        SetTimer(data.timer);

        SetGold(data.gold);
        SetRating(data.rating);
        SetRatingPoint(data.researchPoint);

        ingredients = data.ingredients;

        currentVehicle = data.currentVehicle;
        if (data.unlockedVehicles != null)
        {
            if (data.unlockedVehicles.Length < unlockedVehicles.Length)
            {
                for (int i = 0; i < data.unlockedVehicles.Length; i++)
                {
                    unlockedVehicles[i] = data.unlockedVehicles[i];
                }
            }
            else
                unlockedVehicles = data.unlockedVehicles;
        }
        unlockedVehicles[0] = true; // 무조건
        if (currentVehicle < 0 || currentVehicle >= unlockedVehicles.Length) currentVehicle = 0;
        ChangeVehicle(currentVehicle);

        UIManager.Instance.vehicleMilestone = data.vehicleMilestone;
        UIManager.Instance.tierUpMilestone = data.tierUpMilestone;

        installJumpPostions = data.installJumpPostions;
        for (int i = 0; i < installJumpPostions.Count; i++)
        {
            Instantiate(installJumpObj, installJumpPostions[i], Quaternion.identity); // 세이브에서 불러옴
        }
    }

    public bool stop_control;
    public int slotNum;

    public int day;
    public float timer;
    [HideInInspector] public float remainTime;

    public TextMeshProUGUI timeText;
    private string[] dayStr;
    public GameObject openImage;
    public GameObject closeImage;

    private float displaySpeed;
    public TextMeshProUGUI carSpeedText;
    public RectTransform speedNiddle;

    public PlayerController player;
    public Transform pizzeriaPos;
    public Transform shopEnterPos;

    private int displayGold;
    public TextMeshProUGUI[] goldText;
    private float displayRating;
    public TextMeshProUGUI[] ratingText;
    private float displayResearchPoints;
    public TextMeshProUGUI[] researchPointsText;

    // 플레이어가 가진 자원
    public int gold;
    public float rating;
    public float researchPoint; // 연구로 소모되는 평점

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
        villager,
        upgrade,
        //zombie,
        //loan,
    }
    public enum GetRatingSource
    {
        delivery,
        notComplete,
        //notAccepted,
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
    public bool raided;

    public Queue<int> warningQueue;

    [Header("차량 변경")]
    public int currentVehicle;
    public bool[] unlockedVehicles;
    public PlayerControllerData[] controllerData;
    public int[] costVehicles;
    public float[] ratingVehicles;

    [Header("한밤중")]
    public bool midNight;
    public ShopGate[] shopGates;
    public ZombieEnvSound zombieEnvSound;
    public GameObject runIndicator;
    public TextMeshProUGUI runIndicatorTMP;

    [Header("블러드문")]
    public bool bloodMoon;

    [Space(10f)]
    public GameObject returnIndicator;
    public GameObject rainObj;

    [Space(10f)]
    public Transform footBall;
    private Vector3 footBallPos;

    [Space(10f)]
    public bool pizzeriaStay;
    public bool install;
    public GameObject installJumpObj;
    private List<SavePosition> installJumpPostions;

    public bool lastLaunch;

    public static EventHandler<bool> EndTimeEvent; // true일시 마감
    private SerializableDictionary<KeyMap, KeyMapping> HotKey => SettingManager.Instance.keyMappings;
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
        dayStr = new string[3];
        ingredients = new SerializableDictionary<Ingredient, int>();
        foreach (var temp in DataManager.Instance.ingredientLib.ingredientTypes)
        {
            Ingredient key = (Ingredient)temp;
            ingredients.Add(new SerializableDictionary<Ingredient, int>.Pair(key, 0));
        }
        installJumpPostions = new List<SavePosition>();

        dayOne_Gold = new SerializableDictionary<GetGoldSource, int>();
        dayOne_Rating = new SerializableDictionary<GetRatingSource, float>();
        warningQueue = new Queue<int>();

        darkCanvas.alpha = 0f;
        darkCanvas.interactable = false;
        darkCanvas.blocksRaycasts = false;

        //nextDayBtn.onClick.AddListener(() => { NextDay_Late(); });
        nextDayBtn.onClick.AddListener(() => { Show_SpaceshipProject(); });
        gameOverBtn_ToLobby.onClick.AddListener(() => { LoadingSceneManager.Instance.ToLobby(); });
        //congratulationBtn_ToLobby.onClick.AddListener(() => { LoadingSceneManager.Instance.ToLobby(); });
        congratulationBtn_ToLobby.onClick.AddListener(() => { LoadingSceneManager.Instance.EpilogueStart(); });

        tenDays_RaidRecords = new List<int>();
        unlockedVehicles = new bool[controllerData.Length];

        //

        GameSaveData gameSaveData = null;

        GameStartInfo startInfo = new GameStartInfo();
        if (LoadingSceneManager.Instance != null)
        {
            startInfo = LoadingSceneManager.Instance.StartInfo;
            slotNum = startInfo.slotNum;
        }

        bool saveLoad = false;
        if (SaveManager.Instance != null && !startInfo.saveName.Equals(string.Empty) && (startInfo.slotNum >= 1 && startInfo.slotNum <= 3))
        {
            saveLoad = true;
            gameSaveData = SaveManager.Instance.LoadSaveData(startInfo.slotNum, startInfo.saveName);

            // 리턴된 gameSaveData로 값 적용하기

            Load(gameSaveData.gm.data);
        }
        else
        {
            // 게임 초기화

            //cityName = startInfo.cityName;

            day = 0;

            int initAmount = 10;
            ingredients[Ingredient.meat1] = initAmount;
            ingredients[Ingredient.meat2] = initAmount;
            ingredients[Ingredient.vegetable1] = initAmount;
            ingredients[Ingredient.vegetable2] = initAmount;
            ingredients[Ingredient.herb1] = initAmount;
            ingredients[Ingredient.herb2] = initAmount;

            SetGold(1000);
            float initRating = 0f;
            SetRating(initRating); // initRating = 0f
            SetRatingPoint(initRating); // initRating = 0f

            InitPlayer(); // Tutorial보다 위에
        }

        //CallAfterStart(gameSaveData);
        //SaveDataLoading = false;

        UIManager.Instance.Init();

        LoadingSceneManager.Instance.logueLoading = false;

        TextUpdate();

        DayStringUpdate();
        ResearchManager.Instance.Init();
        //LoanManager.Instance.Init();
        UIManager.Instance.shopUI.Init(saveLoad, gameSaveData);
        TutorialManager.Instance.Init(startInfo.tutorial);
        OrderManager.Instance.Init();
        VillagerManager.Instance.Init(saveLoad, gameSaveData);

        // 저장 불러오기시 주의 >> 매일 아침에만 저장하니 이제 신경쓰지 않아도 됨 09.30
        GiftBoxHide();

        if (saveLoad)
        {
            RocketManager.Instance.Load(gameSaveData.gm.rocket);
            StatManager.Instance.Load(gameSaveData.gm.stat);
            ResearchManager.Instance.Load(gameSaveData.research.data);

            SpecialDay();
            player.transform.position = shopEnterPos.position;
            ShowWarningQueue(); // 세이브 로드할 경우 아침 이벤트 재생시키기
        }
        else
        {
            rainObj.SetActive(false);
        }
        ResearchManager.Instance.ToggleAllHiddenRecipe(true);
        footBallPos = footBall.position;
    }

    public void TextUpdate()
    {
        //accountText[1].text = $"<sprite={2}> {tm.GetCommons("Money")}";
        //accountText[2].text = $"<sprite={1}> {tm.GetCommons("Rating")}";

        StringBuilder st = new StringBuilder();
        st.Append(tm.GetCommons("Delivery")).AppendLine();
        st.Append(tm.GetCommons("Explore")).AppendLine();
        st.Append(tm.GetCommons("Villager")).AppendLine();
        //st.Append(tm.GetCommons("Upgrade")).AppendLine();
        st.Append(tm.GetCommons("Spaceship"));
        //st.Append(tm.GetCommons("Zombie"));
        //.AppendLine();
        //st.Append(tm.GetCommons("Loan")).AppendLine();

        profitText[0].text = st.ToString();

        StringBuilder st2 = new StringBuilder();
        st2.Append(tm.GetCommons("Delivery")).AppendLine();
        st2.Append(tm.GetCommons("NotCompleted")).AppendLine();
        //st2.Append(tm.GetCommons("NotAccepted")).AppendLine();

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
        Vector3 rainPos = player.transform.position;
        rainPos.y = 20f;
        rainObj.transform.position = rainPos;

        if (loading) return;
        if (midNight) return;
        if (TutorialManager.Instance.blackScreen.activeSelf) return;
        if (GameEventManager.Instance.eventPanel.activeSelf) return;
        if (DialogueManager.Instance.eventPanel.activeSelf) return;

        TimeUpdate();

        Return_Indicator_Update();
    }

    private void Return_Indicator_Update()
    {
        if (TutorialManager.Instance.training && TutorialManager.Instance.step <= 1)
        {
            if (returnIndicator.activeSelf) returnIndicator.SetActive(false);
        }
        else
        {
            if (OrderManager.Instance.currentAcceptance == 0)
            {
                var resuced = VillagerManager.Instance.miniUI;
                if (resuced.gameObject.activeSelf)
                {
                    if (resuced.pinObjs[1].activeSelf)
                    {
                        float dist = (player.transform.position - pizzeriaPos.transform.position).magnitude;
                        if (dist > 100f)
                        {
                            if (!returnIndicator.activeSelf) returnIndicator.SetActive(true);
                        }
                        else
                        {
                            if (returnIndicator.activeSelf) returnIndicator.SetActive(false);
                            resuced.Hide();
                        }
                    }
                    else
                    {
                        if (returnIndicator.activeSelf) returnIndicator.SetActive(false);
                    }
                }
                else
                {
                    float dist = (player.transform.position - pizzeriaPos.transform.position).magnitude;
                    if (dist > 100f)
                    {
                        if (!returnIndicator.activeSelf) returnIndicator.SetActive(true);
                    }
                    else
                    {
                        if (returnIndicator.activeSelf) returnIndicator.SetActive(false);
                    }
                }
            }
            else
            {
                if (returnIndicator.activeSelf) returnIndicator.SetActive(false);
            }
        }
    }

    public void TimeUpdate()
    {
        var tuto = TutorialManager.Instance;
        if (!tuto.debug_fixTime && !tuto.debug_fixTime_Noon)
        {
            if (tuto.training && tuto.step <= 7)
            {

            }
            else
            {
                timer += Time.deltaTime;
            }
        }

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

                if (EndTimeEvent != null)
                    EndTimeEvent(null, true);

                timeText.text = dayStr[1];

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
        SetLight(timePercent, hour);
    }

    public void SetTimer(float timer)
    {
        this.timer = timer;
        remainTime = Constant.dayTime - timer;
    }

    public void SetLight(float timePercent, int hour)
    {
        if (bloodMoon)
            globalLight.color = DataManager.Instance.uiLib.timeLightGradient_Bloodmoon.Evaluate(timePercent);
        else
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
        //dayStr[1] = string.Format(tm.GetCommons("Day"), day + 1) + "  <color=#000000>18:00</color>"; // 영업 종료시
        dayStr[1] = "<color=#000000>" + string.Format(tm.GetCommons("Day"), day + 1) + "  18:00</color>"; // 영업 종료시
        dayStr[2] = string.Format(tm.GetCommons("Day"), day + 1) + "  " + tm.GetCommons("Midnight"); // 한밤중
    }

    public void NextDay()
    {
        InstallFuck(false);
        rainObj.SetActive(false);
        zombieEnvSound.Mute(true);
        for (int i = 0; i < shopGates.Length; i++)
        {
            shopGates[i].alwaysClosed = true;
        }

        accountText[0].text = string.Format(tm.GetCommons("Day"), day + 1);
        day++;
        UIManager.Instance.shopUI.DayFirstReview(day);
        player.ShakeOffAllZombies();
        ZombiePooler.Instance.ZombieReset();
        //LoanManager.Instance.PayInterest();
        VillagerManager.Instance.NextDay();

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

            //UpdateAccountUI();
            //accountObj.SetActive(true);
            //UINaviHelper.Instance.SetFirstSelect();

            accountObj.SetActive(true);
            StartCoroutine(UpdateAccountUI2());
        });

        footBall.position = footBallPos;
    }
    private IEnumerator UpdateAccountUI2()
    {
        nextDayBtn.gameObject.SetActive(false);

        accountText[1].text = $"<sprite={2}> {tm.GetCommons("Money")}";
        accountText[2].text = $"<sprite={1}> {tm.GetCommons("Rating")}";


        profitText[1].text = string.Empty;
        profit_totalText[1].text = string.Empty;
        profitText[3].text = string.Empty;
        profit_totalText[3].text = string.Empty;

        AudioManager.Instance.PlaySFX(Sfx.inputFieldStart);
        yield return CoroutineHelper.WaitForSecondsRealtime(0.2f);
        accountText[1].text = $"{accountText[1].text} ({gold})";

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

                AudioManager.Instance.PlaySFX(Sfx.inputFieldStart);
                yield return CoroutineHelper.WaitForSecondsRealtime(0.2f);
                profitText[1].text = st.ToString();
            }
            profitText[1].text = st.ToString();

            AudioManager.Instance.PlaySFX(Sfx.inputFieldStart);
            yield return CoroutineHelper.WaitForSecondsRealtime(0.2f);

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

        AudioManager.Instance.PlaySFX(Sfx.inputFieldStart);
        yield return CoroutineHelper.WaitForSecondsRealtime(0.2f);
        if (rating > 0)
            accountText[2].text = $"{accountText[2].text} ({rating:0.#})";
        else
            accountText[2].text = $"{accountText[2].text} (<color=#A91111>{rating:0.#}</color>)";

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
                AudioManager.Instance.PlaySFX(Sfx.inputFieldStart);
                yield return CoroutineHelper.WaitForSecondsRealtime(0.2f);
                profitText[3].text = st.ToString();
            }
            profitText[3].text = st.ToString();

            AudioManager.Instance.PlaySFX(Sfx.inputFieldStart);
            yield return CoroutineHelper.WaitForSecondsRealtime(0.2f);

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

        AudioManager.Instance.PlaySFX(Sfx.inputFieldEnd);
        yield return CoroutineHelper.WaitForSecondsRealtime(0.2f);

        nextDayBtn.gameObject.SetActive(true);
        UINaviHelper.Instance.SetFirstSelect();

    }

    #region 창고
    /*
    private void UpdateAccountUI()
    {
        accountText[1].text = $"<sprite={2}> {tm.GetCommons("Money")} ({gold})";
        //if (rating >= RivalManager.Instance.rating)
        if (rating > 0)
            accountText[2].text = $"<sprite={1}> {tm.GetCommons("Rating")} ({rating:0.#})";
        else
            accountText[2].text = $"<sprite={1}> {tm.GetCommons("Rating")} (<color=#A91111>{rating:0.#}</color>)";

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

    }
    */
    #endregion

    public void Show_SpaceshipProject()
    {
        dayOne_Gold = new SerializableDictionary<GetGoldSource, int>();
        dayOne_Rating = new SerializableDictionary<GetRatingSource, float>();

        if (day == RocketManager.Countdown) // 엔딩 직전 무조건 발사 화면으로
        {
            ShowRocketPanel();
        }
        else
        {
            if (RocketManager.Instance.Completed)
            {
                //NextDay_Late();
                int currentVillager = VillagerManager.Instance.GetRecruitedVillagerCount();
                if (currentVillager > 0)
                    NextDay_Midnight();
                else
                    NextDay_Late(false);
            }
            else
            {
                ShowRocketPanel();
            }
        }
    }

    private void ShowRocketPanel()
    {
        RocketManager.Instance.ShowPanel();
        accountObj.SetActive(false);
        UINaviHelper.Instance.SetFirstSelect();
    }

    public void NextDay_Midnight()
    {
        loading = false;
        midNight = true;

        accountObj.SetActive(false);
        UINaviHelper.Instance.SetFirstSelect();
        var shopUI = UIManager.Instance.shopUI;
        shopUI.HideUI(true);
        shopUI.ForceMidnightUIUpdate(false);
        UIManager.Instance.speedInfo.SetActive(false);

        player.transform.position = pizzeriaPos.position;
        shopUI.playerStay = false;
        SetLight(1f, 24);

        ChangeMan(true);
        RunIndicatorUpdate(); // 한밤중 업데이트
        UINaviHelper.Instance.ingame.UIUpdate(UINaviHelper.Instance.PadType); // 한밤중 업데이트
        VillagerManager.Instance.SetMidNight(true);
        timeText.text = dayStr[2];

        TutorialManager.Instance.ManMove_Enter();

        darkCanvas.blocksRaycasts = false;
        darkCanvas.interactable = false;
        darkCanvas.alpha = 0f;
    }

    public void NextDay_Late(bool fromMidnight)
    {
        if (fromMidnight && lastLaunch) // 마지막 날엔 탐색 X => 바로 우주선으로
        {
            NextDay();
            return;
        }

        VillagerManager.Instance.villagerSearcher.Clear();

        TutorialManager.Instance.Midnight_Leave();

        //int loanWarning = LoanManager.Instance.NextDayLate();
        int loanWarning = -1;

        bool showWarning = false;
        //if (rating < RivalManager.Instance.rating)
        //if (rating <= 0)
        //{
        //    if (!warning_gameOver)
        //    {
        //        warning_gameOver = true;
        //        showWarning = true;
        //    }
        //    else
        //    {
        //        gameOverText[1].text = tm.GetCommons("Gameover2");
        //        GameOver();
        //        return;
        //    }
        //}
        //else
        //    warning_gameOver = false;


        if (day >= 9) // 9일 => 10일 데모 승리
        {
            //if (!CongratulationTriggered)
            //{
            //    Congratulation(true);
            //    AudioManager.Instance.PlaySFX(Sfx.complete);
            //    UIManager.Instance.shopUI.upgradeDirection.Show(1);
            //    return;
            //}
        }
        if (day >= RocketManager.Countdown) // 30일 => 31일 게임 끝
        {
            // 엔딩 처리
            Debug.Log("엔딩");
            if (!CongratulationTriggered)
            {
                Congratulation(true);
                AudioManager.Instance.PlaySFX(Sfx.complete);
                UIManager.Instance.shopUI.upgradeDirection.Show(1);
                return;
            }
        }

        warning_gameOver = false; // 평점 0점 이하 패배 조건 삭제

        if (loanWarning == 0)
        {
            gameOverText[1].text = tm.GetCommons("Gameover3");
            GameOver();
            return;
        }

        if (raided)
        {
            AudioManager.Instance.PlaySFX(Sfx.raid);
            raided = false;
        }

        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetAutoKill(true);
        if (fromMidnight)
        {
            sequence.AppendCallback(() =>
            {
                darkCanvas.alpha = 0f;
                darkCanvas.interactable = false;
                darkCanvas.blocksRaycasts = true;
                loading = true;
                UINaviHelper.Instance.SetFirstSelect();
            });
            sequence.Append(darkCanvas.DOFade(1f, 0.5f));
            sequence.AppendInterval(1.25f);
        }
        else
        {
            sequence.AppendCallback(() =>
            {
                darkCanvas.alpha = 1f;
                darkCanvas.interactable = false;
                darkCanvas.blocksRaycasts = true;
                loading = true;
                UINaviHelper.Instance.SetFirstSelect();
            });
            sequence.AppendInterval(0.5f);
        }
        sequence.AppendCallback(() =>
        {
            SetTimer(0f);
            ChangeMan(false);
            midNight = false;
            RunIndicatorUpdate(); // 한밤중 => 낮 업데이트
            UINaviHelper.Instance.ingame.UIUpdate(UINaviHelper.Instance.PadType); // 한밤중 => 낮 업데이트
            VillagerManager.Instance.SetMidNight(false);
            DayStringUpdate();

            openImage.SetActive(true);
            closeImage.SetActive(false);

            if (EndTimeEvent != null)
                EndTimeEvent(null, false);
            EndTime = false;
        });
        sequence.Append(darkCanvas.DOFade(0f, 0.25f));
        sequence.AppendCallback(() =>
        {
            VillagerManager.Instance.NextDay_Late();

            SpecialDay();
            if (bloodMoon)
                AudioManager.Instance.PlaySFX(Sfx.bloodmoon);
            else
                AudioManager.Instance.PlaySFX(Sfx.nextDay);

            darkCanvas.blocksRaycasts = false;
            darkCanvas.interactable = false;
            darkCanvas.alpha = 0f;

            loading = false;

            OrderManager.Instance.NewOrder();
            bool hasResult = ExplorationManager.Instance.ShowResultPanel();

            if (showWarning)
            {
                warningQueue.Enqueue(0); // 평점 위험
            }
            if (loanWarning == 1)
            {
                warningQueue.Enqueue(1); // 빚 
            }

            TutorialManager.Instance.DayChanged();

            if (!hasResult)
            {
                ShowWarningQueue();
            }

            StatManager.Instance.NextDay();
            GameEventManager.Instance.NextDay();
            GiftBoxHide();
            ResearchManager.Instance.ToggleAllHiddenRecipe(true);

            if (RivalManager.Instance.NextDay())
            {
                warningQueue.Enqueue(3); // 경쟁업체 평점
            }
            //ShowRatingText();
            zombieEnvSound.Mute(false);
            for (int i = 0; i < shopGates.Length; i++)
            {
                shopGates[i].alwaysClosed = false;
            }

            SaveManager.Instance.ZompizzaAutoSave(day);
        });
        sequence.Append(darkCanvas.DOFade(0f, 0.5f));
    }

    // 고정된 날짜에 진행되는 무언가 => 세이브 로드와 상관없음 => 따라서 세이브 로드 후 메소드 실행시켜 주어야 함
    public void SpecialDay()
    {
        // 임시 비 효과
        if (day == 3 || day == 8 || day == 11 || day == 14 || day == 17 || day == 21 || day == 24 || day == 27)
        {
            rainObj.SetActive(true);
        }
        bloodMoon = false;
        if (day == 4 || day == 9 || day == 14 || day == 19 || day == 24 || day == 28) // 블러드문
        {
            bloodMoon = true;
        }

        if ((day + 1) % 2 == 0)
        {
            InstallFuck(true);
        }
        else
        {
            InstallFuck(false);
        }
    }
    public void InstallFuck(bool on)
    {
        if (on)
        {
            install = true;
            TutorialManager.Instance.ToggleInstallGuide(true);
            player.installDeco.SetActive(true);
        }
        else
        {
            install = false;
            TutorialManager.Instance.ToggleInstallGuide(false);
            player.installDeco.SetActive(false);
        }
    }
    public void InstallFuck2()
    {
        if (player.transform.position.y > 2.5f) return;
        Vector3 pos = player.transform.position + player.transform.forward * 5f;
        Instantiate(installJumpObj, pos, Quaternion.identity); // 플레이어가 직접 설치
        installJumpPostions.Add(pos);
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
                case 3: // 7일마다 랭킹
                    RivalManager.Instance.ShowRankingPanel();
                    break;
            }
        }
        else
        {
            UINaviHelper.Instance.SetFirstSelect();
            switch (day)
            {
                case 2: GameEventManager.Instance.SetEvent(0); // 3일차 아침 고아원 원장 이벤트
                    break;
                case 8: GameEventManager.Instance.SetEvent(2); // 9일차 아침 고양이 이벤트
                    break;
                default: TutorialManager.Instance.NoMoreEvented(); // 이벤트가 없거나, 있으면 이벤트 후에 튜토 활성화
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
        string str = $"{displayGold}G";

        for (int i = 0; i < goldText.Length; i++)
            goldText[i].text = str;
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

        if (value > 0)
        {
            AddResearchPoint(value * 10f); // 연구 포인트는 양수의 평점을 받았을 때만 올라감 (내려가는 일 없음) // 10배수
        }
    }

    private void ShowRatingText()
    {
        //float rivalRating = RivalManager.Instance.rating;

        string str;
        if (displayRating < 0)
            str = $"<color=#A91111>{displayRating:0.#}</color>";
        else
            str = $"{displayRating:0.#}";

        for (int i = 0; i < ratingText.Length; i++)
        {
            //if (displayRating <= 0)
            //    ratingText[i].text = $"<color=#A91111>{displayRating:0.#}</color> / {Constant.winRating:F0}";
            //else
            //    ratingText[i].text = $"{displayRating:0.#} / {Constant.winRating:F0}";

            ratingText[i].text = str;

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
    public void SetRatingPoint(float value)
    {
        displayResearchPoints = value;
        researchPoint = value;
        ShowResearchPointText();
    }
    public void AddResearchPoint(float value)
    {
        float target = researchPoint + value;
        DOVirtual.Float(researchPoint, target, 0.75f, (x) =>
        {
            displayResearchPoints = x;
            ShowResearchPointText();
        }).SetEase(Ease.OutCirc).SetUpdate(true);
        researchPoint = target;
    }
    private void ShowResearchPointText()
    {
        string str = $"{displayResearchPoints:F0}";

        for (int i = 0; i < researchPointsText.Length; i++)
        {
            //researchPointsText[i].text = $"{displayResearchPoints:0.#}";
            researchPointsText[i].text = str;
        }
    }

    public void RunIndicatorUpdate()
    {
        if (midNight)
        {
            var pad = Gamepad.current;
            if (pad == null)
            {
                runIndicatorTMP.text = $"{tm.GetCommons("Run")} ({HotKey[KeyMap.carBreak].GetName()})";
                runIndicator.SetActive(true);
            }
            else
                runIndicator.SetActive(false);
        }
        else
            runIndicator.SetActive(false);
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
            warningQueue.Enqueue(2); // 습격
            tenDays_RaidRecords.Add(1);
            raided = true;
        }
        else
        {
            // 습격 왔을 땐, 빼먹지 않음
            CatSteal();
        }
        UIManager.Instance.UpdateIngredients();
        UIManager.Instance.OrderUIBtnUpdate();
    }
    private bool CheckRaid()
    {
        int hasRes = HasIngredient;

        // 가진 자원수가 60 넘을 때 (1티어), 60 넘을 때 (2티어), 50% 확률로 습격 발생, 습격 관련 업그레이드에 따라서 30%~0% 만큼 자원을 빼앗김
        bool raid = false;
        if (ResearchManager.Instance.globalEffect.tier == 0 && hasRes > 60) raid = true;
        else if (ResearchManager.Instance.globalEffect.tier >= 1 && hasRes > 60) raid = true;

        if (day <= 2) raid = false; // 3일차까진 습격 없음

        if (raid && UnityEngine.Random.Range(0, 2) == 1)
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
        ingredients[ingredient] += 2;
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

    public void ChangeMan(bool man)
    {
        if (man)
        {
            player.manObj.SetActive(true);
            for (int i = 0; i < controllerData.Length; i++)
            {
                controllerData[i].gameObject.SetActive(false);
            }
            player.cam.mainCam.fieldOfView = 20f;
            player.manMode = true;
        }
        else
        {
            player.manMode = false;
            ChangeVehicle(currentVehicle);
            player.manObj.SetActive(false);
            player.cam.mainCam.fieldOfView = 25f;
        }
    }

    public void ChangeVehicle(int idx)
    {
        if (currentVehicle >= 0)
            controllerData[currentVehicle].gameObject.SetActive(false);
        currentVehicle = idx;
        controllerData[idx].SetData(player);
    }
    
    public bool CanBuyVehicle(int idx)
    {
        return !unlockedVehicles[idx] && rating >= ratingVehicles[idx];
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
        if (CanBuyVehicle(idx))
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
    public int Auto_ButVehicle() // 차량 자동 구매
    {
        int result = -1;
        for (int i = 1; i < unlockedVehicles.Length; i++)
        {
            if (CanBuyVehicle(i))
            {
                unlockedVehicles[i] = true;
                result = i;
            }
        }
        return result;
    }
    #endregion

    #region 선물 상자
    public void GiftBoxHide() // 해가 뜬 후 배달 위치의 박스들 모두 숨기기
    {
        var orderGoals = OrderManager.Instance.orderGoals;

        for (int i = 0; i < orderGoals.Count; i++)
        {
            for (int n = 0; n < orderGoals[i].giftGoals.Length; n++)
            {
                orderGoals[i].giftGoals[n].Hide();
            }
        }
    }
    #endregion
}
