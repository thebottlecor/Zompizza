using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopGate : MonoBehaviour
{

    public Transform target;
    public float openDist;
    public float gateSpeed = 1f;

    public Transform movingChild;
    public Transform movingChild_Right;

    public bool alwaysClosed;

    public AudioSource openSFX;
    private bool sfxPlayed;

    [Header("개발자 설정값")]
    public Vector3 closedPos;
    public Vector3 openPos;

    public Vector3 closedPos_Right;
    public Vector3 openPos_Right;

    public Vector3 targetPos_Left;
    public Vector3 targetPos_Right;

    Vector3 vel = Vector3.zero;
    Vector3 vel2 = Vector3.zero;
    public float smoothTime = 0.1f;

    void Update()
    {
        if (alwaysClosed)
        {
            movingChild.position = closedPos;
            movingChild_Right.position = closedPos_Right;
            return;
        }

        float dist = (transform.position - target.position).magnitude;

        if (dist > 100f) return;

        if (dist > openDist)
        {
            if (targetPos_Left == openPos) sfxPlayed = false;

            targetPos_Left = closedPos;
            targetPos_Right = closedPos_Right;
        }
        else
        {
            if (targetPos_Left == closedPos) sfxPlayed = false;

            targetPos_Left = openPos;
            targetPos_Right = openPos_Right;
        }

        float targetDist = (targetPos_Left - movingChild.position).magnitude;

        if (Mathf.Abs(targetDist) > 1f)
        {
            if (!sfxPlayed && !openSFX.isPlaying)
            {
                openSFX.Play();
                sfxPlayed = true;
            }
        }
        else
        {
            sfxPlayed = false;
        }

        movingChild.position = Vector3.SmoothDamp(movingChild.position, targetPos_Left, ref vel, smoothTime, Time.deltaTime * gateSpeed);
        movingChild_Right.position = Vector3.SmoothDamp(movingChild_Right.position, targetPos_Right, ref vel2, smoothTime, Time.deltaTime * gateSpeed);
    }
}
