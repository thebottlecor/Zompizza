using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombiePooler : Singleton<ZombiePooler>
{

    public GameObject zombieSource;
    public Transform zombieSpawnParent;

    public int maxZombie = 500;
    public List<Zombie2> zombiesPool;

    [Header("전역 설정")]
    public Transform target;
    public float knockbackPower;
    public float power;
    public float radius;
    public float height;

    [Header("스폰 설정")]
    public Vector3 spawnOffset;
    public float spawnDist;
    private float timer;

    private void Start()
    {
        zombiesPool = new List<Zombie2>(maxZombie);
        for (int i = 0; i < maxZombie; i++)
        {
            var newZombie = Instantiate(zombieSource, zombieSpawnParent).GetComponent<Zombie2>();
            newZombie.Init(target);
            newZombie.gameObject.SetActive(false);
            zombiesPool.Add(newZombie);
        }
    }

    private void Update()
    {
        timer += 1f * Time.deltaTime;

        if (timer >= 1f)
        {
            timer = 0f;

            float random = Random.Range(-1f, 1f) * 30f;
            var v3 = Quaternion.AngleAxis(random, Vector3.up) * target.forward;
            Spawn(target.transform.position + v3 * spawnDist);
        }
    }

    public void Spawn(Vector3 pos)
    {
        for (int i = 0; i < maxZombie; i++)
        {
            if (!zombiesPool[i].gameObject.activeSelf)
            {
                zombiesPool[i].StateReset();
                zombiesPool[i].transform.position = pos;
                zombiesPool[i].gameObject.SetActive(true);
                break;
            }
        }
    }

}
