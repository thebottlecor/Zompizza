using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombiePooler : Singleton<ZombiePooler>
{

    public Transform zombieSpawnParent;

    public GameObject zombieSource;
    public GameObject zombieSourceHeavy;
    public GameObject zombieSourceFast;
    public GameObject zombieSourceRange;
    public GameObject ZombieSubSourceRange;

    public int maxZombie = 500;
    public List<Zombie2> zombiesPool;

    public int maxZombieHeavy = 10;
    public List<Zombie2> zombiesPoolHeavy;

    public int maxZombieFast = 10;
    public List<Zombie2> zombiesPoolFast;

    public int maxZombieRange = 5;
    public List<Zombie2> zombiesPoolRange;
    public List<GameObject> zombiesSubPoolRange;

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
    private float timer2;
    private float timer3;
    private float timer4;
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
        Pool_Init(ref zombiesPool, maxZombie, zombieSource);
        Pool_Init(ref zombiesPoolHeavy, maxZombieHeavy, zombieSourceHeavy);
        Pool_Init(ref zombiesPoolFast, maxZombieFast, zombieSourceFast);
        Pool_Init(ref zombiesPoolRange, maxZombieRange, zombieSourceRange);
        Pool_Init(ref zombiesSubPoolRange, maxZombieRange, ZombieSubSourceRange);
    }

    private void Pool_Init(ref List<Zombie2> list, int max, GameObject source)
    {
        list = new List<Zombie2>(max);
        for (int i = 0; i < max; i++)
        {
            var newZombie = Instantiate(source, zombieSpawnParent).GetComponent<Zombie2>();
            newZombie.Init(target);
            newZombie.gameObject.SetActive(false);
            list.Add(newZombie);
        }
    }
    private void Pool_Init(ref List<GameObject> list, int max, GameObject source)
    {
        list = new List<GameObject>(max);
        for (int i = 0; i < max; i++)
        {
            var newZombie = Instantiate(source, zombieSpawnParent);
            newZombie.gameObject.SetActive(false);
            list.Add(newZombie);
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

        if (GM.Instance.day > 0)
        {
            timer2 += 1f * Time.deltaTime;
            if (timer2 >= 10f)
            {
                timer2 = 0f;
                SpawnFast(1);
            }
        }
        if (GM.Instance.day > 1)
        {
            timer4 += 1f * Time.deltaTime;
            if (timer4 >= 10f)
            {
                timer4 = 0f;
                SpawnRange(1);
            }
        }
        if (GM.Instance.day > 2)
        {
            timer3 += 1f * Time.deltaTime;
            if (timer3 >= 10f)
            {
                timer3 = 0f;
                SpawnHeavy(1);
            }
        }
    }

    public int GetActiveZomibes()
    {
        int count = 0;
        for (int i = 0; i < maxZombie; i++)
        {
            if (zombiesPool[i].gameObject.activeSelf && !zombiesPool[i].dead)
                count++;
        }
        return count;
    }

    public void ZombieReset()
    {
        ResetSub(zombiesPool);
        ResetSub(zombiesPoolHeavy);
        ResetSub(zombiesPoolFast);
        ResetSub(zombiesPoolRange);
        ResetSub(zombiesSubPoolRange);
    }
    private void ResetSub<T>(List<T> list) where T : MonoBehaviour
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].gameObject.activeSelf)
                list[i].gameObject.SetActive(false);
        }
    }
    private void ResetSub(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].activeSelf)
                list[i].SetActive(false);
        }
    }

    public void Spawn(int count)
    {
        if (count <= 0)
            return;

        for (int i = 0; i < maxZombie; i++)
        {
            var zom = zombiesPool[i];
            if (!zom.gameObject.activeSelf)
            {
                Vector3 node = GetRandomPos(30f);

                float dist = (target.position - node).magnitude;

                if (dist < spawnDist * 0.75f)
                {
                    count--;
                    if (count <= 0)
                        break;

                    continue;
                }

                zom.transform.position = node;

                int rand = UnityEngine.Random.Range(0, models_Info.Length);
                zom.meshRenderer.material = models_Info[rand].material;
                zom.meshRenderer.sharedMesh = models_Info[rand].mesh;
                zom.StateReset();

                zom.gameObject.SetActive(true);

                count--;
                if (count <= 0)
                    break;
            }
        }
    }


    private void SpawnFast(int count)
    {
        for (int i = 0; i < maxZombieFast; i++)
        {
            var zom = zombiesPoolFast[i];
            if (!zom.gameObject.activeSelf)
            {
                Vector3 node = GetRandomPos(10f);

                float dist = (target.position - node).magnitude;

                if (dist < spawnDist * 0.75f)
                {
                    count--;
                    if (count <= 0)
                        break;

                    continue;
                }

                zom.transform.position = node;
                zom.StateReset();

                zom.gameObject.SetActive(true);

                count--;
                if (count <= 0)
                    break;
            }
        }
    }

    private void SpawnHeavy(int count)
    {
        for (int i = 0; i < maxZombieHeavy; i++)
        {
            var zom = zombiesPoolHeavy[i];
            if (!zom.gameObject.activeSelf)
            {
                Vector3 node = GetRandomPos(5f);

                float dist = (target.position - node).magnitude;

                if (dist < spawnDist * 0.75f)
                {
                    count--;
                    if (count <= 0)
                        break;

                    continue;
                }

                zom.transform.position = node;
                zom.StateReset();

                zom.gameObject.SetActive(true);

                count--;
                if (count <= 0)
                    break;
            }
        }
    }

    private void SpawnRange(int count)
    {
        for (int i = 0; i < maxZombieRange; i++)
        {
            var zom = zombiesPoolRange[i];
            if (!zom.gameObject.activeSelf)
            {
                Vector3 node = GetRandomPos(25f);

                float dist = (target.position - node).magnitude;

                if (dist < spawnDist * 0.75f)
                {
                    count--;
                    if (count <= 0)
                        break;

                    continue;
                }

                zom.transform.position = node;
                zom.StateReset();

                zom.gameObject.SetActive(true);

                count--;
                if (count <= 0)
                    break;
            }
        }
    }
    public void SpawnRangeSub(Vector3 pos)
    {
        pos.y = 0.01f;
        // 크기에 따라 수동 조정
        pos.x += 4f;
        pos.z += 4f;
        for (int i = 0; i < maxZombieRange; i++)
        {
            var zom = zombiesSubPoolRange[i];
            if (!zom.activeSelf)
            {
                zom.transform.position = pos;
                StartCoroutine(SubObjectHide(zom));
                break;
            }
        }
    }
    private IEnumerator SubObjectHide(GameObject obj)
    {
        obj.SetActive(true);
        yield return CoroutineHelper.WaitForSeconds(5f);
        obj.SetActive(false);
    }

    private Vector3 GetRandomPos(float max)
    {
        float random = Random.Range(-1f, 1f) * max;
        var v3 = Quaternion.AngleAxis(random, Vector3.up) * target.forward;
        Vector3 newPos = target.transform.position + v3 * spawnDist;
        newPos.y = 0f;

        var node = AstarPath.active.GetNearest(newPos, Pathfinding.NNConstraint.Walkable).position;
        return node;
    }
}
