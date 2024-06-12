using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Catway : MonoBehaviour
{

    public Transform[] wayPoints;

    public float rotSpeed = 270f;
    public float speed = 5f;

    private Animator animator;
    private int targetWayPoint;
    private float dealyToTargetTimer;

    private void Start()
    {
        animator = GetComponent<Animator>();
        RandomTarget();
    }

    public void RandomTarget()
    {
        targetWayPoint = UnityEngine.Random.Range(0, wayPoints.Length);
        dealyToTargetTimer = UnityEngine.Random.Range(0.75f, 3.5f);
    }

    void Update()
    {
        if (dealyToTargetTimer > 0f)
        {
            animator.SetBool("Walk", false);
            dealyToTargetTimer -= Time.deltaTime;
            return;
        }

        Vector3 diff = (wayPoints[targetWayPoint].position - transform.position);
        Vector3 dir = diff.normalized;
        float dist = diff.magnitude;

        if (dist >= 0.25f)
        {
            //위에서 구한 목표 방향(Vector3)을 사분위수로 전환하는 메서드
            Quaternion targetRotation = Quaternion.LookRotation(dir);

            //(시작값, 목표값, 회전 속도)를 인자로 받아 회전 값을 연산해주는 메서드
            Quaternion rotateAmount = Quaternion.RotateTowards(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);

            //회전값 적용
            transform.rotation = rotateAmount;

            if (Mathf.Abs(Quaternion.Angle(transform.rotation, targetRotation)) < 10f)
            {
                transform.position += speed * Time.deltaTime * dir;

                animator.SetBool("Walk", true);
            }
        }
        else
        {
            RandomTarget();
        }
    }
}
