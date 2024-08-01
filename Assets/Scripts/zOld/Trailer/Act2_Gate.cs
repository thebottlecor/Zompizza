using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Act2_Gate : MonoBehaviour
{
    public Transform carTransform;
    public Transform carInitPosNAngle;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Direction();
        }
    }

    public void Direction()
    {
        carTransform.transform.position = carInitPosNAngle.position;
        carTransform.transform.eulerAngles = carInitPosNAngle.eulerAngles;

        //Sequence sequence = DOTween.Sequence().SetUpdate(true);

    }
}
