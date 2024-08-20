using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeSpawner : BaseSpawner
{
    public bool alarmSound;

    public int spawnNum = 50;

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
