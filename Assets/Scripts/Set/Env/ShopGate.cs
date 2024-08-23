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
    private Vector3 closedPos;
    private Vector3 closedPos_Right;

    public float openMove = 8.5f;

    private Vector3 openPos;
    private Vector3 openPos_Right;

    private Vector3 targetPos_Left;
    private Vector3 targetPos_Right;

    Vector3 vel = Vector3.zero;
    Vector3 vel2 = Vector3.zero;
    public float smoothTime = 0.1f;

    private void Start()
    {
        closedPos = movingChild.position;
        closedPos_Right = movingChild_Right.position;

        openPos = closedPos + new Vector3(0f, 0f, openMove);
        openPos_Right = closedPos_Right + new Vector3(0f, 0f, -1f * openMove);
    }

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
