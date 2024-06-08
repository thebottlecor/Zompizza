using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MoreMountains.Feedbacks;
using Pathfinding;
using System;

public class Zombie4 : ZombieBase
{
    public bool contact;
    private float contactTimer;
    private float attackTimer;

    public float range = 3.5f;
    public bool isInstall;

    public bool isRun;

    protected IAstarAI ai;
    protected AIDestinationSetter destinationSetter;
    public Seeker seeker;

    public static EventHandler<float> DamageEvent;

    public override void Init(Transform target)
    {
        rigid = GetComponent<Rigidbody>();

        ai = GetComponent<IAstarAI>();
        destinationSetter = GetComponent<AIDestinationSetter>();

        destinationSetter.target = target;
    }

    void Update()
    {
        if (!dead && !contact)
        {
            float dist = Vector3.Distance(ZombiePooler.Instance.target.transform.position, transform.position);
            if (ZombiePooler.Instance.target != null)
            {
                if (!ai.isStopped)
                {
                    if (!ai.pathPending)
                    {
                        if (ai.remainingDistance <= (ai as FollowerEntity).stopDistance && dist <= range) 
                        {
                            CloseContact(destinationSetter.target.position);
                            GM.Instance.player.contactingZombies.Add(this);
                        }
                    }
                }
            }
        }
        animator.SetBool("Walk", true);

        if (contact)
        {
            contactTimer += Time.deltaTime;
            Attack();
        }
    }

    private void Attack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer > 0.5f)
        {
            attackTimer = 0f;
            AudioManager.Instance.PlaySFX(Sfx.hittngPlayer);
        }
    }


    public override void Hit(Vector3 hitPos, float speed, Vector3 knockbackDir)
    {
        //hitFeedback?.PlayFeedbacks();
        ai.isStopped = true;
        ai.canMove = false;

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

        if (isHeavy)
            rigid.AddExplosionForce(0.05f * ZombiePooler.Instance.power * speed, expPos, ZombiePooler.Instance.radius, ZombiePooler.Instance.height * 0.05f);
        else
            rigid.AddExplosionForce(ZombiePooler.Instance.power * speed, expPos, ZombiePooler.Instance.radius, ZombiePooler.Instance.height);


        DeadHandle();
    }

    public override bool CloseContact(Vector3 hitPos)
    {
        if (isHeavy) return false;
        if (dead) return false;

        ai.isStopped = true;
        ai.canMove = false;

        rigid.velocity = Vector3.zero;
        rigid.isKinematic = true;
        coll.enabled = false;

        contact = true;
        shadow.SetActive(false);

        this.transform.SetParent(ZombiePooler.Instance.target);

        Vector3 origin = ZombiePooler.Instance.target.position;
        origin.y += 2f;
        if (Physics.Raycast(origin, hitPos - origin, out RaycastHit result, 2f, LayerMask.GetMask("Car Contact Coll")))
        {
            transform.position = result.point;
            transform.LookAt(ZombiePooler.Instance.target);
        }

        return true;
    }

    public override void DriftOffContact(float localXvel, float speed) // localXvel < 0 오른쪽 , > 0 왼쪽 (로컬 기준)
    {
        if (gameObject.activeSelf && contactTimer <= 0.25f) // 붙자마자 드리프트로 떼어졌을 때 -> 거의 충돌한 것과 마찬가지
        {
            AudioManager.Instance.PlaySFX(Sfx.zombieCrash);
        }

        Transform tempTarger = ZombiePooler.Instance.target;

        this.transform.SetParent(ZombiePooler.Instance.zombieSpawnParent);

        ai.isStopped = true;
        ai.canMove = false;

        rigid.constraints = RigidbodyConstraints.None;
        rigid.isKinematic = false;
        coll.enabled = true;

        contact = false;

        Vector3 expPos = transform.position;

        Vector3 right = new Vector3(tempTarger.forward.z, tempTarger.forward.y, -1f * tempTarger.forward.x);
        if (localXvel < 0)
            expPos += 0.25f * ZombiePooler.Instance.radius * right;
        else if (localXvel > 0)
            expPos += -0.25f * ZombiePooler.Instance.radius * right;

        expPos.y = -3f; // 평면 밑에서 폭발 (평면 밑으로 들어가지 않도록)

        rigid.velocity = Vector3.zero;
        rigid.AddExplosionForce(ZombiePooler.Instance.power * Mathf.Max(Mathf.Min(0.33f, speed * 0.33f), 0.1f), expPos, ZombiePooler.Instance.radius, ZombiePooler.Instance.height);

        DeadHandle();
    }

    public override void StateReset()
    {
        base.StateReset();

        ai.isStopped = false;
        ai.canMove = false;

        contact = false;
        contactTimer = 0f;
        attackTimer = 0f;
    }
}
