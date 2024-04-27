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

    protected override void AddListeners()
    {
        OrderGoal.PlayerArriveEvent += OnPlayerArrive;
    }

    protected override void RemoveListeners()
    {
        OrderGoal.PlayerArriveEvent -= OnPlayerArrive;
    }

    private void OnPlayerArrive(object sender, int e)
    {
        for (int i = orderList.Count - 1; i >= 0; i--)
        {
            int gIndex = orderList[i].goal;
            if (gIndex == e)
            {
                // 배달 성공
                Debug.Log($"배달 성공 {orderList[i].rewards}");
                orderGoals[gIndex].SuccessEffect();   
                //
                orderList.RemoveAt(i);
            }
        }
    }

    private void Start()
    {
        orderList = new List<OrderInfo>();

        NewOrder();
    }

    [ContextMenu("새로운 주문")]
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
            timeLimit = 60f,
        };
        orderList.Add(newOrder);

        //goal = 1;
        //distance = (orderGoals[goal].transform.position - pizzeria.transform.position).magnitude;

        //newOrder = new OrderInfo
        //{
        //    accepted = false,
        //    customerIdx = 1,
        //    goal = goal,
        //    pizzas = new List<PizzaInfo>
        //    {
        //        new PizzaInfo { stack = 2, ingredients = new SerializableDictionary<Ingredient, int> { new SerializableDictionary<Ingredient, int>.Pair { Key = Ingredient.vegetable1, Value = 1} }},
        //    },
        //    distance = distance,
        //    rewards = UnityEngine.Random.Range(500, 1000),
        //    timeLimit = 60f,
        //};
        //orderList.Add(newOrder);

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
            timeLimit = 60f,
        };
        orderList.Add(newOrder);

        UIManager.Instance.OrderUIUpdate();

        OrderGoalUpdate();
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
                orderGoals[i].Show();
        }
    }

    public void OrderAccepted(OrderInfo info)
    {
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
