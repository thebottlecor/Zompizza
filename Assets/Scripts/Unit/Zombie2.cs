using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Pathfinding;
using System;

public class Zombie2 : ZombieBase
{

    public bool contactingPlayer;
    private bool contactingPrev;
    private float attackTimer;
    private float contactTimer;

    public float range = 3.5f; // 제곱 raw square magnitude
    public bool isInstall;
    private bool installUsed;

    public bool isVomit;
    private bool isVomitted;

    public bool isRun;

    public FollowerEntity ai;
    protected AIDestinationSetter destinationSetter;

    public static EventHandler<float> DamageEvent;

    public override void Init(Transform target)
    {
        rigid = GetComponent<Rigidbody>();

        destinationSetter = GetComponent<AIDestinationSetter>();
        destinationSetter.target = target;
    }

    void Update()
    {
        contactingPlayer = false;

        //float dist = Vector3.Distance(ZombiePooler.Instance.currentTarget.transform.position, transform.position);
        float dist = (ZombiePooler.Instance.currentTarget.transform.position - transform.position).sqrMagnitude;
        if (dist >= 10000f)
        {
            gameObject.SetActive(false);
            return;
        }

        if (!dead)
        {
            if (ZombiePooler.Instance.currentTarget != null)
            {
                if (!ai.isStopped)
                {
                    if (!ai.pathPending)
                    {
                        if (ai.remainingDistance <= (ai as FollowerEntity).stopDistance && dist <= range) // 적당한 거리 (거리에 민감)
                        {
                            contactingPlayer = true;
                            if (attackTimer < 0.5f) attackTimer = 0.5f; // 근접 공격의 경우, 붙는 것보다 공격속도가 2배 빠름 (어차피 유효타가 적으므로)
                        }
                    }
                }
            }
        }

        SetAttackAnim();

        if (contactingPlayer)
        {
            if (isInstall)
            {
                if (!installUsed)
                {
                    installUsed = true;
                    ZombiePooler.Instance.SpawnRangeSub(transform.position);
                    AudioManager.Instance.PlaySFX(Sfx.zombieInstall);
                }
            }
            else if (isVomit)
            {
                if (!isVomitted)
                {
                    isVomitted = true;
                    GM.Instance.SetVomit();
                    AudioManager.Instance.PlaySFX(Sfx.zombieVomit);
                }
            }
            else
            {
                Attack();
            }
        }
        else if (contact)
        {
            contactTimer += Time.deltaTime;
            Attack();
        }

        // 플레이어에게 Attacked 속성을 붙여서, Attacked * damage 만큼 1초당 받게 하기
    }

    private void Attack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer > 1f)
        {
            attackTimer = 0f;
            AudioManager.Instance.PlaySFX(Sfx.hittngPlayer);

            if (DamageEvent != null)
                DamageEvent(null, Constant.zombie_damage * UnityEngine.Random.Range(0.75f, 1.25f));
        }
    }

    public override void Hit(Vector3 hitPos, float speed, Vector3 knockbackDir)
    {
        //hitFeedback?.PlayFeedbacks();
        ai.isStopped = true;
        ai.canMove = false;

        rigid.constraints = RigidbodyConstraints.None;

        Vector3 expPos = hitPos;
        expPos.y = -3f; // 평면 밑에서 폭발 (평면 밑으로 들어가지 않도록)

        rigid.velocity = Vector3.zero;
        if (!dead)
        {
            //knockbackDir.y += 0.1f;
            rigid.AddForce(speed * ZombiePooler.Instance.knockbackPower * knockbackDir, ForceMode.Impulse);

            ZombiePooler.Instance.SpawnHitEffect(hitPos);
        }
        coll.gameObject.layer = LayerMask.NameToLayer("Flying Zombie");

        if (isHeavy)
            rigid.AddExplosionForce(0.05f * ZombiePooler.Instance.power * speed, expPos, ZombiePooler.Instance.radius, ZombiePooler.Instance.height * 0.05f);
        else
            rigid.AddExplosionForce(ZombiePooler.Instance.power * speed, expPos, ZombiePooler.Instance.radius, ZombiePooler.Instance.height);

        DeadHandle();
    }

    public override bool CloseContact(Vector3 hitPos)
    {
        if (isHeavy) return false;
        if (dead) return false;

        ai.isStopped = true;
        ai.canMove = false;

        rigid.velocity = Vector3.zero;
        rigid.isKinematic = true;
        coll.enabled = false;

        SetContact(true);
        shadow.SetActive(false);

        this.transform.SetParent(ZombiePooler.Instance.currentTarget);

        Vector3 origin = ZombiePooler.Instance.currentTarget.position;
        origin.y += 2f;
        if (Physics.Raycast(origin, hitPos - origin, out RaycastHit result, 2f, LayerMask.GetMask("Car Contact Coll")))
        {
            transform.position = result.point;
            transform.LookAt(ZombiePooler.Instance.currentTarget);
        }

        return true;
    }

    public override void DriftOffContact(float localXvel, float speed) // localXvel < 0 오른쪽 , > 0 왼쪽 (로컬 기준)
    {
        if (contactTimer <= 0.25f) // 붙자마자 드리프트로 떼어졌을 때 -> 거의 충돌한 것과 마찬가지
        {
            AudioManager.Instance.PlaySFX(Sfx.zombieCrash);
        }

        Transform tempTarger = ZombiePooler.Instance.currentTarget;

        this.transform.SetParent(ZombiePooler.Instance.zombieSpawnParent);

        ai.isStopped = true;
        ai.canMove = false;

        rigid.constraints = RigidbodyConstraints.None;
        rigid.isKinematic = false;
        coll.enabled = true;

        SetContact(false);

        Vector3 expPos = transform.position;

        Vector3 right = new Vector3(tempTarger.forward.z, tempTarger.forward.y, -1f * tempTarger.forward.x);
        if (localXvel < 0)
            expPos += 0.25f * ZombiePooler.Instance.radius * right;
        else if (localXvel > 0)
            expPos += -0.25f * ZombiePooler.Instance.radius * right;

        expPos.y = -3f; // 평면 밑에서 폭발 (평면 밑으로 들어가지 않도록)

        rigid.velocity = Vector3.zero;
        rigid.AddExplosionForce(ZombiePooler.Instance.power * Mathf.Max(Mathf.Min(0.33f, speed * 0.33f), 0.1f), expPos, ZombiePooler.Instance.radius, ZombiePooler.Instance.height);

        DeadHandle();
    }

    public override void StateReset()
    {
        base.StateReset();

        destinationSetter.target = ZombiePooler.Instance.currentTarget;

        ai.isStopped = false;
        ai.canMove = false;

        SetContact(false);
        installUsed = false;
        isVomitted = false;

        attackTimer = 0f;
        contactTimer = 0f;

        contactingPrev = false;
        CoroutineHelper.StartCoroutine(SetWalk());
    }
    private IEnumerator SetWalk()
    {
        yield return null;
        // 항상 true
        if (animator != null)
            animator.SetBool(TextManager.WalkId, true);
    }

    public override void DeadHandle()
    {
        base.DeadHandle();

        animator.SetBool(TextManager.WalkId, false);

        //if (!GM.Instance.EndTime && OrderManager.Instance.IsDelivering())
        //    GM.Instance.AddGold(1, GM.GetGoldSource.zombie);

        CoroutineHelper.StartCoroutine(Reanimate());
    }

    private IEnumerator Reanimate()
    {
        yield return CoroutineHelper.WaitForSeconds(2.5f);
        if (gameObject.activeSelf && dead)
        {
            while (transform.position.y >= Constant.spawnPosY)
            {
                if (!gameObject.activeSelf || !dead)
                {
                    yield break;
                }
                yield return null;
            }

            gameObject.SetActive(false);
            yield return null;

            transform.position = ZombiePooler.Instance.GetReAnimatedPos(transform);
            StateReset();
            gameObject.SetActive(true);
        }
    }

    protected void SetContact(bool on)
    {
        contact = on;

        if (isRun)
            animator.SetBool(TextManager.ContactId, contact);
    }
    protected void SetAttackAnim()
    {
        if (contactingPlayer)
        {
            if (!contactingPrev)
            {
                animator.SetBool(TextManager.AttackId, true);
                contactingPrev = true;
            }
        }
        else
        {
            if (contactingPrev)
            {
                animator.SetBool(TextManager.AttackId, false);
                contactingPrev = false;
            }
        }
    }
}