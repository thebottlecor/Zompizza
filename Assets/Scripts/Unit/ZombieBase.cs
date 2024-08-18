using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ZombieBase : MonoBehaviour
{
    protected Rigidbody rigid;

    [SerializeField] protected Collider coll;
    [SerializeField] protected Animator animator;

    public bool dead;
    public bool isHeavy;
    public bool contact;

    public SkinnedMeshRenderer meshRenderer;
    public GameObject shadow;

    public abstract void Hit(Vector3 hitPos, float speed, Vector3 knockbackDir);

    public abstract bool CloseContact(Vector3 hitPos);

    public abstract void DriftOffContact(float localXvel, float speed); // localXvel < 0 오른쪽 , > 0 왼쪽 (로컬 기준)

    public abstract void Init(Transform target);


    [ContextMenu("부활")]
    public virtual void StateReset()
    {
        transform.eulerAngles = Vector3.zero;

        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        rigid.isKinematic = false;
        coll.enabled = true;

        dead = false;
        shadow.SetActive(true);

        coll.gameObject.layer = LayerMask.NameToLayer("Zombie");
    }

    public virtual void DeadHandle()
    {
        dead = true;
        shadow.SetActive(false);
    }
}
