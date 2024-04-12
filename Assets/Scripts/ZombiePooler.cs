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
    public int spawnCount = 1;
    private float timer;
    public AstarPath astarPath;

    [Header("좀비 모델")]
    public ZombieModel[] models_Info;
    [System.Serializable]
    public struct ZombieModel
    {
        public Material material;
        public Mesh mesh;
    }


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
            Spawn(spawnCount);
        }
    }

    public void Spawn(int count)
    {
        for (int i = 0; i < maxZombie; i++)
        {
            if (!zombiesPool[i].gameObject.activeSelf)
            {
                float random = Random.Range(-1f, 1f) * 30f;
                var v3 = Quaternion.AngleAxis(random, Vector3.up) * target.forward;
                Vector3 newPos = target.transform.position + v3 * spawnDist;
                newPos.y = 0f;

                var node = AstarPath.active.GetNearest(newPos, Pathfinding.NNConstraint.Walkable).position;

                float dist = (target.position - node).magnitude;

                if (dist < spawnDist * 0.75f)
                {
                    count--;
                    if (count <= 0)
                        break;

                    continue;
                }

                zombiesPool[i].transform.position = node;

                int rand = UnityEngine.Random.Range(0, models_Info.Length);
                zombiesPool[i].meshRenderer.material = models_Info[rand].material;
                zombiesPool[i].meshRenderer.sharedMesh = models_Info[rand].mesh;
                zombiesPool[i].StateReset();

                zombiesPool[i].gameObject.SetActive(true);

                count--;
                if (count <= 0)
                    break;
            }
        }
    }

}
