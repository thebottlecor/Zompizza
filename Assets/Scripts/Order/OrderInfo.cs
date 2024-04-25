using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OrderInfo
{

    // 주문을 승낙했는지?
    public bool accepted;

    // 주문자 인덱스
    public int customerIdx;

    // 주문 배달 장소의 인덱스 정보 (OrderManager 참조)
    public int goal;

    // 배달 성공시 받는 금액
    public int rewards;

    // 배달 제한 시간
    public float timeLimit;

}
