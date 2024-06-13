using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Act5_NinjaChicken : MonoBehaviour
{

    public float speedBoss = 5f;
    public Transform bossCar;
    public Transform bossCarTarget;


    public float speed = 5f;

    public Transform[] cars;

    private float[] randomSpeed;

    private bool[] reached;
    private int reachCount;

    public Transform[] carTargets;

    public int step = 0;

    public CanvasGroup dark;

    private float timer;

    public AudioClip appearCilp;

    private void Start()
    {
        reachCount = 0;

        randomSpeed = new float[cars.Length];
        reached = new bool[cars.Length];

        dark.alpha = 0f;

        for (int i = 0; i < randomSpeed.Length; i++)
        {
            if (cars[i].position.y > 0.005f)
                randomSpeed[i] = UnityEngine.Random.Range(2.5f, 10f);
            else
                randomSpeed[i] = UnityEngine.Random.Range(-5f, 0);
        }
    }


    private void Update()
    {
        if (step == 0)
        {
            Vector3 diff = (bossCarTarget.position - bossCar.position);

            Vector3 dir = diff.normalized;
            float dist = diff.magnitude;

            if (dist > 0.33f)
            {
                bossCar.position += speed * Time.deltaTime * dir;
            }
            else
            {
                step = 1;
                AudioManager.Instance.PlaySFX(appearCilp);
            }
        }

        if (step == 0) return;

        if (step == 1)
        {
            if (timer >= 0.5f)
            {
                step = 2;
                timer = 0;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }

        if (step == 1) return;

        if (step == 2)
        {
            for (int i = 0; i < cars.Length; i++)
            {
                Vector3 diff = (carTargets[i].position - cars[i].position);

                Vector3 dir = diff.normalized;
                float dist = diff.magnitude;

                if (dist > 0.33f)
                {
                    cars[i].position += (speed + randomSpeed[i]) * Time.deltaTime * dir;
                }
                else if (!reached[i])
                {
                    reached[i] = true;
                    reachCount++;
                }
            }

            if (reachCount >= cars.Length)
            {
                step = 3;
            }
        }

        if (step == 2) return;

        if (step == 3)
        {
            if (timer >= 1.5f)
            {
                step = 4;

                dark.DOFade(1f, 0.5f).OnComplete(() =>
                {
                    LoadingSceneManager.Instance.ToLobby();
                });

            }
            else
            {
                timer += Time.deltaTime;
            }
        }
    }

}
