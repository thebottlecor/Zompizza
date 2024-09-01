using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constant
{

    public const float dayTime = 180f;
    public const int dayStartHour = 6;
    public const int dayEndHour = 18;
    public const float oneHour = dayTime / (dayEndHour - dayStartHour);
    public const float oneMinute = oneHour / 60f;
    public const float oneSec = oneMinute / 60f;

    public const int npcNameOffset = 2;
    public const int ingredientSpriteOffset = 12;

    // 배달 관련
    public const float distanceScale = 0.005f; // 게임상 거리 200 = 1km
    public const float distance_1km = 1 / distanceScale;

    // public const float delivery_timeLimit_1km = 35f;
    public const float delivery_timeLimit_1km = 35f; // 기존 35초 => 45초
    public const float delivery_timeLimit_base = 20f; // 기본으로 20초 보장
    public const int delivery_reward_1km = 250;
    public const int delivery_reward_ingredients = 130;

    public const float delivery_order_km = 10f;

    public const int customer_max_ingredient = 7;

    public const float delivery_Not_completed_rating = -5f;
    public const float delivery_Not_accepted_rating = -2.5f;
    public const float delivery_Impossible_accepted_rating = -1.5f;

    public const float crash_damage = 0.05f;
    public const float zombie_damage = 0.01f;
    public const float min_damage = 0f;

    // 동시 배달 가능 수 (기본)
    public const int explorationQuantityMax = 30;

    public const float remainTime_Percent = 0.5f;
    public const float remainTimeRating1 = 2.5f;
    public const float remainTimeRating2 = 2f;
    public const float remainTimeRating3 = 0.5f;
    public const float remainTimeRating4 = -2.5f;

    public const float remainHP_Percent = 0.5f;
    //public const float remainHP_Percent = 0.9f;
    public const float remainHpRating1 = 2.5f;
    public const float remainHpRating2 = 2f;
    public const float remainHpRating3 = 0.5f;
    public const float remainHpRating4 = -2.5f;

    public static float Point05(float x) => Mathf.Floor(x * 2f) / 2f;


    public const float winRating = 20f;


    public const float friendShip3 = 4f;
    public const float friendShip2 = 3.5f;
    public const float friendShip1 = 3f;

    // 주민 관련
    public const int maxVillager = 5;
    public const int villagerIncome = 500;
    public const float sos_timeLimit_1km = 55f; //1.5f * delivery_timeLimit_1km; (기존 35초일때 52.5)

    // 높이 관련

    public const float spawnPosY = 3.5f;
}
