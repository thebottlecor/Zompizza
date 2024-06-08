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
            //    //if (dist >= 100f) // �׸��� ������ 200f (������ 100)
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
            // ���߿��� ������� ����Ʈ �ȿ� �ְ�, �� �ָ� ������ ������� �ƿ� ��Ȱ��ȭ ��Ű��

            walk = true; // �׻� true

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
                            && dist <= range) // ������ �Ÿ� (�Ÿ��� �ΰ�)
                        {
                            contactingPlayer = true;
                            //transform.LookAt(ZombiePooler.Instance.target);
                            if (attackTimer < 0.5f) attackTimer = 0.5f; // ���� ������ ���, �ٴ� �ͺ��� ���ݼӵ��� 2�� ���� (������ ��ȿŸ�� �����Ƿ�)
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

        // �÷��̾�� Attacked �Ӽ��� �ٿ���, Attacked * damage ��ŭ 1�ʴ� �ް� �ϱ�
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
        expPos.y = -3f; // ��� �ؿ��� ���� (��� ������ ���� �ʵ���)

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

    public override bool CloseContact(Vector3 hitPos) // ��Ÿ�� ���� ����
    {
        return false;
    }

    public override void DriftOffContact(float localXvel, float speed) // localXvel < 0 ������ , > 0 ���� (���� ����)
    {
        if (contactTimer <= 0.25f) // ���ڸ��� �帮��Ʈ�� �������� �� -> ���� �浹�� �Ͱ� ��������
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

        expPos.y = -3f; // ��� �ؿ��� ���� (��� ������ ���� �ʵ���)

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
