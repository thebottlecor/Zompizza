using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{

    protected NavMeshAgent navMeshAgent;
    protected Rigidbody rigid;

    [SerializeField] protected Collider coll;
    [SerializeField] protected Animator animator;

    public bool contactingPlayer;
    public bool tooClose;

    public bool dead;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        contactingPlayer = false;
        bool walk = false;
        bool attack = false;

        if (!dead)
        {
            walk = true; // �׻� true

            if (ZombiePooler.Instance.currentTarget != null)
            {
                if (navMeshAgent.enabled)
                {
                    walk = true;
                    navMeshAgent.SetDestination(ZombiePooler.Instance.currentTarget.position);

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

        animator.SetBool(TextManager.WalkId, walk);
        animator.SetBool(TextManager.AttackId, attack);

        // �÷��̾�� Attacked �Ӽ��� �ٿ���, Attacked * damage ��ŭ 1�ʴ� �ް� �ϱ�
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

    public void Hit(Vector3 hitPos, float speed, Vector3 knockbackDir)
    {
        //hitFeedback?.PlayFeedbacks();
        navMeshAgent.enabled = false;
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

        navMeshAgent.enabled = false;
        rigid.velocity = Vector3.zero;
        rigid.isKinematic = true;
        coll.enabled = false;

        this.transform.SetParent(ZombiePooler.Instance.currentTarget);

        Vector3 origin = ZombiePooler.Instance.currentTarget.position;
        origin.y += 2f;
        if (Physics.Raycast(origin, hitPos - origin, out RaycastHit result, 2f, LayerMask.GetMask("Car Contact Coll")))
        {
            transform.position = result.point;
            transform.LookAt(ZombiePooler.Instance.currentTarget);
        }

        return true;
    }

    public void DriftOffContact(float localXvel, float speed) // localXvel < 0 ������ , > 0 ���� (���� ����)
    {
        Transform tempTarger = ZombiePooler.Instance.currentTarget;

        this.transform.SetParent(ZombiePooler.Instance.zombieSpawnParent);

        navMeshAgent.enabled = false;
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
