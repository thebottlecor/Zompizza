using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MoreMountains.Feedbacks;
using Pathfinding;
using System;

public class Zombie2 : MonoBehaviour
{

    protected Rigidbody rigid;

    protected MMFeedbacks hitFeedback;
    [SerializeField] protected Collider coll;
    [SerializeField] protected Animator animator;

    public bool contactingPlayer;
    public bool contact;
    private float attackTimer;
    private float contactTimer;

    public bool dead;

    public SkinnedMeshRenderer meshRenderer;
    public GameObject shadow;

    protected IAstarAI ai;
    protected AIDestinationSetter destinationSetter;
    public Seeker seeker;

    public static EventHandler<float> DamageEvent;

    public void Init(Transform target)
    {
        rigid = GetComponent<Rigidbody>();
        hitFeedback = GetComponent<MMFeedbacks>();

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
            this.gameObject.SetActive(false);
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
                            && dist <= 3.5f) // 적당한 거리 (거리에 민감)
                        {
                            contactingPlayer = true;
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

        if (attack)
        {
            Attack();
        }
        else if (contact)
        {
            contactTimer += Time.deltaTime;
            Attack();
        }

        // 플레이어에게 Attacked 속성을 붙여서, Attacked * damage 만큼 1초당 받게 하기
    }

    private void Attack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer > 1f)
        {
            attackTimer = 0f;
            AudioManager.Instance.PlaySFX(Sfx.hittngPlayer);

            if (DamageEvent != null)
                DamageEvent(null, 0.005f * UnityEngine.Random.Range(0.75f, 1.25f));
        }
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
        if (!dead)
        {
            //knockbackDir.y += 0.1f;
            rigid.AddForce(speed * ZombiePooler.Instance.knockbackPower * knockbackDir, ForceMode.Impulse);
        }
        coll.gameObject.layer = LayerMask.NameToLayer("Flying Zombie");
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

    public void DriftOffContact(float localXvel, float speed) // localXvel < 0 오른쪽 , > 0 왼쪽 (로컬 기준)
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

    [ContextMenu("부활")]
    public void StateReset()
    {
        transform.eulerAngles = Vector3.zero;

        ai.isStopped = false;
        ai.canMove = false;

        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        rigid.isKinematic = false;
        coll.enabled = true;

        contact = false;
        dead = false;
        shadow.SetActive(true);

        attackTimer = 0f;
        contactTimer = 0f;

        coll.gameObject.layer = LayerMask.NameToLayer("Zombie");
    }

    public void DeadHandle()
    {
        dead = true;
        shadow.SetActive(false);

        if (OrderManager.Instance.IsDelivering())
            GM.Instance.AddGold(1, GM.GetGoldSource.zombie);
    }
}
