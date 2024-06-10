using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Pathfinding;
using System;

public class ZombieTitle : MonoBehaviour
{

    protected Rigidbody rigid;

    [SerializeField] protected Collider coll;
    [SerializeField] protected Animator animator;

    public ZombiePoolerTitle zombiePooler;
    public SkinnedMeshRenderer meshRenderer;
    public GameObject shadow;

    private Vector3 initPos;
    private Vector3 targetPos;

    private float timer;
    public float speed = 3f;
    public float seekTime = 3f;
    public float seekDist = 5f;

    public bool alwaysWalking;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();

        initPos = transform.position;
        targetPos = transform.position;
        speed += UnityEngine.Random.Range(-0.15f, 0.15f);
        seekDist += UnityEngine.Random.Range(-0.5f, 0.5f);
        timer = seekTime * 0.5f + UnityEngine.Random.Range(-1.5f , 0.5f);

        var models_Info = zombiePooler.models_Info;
        int rand = UnityEngine.Random.Range(0, models_Info.Length);
        meshRenderer.material = models_Info[rand].material;
        meshRenderer.sharedMesh = models_Info[rand].mesh;
    }

    void Update()
    {
        if (alwaysWalking)
        {
            
        }
        else
        {
            bool walk = false;
            bool attack = false;

            if (timer > 0f)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                targetPos = initPos + new Vector3(UnityEngine.Random.Range(-seekDist, seekDist), 0f, UnityEngine.Random.Range(-seekDist, seekDist));
                timer = seekTime + UnityEngine.Random.Range(-0.5f, 0.5f);
            }

            Vector3 diff = (targetPos - transform.position);
            float dist = diff.magnitude;
            if (dist > 0.25f)
            {
                Vector3 dir = diff.normalized;
                Vector3 look = new Vector3(dir.x, 0f, dir.z);
                transform.rotation = Quaternion.LookRotation(look);
                transform.position += speed * Time.deltaTime * dir;
                walk = true;
            }

            animator.SetBool("Walk", walk);
            animator.SetBool("Attack", attack);
        }
    }
}
