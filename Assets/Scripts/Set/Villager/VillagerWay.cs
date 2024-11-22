using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class VillagerWay : MonoBehaviour
{
    [Serializable]
    public struct SaveData
    {
        public bool recruited;
        public bool expelled;

        public int relations; 
        public float relationExp; 
        public int condition;
        public int currentNeeds;
    }

    public SaveData Save()
    {
        SaveData data = new SaveData
        {
            recruited = this.recruited,
            expelled = this.expelled,

            relations = this.relations,
            relationExp = this.relationExp,
            condition = this.condition,
            currentNeeds = this.currentNeeds,
        };
        return data;
    }

    public void Load(SaveData data)
    {
        recruited = data.recruited;
        expelled = data.expelled;

        if (expelled)
        {
            Expel();
        }
        else if (recruited)
        {
            gameObject.SetActive(true);
            loading = true;
            StartCoroutine(ResetPos());
        }

        relations = data.relations;
        relationExp = data.relationExp;
        condition = data.condition;
        currentNeeds = data.currentNeeds;
    }


    public bool recruited;
    public bool expelled;

    public int relations; // ���� ����
    public float relationExp; // 0 ~ 1 => 1 ���޽� ���� relations���� ������
    public int condition;
    public int currentNeeds;

    public int chatIdx;


    public GameObject interactionObj;
    public GameObject minimapObj;

    public Transform midnightFixedPos;
    public Transform[] wayPoints;

    [SerializeField] private Animator animator;
    private int targetWayPoint;
    private float dealyToTargetTimer;

    protected IAstarAI ai;
    protected AIDestinationSetter destinationSetter;

    private Transform target;
    public float seeDist = 20f;
    public float rotSpeed = 60f;
    private Quaternion firstQuat;
    public int idx;
    public int gender;

    private bool loading = false;

    public void Init()
    {
        ai = GetComponent<IAstarAI>();
        destinationSetter = GetComponent<AIDestinationSetter>();

        target = GM.Instance.player.transform;
        minimapObj.SetActive(false);
        RandomTarget(1f);

        gameObject.SetActive(false);
    }

    public void RandomTarget(float maxWait)
    {
        targetWayPoint = UnityEngine.Random.Range(0, wayPoints.Length);
        dealyToTargetTimer = UnityEngine.Random.Range(0.75f, maxWait);
        destinationSetter.target = wayPoints[targetWayPoint];
        ai.canMove = false;
        animator.SetBool(TextManager.WalkId, false);
    }

    public int Income()
    {
        int value = Constant.villagerIncome;
        if (GM.Instance.hardMode)
            value += 100;
        value *= (relations + 1);

        float modify = 1f;
        switch (condition)
        {
            case 0:
                modify = 0f;
                break;
            case 1:
                modify = 0.5f;
                break;
            case 3:
                modify = 1.5f;
                break;
            case 4:
                modify = 2.0f;
                break;
        }
        value = (int)(value * modify * (1f + ResearchManager.Instance.globalEffect.villager_bonus));
        if (value < 0) value = 0;
        return value;
    }

    void Update()
    {
        if (ai == null) return;
        if (expelled) return;
        if (!recruited) return;
        if (loading) return;

        if (GM.Instance.midNight)
        {
            Vector3 differ = (target.position - transform.position);
            differ.y = 0f;
            float dist = differ.magnitude;

            if (dist < seeDist)
            {
                var step = rotSpeed * Time.deltaTime;
                Quaternion _rot = Quaternion.LookRotation(differ);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _rot, step);
            }
            else
            {
                var step = rotSpeed * Time.deltaTime;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, firstQuat, step);
            }

            return;
        }

        bool walk = false;
        if (dealyToTargetTimer > 0f)
        {
            animator.SetBool(TextManager.WalkId, walk);
            dealyToTargetTimer -= Time.deltaTime;
            return;
        }
        ai.canMove = true;
        walk = true;

        if (!ai.pathPending)
        {
            if (ai.remainingDistance <= (ai as FollowerEntity).stopDistance)
            {
                RandomTarget(3.5f);
            }
        }

        animator.SetBool(TextManager.WalkId, walk);
    }

    public void MidNight(bool on)
    {
        if (!gameObject.activeSelf) return;

        if (on)
        {
            animator.SetBool(TextManager.WalkId, false);
            ai.canMove = false;
            (ai as FollowerEntity).updateRotation = false;
            StartCoroutine(ResetPos());
            minimapObj.SetActive(true);
        }
        else
        {
            minimapObj.SetActive(false);
            (ai as FollowerEntity).updateRotation = true;
        }
    }
    private IEnumerator ResetPos()
    {
        yield return null;
        yield return null;
        yield return null;

        transform.position = midnightFixedPos.position;
        transform.rotation = midnightFixedPos.rotation;
        firstQuat = midnightFixedPos.rotation;

        loading = false;
    }


    public void AddCondition(int value)
    {
        condition += value;

        if (condition > 4) condition = 4;
        else if (condition < 0) condition = 0;
    }
    public void AddExp(float value)
    {
        if (relations >= 4)
        {
            relations = 4;
            relationExp = 1f;
            return;
        }

        relationExp += value;

        if (relationExp >= 1f)
        {
            relations += 1;
            if (condition > 2) condition = 2;
            relationExp = 0f;
        }
        else if (relationExp < 0f) relationExp = 0f;
    }
    public void SetNeeds(int value)
    {
        currentNeeds = value;
    }

    public void CalcNeedsMet() // �ѹ��߿��� �Ѿ �� ���
    {
        if (!recruited) return;

        if (currentNeeds > -1) // �䱸�� �־��µ� �� ������ 33% Ȯ���� ��Ƽ�� 1 ����
        {
            int rand = UnityEngine.Random.Range(0, 100);

            if (rand < 25)
            {
                AddCondition(-1);
            }

            // �䱸�� ������� �ʾҰ�, �䱸�� ������ ������ ���� �ʴٸ�, �䱸�� ������ ������ �ٸ� �������� �䱸�ϴ� ���� �����
            //if (VillagerManager.Instance.inventory[currentNeeds] == 0)
            //{
            //    while (true)
            //    {
            //        int other = UnityEngine.Random.Range(0, VillagerManager.Instance.inventory.Length);
            //        if (other != currentNeeds)
            //        {
            //            currentNeeds = other;
            //            break;
            //        }
            //    }
            //}
        }
        else
        {
            // ������ ���� => ���ο� �屸
            currentNeeds = UnityEngine.Random.Range(0, VillagerManager.Instance.inventory.Length);
        }

        float exp = 0.25f; // �׳� 4�ϸ��� 1�� ������
        //switch (condition)
        //{
        //    case 0:
        //        exp = 0.2f;
        //        break;
        //    case 1:
        //        exp = 0.25f;
        //        break;
        //    case 2:
        //        exp = 0.34f;
        //        break;
        //    case 3:
        //        exp = 0.5f;
        //        break;
        //    case 4:
        //        exp = 1f;
        //        break;
        //}
        AddExp(exp);
    }

    public void Expel()
    {
        expelled = true;
        recruited = false;
        gameObject.SetActive(false);
    }

    public void Recruit()
    {
        recruited = true;
        expelled = false;

        int day = GM.Instance.day;

        relations = 0;
        if (day >= 14) relations++;
        if (day >= 18) relations++;
        if (day >= 21) relations++;

        //condition = UnityEngine.Random.Range(1, 4);
        condition = 2;

        var inven = VillagerManager.Instance.inventory;
        List<int> hasList = new List<int>();
        for (int i = 0; i < inven.Length; i++)
        {
            if (inven[i] > 0)
            {
                hasList.Add(i);
                break;
            }
        }
        hasList.Shuffle();
        if (hasList != null && hasList.Count > 0)
            currentNeeds = hasList[0];
        else
            currentNeeds = UnityEngine.Random.Range(0, VillagerManager.Instance.inventory.Length);

        gameObject.SetActive(true);
        StartCoroutine(ResetPos());
    }
}
