using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class OrderInfo
{
    // 주문을 승낙했는지?
    public bool accepted;

    // 주문자 인덱스
    public int customerIdx;

    // 주문한 피자
    public List<PizzaInfo> pizzas;

    // 주문 배달 장소의 인덱스 정보 (OrderManager 참조)
    public int goal;
    // 주문 배달 장소와의 거리
    public float km;

    // 배달 성공시 받는 금액
    public int rewards;

    // 현재 피자 Hp 퍼센트 (최소 0 최대 1)
    public float hp;

    // 배달 제한 시간
    public float timeLimit;
    // 배달 수락후 지나간 시간
    public float timer;

}

[System.Serializable]
public struct PizzaInfo
{
    // 필요한 재료
    public SerializableDictionary<Ingredient, int> ingredients;
}

public enum Ingredient
{
    meat1,
    meat2,
    meat3,

    vegetable1,
    vegetable2,
    vegetable3,

    herb1,
    herb2,
    herb3,
}
