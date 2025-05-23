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
    //public List<PizzaInfo> pizzas;

    // 주문 배달 장소의 인덱스 정보 (OrderManager 참조)
    public int goal;
    // 주문 배달 장소와의 거리
    public float km;

    public int comboSpecial; // 주문자의 취향 (0은 없는 경우)

    // 배달 성공시 받는 금액
    public int rewards;

    //
    // inputs과 rewards는 가변적!
    //

    // 호감도에 따른 보너스로 받은 금액 표시용 (실제 받는 돈은 rewards에 이미 포함됨)
    public int bouns_friendship;

    // 현재 피자 Hp 퍼센트 (최소 0 최대 1)
    public float hp;

    // 배달 제한 시간
    public float timeLimit;

    // 배달 수락후 지나간 시간
    public float timer;

    // 산타가 훔쳐갔어요!
    public bool stolen;

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
    meat4,
    meat5,
    meat6,
    meat7,
    meat8,
    meat9,
    meat10,
    meat11,
    meat12,

    vegetable1,
    vegetable2,
    vegetable3,
    vegetable4,
    vegetable5,
    vegetable6,
    vegetable7,
    vegetable8,
    vegetable9,
    vegetable10,
    vegetable11,
    vegetable12,

    herb1,
    herb2,
    herb3,
    herb4,
    herb5,
    herb6,
    herb7,
    herb8,
    herb9,
    herb10,
    herb11,
    herb12,
}
