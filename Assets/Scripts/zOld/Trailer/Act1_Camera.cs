using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Act1_Camera : MonoBehaviour
{

    public CameraFollow2 cameraFollow2;
    public Transform[] innerCameraTarget;

    public GameObject carObj;
    public GameObject hideRocket;
    public Catway cat;


    public float[] interval;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Direction();
        }
    }

    public void Direction()
    {
        cat.Cat_Direction();
        hideRocket.SetActive(false);   
        carObj.SetActive(false);

        Sequence sequence = DOTween.Sequence().SetUpdate(true);

        sequence.AppendCallback(() =>
        {
            Debug.Log(Time.frameCount + " 1번째");
            cameraFollow2.carTransform = innerCameraTarget[0];
            cameraFollow2.absoluteInitCameraPosition = new Vector3(-35.48f, 64.8f, 100f);
        });
        sequence.AppendInterval(interval[0]);
        sequence.AppendCallback(() =>
        {
            Debug.Log(Time.frameCount + " 2번째");
            cameraFollow2.absoluteInitCameraPosition = new Vector3(0.39f, 3.05f, 41.7f);
        });
        sequence.AppendInterval(interval[1]);
        sequence.AppendCallback(() =>
        {
            Debug.Log(Time.frameCount + " 3번째");
            cameraFollow2.carTransform = innerCameraTarget[1];
            cameraFollow2.absoluteInitCameraPosition = new Vector3(0.39f, -1.68f, 14.6f);
        });
    }
}
