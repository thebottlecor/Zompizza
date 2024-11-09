using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using MTAssets.EasyMinimapSystem;
using UnityEngine.InputSystem;

public class OrderManager : Singleton<OrderManager>
{

    public Transform pizzeria;
    public List<OrderGoal> orderGoals;

    public List<OrderInfo> orderList;
    public List<OrderUIObject> orderUIList_Ordered;

    public PizzaDirection pizzaDirection;
    public OvenMiniGame ovenMiniGame;
    //public MoneyDirection moneyDirection;

    public SerializableDictionary<OrderInfo, OrderMiniUI> orderMiniUIPair;


    // 저장 정보
    // 호감도 관련, 손님 개별 평점 평균을 위한 정보
    [Serializable]
    public struct CustomerInfo
    {
        public float totalRating;
        public int totalOrder;

        public float AverageRating()
        {
            return totalOrder > 0 ? totalRating / totalOrder : 0f;
        }
    }
    public SerializableDictionary<int, CustomerInfo> customersInfos;
    // 어제 '수락'을 했던 주문들 => 다음날 주문에서 제외
    public List<int> yesterdayOrders;

    public static EventHandler<int> OrderRemovedEvent;

    public int MaxAccpetance => GM.Instance.player.maxLoad;
    public int currentAcceptance;
    public bool IsMaxDelivery => currentAcceptance >= MaxAccpetance;

    private List<Ingredient> ingredients_Tier1;
    private List<Ingredient> ingredients_Tier2;
    private List<Ingredient> ingredients_Tier3;
    private List<Ingredient> ingredients_Tier4;
    private HashSet<Ingredient> ingredients_Tier1_Hash;
    private HashSet<Ingredient> ingredients_Tier2_Hash;
    private HashSet<Ingredient> ingredients_Tier3_Hash;
    private HashSet<Ingredient> ingredients_Tier4_Hash;

    [Header("빠른 이동")]
    public GameObject fastTravelBtnParnet;
    public Button fastTravleBtn;
    public RectTransform padKeyIndicators;
    public TextMeshProUGUI fastTravelText;
    //public TextMeshProUGUI fastTravelTimeText;

    public MinimapRenderer minimap;
    public MinimapRenderer worldmap;

    [Header("콤보")]
    public bool comboMode;
    public GameObject bonusObj;
    public int bonusIngredients; // 오늘의 보너스 재료 - 이 재료를 넣을시 (원재료값x1.5) 만큼의 보상이 더해짐 (-1은 없는 경우)
    public Image bonusIcon;
    public TextMeshProUGUI[] bonusTexts;

    protected override void Awake()
    {
        base.Awake();
        customersInfos = new SerializableDictionary<int, CustomerInfo>();
        for (int i = 0; i < orderGoals.Count; i++)
        {
            orderGoals[i].Init(i);

            customersInfos.Add(new SerializableDictionary<int, CustomerInfo>.Pair { Key = i, Value = new CustomerInfo() });
        }
    }

    public void Init()
    {
        ingredients_Tier1 = new List<Ingredient>();
        ingredients_Tier2 = new List<Ingredient>();
        ingredients_Tier3 = new List<Ingredient>();
        ingredients_Tier4 = new List<Ingredient>();
        ingredients_Tier1_Hash = new HashSet<Ingredient>();
        ingredients_Tier2_Hash = new HashSet<Ingredient>();
        ingredients_Tier3_Hash = new HashSet<Ingredient>();
        ingredients_Tier4_Hash = new HashSet<Ingredient>();
        var ingLib = DataManager.Instance.ingredientLib;
        foreach (var temp in ingLib.ingredientTypes)
        {
            var key = (Ingredient)temp;

            int tier = -1;

            // 임시유효성 검사
            if (ingLib.meats.ContainsKey(key) && ingLib.meats[key].valid)
            {
                tier = ingLib.meats[key].tier;
            }
            else if (ingLib.vegetables.ContainsKey(key) && ingLib.vegetables[key].valid)
            {
                tier = ingLib.vegetables[key].tier;
            }
            else if (ingLib.herbs.ContainsKey(key) && ingLib.herbs[key].valid)
            {
                tier = ingLib.herbs[key].tier;
            }

            Ingredient ingredient = (Ingredient)temp;
            switch (tier)
            {
                case 0:
                    ingredients_Tier1.Add(ingredient);
                    ingredients_Tier1_Hash.Add(ingredient);
                    break;
                case 1:
                    ingredients_Tier2.Add(ingredient);
                    ingredients_Tier2_Hash.Add(ingredient);
                    break;
                case 2:
                    ingredients_Tier3.Add(ingredient);
                    ingredients_Tier3_Hash.Add(ingredient);
                    break;
                case 3:
                    ingredients_Tier4.Add(ingredient);
                    ingredients_Tier4_Hash.Add(ingredient);
                    break;
            }
        }

        yesterdayOrders = new List<int>();
        orderUIList_Ordered = new List<OrderUIObject>();

        orderMiniUIPair = new SerializableDictionary<OrderInfo, OrderMiniUI>();
        orderList = new List<OrderInfo>();
    }
    public void AfterInit()
    {
        if (TutorialManager.Instance.training)
        {

        }
        else
        {
            NewOrder();
        }

        for (int i = 0; i < orderGoals.Count; i++)
        {
            minimap.minimapItemsToHightlight.Add(orderGoals[i].minimapItem);
            worldmap.minimapItemsToHightlight.Add(orderGoals[i].minimapItem);
            worldmap.minimapItemsToHightlight.Add(orderGoals[i].minimapItem_customer);
        }
    }
    public Ingredient GetRandomIngredient_HighTier()
    {
        var list = ingredients_Tier1;
        switch (ResearchManager.Instance.globalEffect.tier)
        {
            case 1:
                list = ingredients_Tier2;
                break;
            case 2:
                list = ingredients_Tier3;
                break;
            case 3:
                list = ingredients_Tier4;
                break;
        }
        int random = UnityEngine.Random.Range(0, list.Count);
        return list[random];
    }
    protected override void AddListeners()
    {
        OrderGoal.PlayerArriveEvent += OnPlayerArrive;
        Zombie2.DamageEvent += OnPlayerDamaged;
        ZombieSanta.StealEvent += OnPizzeStolen;
        PlayerController.DamageEvent += OnPlayerDamaged;
    }

    protected override void RemoveListeners()
    {
        OrderGoal.PlayerArriveEvent -= OnPlayerArrive;
        Zombie2.DamageEvent -= OnPlayerDamaged;
        ZombieSanta.StealEvent -= OnPizzeStolen;
        PlayerController.DamageEvent -= OnPlayerDamaged;
    }

    private void OnPlayerArrive(object sender, int e)
    {
        for (int i = orderList.Count - 1; i >= 0; i--)
        {
            var info = orderList[i];
            int gIndex = info.goal;
            if (!info.stolen && gIndex == e)
            {
                // 배달 성공

                float timeRating;
                float timeLimit = info.timeLimit;
                float overTime = info.timer - timeLimit;
                if (overTime <= 0f)
                {
                    float remainPercent = Mathf.Abs(overTime) / timeLimit;
                    if (remainPercent >= Constant.remainTime_Percent) // 남은 시간이 50% 이상
                    {
                        timeRating = Constant.remainTimeRating1;
                    }
                    else if (remainPercent >= 0.25f)
                    {
                        timeRating = 2f;
                    }
                    else
                    {
                        timeRating = 1.5f;
                    }
                }
                else
                {
                    timeRating = 1f;
                }

                float hpRating;
                float hpPercent = info.hp;
                if (hpPercent == 1f)
                {
                    hpRating = Constant.remainHpRating1;
                }
                else if (hpPercent >= 0.75f)
                {
                    hpRating = 2f;
                }
                else if (hpPercent >= 0.5f)
                {
                    hpRating = 1.5f;
                }
                else
                {
                    hpRating = 1f;
                }

                //else if (hpPercent >= Constant.remainHP_Percent)
                //{
                //    hpRating = Constant.Point05(((Constant.remainHpRating2 - Constant.remainHpRating3) / (1f - Constant.remainHP_Percent)) * hpPercent +
                //        ((-Constant.remainHP_Percent * Constant.remainHpRating2) + Constant.remainHpRating3) / (1f - Constant.remainHP_Percent));
                //}
                //else
                //{
                //    //hpRating = Constant.Point05((-1f * Constant.remainHpRating4 / Constant.remainHP_Percent) * hpPercent + Constant.remainHpRating4);

                //    // 원래는 0점 ~ -2.5점 사이
                //    hpRating = 0f;
                //}

                float resultRating = timeRating + hpRating;
                //if (resultRating > 0f) resultRating *= (1f + ResearchManager.Instance.globalEffect.ratingGet);
                //resultRating = Mathf.Min(Constant.remainHpRating1 + Constant.remainTimeRating1, resultRating);
                GM.Instance.AddRating(resultRating, GM.GetRatingSource.delivery);

                int rewards = info.rewards;

                GM.Instance.AddGold(rewards, GM.GetGoldSource.delivery);
                UIManager.Instance.shopUI.AddReview(GM.Instance.day, info.customerIdx, info.goal, timeRating, hpRating);

                orderGoals[gIndex].SuccessEffect(rewards, resultRating);

                //moneyDirection.RestartSequence_Debug();
                
                orderMiniUIPair[info].Hide();
                orderMiniUIPair.Remove(info);
                orderList.RemoveAt(i);

                StatManager.Instance.CalcAverageDeliveryStat(overTime, hpPercent, rewards, resultRating);

                TutorialManager.Instance.OrderCompleted();
                break;
            }
        }

        if (OrderRemovedEvent != null)
            OrderRemovedEvent(null, orderList.Count);

        UIManager.Instance.shopUI.OrderLoadCountTextUpdate();
        RemoveRemainOrders();

        if (currentAcceptance == 0)
        {
            bool someSos = VillagerManager.Instance.CreateSOS(false);
            if (!someSos)
            {
                FastTravelShow(); // 주문 없고, 구조 신호도 없을 떄
            }
        }

        UIManager.Instance.OrderUIBtnUpdate();
    }

    private void OnPlayerDamaged(object sender, float e)
    {
        for (int i = 0; i < orderList.Count; i++)
        {
            var info = orderList[i];
            if (info.accepted && !info.stolen)
            {
                float damage = Mathf.Max(Constant.min_damage, GM.Instance.player.DamageReduction * e);
                info.hp -= damage;
                if (info.hp < 0f) info.hp = 0f;

                orderMiniUIPair[info].UpdateHpGauge(info.hp);

                GM.Instance.player.HitBlink();
            }
        }
    }
    private void OnPizzeStolen(object sender, ZombieSanta e)
    {
        for (int i = 0; i < orderList.Count; i++)
        {
            var info = orderList[i];
            if (info.accepted && !info.stolen)
            {
                AudioManager.Instance.PlaySFX(Sfx.santaTake);
                info.stolen = true;
                e.StealPizza();
                GM.Instance.player.HitBlink();
                GM.Instance.player.UpdateBox(GetCurrentPizzaBox());
                return;
            }
        }
    }
    public void ReturnStolenPizza()
    {
        for (int i = 0; i < orderList.Count; i++)
        {
            var info = orderList[i];
            if (info.accepted && info.stolen)
            {
                AudioManager.Instance.PlaySFX(Sfx.santaReturn);
                info.stolen = false;
                GM.Instance.player.UpdateBox(GetCurrentPizzaBox());
                return;
            }
        }
    }
    public void FailToReturnStolenPizza()
    {
        for (int i = orderList.Count - 1; i >= 0; i--)
        {
            var info = orderList[i];
            if (info.accepted && info.stolen)
            {
                // 배달 실패
                float fail = Constant.remainTimeRating4 + Constant.remainHpRating4;
                GM.Instance.AddRating(fail, GM.GetRatingSource.notComplete); // 산타
                UIManager.Instance.shopUI.AddReview(GM.Instance.day, info.customerIdx, info.goal, Constant.remainTimeRating4, Constant.remainHpRating4);

                orderGoals[info.goal].Hide();

                orderMiniUIPair[info].Hide();
                orderMiniUIPair.Remove(info);
                orderList.RemoveAt(i);

                break;
            }
        }

        if (OrderRemovedEvent != null)
            OrderRemovedEvent(null, orderList.Count);

        UIManager.Instance.shopUI.OrderLoadCountTextUpdate();

        UIManager.Instance.OrderUIBtnUpdate();
    }

    private void SetBonusIngredients()
    {
        bonusIngredients = -1;
        List<Ingredient> ingredients_Descending = new List<Ingredient>();
        ingredients_Descending.AddRange(ingredients_Tier4.ShuffleAndDeepCopy());
        ingredients_Descending.AddRange(ingredients_Tier3.ShuffleAndDeepCopy());
        ingredients_Descending.AddRange(ingredients_Tier2.ShuffleAndDeepCopy());
        ingredients_Descending.AddRange(ingredients_Tier1.ShuffleAndDeepCopy());

        for (int i = 0; i < ingredients_Descending.Count; i++)
        {
            if (GM.Instance.ingredients[ingredients_Descending[i]] > 0)
            {
                bonusIngredients = (int)ingredients_Descending[i];
                break;
            }
        }
        UpdateBonusToday();
    }
    private void UpdateBonusToday()
    {
        bonusObj.SetActive(true);
        if (bonusIngredients >= 0)
        {
            Ingredient bonus = (Ingredient)bonusIngredients;
            bonusIcon.sprite = DataManager.Instance.uiLib.ingredients[bonus];
            bonusTexts[0].text = TextManager.Instance.GetIngredient(bonus);
            bonusTexts[1].text = $"+{(FindTier(bonus) + 1) * Constant.bonus_reward_ingredients}G";
            bonusIcon.gameObject.SetActive(true);
        }
        else
        {
            bonusIcon.gameObject.SetActive(false);
        }
    }

    [ContextMenu("새로운 주문")]
    public void NewOrder()
    {
        SetBonusIngredients();

        int day = GM.Instance.day; // 중요 날짜별 주문 추가!!!!!!!!!!!!!!!!!
        int extraOrder = ResearchManager.Instance.globalEffect.order_max;
        int minOrderCount;
        switch (day)
        {
            case 0:
            case 1:
                minOrderCount = 2;
                break;
            case 2:
                minOrderCount = 3;
                break;
            default:
                minOrderCount = 4;
                break;
        }
        minOrderCount += extraOrder;

        minOrderCount = Mathf.Min(Constant.maxOrderDaily, minOrderCount);

        // 데모용 2일:2개 / 3일 : 3개 / 4일 : 4개 ~~
        //List<int> rand = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        //List<int> rand = new List<int> { 0, 1, 2, 3, 4, 5, 6, 8 };
        List<int> rand = new List<int> { 1, 4, 5, 6, 8 };
        //**** 중요 날짜별 새로운 주문 추가
        {
            if (day >= 2)
            {
                rand.Add(7); // 떠돌이
                rand.Add(9); // 배관공
            }
            if (day >= 3)
            {
                rand.Add(3); // 운동선수
            }
            if (day >= 4)
            {
                rand.Add(2); // 해커
            }
            if (day >= 5)
            {
                rand.Add(0); // 경찰
                rand.Add(28); // 달리기 선수
            }

            if (day >= 6)
            {
                rand.Add(14); // 자경단원
            }
            if (day >= 7)
            {
                rand.Add(10); // 시장
            }
            if (day >= 8)
            {
                rand.Add(24); // 경비원
                rand.Add(18); // 신부님
            }

            if (day >= 10)
            {
                rand.Add(19); // 목장주인
                rand.Add(16); // 스카웃
            }
            if (day >= 11)
            {
                rand.Add(26); // 벨보이
                rand.Add(30); // CEO
            }
            if (day >= 12)
            {
                rand.Add(11); // 회사원
                rand.Add(25); // 용접공
            }
            if (day >= 13)
            {
                rand.Add(23); // 파일럿
                rand.Add(22); // 공원관리인
                rand.Add(13); // 생존주의자
            }

            if (day >= 15)
            {
                rand.Add(12); // 검역관
                rand.Add(17); // 죄수
            }
            if (day >= 16)
            {
                rand.Add(20); // 소방관
                rand.Add(21); // 양조자
            }
            if (day >= 17)
            {
                rand.Add(15); // 의사
                rand.Add(27); // 관광객
                rand.Add(29); // 조선공
            }
        }

        for (int i = rand.Count - 1; i >= 0; i--)
        {
            if (yesterdayOrders.Count == 0) break;
            if (rand.Count <= minOrderCount) break; // 최소 주문만큼만

            if (yesterdayOrders.Contains(rand[i]))
            {
                yesterdayOrders.Remove(rand[i]);
                rand.RemoveAt(i);
            }
        }
        yesterdayOrders.Clear();

        orderUIList_Ordered = new List<OrderUIObject>();
        rand.Shuffle();
        var tempList = rand.Take(minOrderCount);
        tempList = tempList.OrderByDescending(a => orderGoals[a].compassDir).ThenBy(a => orderGoals[a].Km);
        rand = tempList.ToList();

        for (int i = 0; i < rand.Count; i++)
        {
            orderUIList_Ordered.Add(UIManager.Instance.orderUIObjects[rand[i]]);
            AddOrder_New(rand[i]);
        }
        for (int i = 0; i < orderUIList_Ordered.Count; i++)
        {
            orderUIList_Ordered[i].transform.SetAsLastSibling();
        }

        //int lastRand = rand.Count;
        //int halfRand = lastRand / 2;

        //if (GM.Instance.day <= 2) // 3일차 까진, 자신이 가진 재료에서만 주문이 나옴
        //    halfRand = lastRand;

        //int hasRes = GM.Instance.HasIngredient;
        //if (lastRand == 1 && hasRes >= 1)
        //    halfRand = 1;

        //if (halfRand > hasRes)
        //{
        //    halfRand = hasRes;
        //}
        //lastRand -= halfRand;

        //SerializableDictionary<Ingredient, int> tempRes = new SerializableDictionary<Ingredient, int>();
        //foreach (var temp in GM.Instance.ingredients)
        //{
        //    tempRes.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = temp.Key, Value = temp.Value });
        //}

        //int error = 0; // 플레이어가 가지고 있는 자원 내에서 생성되지 못한 주문
        //for (int i = 0; i < halfRand; i++) // 플레이어가 가지고 있는 자원 내에서 생성될 주문들
        //{
        //    bool result = AddOrder_Adjust(rand[i], ref tempRes);

        //    //Debug.Log($"----{i}--------------{tempRes.Count}");
        //    //foreach (var temp in tempRes)
        //    //{
        //    //    Debug.Log($">{temp.Key} : {temp.Value}");
        //    //}
        //    if (!result) error++;
        //}
        //for (int i = 0; i < lastRand + error; i++) // 아무렇게나 생성될 주문들
        //{
        //    AddOrder(rand[i + halfRand - error]);
        //}
        //Debug.Log(error);

        UIManager.Instance.OrderUIUpdate();

        OrderGoalUpdate();

        var shopUI = UIManager.Instance.shopUI;
        var orderUIs = UIManager.Instance.orderUIObjects;
        shopUI.OrderLoadCountTextUpdate();
        shopUI.SnapTo(null);
        for (int i = 0; i < orderUIs.Count; i++)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(orderUIs[i].transform as RectTransform);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(shopUI.contentPanel);
    }

    public void NewOrder_Tutorial()
    {
        bonusIngredients = 0;
        UpdateBonusToday();

        int oldMan = 4;

        // 튜토리얼 - (노인 4)
        //SerializableDictionary<Ingredient, int> randInfo_sub = new SerializableDictionary<Ingredient, int>();
        //ingredients_Tier1.Shuffle();
        //randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = ingredients_Tier1[0], Value = 1 });
        //randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = ingredients_Tier1[1], Value = 1 });
        //randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = ingredients_Tier1[2], Value = 1 });
        //int ingredientTotal = 3;
        //AddOrder_Sub(4, randInfo_sub, ingredientTotal, 0.35f); // 0.35 넣어서 골드 보너스 (135%)

        orderUIList_Ordered = new List<OrderUIObject>();
        AddOrder_New(oldMan, 1); // 미트러버 고정
        orderUIList_Ordered.Add(UIManager.Instance.orderUIObjects[oldMan]);

        UIManager.Instance.OrderUIUpdate();

        OrderGoalUpdate();

        UIManager.Instance.shopUI.OrderLoadCountTextUpdate();
    }

    public int CustomerMaxIngredient()
    {
        return Constant.customer_max_ingredient + ResearchManager.Instance.globalEffect.customer_max_amount;
    }

    public int FindTier(Ingredient ingredient)
    {
        int value = 0;
        if (ingredients_Tier2_Hash.Contains(ingredient))
            value = 1;
        else if (ingredients_Tier3_Hash.Contains(ingredient))
            value = 2;
        else if (ingredients_Tier4_Hash.Contains(ingredient))
            value = 3;
        return value;
    }

    private bool AddOrder_Adjust(int goal, ref SerializableDictionary<Ingredient, int> tempRes)
    {
        SerializableDictionary<Ingredient, int> randInfo_sub = new SerializableDictionary<Ingredient, int>();

        List<Ingredient> ingredients2 = new List<Ingredient>();
        foreach (var temp in tempRes)
        {
            if (temp.Value > 0)
                ingredients2.Add(temp.Key);
        }
        ingredients2.Shuffle();

        int ingredientTotal = 0;
        float totalTier = 0;
        int orderType = UnityEngine.Random.Range(0, 3);

        if (orderType == 2 && ingredients2.Count < 3)
            orderType = 1;
        if (orderType == 1 && ingredients2.Count < 2)
            orderType = 0;
        if (orderType == 0 && ingredients2.Count < 1)
        {
            return false; // 오류 발생
        }

        // 자신이 가진 재료의 랜덤 주문의 재료 최소 기대값을 1 -> 5로 바꿈 (배달로 버는 돈++)
        switch (orderType)
        {
            case 0:
                {
                    Ingredient randRes = ingredients2[0];
                    //int randCount = Mathf.Min(tempRes[randRes], UnityEngine.Random.Range(1, CustomerMaxIngredient() + 1));
                    int randCount = Mathf.Min(tempRes[randRes], UnityEngine.Random.Range(3, CustomerMaxIngredient() + 1));
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = randRes, Value = randCount });
                    ingredientTotal = randCount;
                    tempRes[randRes] -= randCount;
                    totalTier = FindTier(randRes);
                }
                break;
            case 1:
                {
                    Ingredient randRes = ingredients2[0];
                    Ingredient randRes2 = ingredients2[1];
                    int randCount = Mathf.Min(tempRes[randRes], UnityEngine.Random.Range(3, CustomerMaxIngredient()));
                    int randCount2 = Mathf.Min(tempRes[randRes2], UnityEngine.Random.Range(1, CustomerMaxIngredient() - randCount + 1));
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = randRes, Value = randCount });
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = randRes2, Value = randCount2 });
                    ingredientTotal = randCount + randCount2;
                    tempRes[randRes] -= randCount;
                    tempRes[randRes2] -= randCount2;
                    totalTier = (FindTier(randRes) * randCount + FindTier(randRes2) * randCount2) / (float)ingredientTotal;
                }
                break;
            case 2:
                {
                    Ingredient randRes = ingredients2[0];
                    Ingredient randRes2 = ingredients2[1];
                    Ingredient randRes3 = ingredients2[2];
                    int randCount = Mathf.Min(tempRes[randRes], UnityEngine.Random.Range(3, CustomerMaxIngredient() - 1));
                    int randCount2 = Mathf.Min(tempRes[randRes2], UnityEngine.Random.Range(1, CustomerMaxIngredient() - randCount));
                    int randCount3 = Mathf.Min(tempRes[randRes3], UnityEngine.Random.Range(1, CustomerMaxIngredient() - randCount - randCount2 + 1));
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = randRes, Value = randCount });
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = randRes2, Value = randCount2 });
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = randRes3, Value = randCount3 });
                    ingredientTotal = randCount + randCount2 + randCount3;
                    tempRes[randRes] -= randCount;
                    tempRes[randRes2] -= randCount2;
                    tempRes[randRes3] -= randCount3;
                    totalTier = (FindTier(randRes) * randCount + FindTier(randRes2) * randCount2 + FindTier(randRes3) * randCount3) / (float)ingredientTotal;
                }
                break;
        }

        AddOrder_Sub(goal, randInfo_sub, ingredientTotal, totalTier);
        return true;
    }

    private void AddOrder(int goal)
    {
        SerializableDictionary<Ingredient, int> randInfo_sub = new SerializableDictionary<Ingredient, int>();

        int maxTier = ResearchManager.Instance.globalEffect.customer_max_tier;
        int selectedTier = 0;
        List<Ingredient> selectedTierGroup = ingredients_Tier1;
        switch (maxTier)
        {
            case 0:
                break;
            case 1: // 2티어 개발시 1,2 등급 재료가 랜덤으로 등장
                //if (UnityEngine.Random.Range(0, 2) == 1)
                //{
                //    selectedTier = 1;
                //    selectedTierGroup = ingredients_Tier2;
                //}
                //else
                //{
                //    selectedTier = 0;
                //    selectedTierGroup = ingredients_Tier1;
                //}

                // 그냥 2티어 개발시, 랜덤은 모두 2티어 주문만
                selectedTier = 1;
                selectedTierGroup = ingredients_Tier2;
                break;
            case 2:
                selectedTier = 2;
                selectedTierGroup = ingredients_Tier3;
                break;
            case 3:
                selectedTier = 3;
                selectedTierGroup = ingredients_Tier4;
                break;
        }

        int ingredientTotal = 0;
        int orderType = UnityEngine.Random.Range(0, 3);
        switch (orderType)
        {
            case 0:
                {
                    int randCount = UnityEngine.Random.Range(3, CustomerMaxIngredient() + 1);
                    selectedTierGroup.Shuffle();
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = selectedTierGroup[0], Value = randCount });
                    ingredientTotal = randCount;
                }
                break;
            case 1:
                {
                    int randCount = UnityEngine.Random.Range(3, CustomerMaxIngredient());
                    int randCount2 = UnityEngine.Random.Range(1, CustomerMaxIngredient() - randCount + 1);
                    selectedTierGroup.Shuffle();
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = selectedTierGroup[0], Value = randCount });
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = selectedTierGroup[1], Value = randCount2 });
                    ingredientTotal = randCount + randCount2;
                }
                break;
            case 2:
                {
                    int randCount = UnityEngine.Random.Range(3, CustomerMaxIngredient() - 1);
                    int randCount2 = UnityEngine.Random.Range(1, CustomerMaxIngredient() - randCount);
                    int randCount3 = UnityEngine.Random.Range(1, CustomerMaxIngredient() - randCount - randCount2 + 1);
                    selectedTierGroup.Shuffle();
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = selectedTierGroup[0], Value = randCount });
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = selectedTierGroup[1], Value = randCount2 });
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = selectedTierGroup[2], Value = randCount3 });
                    ingredientTotal = randCount + randCount2 + randCount3;
                }
                break;
        }

        AddOrder_Sub(goal, randInfo_sub, ingredientTotal, selectedTier);
    }

    private void AddOrder_Sub(int goal, SerializableDictionary<Ingredient, int> randInfo_sub, int ingredientTotal, float tier = 0)
    {
        float km = orderGoals[goal].Km;

        List<PizzaInfo> randPizzas = new List<PizzaInfo>();

        PizzaInfo randInfo = new PizzaInfo { ingredients = randInfo_sub };
        randPizzas.Add(randInfo);

        int rewards = 0;
        int friendshipBonus = 0;

        //int rewards = ingredientTotal * Constant.delivery_reward_ingredients; // 현재 재료값은 100으로 고정 => 130% 받음 (데모) ==> 100%로 바꿈 (1.0ver)
        //float mileBouns = Constant.delivery_reward_1km * km;
        //rewards += (int)mileBouns;

        //// 티어별 보상 // 2티어면 2배 보상
        //rewards = (int)((tier + 1f) * (1f + ResearchManager.Instance.globalEffect.goldGet) * rewards);

        //if (GameEventManager.Instance.ninjaPriceUp) // 첫번째 닌자 치킨의 제안 수락시 10% 가격 상승
        //    rewards += (int)(0.1f * rewards);

        //// 호감도에 따른 보너스
        //int friendshipBonus = 0;
        //float averageRating = customersInfos[goal].AverageRating();

        //if (GameEventManager.Instance.friendshipFixed > 0f) averageRating = GameEventManager.Instance.friendshipFixed;

        //if (averageRating >= Constant.friendShip3) friendshipBonus = (int)(0.3f * rewards);
        //else if (averageRating >= Constant.friendShip2) friendshipBonus = (int)(0.2f * rewards);
        //else if (averageRating >= Constant.friendShip1) friendshipBonus = (int)(0.1f * rewards);
        //rewards += friendshipBonus;

        float timeLimit = (Constant.delivery_timeLimit_1km * km) * (1f + ResearchManager.Instance.globalEffect.customer_timelimit) + Constant.delivery_timeLimit_base;

        int comboSpecial = UnityEngine.Random.Range(1, 9); // 1 ~ 8

        OrderInfo newOrder = new OrderInfo
        {
            accepted = false,
            customerIdx = goal,
            goal = goal,

            comboSpecial = comboSpecial,     

            km = km,
            rewards = rewards,
            bouns_friendship = friendshipBonus,
            hp = 1f,
            timeLimit = timeLimit,
            timer = 0f,
            stolen = false,
        };
        orderList.Add(newOrder);
        PairingMiniUI(newOrder);
    }

    private void AddOrder_New(int goal, int comboSpecial = -1)
    {
        float km = orderGoals[goal].Km;

        int friendshipBonus = 0;

        float timeLimit = (Constant.delivery_timeLimit_1km * km) * (1f + ResearchManager.Instance.globalEffect.customer_timelimit) + Constant.delivery_timeLimit_base;

        if (comboSpecial == -1)
            comboSpecial = UnityEngine.Random.Range(1, 9); // 1 ~ 8

        OrderInfo newOrder = new OrderInfo
        {
            accepted = false,
            customerIdx = goal,
            goal = goal,

            comboSpecial = comboSpecial,

            km = km,

            rewards = 0,

            bouns_friendship = friendshipBonus,
            hp = 1f,
            timeLimit = timeLimit,
            timer = 0f,
            stolen = false,
        };
        orderList.Add(newOrder);
        PairingMiniUI(newOrder);
    }

    private void PairingMiniUI(OrderInfo info)
    {
        var list = UIManager.Instance.orderMiniUIs;
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].isActive)
            {
                orderMiniUIPair.Add(new SerializableDictionary<OrderInfo, OrderMiniUI>.Pair { Key = info, Value = list[i] });
                list[i].Init(info);
                break;
            }
        }
    }

    private void Update()
    {
        bool someOrderRemoved = false;
        float globalTimeLimit = 1f / (1f + ResearchManager.Instance.globalEffect.customer_timelimit); // 시간제한이 늘어나면 그만큼 빨리 접수종료되는 상황이 발생, 역수를 곱하면 업글 영향 안 받음
        if (TutorialManager.Instance.training)
        {
            globalTimeLimit = 0f; // 튜토리얼 중에는 시간에 상관없이 주문이 안 없어짐
        }

        for (int i = orderList.Count - 1; i >= 0; i--)
        {
            if (orderList[i].accepted)
            {
                orderList[i].timer += Time.deltaTime;

                orderMiniUIPair[orderList[i]].UpdateTimer(orderList[i]);
            }
            else
            {
                if (orderList[i].timeLimit * globalTimeLimit * 0.5f > GM.Instance.remainTime) // 배달 제한 시간이 남은 영업 시간 초과시
                {
                    // 미-접수 패널티
                    NotAcceptedOrderPenalty(orderList[i]);

                    UIManager.Instance.orderUIObjects[orderList[i].customerIdx].OrderReset();
                    orderMiniUIPair[orderList[i]].Hide();
                    orderMiniUIPair.Remove(orderList[i]);
                    orderList.RemoveAt(i);
                    
                    someOrderRemoved = true;
                }
            }
        }
        if (someOrderRemoved)
        {
            if (OrderRemovedEvent != null)
                OrderRemovedEvent(null, orderList.Count);
        }

        if (fastTravleBtn.gameObject.activeSelf)
        {
            FastTravelCalc();
        }
    }

    public void OrderGoalUpdate()
    {
        for (int i = 0; i < orderGoals.Count; i++)
        {
            orderGoals[i].Hide();
        }

        for (int i = 0; i < orderList.Count; i++)
        {
            if (orderList[i].accepted)
            {
                orderGoals[orderList[i].goal].Show();
            }
        }
    }

    public void OrderAccepted(OrderInfo info)
    {
        if (orderMiniUIPair.ContainsKey(info))
            orderMiniUIPair[info].gameObject.SetActive(true);

        yesterdayOrders.Add(info.customerIdx);

        //// 재료 소모
        //for (int i = 0; i < info.inputs.Count; i++)
        //{
        //    foreach (var temp in info.inputs)
        //    {
        //        GM.Instance.ingredients[temp.Key] -= temp.Value;
        //    }
        //}
        //UIManager.Instance.UpdateIngredients();

        //UIManager.Instance.shopUI.OrderLoadCountTextUpdate();
        //UIManager.Instance.OrderUIBtnUpdate();

        StatManager.Instance.acceptedOrders++;

        ovenMiniGame.SetPizzaPrepare(info);
        //pizzaDirection.RestartSequence(info);
    }
    public void PizzaMakingComplete(OrderInfo info)
    {
        pizzaDirection.PizzaCompleteDirection();
    }

    public bool IsDelivering()
    {
        for (int i = 0; i < orderList.Count; i++)
        {
            if (orderList[i].accepted)
            {
                return true;
            }
        }
        return false;
    }
    public int GetDeliveringCount()
    {
        int value = 0;
        for (int i = 0; i < orderList.Count; i++)
        {
            if (orderList[i].accepted)
            {
                value++;
            }
        }
        currentAcceptance = value;
        return value;
    }
    public int GetCurrentPizzaBox()
    {
        int count = 0;
        for (int i = 0; i < orderList.Count; i++)
        {
            var info = orderList[i];
            if (info.accepted && !info.stolen)
            {
                count += 1;
            }
        }
        return count;
    }

    public int GetAcceptedOrderCount()
    {
        int count = 0;
        for (int i = 0; i < orderList.Count; i++)
        {
            if (orderList[i].accepted)
            {
                count++;
            }
        }
        return count;
    }

    private void RemoveRemainOrders() // 정식 - 하루에 한번이라도 배달 완료시 남은 주문들은 사라짐
    {
        for (int i = orderList.Count - 1; i >= 0; i--)
        {
            if (!orderList[i].accepted)
            {
                // 미-접수 패널티
                NotAcceptedOrderPenalty(orderList[i]);
                UIManager.Instance.orderUIObjects[orderList[i].customerIdx].OrderReset();
                orderMiniUIPair[orderList[i]].Hide();
                orderMiniUIPair.Remove(orderList[i]);
                orderList.RemoveAt(i);
            }
        }
        if (OrderRemovedEvent != null)
            OrderRemovedEvent(null, orderList.Count);
    }

    public void RemoveAllOrders()
    {
        for (int i = orderList.Count - 1; i >= 0; i--)
        {
            if (!orderList[i].accepted)
            {
                // 미-접수 패널티
                NotAcceptedOrderPenalty(orderList[i]);
            }
            else
            {
                // 미완료 리뷰 남기기 -5점
                GM.Instance.AddRating(Constant.delivery_Not_completed_rating, GM.GetRatingSource.notComplete);
                UIManager.Instance.shopUI.AddReview(GM.Instance.day, orderList[i].customerIdx, orderList[i].goal, -10000f, Constant.delivery_Not_completed_rating); // 구분 기능 -1000 => -5는 점수
                orderGoals[orderList[i].goal].Hide();
            }

            orderMiniUIPair[orderList[i]].Hide();
            orderMiniUIPair.Remove(orderList[i]);
            orderList.RemoveAt(i);
        }

        UIManager.Instance.OrderUIReset();
        UIManager.Instance.shopUI.OrderLoadCountTextUpdate();

        if (OrderRemovedEvent != null)
            OrderRemovedEvent(null, orderList.Count);
    }

    private void NotAcceptedOrderPenalty(OrderInfo info)
    {
        // 6월 18일 데모에서 패널티 제거

        //if (CheckIngredient(info)) // 재료를 가지고 있었던 경우
        //{
        //    GM.Instance.AddRating(Constant.delivery_Not_accepted_rating, GM.GetRatingSource.notAccepted);
        //    UIManager.Instance.shopUI.AddReview(info, -1000f, Constant.delivery_Not_accepted_rating); // 구분 기능 -100 => -2.5는 점수
        //}
        //else
        //{
        //    GM.Instance.AddRating(Constant.delivery_Impossible_accepted_rating, GM.GetRatingSource.notAccepted);
        //    UIManager.Instance.shopUI.AddReview(info, -100f, Constant.delivery_Impossible_accepted_rating); // 구분 기능 -100 => -1.5는 점수
        //}

        //GM.Instance.AddRating(0f, GM.GetRatingSource.notAccepted);
        
        /// 정식 버전 - 받지 않은 주문은 기록도 하지 않음
        //UIManager.Instance.shopUI.AddReview(GM.Instance.day, info.customerIdx, info.goal, 0f, 0f);
    }


    private float travelTime;

    public void FastTravelShow()
    {
        if (GM.Instance.day <= 0 && TutorialManager.Instance.step <= 9) return;

        var tm = TextManager.Instance;
        var pad = Gamepad.current;
        if (pad == null)
            fastTravelText.text = $"{tm.GetKeyMaps(KeyMap.fastTravel)} ({SettingManager.Instance.keyMappings[KeyMap.fastTravel].GetName()})";
        else
            fastTravelText.text = tm.GetKeyMaps(KeyMap.fastTravel);

        fastTravleBtn.gameObject.SetActive(true);
        padKeyIndicators.anchoredPosition = new Vector2(0f, 120f);
        FastTravelCalc();
    }

    public void FastTravelAction()
    {
        GM.Instance.timer += travelTime;

        var player = GM.Instance.player;
        player.StopPlayer(instance: true);
        Vector3 pos = GM.Instance.pizzeriaPos.position;
        pos.y += 2.5f;
        player.transform.position = pos;
        player.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        player.cam.ForceUpdate();
        player.ShakeOffAllZombies();

        fastTravleBtn.gameObject.SetActive(false);
        padKeyIndicators.anchoredPosition = new Vector2(0f, 0f);
    }

    private void FastTravelCalc()
    {
        float dist = (GM.Instance.player.transform.position - pizzeria.position).magnitude;
        float km = dist * Constant.distanceScale; // 게임상 거리 200 = 1km

        if (km <= 0.5f)
        {
            fastTravleBtn.gameObject.SetActive(false);
            padKeyIndicators.anchoredPosition = new Vector2(0f, 0f);
            return;
        }

        travelTime = km * 35f;

        //int hour = (int)(travelTime / Constant.oneHour);
        //int minute = (int)((travelTime - hour * Constant.oneHour) / Constant.oneMinute);

        //fastTravelTimeText.text = TextManager.Instance.GetCommons("TravelTime") + $" {hour:00}:{minute:00}";
    }
}
