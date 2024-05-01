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

    private void Start()
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

                float statePercent = orderList[i].hp;
                bool keepTime = orderList[i].timer <= orderList[i].timeLimit;

                // 평점 최대 +0.5, 최소 -0.5
                // 시간 초과시 -0.5 깎임
                // 상태 0%면 -0.5 (비례)

                float ratingModify = statePercent;
                if (!keepTime) ratingModify -= 1f;

                float resultRating = 0.5f * ratingModify;

                GM.Instance.AddRating(resultRating);

                // 최대 평점 구간 (90%~100%)일시 10% 보너스
                // 마이너스일시 0에서 시작해서 최대 20% 패널티

                bool bonus = false;

                int rewards = orderList[i].rewards;
                if (ratingModify >= 0.9f)
                {
                    rewards = (int)(1.1f * rewards);
                    bonus = true;
                }
                else if (ratingModify < 0f)
                {
                    rewards = (int)((1f - (0.2f * ratingModify)) * rewards);
                }

                GM.Instance.AddGold(rewards);

                orderGoals[gIndex].SuccessEffect(rewards, bonus, resultRating);

                //moneyDirection.RestartSequence_Debug();
                //
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

        for (int i = 0; i < 4; i++)
        {
            AddOrder(rand[i]);
        }

        UIManager.Instance.OrderUIUpdate();

        OrderGoalUpdate();
    }

    private void AddOrder(int goal)
    {
        float distance = (orderGoals[goal].transform.position - pizzeria.transform.position).magnitude;

        List<PizzaInfo> randPizzas = new List<PizzaInfo>();
        SerializableDictionary<Ingredient, int> randInfo_sub = new SerializableDictionary<Ingredient, int>();

        int rand2 = UnityEngine.Random.Range(1, 4);
        int ingredientTotal = 0;
        ingredients.Shuffle();
        for (int i = 0; i < rand2; i++)
        {
            int count = UnityEngine.Random.Range(1, 4);
            ingredientTotal += count;
            randInfo_sub.Add(new SerializableDictionary<Ingredient, int>.Pair { Key = ingredients[i], Value = count });
        }

        PizzaInfo randInfo = new PizzaInfo { ingredients = randInfo_sub };
        randPizzas.Add(randInfo);

        int rewards = UnityEngine.Random.Range(400, 600);
        rewards += ingredientTotal * 50;
        // 400이 가장 작은 거리로 정함
        rewards = (int)(rewards * ((distance / 800f) + 0.5f));
        float timeLimit = 90f * (distance / 400f);

        OrderInfo newOrder = new OrderInfo
        {
            accepted = false,
            customerIdx = goal,
            goal = goal,
            pizzas = randPizzas,
            distance = distance,
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
            if (orderList[i].accepted)
            {
                // 미완료 패널티
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
