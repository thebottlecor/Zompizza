using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MoreMountains.Feedbacks;
using Pathfinding;
using System;

public class Zombie3 : ZombieBase
{

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }
    public override void Init(Transform target)
    {

    }

    public override void Hit(Vector3 hitPos, float speed, Vector3 knockbackDir)
    {
        rigid.constraints = RigidbodyConstraints.None;

        Vector3 expPos = hitPos;
        expPos.y = -3f; // 평면 밑에서 폭발 (평면 밑으로 들어가지 않도록)

        rigid.velocity = Vector3.zero;
        if (!dead)
        {
            //knockbackDir.y += 0.1f;
            rigid.AddForce(speed * ZombiePooler.Instance.knockbackPower * knockbackDir, ForceMode.Impulse);
        }
        coll.gameObject.layer = LayerMask.NameToLayer("Flying Zombie");

        rigid.AddExplosionForce(ZombiePooler.Instance.power * speed, expPos, ZombiePooler.Instance.radius, ZombiePooler.Instance.height);

        DeadHandle();
    }

    public override bool CloseContact(Vector3 hitPos)
    {
        return false;
    }

    public override void DriftOffContact(float localXvel, float speed)
    {
        
    }
}
