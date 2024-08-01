using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Vehicle3DShowcase : MonoBehaviour
{

    public Transform[] vehicleModels;

    public Transform stackTarget11;

    public Canvas scaleReferCanvas;

    public float rotSpeed = 100f;

    public void ResetPos()
    {
        Vector3 tempPos2 = stackTarget11.position;
        tempPos2.z = 0f;

        for (int i = 0; i < vehicleModels.Length; i++)
        {
            vehicleModels[i].position = tempPos2;
            vehicleModels[i].localScale = (scaleReferCanvas.transform.localScale.x / 0.00925f) * Vector3.one;
        }
    }

    public void ResetAngle()
    {
        for (int i = 0; i < vehicleModels.Length; i++)
        {
            vehicleModels[i].localEulerAngles = new Vector3(0f, 14f, 0f);
        }
    }

    private void Update()
    {
        for (int i = 0; i < vehicleModels.Length; i++)
        {
            vehicleModels[i].Rotate(new Vector3(0, rotSpeed * Time.unscaledDeltaTime, 0));
        }
    }
}
