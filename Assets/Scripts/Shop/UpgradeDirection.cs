using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeDirection : MonoBehaviour
{

    public ParticleSystem effect;
    private Coroutine hideCoroutine;

    public Transform stackTarget11;

    public void Show()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        hideCoroutine = StartCoroutine(Hide(effect.main.duration));
    }

    private IEnumerator Hide(float time)
    {
        effect.gameObject.SetActive(false);

        yield return null;

        effect.gameObject.SetActive(true);
        effect.Play(true);

        yield return CoroutineHelper.WaitForSecondsRealtime(time);

        effect.gameObject.SetActive(false);
        hideCoroutine = null;
    }

    [ContextMenu("µð¹ö±ë")]
    public void Show_Debug()
    {
        Show();
    }
}
