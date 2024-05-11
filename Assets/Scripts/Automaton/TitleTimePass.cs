using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleTimePass : MonoBehaviour
{

    public float timer;

    public Light globalLight;
    public Vector3 lightAngleX = new Vector3(140f, 120f, 160f);
    public Vector2 lightAngleY = new Vector2(-80f, 100f);

    void Update()
    {
        timer += 0.5f * Time.deltaTime;

        if (timer >= Constant.dayTime)
        {
            timer = Constant.dayTime;
        }

        int hour = (int)(timer / Constant.oneHour);
        //int minute = (int)((timer - hour * Constant.oneHour) / Constant.oneMinute);

        hour += Constant.dayStartHour;
        if (hour >= Constant.dayEndHour)
        {
            hour = Constant.dayEndHour;
            //minute = 0;
        }

        float timePercent = timer / Constant.dayTime;
        globalLight.color = DataManager.Instance.uiLib.timeLightGradient.Evaluate(timePercent);
        Vector3 lightAngle = globalLight.transform.localEulerAngles;
        lightAngle.y = (lightAngleY.y - lightAngleY.x) * timePercent + lightAngleY.x;

        if (hour < 12)
        {
            timePercent = timer / (Constant.oneHour * 6);
            lightAngle.x = (lightAngleX.y - lightAngleX.x) * timePercent + lightAngleX.x;
        }
        else
        {
            timePercent = timer / (Constant.oneHour * 12) - 0.5f;
            lightAngle.x = (lightAngleX.z - lightAngleX.y) * timePercent + lightAngleX.y;
        }

        globalLight.transform.localEulerAngles = lightAngle;
    }
}
