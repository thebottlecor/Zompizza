using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MoreMountains.Feedbacks;

public class Zombie : MonoBehaviour
{

    protected NavMeshAgent navMeshAgent;
    protected Rigidbody rigid;

    protected MMFeedbacks hitFeedback;
    [SerializeField] protected Collider coll;
    [SerializeField] protected Animator animator;

    public Transform target;


    public bool contactingPlayer;
    public bool tooClose;

    public bool dead;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        hitFeedback = GetComponent<MMFeedbacks>();
    }

    // Update is called once per frame
    void Update()
    {
        contactingPlayer = false;
        bool walk = false;
        bool attack = false;

        if (!dead)
        {
            walk = true; // 항상 true

            if (target != null)
            {
                if (navMeshAgent.enabled)
                {
                    walk = true;
                    navMeshAgent.SetDestination(target.position);

                    if (!navMeshAgent.pathPending)
                    {
                        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                        {
                            if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
                            {
                                contactingPlayer = true;
                            }
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

        // 플레이어에게 Attacked 속성을 붙여서, Attacked * damage 만큼 1초당 받게 하기
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        tooClose = true;

    //        CloseContact();
    //    }
    //}

    //private void OnCollisionExit(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        tooClose = false;
    //    }
    //}

    public float power;
    public float radius;
    public float height;
    public Transform pos;

    public void Hit(Vector3 hitPos, float speed)
    {
        //hitFeedback?.PlayFeedbacks();
        navMeshAgent.enabled = false;
        rigid.constraints = RigidbodyConstraints.None;

        Vector3 expPos = hitPos;
        expPos.y = -3f; // 평면 밑에서 폭발 (평면 밑으로 들어가지 않도록)

        rigid.velocity = Vector3.zero;
        rigid.AddExplosionForce(power * speed, expPos, radius, height);

        Debug.Log("Hit");

        dead = true;
    }

    public bool CloseContact(Vector3 hitPos)
    {
        if (dead) return false;

        navMeshAgent.enabled = false;
        rigid.velocity = Vector3.zero;
        rigid.isKinematic = true;
        coll.enabled = false;

        this.transform.SetParent(target);

        Vector3 origin = target.position;
        origin.y += 2f;
        if (Physics.Raycast(origin, hitPos - origin, out RaycastHit result, 2f, LayerMask.GetMask("CarContact")))
        {
            transform.position = result.point;
            transform.LookAt(target);
        }

        return true;
    }

    public void DriftOffContact(float localXvel, float speed) // localXvel < 0 오른쪽 , > 0 왼쪽 (로컬 기준)
    {
        Transform tempTarger = target;

        transform.parent = null;

        navMeshAgent.enabled = false;
        rigid.constraints = RigidbodyConstraints.None;
        rigid.isKinematic = false;
        coll.enabled = true;

        Vector3 expPos = transform.position;

        Vector3 right = new Vector3(tempTarger.forward.z, tempTarger.forward.y, -1f * tempTarger.forward.x);
        if (localXvel < 0)
            expPos += 0.25f * radius * right;
        else if (localXvel > 0)
            expPos += -0.25f * radius * right;

        expPos.y = -3f; // 평면 밑에서 폭발 (평면 밑으로 들어가지 않도록)

        rigid.velocity = Vector3.zero;
        rigid.AddExplosionForce(power * Mathf.Max(Mathf.Min(0.33f, speed * 0.33f), 0.1f), expPos, radius, height);

        Debug.Log("Off");

        dead = true;
    }
}
