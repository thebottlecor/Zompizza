using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Act3_Sunset : MonoBehaviour
{
    public Transform carTransform;
    public Transform carInitPosNAngle;

    private void Start()
    {
        StartCoroutine(DelayStart());
    }

    private IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(0.1f);

        Direction();
    }

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
    }
}
