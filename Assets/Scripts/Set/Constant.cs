using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constant
{

    public const float dayTime = 360f;
    public const int dayStartHour = 6;
    public const int dayEndHour = 18;
    public const float oneHour = dayTime / (dayEndHour - dayStartHour);
    public const float oneMinute = oneHour / 60f;
    public const float oneSec = oneMinute / 60f;

    public const int npcNameOffset = 2;
    public const int ingredientSpriteOffset = 6;

    // 배달 관련
    public const float distanceScale = 0.005f; // 게임상 거리 200 = 1km
    public const float distance_1km = 1 / distanceScale;

    public const float delivery_timeLimit_1km = 60f;
    public const int delivery_reward_1km = 100;

    public const float delivery_order_km = 12f;

    public const int customer_max_ingredient = 10;

    public const float delivery_Not_completed_rating = -10f;
    public const float delivery_Not_accepted_rating = -5f;
    public const float delivery_Impossible_accepted_rating = -2.5f;
}
