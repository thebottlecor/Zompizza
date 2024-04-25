using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : Singleton<OrderManager>
{

    public List<OrderGoal> orderGoals;

    public List<OrderInfo> orderList;

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
        OrderInfo newOrder = new OrderInfo
        {
            accepted = false,
            customerIdx = 0,
            goal = 0,
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

}
