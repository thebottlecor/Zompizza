using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class VillagerWay : MonoBehaviour
{

    public bool recruited;
    public bool expelled;

    public int relations; // 레벨 개념
    public float relationExp; // 0 ~ 1 => 1 도달시 다음 relations으로 레벨업
    public int condition;
    public int currentNeeds;


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

    private void Start()
    {
        ai = GetComponent<IAstarAI>();
        destinationSetter = GetComponent<AIDestinationSetter>();

        target = GM.Instance.player.transform;
        minimapObj.SetActive(false);
        RandomTarget();

        gameObject.SetActive(false);
        //Recruit();
    }

    public void RandomTarget()
    {
        targetWayPoint = UnityEngine.Random.Range(0, wayPoints.Length);
        dealyToTargetTimer = UnityEngine.Random.Range(0.75f, 3.5f);
        destinationSetter.target = wayPoints[targetWayPoint];
        ai.canMove = false;
    }

    public int Income()
    {
        int value = Constant.villagerIncome;
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
        value = (int)(value * modify);
        if (value < 0) value = 0;
        return value;
    }

    void Update()
    {
        if (expelled) return;
        if (!recruited) return;

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
                RandomTarget();
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
            AddCondition(-2);
            relationExp = 0f;
        }
        else if (relationExp < 0f) relationExp = 0f;
    }
    public void SetNeeds(int value)
    {
        currentNeeds = value;
    }

    public void CalcNeedsMet() // 한밤중에서 넘어갈 때 계산
    {
        if (!recruited) return;

        if (currentNeeds > -1) // 요구가 있었는데 안 줬으면 33% 확률로 컨티션 1 감소
        {
            int rand = UnityEngine.Random.Range(0, 100);

            if (rand < 33)
            {
                AddCondition(-1);
            }

            // 요구가 들어지지 않았고, 요구한 물건을 가지고 있지 않다면, 요구한 물건을 제외한 다른 물건으로 요구하는 것이 변경됨
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
            // 만족한 상태 => 새로운 욕구
            currentNeeds = UnityEngine.Random.Range(0, VillagerManager.Instance.inventory.Length);
        }

        float exp = 0.25f; // 그냥 4일마다 1씩 레벨업
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

        recruited = true;
        relations = 0;
        //condition = UnityEngine.Random.Range(1, 4);
        condition = 2;
        currentNeeds = UnityEngine.Random.Range(0, VillagerManager.Instance.inventory.Length);

        gameObject.SetActive(true);
        StartCoroutine(ResetPos());
    }
}
