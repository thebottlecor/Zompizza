using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OrderManager : Singleton<OrderManager>
{

    public Transform pizzeria;
    public List<OrderGoal> orderGoals;

    public List<OrderInfo> orderList;

    public PizzaDirection pizzaDirection;
    public MoneyDirection moneyDirection;

    public SerializableDictionary<OrderInfo, OrderMiniUI> orderMiniUIPair;

    public static EventHandler AllOrderRemovedEvent;

    public List<Ingredient> ingredients;

    public void Init()
    {
        var list = Enum.GetValues(typeof(Ingredient));
        ingredients = new List<Ingredient>();
        foreach (var temp in list)
        {
            Ingredient ingredient = (Ingredient)temp;
            ingredients.Add(ingredient);
        }

        for (int i = 0; i < orderGoals.Count; i++)
        {
            orderGoals[i].index = i;
            orderGoals[i].minimapItem.spriteColor = DataManager.Instance.uiLib.customerPinColor[i];

            // orderGoals[i].minimapItem_customer.itemSprite = ?
        }

        orderMiniUIPair = new SerializableDictionary<OrderInfo, OrderMiniUI>();
        orderList = new List<OrderInfo>();

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
                float overTime = orderList[i].timer - orderList[i].timeLimit;
                if (overTime <= 0f)
                    timeRating = 5f;
                else
                {
                    float overPercent = Mathf.Min(1f, overTime / orderList[i].timeLimit); // 1이면 최대
                    timeRating = -5f * overPercent;
                }

                float hpRating;
                float hpPercent = orderList[i].hp;
                if (hpPercent >= 0.9f)
                {
                    hpRating = 5f;
                }
                else
                {
                    hpRating = 11f * hpPercent - 5f; // 90%면 +4.9 ~ 0%면 -5
                }

                float resultRating = timeRating + hpRating;
                GM.Instance.AddRating(resultRating, GM.GetRatingSource.delivery);

                int rewards = orderList[i].rewards;

                GM.Instance.AddGold(rewards, GM.GetGoldSource.delivery);
                UIManager.Instance.shopUI.AddReview(orderList[i], timeRating, hpRating);

                orderGoals[gIndex].SuccessEffect(rewards, resultRating);

                //moneyDirection.RestartSequence_Debug();
                
                orderMiniUIPair[orderList[i]].Hide();
                orderMiniUIPair.Remove(orderList[i]);
                orderList.RemoveAt(i);
            }
        }
    }

    private void OnPlayerDamaged(object sender, float e)
    {
        for (int i = 0; i < orderList.Count; i++)
        {
            if (orderList[i].accepted)
            {
                orderList[i].hp -= e;
                if (orderList[i].hp < 0f) orderList[i].hp = 0f;

                orderMiniUIPair[orderList[i]].UpdateHpGauge(orderList[i].hp);

                GM.Instance.player.HitBlink();
            }
        }
    }

    [ContextMenu("새로운 주문")]
    public void NewOrder()
    {

        List<int> rand = new List<int> { 0, 1, 2, 3, 4, 5 };
        rand.Shuffle();

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
                    int randCount = Mathf.Min(tempRes[randRes], UnityEngine.Random.Range(1, Constant.customer_max_ingredient + 1));
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = randRes, Value = randCount });
                    ingredientTotal = randCount;
                    tempRes[randRes] -= randCount;
                }
                break;
            case 1:
                {
                    Ingredient randRes = ingredients2[0];
                    Ingredient randRes2 = ingredients2[1];
                    int randCount = Mathf.Min(tempRes[randRes], UnityEngine.Random.Range(1, Constant.customer_max_ingredient));
                    int randCount2 = Mathf.Min(tempRes[randRes2], UnityEngine.Random.Range(1, Constant.customer_max_ingredient - randCount + 1));
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = randRes, Value = randCount });
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = randRes2, Value = randCount2 });
                    ingredientTotal = randCount + randCount2;
                    tempRes[randRes] -= randCount;
                    tempRes[randRes2] -= randCount2;
                }
                break;
            case 2:
                {
                    Ingredient randRes = ingredients2[0];
                    Ingredient randRes2 = ingredients2[1];
                    Ingredient randRes3 = ingredients2[2];
                    int randCount = Mathf.Min(tempRes[randRes], UnityEngine.Random.Range(1, Constant.customer_max_ingredient - 1));
                    int randCount2 = Mathf.Min(tempRes[randRes2], UnityEngine.Random.Range(1, Constant.customer_max_ingredient - randCount));
                    int randCount3 = Mathf.Min(tempRes[randRes3], UnityEngine.Random.Range(1, Constant.customer_max_ingredient - randCount - randCount2 + 1));
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = randRes, Value = randCount });
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = randRes2, Value = randCount2 });
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = randRes3, Value = randCount3 });
                    ingredientTotal = randCount + randCount2 + randCount3;
                    tempRes[randRes] -= randCount;
                    tempRes[randRes2] -= randCount2;
                    tempRes[randRes3] -= randCount3;
                }
                break;
        }

        AddOrder_Sub(goal, randInfo_sub, ingredientTotal);
        return true;
    }

    private void AddOrder(int goal)
    {
        SerializableDictionary<Ingredient, int> randInfo_sub = new SerializableDictionary<Ingredient, int>();

        int ingredientTotal = 0;
        int orderType = UnityEngine.Random.Range(0, 3);
        switch (orderType)
        {
            case 0:
                {
                    int randCount = UnityEngine.Random.Range(1, Constant.customer_max_ingredient + 1);
                    ingredients.Shuffle();
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = ingredients[0], Value = randCount });
                    ingredientTotal = randCount;
                }
                break;
            case 1:
                {
                    int randCount = UnityEngine.Random.Range(1, Constant.customer_max_ingredient);
                    int randCount2 = UnityEngine.Random.Range(1, Constant.customer_max_ingredient - randCount + 1);
                    ingredients.Shuffle();
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = ingredients[0], Value = randCount });
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = ingredients[1], Value = randCount2 });
                    ingredientTotal = randCount + randCount2;
                }
                break;
            case 2:
                {
                    int randCount = UnityEngine.Random.Range(1, Constant.customer_max_ingredient - 1);
                    int randCount2 = UnityEngine.Random.Range(1, Constant.customer_max_ingredient - randCount);
                    int randCount3 = UnityEngine.Random.Range(1, Constant.customer_max_ingredient - randCount - randCount2 + 1);
                    ingredients.Shuffle();
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = ingredients[0], Value = randCount });
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = ingredients[1], Value = randCount2 });
                    randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = ingredients[2], Value = randCount3 });
                    ingredientTotal = randCount + randCount2 + randCount3;
                }
                break;
        }

        AddOrder_Sub(goal, randInfo_sub, ingredientTotal);
    }

    private void AddOrder_Sub(int goal, SerializableDictionary<Ingredient, int> randInfo_sub, int ingredientTotal, int test = 0)
    {
        float dist = (orderGoals[goal].transform.position - pizzeria.transform.position).magnitude;
        float km = dist * Constant.distanceScale; // 게임상 거리 200 = 1km

        List<PizzaInfo> randPizzas = new List<PizzaInfo>();

        PizzaInfo randInfo = new PizzaInfo { ingredients = randInfo_sub };
        randPizzas.Add(randInfo);

        int rewards = ingredientTotal * 130; // 현재 재료값은 100으로 고정 => 130% 받음
        rewards += (int)(Constant.delivery_reward_1km * km);

        rewards += test;

        float timeLimit = (Constant.delivery_timeLimit_1km * km);

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
        for (int i = 0; i < orderList.Count; i++)
        {
            if (orderList[i].accepted)
            {
                orderList[i].timer += Time.deltaTime;

                orderMiniUIPair[orderList[i]].UpdateTimer(orderList[i]);
            }
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

        pizzaDirection.RestartSequence(info);
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
                if (CheckIngredient(orderList[i])) // 재료를 가지고 있었던 경우
                {
                    GM.Instance.AddRating(Constant.delivery_Not_accepted_rating, GM.GetRatingSource.notAccepted);
                    UIManager.Instance.shopUI.AddReview(orderList[i], -2.5f, -2.5f);
                }
                else
                {
                    GM.Instance.AddRating(Constant.delivery_Impossible_accepted_rating, GM.GetRatingSource.notAccepted);
                    UIManager.Instance.shopUI.AddReview(orderList[i], -1.25f, -1.25f);
                }
            }
            else
            {
                // 미완료 리뷰 남기기 -10점
                UIManager.Instance.shopUI.AddReview(orderList[i], -5f, -5f);
            }

            orderMiniUIPair[orderList[i]].Hide();
            orderMiniUIPair.Remove(orderList[i]);
            orderList.RemoveAt(i);
        }

        UIManager.Instance.OrderUIReset();

        if (AllOrderRemovedEvent != null)
            AllOrderRemovedEvent(null, null);
    }
}
