using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MoreMountains.Feedbacks;
using Pathfinding;
using System;

public class ZombieSanta : ZombieBase
{

    public bool contactingPlayer;
    public bool contact;
    private float attackTimer;
    private float contactTimer;

    public float range = 3.5f;

    public bool isRun;

    protected IAstarAI ai;
    protected AIDestinationSetter destinationSetter;
    public Seeker seeker;

    public GameObject pizzaBox;
    public bool stealSomething;

    public static EventHandler<ZombieSanta> StealEvent;

    public override void Init(Transform target)
    {
        rigid = GetComponent<Rigidbody>();

        ai = GetComponent<IAstarAI>();
        destinationSetter = GetComponent<AIDestinationSetter>();

        destinationSetter.target = target;
    }

    void Update()
    {
        contactingPlayer = false;
        bool walk = false;
        bool attack = false;

        float dist = Vector3.Distance(ZombiePooler.Instance.target.transform.position, transform.position);
        if (dist >= 100f)
        {
            gameObject.SetActive(false);
            return;
        }

        if (!dead)
        {
            //if (!contact)
            //{
            //    float dist = Vector3.Distance(ZombiePooler.Instance.target.transform.position, transform.position);
            //    //if (dist >= 100f) // 그리드 범위는 200f (절반이 100)
            //    //{
            //    //    ai.isStopped = true;
            //    //    ai.canMove = false;
            //    //    destinationSetter.enabled = false;
            //    //}
            //    //else
            //    //{
            //    //    ai.isStopped = false;
            //    //    ai.canMove = true;
            //    //    destinationSetter.enabled = true;
            //    //}

            //    if (dist >= 100f)
            //    {
            //        this.gameObject.SetActive(false);
            //        return;
            //    }
            //}
            // 나중에는 좀비들을 리스트 안에 넣고, 더 멀리 떨어진 좀비들은 아예 비활성화 시키자

            walk = true; // 항상 true

            if (ZombiePooler.Instance.target != null)
            {
                if (!ai.isStopped)
                {
                    walk = true;

                    if (!ai.pathPending)
                    {
                        //if (ai.remainingDistance <= (ai as FollowerEntity).stopDistance)
                        //{
                        //    if (!ai.hasPath || ai.velocity.magnitude == 0f)
                        //    {
                        //        contactingPlayer = true;
                        //    }
                        //}

                        if (ai.remainingDistance <= (ai as FollowerEntity).stopDistance 
                            && dist <= range) // 적당한 거리 (거리에 민감)
                        {
                            contactingPlayer = true;
                            //transform.LookAt(ZombiePooler.Instance.target);
                            if (attackTimer < 0.5f) attackTimer = 0.5f; // 근접 공격의 경우, 붙는 것보다 공격속도가 2배 빠름 (어차피 유효타가 적으므로)
                        }
                    }

                    if (contactingPlayer)
                    {
                        attack = true;
                    }
                }
            }
        }

        animator.SetBool("Walk", walk);
        animator.SetBool("Attack", attack);

        if (isRun)
            animator.SetBool("Contact", contact);

        if (attack)
        {
            Attack();
        }

        // 플레이어에게 Attacked 속성을 붙여서, Attacked * damage 만큼 1초당 받게 하기
    }

    private void Attack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer > 0.25f)
        {
            attackTimer = 0f;

            if (!stealSomething)
            {
                if (StealEvent != null)
                    StealEvent(null, this);
            }
        }
    }
    public void StealPizza()
    {
        stealSomething = true;
        pizzaBox.SetActive(true);

        destinationSetter.target = ZombiePooler.Instance.fleeTarget;
    }

    public override void Hit(Vector3 hitPos, float speed, Vector3 knockbackDir)
    {
        if (stealSomething)
        {
            stealSomething = false;
            pizzaBox.SetActive(false);
            OrderManager.Instance.ReturnStolenPizza();
        }

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

    public override bool CloseContact(Vector3 hitPos) // 산타는 붙지 않음
    {
        return false;
    }

    public override void DriftOffContact(float localXvel, float speed) // localXvel < 0 오른쪽 , > 0 왼쪽 (로컬 기준)
    {
        if (contactTimer <= 0.25f) // 붙자마자 드리프트로 떼어졌을 때 -> 거의 충돌한 것과 마찬가지
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

        destinationSetter.target = ZombiePooler.Instance.target;
        stealSomething = false;
        pizzaBox.SetActive(false);

        ai.isStopped = false;
        ai.canMove = false;

        contact = false;

        attackTimer = 0f;
        contactTimer = 0f;
    }

    public override void DeadHandle()
    {
        base.DeadHandle();

        if (!GM.Instance.EndTime && OrderManager.Instance.IsDelivering())
            GM.Instance.AddGold(1, GM.GetGoldSource.zombie);
    }
}
