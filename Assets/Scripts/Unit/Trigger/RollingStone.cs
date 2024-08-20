using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingStone : MonoBehaviour
{

    public SphereCollider coll;
    public Rigidbody rigid;

    public float power = 1000f;


    public void PushToPlayer()
    {
        Vector3 thisPos = transform.position;
        Vector3 playerPos = GM.Instance.player.transform.position;
        playerPos.y = thisPos.y;

        Vector3 dir = (playerPos - thisPos).normalized;

        float scale = transform.localScale.y;

        rigid.velocity = Vector3.zero;
        rigid.AddExplosionForce(power, transform.position + coll.center * scale - dir * (coll.radius * scale + 0.1f), 0.2f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Zombie"))
        {
            if (rigid.velocity.magnitude > 0.5f)
            {
                ContactPoint cp = collision.GetContact(0);
                ZombieBase zombie = collision.gameObject.GetComponent<ZombieBase>();
                Vector3 targetDirection = (collision.transform.position - transform.position).normalized;
                zombie.Hit(cp.point, 0.5f, targetDirection);
            }
        }
    }
}
