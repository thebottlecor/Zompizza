using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Catway : MonoBehaviour
{

    public Transform[] wayPoints;

    private Animator animator;
    private int targetWayPoint;
    private float dealyToTargetTimer;

    protected IAstarAI ai;
    protected AIDestinationSetter destinationSetter;

    private void Start()
    {
        ai = GetComponent<IAstarAI>();
        destinationSetter = GetComponent<AIDestinationSetter>();

        animator = GetComponent<Animator>();
        RandomTarget();
    }

    public void RandomTarget()
    {
        targetWayPoint = UnityEngine.Random.Range(0, wayPoints.Length);
        dealyToTargetTimer = UnityEngine.Random.Range(0.75f, 3.5f);
        destinationSetter.target = wayPoints[targetWayPoint];
        ai.isStopped = true;
    }

    void Update()
    {
        bool walk = false;
        if (dealyToTargetTimer > 0f)
        {
            animator.SetBool(TextManager.WalkId, false);
            dealyToTargetTimer -= Time.deltaTime;
            return;
        }
        ai.isStopped = false;

        if (!ai.isStopped)
        {
            walk = true;

            if (!ai.pathPending)
            {
                if (ai.remainingDistance <= (ai as FollowerEntity).stopDistance)
                {
                    RandomTarget();
                }
            }
        }

        animator.SetBool(TextManager.WalkId, walk);
    }

    public void Cat_Direction()
    {
        transform.position = wayPoints[0].position;
        targetWayPoint = 1;
        dealyToTargetTimer = 0f;
        destinationSetter.target = wayPoints[targetWayPoint];
        ai.isStopped = true;
    }
}
