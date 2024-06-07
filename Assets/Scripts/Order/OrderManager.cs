using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class OrderManager : Singleton<OrderManager>
{

    public Transform pizzeria;
    public List<OrderGoal> orderGoals;

    public List<OrderInfo> orderList;

    public PizzaDirection pizzaDirection;
    public OvenMiniGame ovenMiniGame;
    //public MoneyDirection moneyDirection;

    public SerializableDictionary<OrderInfo, OrderMiniUI> orderMiniUIPair;

    public static EventHandler<int> OrderRemovedEvent;

    public int MaxAccpetance => Constant.baseMaxDeliveryAcceptance;
    public int currentAcceptance;
    public bool IsMaxDelivery => currentAcceptance >= MaxAccpetance;

    private List<Ingredient> ingredients_Tier1;
    private List<Ingredient> ingredients_Tier2;
    private HashSet<Ingredient> ingredients_Tier1_Hash;
    private HashSet<Ingredient> ingredients_Tier2_Hash;

    public void Init()
    {
        ingredients_Tier1 = new List<Ingredient>();
        ingredients_Tier2 = new List<Ingredient>();
        ingredients_Tier1_Hash = new HashSet<Ingredient>();
        ingredients_Tier2_Hash = new HashSet<Ingredient>();
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
            }
        }

        for (int i = 0; i < orderGoals.Count; i++)
        {
            orderGoals[i].index = i;
            orderGoals[i].minimapItem.spriteColor = DataManager.Instance.uiLib.customerPinColor[i];

            // orderGoals[i].minimapItem_customer.itemSprite = ?
        }

        orderMiniUIPair = new SerializableDictionary<OrderInfo, OrderMiniUI>();
        orderList = new List<OrderInfo>();


        if (TutorialManager.Instance.training)
            NewOrder_Tutorial();
        else
            NewOrder();
    }
    protected override void AddListeners()
    {
        OrderGoal.PlayerArriveEvent += OnPlayerArrive;
        Zombie2.DamageEvent += OnPlayerDamaged;
        PlayerController.DamageEvent += OnPlayerDamaged;
    }

    protected override void RemoveListeners()
    {
        OrderGoal.PlayerArriveEvent -= OnPlayerArrive;
        Zombie2.DamageEvent -= OnPlayerDamaged;
        PlayerController.DamageEvent -= OnPlayerDamaged;
    }

    private void OnPlayerArrive(object sender, int e)
    {
        for (int i = orderList.Count - 1; i >= 0; i--)
        {
            int gIndex = orderList[i].goal;
            if (gIndex == e)
            {
                // 배달 성공

                float timeRating;
                float timeLimit = orderList[i].timeLimit;
                float overTime = orderList[i].timer - timeLimit;
                if (overTime <= 0f)
                {
                    float remainPercent = Mathf.Abs(overTime) / timeLimit;
                    if (remainPercent >= Constant.remainTime_Percent) // 남은 시간이 33% 이상
                        timeRating = Constant.remainTimeRating1;
                    else
                    {
                        timeRating = Constant.Point05((Constant.remainTimeRating2 - Constant.remainTimeRating3) / Constant.remainTime_Percent * remainPercent + Constant.remainTimeRating3);
                    }
                }
                else
                {
                    float overPercent = Mathf.Min(1f, overTime / timeLimit); // 1이면 최대
                    timeRating = Constant.Point05(Constant.remainTimeRating4 * overPercent);
                }

                float hpRating;
                float hpPercent = orderList[i].hp;
                if (hpPercent == 1f)
                {
                    hpRating = Constant.remainHpRating1;
                }
                else if (hpPercent >= Constant.remainHP_Percent)
                {
                    hpRating = Constant.Point05(((Constant.remainHpRating2 - Constant.remainHpRating3) / (1f - Constant.remainHP_Percent)) * hpPercent +
                        ((-Constant.remainHP_Percent * Constant.remainHpRating2) + Constant.remainHpRating3) / (1f - Constant.remainHP_Percent));
                }
                else
                {
                    hpRating = Constant.Point05((-1f * Constant.remainHpRating4 / Constant.remainHP_Percent) * hpPercent + Constant.remainHpRating4);
                }

                float resultRating = timeRating + hpRating;
                if (resultRating > 0f) resultRating *= (1f + ResearchManager.Instance.globalEffect.ratingGet);
                resultRating = Mathf.Min(Constant.remainHpRating1 + Constant.remainTimeRating1, resultRating);
                GM.Instance.AddRating(resultRating, GM.GetRatingSource.delivery);

                int rewards = orderList[i].rewards;

                GM.Instance.AddGold(rewards, GM.GetGoldSource.delivery);
                UIManager.Instance.shopUI.AddReview(orderList[i], timeRating, hpRating);

                orderGoals[gIndex].SuccessEffect(rewards, resultRating);

                //moneyDirection.RestartSequence_Debug();
                
                orderMiniUIPair[orderList[i]].Hide();
                orderMiniUIPair.Remove(orderList[i]);
                orderList.RemoveAt(i);

                ComboCalc(resultRating);

                StatManager.Instance.CalcAverageDeliveryStat(overTime, hpPercent, rewards, resultRating);

                TutorialManager.Instance.OrderCompleted();
            }
        }

        if (OrderRemovedEvent != null)
            OrderRemovedEvent(null, orderList.Count);

        UIManager.Instance.shopUI.OrderTextUpdate();

        UIManager.Instance.OrderUIBtnUpdate();
    }

    private void ComboCalc(float result)
    {
        if (result > 0f)
        {
            GM.Instance.combo++;
        }
        else
        {
            GM.Instance.combo = 0;
        }
        if (GM.Instance.combo > StatManager.Instance.maxCombo)
            StatManager.Instance.maxCombo = GM.Instance.combo;
    }

    private void OnPlayerDamaged(object sender, float e)
    {
        for (int i = 0; i < orderList.Count; i++)
        {
            if (orderList[i].accepted)
            {
                float damage = Mathf.Max(Constant.min_damage, GM.Instance.player.DamageReduction * e);
                orderList[i].hp -= damage;
                if (orderList[i].hp < 0f) orderList[i].hp = 0f;

                orderMiniUIPair[orderList[i]].UpdateHpGauge(orderList[i].hp);

                GM.Instance.player.HitBlink();
            }
        }
    }

    [ContextMenu("새로운 주문")]
    public void NewOrder()
    {
        // 데모용 2일:3개 = 3일 : 4개 ~~
        List<int> rand = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        rand.Shuffle();
        switch (GM.Instance.day)
        {
            case 0:
            case 1:
                rand = rand.Take(3).ToList();
                break;
            case 2:
                rand = rand.Take(4).ToList();
                break;
            case 3:
                rand = rand.Take(5).ToList();
                break;
                //default:
                //    {
                //        // 이제부터 전날 평점에 영향받음
                //        float previousRating = GM.Instance.RatingDailyChange;
                //        int maxOrder = 1;
                //        if (previousRating >= 10f)
                //            maxOrder = 6;
                //        else if (previousRating >= 8f)
                //            maxOrder = 5;
                //        else if (previousRating >= 6f)
                //            maxOrder = 4;
                //        else if (previousRating >= 4f)
                //            maxOrder = 3;
                //        else if (previousRating >= 2f)
                //            maxOrder = 2;
                //        rand = rand.Take(maxOrder).ToList();
                //    }
                //    break;
        }

        //bool hasMinimumRes = GM.Instance.HasIngredient >= Constant.customer_max_ingredient;

        float totalDist = 0;
        int lastRand = rand.Count;

        for (int i = 0; i < rand.Count; i++)
        {
            float dist = (orderGoals[i].transform.position - pizzeria.transform.position).magnitude;
            float km = dist * Constant.distanceScale; // 게임상 거리 200 = 1km
            totalDist += km;

            if (totalDist >= Constant.delivery_order_km) // 최대 주행거리 이상일 경우 주문 그만 받음
            {
                lastRand = i + 1;
                break;
            }
            if (i == 5)
            {
                lastRand = i + 1;
                break;
            }
        }
        
        int halfRand = lastRand / 2;
        int hasRes = GM.Instance.HasIngredient;
        if (lastRand == 1 && hasRes >= 1) 
            halfRand = 1;
        
        if (halfRand > hasRes)
        {
            halfRand = hasRes;
        }
        lastRand -= halfRand;

        SerializableDictionary<Ingredient, int> tempRes = new SerializableDictionary<Ingredient, int>();
        foreach (var temp in GM.Instance.ingredients)
        {
            tempRes.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = temp.Key, Value = temp.Value });
        }

        int error = 0; // 플레이어가 가지고 있는 자원 내에서 생성되지 못한 주문
        for (int i = 0; i < halfRand; i++) // 플레이어가 가지고 있는 자원 내에서 생성될 주문들
        {
            bool result = AddOrder_Adjust(rand[i], ref tempRes);

            //Debug.Log($"----{i}--------------{tempRes.Count}");
            //foreach (var temp in tempRes)
            //{
            //    Debug.Log($">{temp.Key} : {temp.Value}");
            //}
            if (!result) error++;
        }
        for (int i = 0; i < lastRand + error; i++) // 아무렇게나 생성될 주문들
        {
            AddOrder(rand[i + halfRand - error]);
        }
        //Debug.Log(error);

        UIManager.Instance.OrderUIUpdate();

        OrderGoalUpdate();

        UIManager.Instance.shopUI.OrderTextUpdate();
        UIManager.Instance.shopUI.SnapTo(null);
    }

    public void NewOrder_Tutorial()
    {
        // 튜토리얼 - 노인
        SerializableDictionary<Ingredient, int> randInfo_sub = new SerializableDictionary<Ingredient, int>();
        ingredients_Tier1.Shuffle();
        randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = ingredients_Tier1[0], Value = 1 });
        randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = ingredients_Tier1[1], Value = 1 });
        randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = ingredients_Tier1[2], Value = 1 });
        int ingredientTotal = 3;

        AddOrder_Sub(4, randInfo_sub, ingredientTotal);

        UIManager.Instance.OrderUIUpdate();

        OrderGoalUpdate();

        UIManager.Instance.shopUI.OrderTextUpdate();
    }

    public int CustomerMaxIngredient()
    {
        return Constant.customer_max_ingredient + ResearchManager.Instance.globalEffect.customer_max_amount;
    }

    private int FindTier(Ingredient ingredient)
    {
        int value = 0;
        if (ingredients_Tier2_Hash.Contains(ingredient))
            value = 1;
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

        switch (orderType)
        {
            case 0:
                {
                    Ingredient randRes = ingredients2[0];
                    int randCount = Mathf.Min(tempRes[randRes], UnityEngine.Random.Range(1, CustomerMaxIngredient() + 1));
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
                    int randCount = Mathf.Min(tempRes[randRes], UnityEngine.Random.Range(1, CustomerMaxIngredient()));
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
                    int randCount = Mathf.Min(tempRes[randRes], UnityEngine.Random.Range(1, CustomerMaxIngredient() - 1));
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

                break;
        }

        int ingredientTotal = 0;
        int orderType = UnityEngine.Random.Range(0, 3);
        switch (orderType)
        {
            case 0:
                {
                    int randCount = UnityEngine.Random.Range(1, CustomerMaxIngredient() + 1);
                    selectedTierGroup.Shuffle();
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = selectedTierGroup[0], Value = randCount });
                    ingredientTotal = randCount;
                }
                break;
            case 1:
                {
                    int randCount = UnityEngine.Random.Range(1, CustomerMaxIngredient());
                    int randCount2 = UnityEngine.Random.Range(1, CustomerMaxIngredient() - randCount + 1);
                    selectedTierGroup.Shuffle();
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = selectedTierGroup[0], Value = randCount });
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = selectedTierGroup[1], Value = randCount2 });
                    ingredientTotal = randCount + randCount2;
                }
                break;
            case 2:
                {
                    int randCount = UnityEngine.Random.Range(1, CustomerMaxIngredient() - 1);
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
        float dist = (orderGoals[goal].transform.position - pizzeria.transform.position).magnitude;
        float km = dist * Constant.distanceScale; // 게임상 거리 200 = 1km

        List<PizzaInfo> randPizzas = new List<PizzaInfo>();

        PizzaInfo randInfo = new PizzaInfo { ingredients = randInfo_sub };
        randPizzas.Add(randInfo);

        int rewards = ingredientTotal * 130; // 현재 재료값은 100으로 고정 => 130% 받음
        float mileBouns = Constant.delivery_reward_1km * km;
        rewards += (int)mileBouns;

        // 티어별 보상 // 2티어면 2배 보상
        rewards = (int)((tier + 1f) * (1f + ResearchManager.Instance.globalEffect.goldGet) * rewards);

        float timeLimit = (Constant.delivery_timeLimit_1km * km) * (1f + ResearchManager.Instance.globalEffect.customer_timelimit);

        OrderInfo newOrder = new OrderInfo
        {
            accepted = false,
            customerIdx = goal,
            goal = goal,
            pizzas = randPizzas,
            km = km,
            rewards = rewards,
            hp = 1f,
            timeLimit = timeLimit,
            timer = 0f,
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
        for (int i = orderList.Count - 1; i >= 0; i--)
        {
            if (orderList[i].accepted)
            {
                orderList[i].timer += Time.deltaTime;

                orderMiniUIPair[orderList[i]].UpdateTimer(orderList[i]);
            }
            else
            {
                if (orderList[i].timeLimit * 0.5f > GM.Instance.remainTime) // 배달 제한 시간이 남은 영업 시간 초과시
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

        // 재료 소모
        for (int i = 0; i < info.pizzas.Count; i++)
        {
            foreach (var temp in info.pizzas[i].ingredients)
            {
                GM.Instance.ingredients[temp.Key] -= temp.Value;
            }
        }
        UIManager.Instance.UpdateIngredients();
        UIManager.Instance.OffAll_Ingredient_Highlight();

        UIManager.Instance.shopUI.OrderTextUpdate();

        UIManager.Instance.OrderUIBtnUpdate();

        TutorialManager.Instance.OrderAccpeted();

        StatManager.Instance.acceptedOrders++;

        ovenMiniGame.StartOven(info);
        pizzaDirection.RestartSequence(info);
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
            if (orderList[i].accepted)
            {
                count += orderList[i].pizzas.Count;
            }
        }
        return count;
    }

    public bool CheckIngredient(OrderInfo info)
    {
        bool makable = true;

        for (int i = 0; i < info.pizzas.Count; i++)
        {
            foreach (var temp in info.pizzas[i].ingredients)
            {
                if (GM.Instance.ingredients[temp.Key] < temp.Value)
                {
                    makable = false;
                    break;
                }
            }
            if (!makable) break;
        }

        return makable;
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
                UIManager.Instance.shopUI.AddReview(orderList[i], -10000f, Constant.delivery_Not_completed_rating); // 구분 기능 -1000 => -5는 점수
            }

            orderMiniUIPair[orderList[i]].Hide();
            orderMiniUIPair.Remove(orderList[i]);
            orderList.RemoveAt(i);
        }

        UIManager.Instance.OrderUIReset();
        UIManager.Instance.shopUI.OrderTextUpdate();

        if (OrderRemovedEvent != null)
            OrderRemovedEvent(null, orderList.Count);
    }

    private void NotAcceptedOrderPenalty(OrderInfo info)
    {
        if (CheckIngredient(info)) // 재료를 가지고 있었던 경우
        {
            GM.Instance.AddRating(Constant.delivery_Not_accepted_rating, GM.GetRatingSource.notAccepted);
            UIManager.Instance.shopUI.AddReview(info, -1000f, Constant.delivery_Not_accepted_rating); // 구분 기능 -100 => -2.5는 점수
        }
        else
        {
            GM.Instance.AddRating(Constant.delivery_Impossible_accepted_rating, GM.GetRatingSource.notAccepted);
            UIManager.Instance.shopUI.AddReview(info, -100f, Constant.delivery_Impossible_accepted_rating); // 구분 기능 -100 => -1.5는 점수
        }
    }
}
