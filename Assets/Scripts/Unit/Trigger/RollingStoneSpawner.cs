using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingStoneSpawner : BaseSpawner
{

    public int spawnNum = 1;


    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (GM.Instance.loading) return;

        if (other.gameObject.CompareTag("Player"))
        {
            triggered = true;

            ZombiePooler.Instance.SpawnRollingStone(spawnNum, spawnPos);
            AudioManager.Instance.PlaySFX(Sfx.rollingStone);
        }
    }
}
