using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerIndicator : MonoBehaviour
{

    public Vector3 fixedAngle = new Vector3(0f, 135f, 0f);

    void Update()
    {
        transform.eulerAngles = fixedAngle;
    }
}
