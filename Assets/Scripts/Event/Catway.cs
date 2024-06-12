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
            //������ ���� ��ǥ ����(Vector3)�� ��������� ��ȯ�ϴ� �޼���
            Quaternion targetRotation = Quaternion.LookRotation(dir);

            //(���۰�, ��ǥ��, ȸ�� �ӵ�)�� ���ڷ� �޾� ȸ�� ���� �������ִ� �޼���
            Quaternion rotateAmount = Quaternion.RotateTowards(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);

            //ȸ���� ����
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
