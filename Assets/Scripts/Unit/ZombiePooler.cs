using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombiePooler : Singleton<ZombiePooler>
{

    public Transform zombieSpawnParent;
    public ZombieHordeSpawner[] hordeSpawners;

    public GameObject zombieSource;
    public GameObject zombieSourceHorde;
    public GameObject zombieSourceHeavy;
    public GameObject zombieSourceFast;
    public GameObject zombieSourceRange;
    public GameObject ZombieSubSourceRange;
    public GameObject zombieSourceSanta;

    public int maxZombie = 500;
    public List<ZombieBase> zombiesPool;

    public int maxZombieHorde = 50;
    public List<ZombieBase> zombieHordePool;

    public int maxZombieHeavy = 10;
    public List<ZombieBase> zombiesPoolHeavy;

    public int maxZombieFast = 10;
    public List<ZombieBase> zombiesPoolFast;

    public int maxZombieRange = 5;
    public List<ZombieBase> zombiesPoolRange;
    public List<GameObject> zombiesSubPoolRange;

    public int maxZombieSanta = 1;
    public List<ZombieBase> zombiesPoolSanta;

    public int maxHitEffect = 10;
    public List<GameObject> hitEffectPool;

    [Header("전역 설정")]
    public Transform currentTarget;
    public Transform fleeTarget;
    public float knockbackPower;
    public float power;
    public float radius;
    public float height;

    [Header("스폰 설정")]
    public Vector3 spawnOffset;
    public float spawnDist;
    public int spawnCount = 1;
    public int spawnCountRandomAdd = 0;
    private float timer;
    private float timer2;
    private float timer3;
    private float timer4;
    private float timer5;
    public AstarPath astarPath;

    [Header("좀비 모델")]
    public ZombieModel[] models_Info;
    [System.Serializable]
    public struct ZombieModel
    {
        public Material material;
        public Mesh mesh;
    }

    private Pathfinding.NNConstraint constraint;

    private void Start()
    {
        Pool_Init(ref zombiesPool, maxZombie, zombieSource);
        Pool_Init(ref zombieHordePool, maxZombieHorde, zombieSourceHorde);
        Pool_Init(ref zombiesPoolHeavy, maxZombieHeavy, zombieSourceHeavy);
        Pool_Init(ref zombiesPoolFast, maxZombieFast, zombieSourceFast);
        Pool_Init(ref zombiesPoolRange, maxZombieRange, zombieSourceRange);
        Pool_Init(ref zombiesSubPoolRange, maxZombieRange, ZombieSubSourceRange);
        Pool_Init(ref zombiesPoolSanta, maxZombieSanta, zombieSourceSanta);

        Pool_Init(ref hitEffectPool, maxHitEffect, DataManager.Instance.effectLib.hitEffects);

        constraint = Pathfinding.NNConstraint.None;
        constraint.constrainWalkability = true;
        constraint.walkable = true;
        constraint.constrainTags = true;
        constraint.tags = (1 << 0);

        hordeSpawners = FindObjectsOfType<ZombieHordeSpawner>(false);
        ResetHordeSpawner();
    }

    private void Pool_Init(ref List<ZombieBase> list, int max, GameObject source)
    {
        list = new List<ZombieBase>(max);
        for (int i = 0; i < max; i++)
        {
            var newZombie = Instantiate(source, zombieSpawnParent).GetComponent<ZombieBase>();
            newZombie.Init(currentTarget);
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
            newZombie.SetActive(false);
            list.Add(newZombie);
        }
    }
    private void Pool_Init(ref List<GameObject> list, int max, GameObject[] source)
    {
        list = new List<GameObject>(max);
        for (int i = 0; i < max; i++)
        {
            var newZombie = Instantiate(source[UnityEngine.Random.Range(0, source.Length)], zombieSpawnParent);
            newZombie.SetActive(false);
            list.Add(newZombie);
        }
    }

    private void Update()
    {
        if (hitEffectCooldown > 0f)
            hitEffectCooldown -= Time.deltaTime;

        if (TutorialManager.Instance.training && TutorialManager.Instance.step <= 1) return;
        if (GM.Instance.midNight) return; // 한밤중 좀비 스폰 중지

        timer += Time.deltaTime;

        float dist = (GM.Instance.player.transform.position - GM.Instance.pizzeriaPos.transform.position).magnitude;
        bool playerInBase = dist < 60f;

        if (timer >= 1f)
        {
            timer = 0f;
            int count = spawnCount + UnityEngine.Random.Range(0, spawnCountRandomAdd + 1);
            Spawn(count, playerInBase);
        }

        if (!playerInBase)
        {
            int day = GM.Instance.day;
            if (day > 0)
            {
                timer2 += Time.deltaTime;
                if (timer2 >= 10f)
                {
                    timer2 = 0f;
                    SpawnFast(1);
                }
            }
            if (day > 2)
            {
                timer4 += Time.deltaTime;
                if (timer4 >= 10f)
                {
                    timer4 = 0f;
                    SpawnRange(1);
                }
            }
            if (day > 4)
            {
                timer3 += Time.deltaTime;
                if (timer3 >= 10f)
                {
                    timer3 = 0f;
                    SpawnHeavy(1);
                }
            }
            if (day > 6)
            {
                timer5 += Time.deltaTime;
                if (timer5 >= 15f)
                {
                    timer5 = 0f;
                    SpawnSanta(1);
                }
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
        ResetSub(zombieHordePool);
        ResetSub(zombiesPoolHeavy);
        ResetSub(zombiesPoolFast);
        ResetSub(zombiesPoolRange);
        ResetSub(zombiesSubPoolRange);
        ResetSub(zombiesPoolSanta);

        ResetHordeSpawner();
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

    public void Spawn(int count, bool playerInBase)
    {
        if (count <= 0) return;

        int max = playerInBase ? 100 : maxZombie;

        for (int i = 0; i < max; i++)
        {
            var zom = zombiesPool[i];
            if (!zom.gameObject.activeSelf)
            {
                Vector3 node = GetRandomPos(30f);

                float dist = (currentTarget.position - node).magnitude;

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

    public void SpawnHorde(int count, BoxCollider rect)
    {
        if (count <= 0) count = maxZombieHorde;

        int max = maxZombieHorde;

        for (int i = 0; i < max; i++)
        {
            var zom = zombieHordePool[i];
            if (!zom.gameObject.activeSelf)
            {
                Vector3 node = GetRandomPos(rect.bounds);

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

    private void SpawnSanta(int count)
    {
        for (int i = 0; i < maxZombieSanta; i++)
        {
            var zom = zombiesPoolSanta[i];
            if (!zom.gameObject.activeSelf)
            {
                Vector3 node = GetRandomPos(10f);

                float dist = (currentTarget.position - node).magnitude;

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

    private void SpawnFast(int count)
    {
        for (int i = 0; i < maxZombieFast; i++)
        {
            var zom = zombiesPoolFast[i];
            if (!zom.gameObject.activeSelf)
            {
                Vector3 node = GetRandomPos(10f);

                float dist = (currentTarget.position - node).magnitude;

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

                float dist = (currentTarget.position - node).magnitude;

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

                float dist = (currentTarget.position - node).magnitude;

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

    private Vector3 GetRandomPos(float maxAngle)
    {
        float randomAngle = Random.Range(-1f, 1f) * maxAngle;
        var v3 = Quaternion.AngleAxis(randomAngle, Vector3.up) * currentTarget.forward;
        Vector3 newPos = currentTarget.transform.position + v3 * spawnDist;
        newPos.y = 0f;

        var node = AstarPath.active.GetNearest(newPos, constraint).position;
        return node;
    }
    private Vector3 GetRandomPos(Bounds bounds)
    {
        Vector3 newPos = new Vector3
        (
            Random.Range(bounds.min.x, bounds.max.x),
            0f,
            Random.Range(bounds.min.z, bounds.max.z)
        );

        var node = AstarPath.active.GetNearest(newPos, constraint).position;
        return node;
    }


    private float hitEffectCooldown;

    public void SpawnHitEffect(Vector3 pos)
    {
        if (hitEffectCooldown > 0f) return;

        pos.y = 2f;

        for (int i = 0; i < maxHitEffect; i++)
        {
            var zom = hitEffectPool[i];
            if (!zom.activeSelf)
            {
                zom.transform.position = pos;
                hitEffectCooldown = 0.05f;
                StartCoroutine(HitEffectRecycle(zom));
                break;
            }
        }
    }
    private IEnumerator HitEffectRecycle(GameObject obj)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            obj.transform.GetChild(i).gameObject.SetActive(false);
        }
        obj.transform.GetChild(UnityEngine.Random.Range(0, obj.transform.childCount)).gameObject.SetActive(true);
        obj.SetActive(true);
        yield return CoroutineHelper.WaitForSeconds(3f);
        obj.SetActive(false);
    }

    public void ResetHordeSpawner()
    {
        // 매일 호드 스포너의 절반만 랜덤으로 활성화시킴

        for (int i = 0; i < hordeSpawners.Length; i++)
        {
            hordeSpawners[i].triggered = false;
        }

        hordeSpawners.Shuffle();

        int halfDisable = (hordeSpawners.Length / 2);
        if (halfDisable >= hordeSpawners.Length) return;

        for (int i = 0; i < halfDisable; i++)
        {
            hordeSpawners[i].triggered = true;
        }
    }
}
