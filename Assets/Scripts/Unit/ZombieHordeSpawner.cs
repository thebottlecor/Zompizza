using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieHordeSpawner : MonoBehaviour
{

    public bool triggered;

    public bool alarmSound;

    public int spawnNum = 50;
    public BoxCollider spawnPos;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (GM.Instance.loading) return;

        if (other.gameObject.CompareTag("Player"))
        {
            triggered = true;

            ZombiePooler.Instance.SpawnHorde(spawnNum, spawnPos);
            AudioManager.Instance.PlaySFX(Sfx.zombieHorde);

            if (alarmSound)
            {
                AudioManager.Instance.PlaySFX(Sfx.carAlarm);
            }
        }
    }
}
