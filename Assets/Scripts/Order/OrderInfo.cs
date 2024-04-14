using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OrderInfo
{


    // 주문 배달 장소의 인덱스 정보 (OrderManager 참조)
    public int goal;

    // 배달 성공시 받는 금액
    public int pay;

}
