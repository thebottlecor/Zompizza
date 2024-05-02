using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constant
{

    public const float dayTime = 180f;
    public const int dayStartHour = 8;
    public const int dayEndHour = 20;
    public const float oneHour = dayTime / (dayEndHour - dayStartHour);
    public const float oneMinute = oneHour / 60f;
    public const float oneSec = oneMinute / 60f;

    public const float distanceScale = 0.005f;

    public const int npcNameOffset = 2;
    public const int ingredientSpriteOffset = 6;

}
