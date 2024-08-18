using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieCar : MonoBehaviour
{
    protected IAstarAI ai;
    protected AIDestinationSetter destinationSetter;

    public PlayerController player;
    public Transform playerForward;

    public float pathCalcTime = 5f;
    private float timer;

    private void Start()
    {
        ai = GetComponent<IAstarAI>();
        destinationSetter = GetComponent<AIDestinationSetter>();
    }

    private void Update()
    {
        Vector3 target = player.transform.position;

        target += player.carSpeed * player.carRigidbody.velocity.normalized;

        ai.destination = target;

        //ai.SearchPath();

        //float dist = (player.transform.position - transform.position).magnitude;

        //if (dist < 3f || player.carSpeed < 12f)
        //{
        //    destinationSetter.target = player.transform;
        //}
        //else
        //{
        //    destinationSetter.target = playerForward;
        //}

        //if (timer >= pathCalcTime)
        //{
        //    timer = 0;
        //    ai.SearchPath();
        //}
        //else
        //{
        //    timer += Time.deltaTime;
        //}
    }
}
