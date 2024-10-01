using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerIndicator : MonoBehaviour
{

    public Vector3 fixedAngle = new Vector3(0f, 135f, 0f);

    void Update()
    {
        bool mode2 = GM.Instance.player.cam.secondMode;

        if (mode2) // 새로운 시점
        {
            transform.LookAt(GM.Instance.player.cam.transform);
        }
        else // 기존 시점
        {
            transform.eulerAngles = fixedAngle;
        }

    }
}
