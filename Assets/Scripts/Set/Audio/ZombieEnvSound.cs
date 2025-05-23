using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieEnvSound : MonoBehaviour
{

    public AudioSource[] sources;

    private float timer;
    private float randomInterval;

    private float minInterval = 1f;
    private float maxInterval = 5f;

    // Start is called before the first frame update
    void Start()
    {
        randomInterval = maxInterval;
    }

    public void Mute(bool on)
    {
        for (int i = 0; i < sources.Length; i++)
        {
            sources[i].mute = on;
        }
    }

    private float GetSoundInterval()
    {
        float result = 10f;
        if (ZombiePooler.Instance != null)
            result = ((minInterval - maxInterval) / ZombiePooler.Instance.maxZombie) * ZombiePooler.Instance.GetActiveZomibes() + maxInterval;
        float random = UnityEngine.Random.Range(-1f, 1f) + result;
        return Mathf.Max(random, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (TutorialManager.Instance.training && TutorialManager.Instance.step <= 1) return;

        timer += 1f * Time.deltaTime;

        if (timer > randomInterval)
        {
            timer = 0f;
            randomInterval = GetSoundInterval();

            RandomSound();
        }
    }

    public void RandomSound()
    {
        sources.Shuffle();

        int randomSource = 0;

        while (true)
        {
            if (sources[randomSource].isPlaying)
                randomSource++;
            else
                break;

            if (randomSource >= sources.Length)
                return;
        }

        float randomVolume = UnityEngine.Random.Range(0.6f, 0.8f);
        float randomPitch = UnityEngine.Random.Range(0.9f, 1.1f);

        sources[randomSource].volume = randomVolume;
        sources[randomSource].pitch = randomPitch;

        sources[randomSource].Play();

        StartCoroutine(StopSound(randomSource));
    }

    private IEnumerator StopSound(int idx)
    {
        float duration = sources[idx].clip.length * 0.99f;

        yield return CoroutineHelper.WaitForSecondsRealtime(duration);

        sources[idx].Stop();
    }
}
