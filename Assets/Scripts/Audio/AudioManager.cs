using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public struct SFXInfo
{
    public AudioClip clip;
    public float volumeOffset;
    public int maxPlay;
}

public class AudioManager : Singleton<AudioManager>
{

    public AudioSource audioSourceBGM;
    public AudioSource audioSourceSFX;
    public AudioSource audioSourceSFX_Loop;

    [NonReorderable] // 임시조치
    public SFXInfo[] bgms;
    public float globalBGMVolumeOffset;
    private int currentBGM;

    public AudioMixer masterMixer;

    [NonReorderable] // 임시조치
    [SerializeField]
    private SerializableDictionary<Sfx, SFXInfo> audios;

    //public override void CallAfterAwake()
    //{

    //}

    //public override void CallAfterStart(ConfigData config)
    //{
    //    bgms.Shuffle();
    //    audioSourceBGM.volume = 1f + globalBGMVolumeOffset;
    //    StartBGMPeace(0);
    //}

    public Dictionary<Sfx, int> concurrentLimit;
    public Dictionary<Sfx, int> frameLimit;

    private void Start()
    {
        concurrentLimit = new Dictionary<Sfx, int>();
        frameLimit = new Dictionary<Sfx, int>();
        var list = Enum.GetValues(typeof(Sfx));
        foreach (var temp in list)
        {
            Sfx sfx = (Sfx)temp;
            concurrentLimit.Add(sfx, 0);
            frameLimit.Add(sfx, 0);
        }

        bgms.Shuffle();
        audioSourceBGM.volume = 1f + globalBGMVolumeOffset;
        StartBGM(0);
    }

    bool isPaused;

    void OnApplicationFocus(bool hasFocus)
    {
        isPaused = !hasFocus;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        isPaused = pauseStatus;
    }

    public void SetBGMVolume(float value)
    {
        // 0.001f ~ 1f
        masterMixer.SetFloat("volumeBGM", Mathf.Log(value) * 20f);
    }
    public void SetSFXVolume(float value)
    {
        // 0.001f ~ 1f
        masterMixer.SetFloat("volumeSFX", Mathf.Log(value) * 20f);
    }

    private void Update()
    {
        if (isPaused) return;

        if (!audioSourceBGM.isPlaying)
        {
            if (++currentBGM >= bgms.Length)
            {
                currentBGM = 0;
                bgms.Shuffle();
            }
            StartBGM(currentBGM);
        }
    }

    public void StartBGM(int num)
    {
        audioSourceBGM.clip = bgms[num].clip;
        audioSourceBGM.volume = 1f + bgms[num].volumeOffset + globalBGMVolumeOffset;

        audioSourceBGM.Play();
        currentBGM = num;
    }

    public bool IsPlayingSFX()
    {
        return audioSourceSFX.isPlaying;
    }

    public void PlaySFX(Sfx idx, bool loop = false, float volume = 1f)
    {
        if (!loop)
        {
            if (frameLimit[idx] == 0 && concurrentLimit[idx] < audios[idx].maxPlay + 1)
            {
                audioSourceSFX.PlayOneShot(audios[idx].clip, volume + audios[idx].volumeOffset);
                StartCoroutine(FrameSoundCheck(idx));
                StartCoroutine(CheckSoundFinish(idx));
            }
        }
        else
        {
            audioSourceSFX_Loop.clip = audios[idx].clip;
            audioSourceSFX_Loop.volume = volume + audios[idx].volumeOffset;
            audioSourceSFX_Loop.Play();
        }
    }

    private IEnumerator FrameSoundCheck(Sfx idx)
    {
        frameLimit[idx]++;
        float shortDuration = audios[idx].clip.length * 0.1f;
        shortDuration = Mathf.Max(shortDuration, 0.02f);
        yield return CoroutineHelper.WaitForSecondsRealtime(shortDuration);
        frameLimit[idx]--;
    }

    private IEnumerator CheckSoundFinish(Sfx idx)
    {
        concurrentLimit[idx]++;
        float duration = audios[idx].clip.length;
        yield return CoroutineHelper.WaitForSecondsRealtime(duration);
        concurrentLimit[idx]--;
    }

    public void StopSFX(bool loop = true)
    {
        if (loop)
        {
            audioSourceSFX_Loop.Stop();
        }
        else
        {
            audioSourceSFX.Stop();
        }
    }
}

public enum Sfx
{
    crash,
    zombieCrash,
    hittngPlayer,
    toggle,
    buttons,
    deny,
    inputFieldStart,
    inputFieldEnd,
    newInfo,
    money,
    select,
    complete,
    nextDay,
    chop,
    kitchenTimer,
    close,
    btnHighlight,
    btnHighlight2,
}
