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
            if (dist > 75f) // 그리드 범위는 100f
            {
                ai.isStopped = true;
            }
            else
            {
                ai.isStopped = false;
            }
            // 나중에는 좀비들을 리스트 안에 넣고, 더 멀리 떨어진 좀비들은 아예 비활성화 시키자

            walk = true; // 항상 true

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

        // 플레이어에게 Attacked 속성을 붙여서, Attacked * damage 만큼 1초당 받게 하기
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
        expPos.y = -3f; // 평면 밑에서 폭발 (평면 밑으로 들어가지 않도록)

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

    public void DriftOffContact(float localXvel, float speed) // localXvel < 0 오른쪽 , > 0 왼쪽 (로컬 기준)
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

        expPos.y = -3f; // 평면 밑에서 폭발 (평면 밑으로 들어가지 않도록)

        rigid.velocity = Vector3.zero;
        rigid.AddExplosionForce(ZombiePooler.Instance.power * Mathf.Max(Mathf.Min(0.33f, speed * 0.33f), 0.1f), expPos, ZombiePooler.Instance.radius, ZombiePooler.Instance.height);

        DeadHandle();
    }


    public void DeadHandle()
    {
        dead = true;
    }
}
