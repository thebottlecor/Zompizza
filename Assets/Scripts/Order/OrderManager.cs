using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : Singleton<OrderManager>
{

    public Transform pizzeria;
    public List<OrderGoal> orderGoals;

    public List<OrderInfo> orderList;

    public PizzaDirection pizzaDirection;

    public SerializableDictionary<OrderInfo, OrderMiniUI> orderMiniUIPair;

    private void Start()
    {
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
                // ��� ����
                GM.Instance.AddGold(orderList[i].rewards);
                GM.Instance.AddRating(0.5f);

                orderGoals[gIndex].SuccessEffect();
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
            }
        }
    }

    [ContextMenu("���ο� �ֹ�")]
    public void NewOrder()
    {
        int goal = 0;
        float distance = (orderGoals[goal].transform.position - pizzeria.transform.position).magnitude;

        OrderInfo newOrder = new OrderInfo
        {
            accepted = false,
            customerIdx = 0,
            goal = goal,
            pizzas = new List<PizzaInfo>
            {
                new PizzaInfo { ingredients = new SerializableDictionary<Ingredient, int> { new SerializableDictionary<Ingredient, int>.Pair { Key = Ingredient.meat1, Value = 1} }},
            },
            distance = distance,
            rewards = UnityEngine.Random.Range(500, 1000),
            hp = 1f,
            timeLimit = 60f,
            timer = 0f,
        };
        orderList.Add(newOrder);
        PairingMiniUI(newOrder);

        goal = 2;
        distance = (orderGoals[goal].transform.position - pizzeria.transform.position).magnitude;

        newOrder = new OrderInfo
        {
            accepted = false,
            customerIdx = 2,
            goal = goal,
            pizzas = new List<PizzaInfo>
            {
                new PizzaInfo { ingredients = new SerializableDictionary<Ingredient, int> { new SerializableDictionary<Ingredient, int>.Pair { Key = Ingredient.herb1, Value = 1},
                new SerializableDictionary<Ingredient, int>.Pair { Key = Ingredient.meat2, Value = 1}}},
            },
            distance = distance,
            rewards = UnityEngine.Random.Range(500, 1000),
            hp = 1f,
            timeLimit = 60f,
            timer = 0f,
        };
        orderList.Add(newOrder);
        PairingMiniUI(newOrder);

        UIManager.Instance.OrderUIUpdate();

        OrderGoalUpdate();
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

        // ��� �Ҹ�
        for (int i = 0; i < info.pizzas.Count; i++)
        {
            foreach (var temp in info.pizzas[i].ingredients)
            {
                GM.Instance.ingredients[temp.Key] -= temp.Value;
            }
        }
        UIManager.Instance.UpdateIngredients();

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
}
