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
        if (!audioSourceBGM.isPlaying)
        {
            if (++currentBGM >= bgms.Length)
            {
                currentBGM = 0;
                bgms.Shuffle();
            }
            StartBGMPeace(currentBGM);
        }
    }

    public void StartBGMPeace(int num)
    {
        //if (BGMReplaceCoroutine != null)
        //{
        //    StopCoroutine(BGMReplaceCoroutine);
        //    BGMReplaceCoroutine = null;
        //}

        audioSourceBGM.clip = bgms[num].clip;
        audioSourceBGM.volume = 1f + bgms[num].volumeOffset + globalBGMVolumeOffset;

        //if (currentBGM == -1)
        //{
        //    audioSourceBGMPeace.Play();
        //    currentBGM = num;
        //}
        //else
        //{
        //    BGMReplaceCoroutine = StartCoroutine(BGMReplaceSeq(num));
        //}

        audioSourceBGM.Play();
        currentBGM = num;
    }

    private Coroutine BGMReplaceCoroutine;
    //private IEnumerator BGMReplaceSeq(int num)
    //{
    //    currentBGM = num;

    //    float timer = 0f;
    //    float duration = 2f;

    //    float battleBGMVolume = audioSourceBGM.volume;
    //    float peaceBGMVolume = audioSourceBGMPeace.volume;

    //    audioSourceBGMPeace.mute = false;
    //    audioSourceBGMPeace.volume = 0f;

    //    while (timer < duration)
    //    {
    //        yield return CoroutineHelper.WaitForSecondsRealtime(Time.unscaledDeltaTime);
    //        timer += Time.unscaledDeltaTime;
    //        audioSourceBGM.volume = battleBGMVolume * (duration - timer) / duration;
    //        audioSourceBGMPeace.volume = peaceBGMVolume * (timer) / duration;
    //    }

    //    audioSourceBGM.mute = true;

    //    audioSourceBGMPeace.volume = peaceBGMVolume;
    //}

    public bool IsPlayingSFX()
    {
        return audioSourceSFX.isPlaying;
    }

    public void PlaySFX(Sfx index, bool loop = false, float volume = 1f)
    {
        if (!loop)
            audioSourceSFX.PlayOneShot(audios[index].clip, volume + audios[index].volumeOffset);
        else
        {
            audioSourceSFX_Loop.clip = audios[index].clip;
            audioSourceSFX_Loop.volume = volume + audios[index].volumeOffset;
            audioSourceSFX_Loop.Play();
        }
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
    construction,
    demolish,
    toggle,
    buttons,
    speedButtons,
    newYear,
    newInformation,
    research,
    researchUnlockSuccess,
    deny,
    inputFieldStart,
    inputFieldEnd,
    smallButtons,
    tradeSuccess,
    recipe,
    entityUtils,
    naturalObjectRemove,
    entitySelect,
    tutorialComplete,
}
