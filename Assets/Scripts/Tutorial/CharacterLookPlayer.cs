using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLookPlayer : MonoBehaviour
{
    public Transform target;
    public float seeDist = 20f;
    public float rotSpeed = 60f;
    private Quaternion firstQuat;

    void Start()
    {
        firstQuat = transform.rotation;
        target = GM.Instance.player.transform;
    }

    void Update()
    {
        Vector3 differ = (target.position - transform.position);
        differ.y = 0f;
        float dist = differ.magnitude;

        if (dist < seeDist)
        {
            var step = rotSpeed * Time.deltaTime;
            Quaternion _rot = Quaternion.LookRotation(differ);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _rot, step);
        }
        else
        {
            var step = rotSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, firstQuat, step);
        }
    }
}
