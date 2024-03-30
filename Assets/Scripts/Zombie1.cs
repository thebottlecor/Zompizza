using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MoreMountains.Feedbacks;
using Pathfinding;
using System;

public class Zombie1 : MonoBehaviour
{

    protected Rigidbody rigid;

    protected MMFeedbacks hitFeedback;
    [SerializeField] protected Collider coll;
    [SerializeField] protected Animator animator;

    public bool contactingPlayer;
    public bool tooClose;

    public bool dead;

    public FloodPathTest fPathTest;
    protected IAstarAI ai;
    public Seeker seeker;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        hitFeedback = GetComponent<MMFeedbacks>();

        ai = GetComponent<IAstarAI>();
        ai.canSearch = false;
    }

    void Update()
    {
        contactingPlayer = false;
        bool walk = false;
        bool attack = false;

        if (!dead)
        {
            float dist = Vector3.Distance(ZombiePooler.Instance.target.transform.position, transform.position);
            if (dist > 75f) // �׸��� ������ 100f
            {
                ai.isStopped = true;
            }
            else
            {
                ai.isStopped = false;
            }
            // ���߿��� ������� ����Ʈ �ȿ� �ְ�, �� �ָ� ������ ������� �ƿ� ��Ȱ��ȭ ��Ű��

            walk = true; // �׻� true

            if (ZombiePooler.Instance.target != null)
            {
                if (!ai.isStopped)
                {
                    walk = true;

                    ai.SetPath(FloodPathTracer.Construct(transform.position, fPathTest.fPath));

                    //FloodPathTracer floodPathTracer = FloodPathTracer.Construct(transform.position, fPathTest.fPath, null);
                    //seeker.StartPath(floodPathTracer, OnPathComplete);

                    //contactingPlayer = true;

                    if (contactingPlayer)
                    {
                        attack = true;
                    }
                }
            }
        }

        animator.SetBool("Walk", walk);
        animator.SetBool("Attack", attack);

        // �÷��̾�� Attacked �Ӽ��� �ٿ���, Attacked * damage ��ŭ 1�ʴ� �ް� �ϱ�
    }

    private void OnPathComplete(Path p)
    {
        Debug.Log("PathComplete");
    }

    public void Hit(Vector3 hitPos, float speed, Vector3 knockbackDir)
    {
        //hitFeedback?.PlayFeedbacks();
        ai.isStopped = true;
        ai.canMove = false;

        rigid.constraints = RigidbodyConstraints.None;

        Vector3 expPos = hitPos;
        expPos.y = -3f; // ��� �ؿ��� ���� (��� ������ ���� �ʵ���)

        rigid.velocity = Vector3.zero;
        rigid.AddForce(knockbackDir * ZombiePooler.Instance.knockbackPower * speed, ForceMode.Impulse);
        rigid.AddExplosionForce(ZombiePooler.Instance.power * speed, expPos, ZombiePooler.Instance.radius, ZombiePooler.Instance.height);

        DeadHandle();
    }

    public bool CloseContact(Vector3 hitPos)
    {
        if (dead) return false;

        ai.isStopped = true;
        ai.canMove = false;

        rigid.velocity = Vector3.zero;
        rigid.isKinematic = true;
        coll.enabled = false;

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

    public void DriftOffContact(float localXvel, float speed) // localXvel < 0 ������ , > 0 ���� (���� ����)
    {
        Transform tempTarger = ZombiePooler.Instance.target;

        this.transform.SetParent(ZombiePooler.Instance.zombieSpawnParent);

        ai.isStopped = true;
        ai.canMove = false;

        rigid.constraints = RigidbodyConstraints.None;
        rigid.isKinematic = false;
        coll.enabled = true;

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


    public void DeadHandle()
    {
        dead = true;
    }
}
