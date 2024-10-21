using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringJump : BaseSpawner
{

    public float jumpForce = 10f;
    public float yForce = 10f;

    private void OnTriggerEnter(Collider other)
    {
        //if (triggered) return;

        if (GM.Instance.loading) return;

        if (other.gameObject.CompareTag("Player"))
        {
            //triggered = true;

            //var arrivePos = ZombiePooler.Instance.GetRandomPos(spawnPos.bounds);

            var pusher = GM.Instance.player.carRigidbody;

            //Vector3 dir = (arrivePos - pusher.transform.position);
            //dir.y = 0f;

            Vector3 dir = pusher.transform.forward * 50f;

            StartCoroutine(Jump(pusher, dir));
        }
    }

    private IEnumerator Jump(Rigidbody pusher, Vector3 dir)
    {
        //pusher.velocity = Vector3.zero;
        //pusher.angularVelocity = Vector3.zero;

        var player = GM.Instance.player;
        player.StopPlayer(false, true);

        yield return null;

        player.ShakeOffAllZombies();

        pusher.AddForce(dir * jumpForce + Vector3.up * yForce, ForceMode.Impulse);

        AudioManager.Instance.PlaySFX(Sfx.springJump);
    }
}
