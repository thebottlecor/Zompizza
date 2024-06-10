using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Vehicle3DShowcase : MonoBehaviour
{

    public Transform[] vehicleModels;

    public Transform stackTarget11;

    public void ResetPos()
    {
        Vector3 tempPos2 = stackTarget11.position;
        tempPos2.z = 0f;

        for (int i = 0; i < vehicleModels.Length; i++)
        {
            vehicleModels[i].position = tempPos2;
        }
    }
}
