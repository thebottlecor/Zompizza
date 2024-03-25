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

    public Transform target;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        hitFeedback = GetComponent<MMFeedbacks>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            if (navMeshAgent.enabled)
            {
                navMeshAgent.SetDestination(target.position);
            }
        }
    }

    public void Hit(Vector3 hitPos)
    {
        //hitFeedback?.PlayFeedbacks();
        navMeshAgent.enabled = false;
        rigid.constraints = RigidbodyConstraints.None;

        Vector3 expPos = hitPos;
        expPos.y = -3f; // 평면 밑에서 폭발 (평면 밑으로 들어가지 않도록)

        rigid.velocity = Vector3.zero;
        rigid.AddExplosionForce(power, expPos, radius, height);

        Debug.Log("Hit");
    }

    public float power;
    public float radius;
    public float height;
    public Transform pos;

    [ContextMenu("테스트")]
    private void Test()
    {
        navMeshAgent.enabled = false;
        rigid.constraints = RigidbodyConstraints.None;
        rigid.AddExplosionForce(power, pos.position, radius, height);
    }
}
