using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeDirection : MonoBehaviour
{

    public ParticleSystem effect;
    private Coroutine hideCoroutine;

    public Transform stackTarget11;
    public Transform stackTarget12;

    public void Show(int pos = 0)
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        if (pos == 0)
        {
            effect.transform.position = stackTarget11.position;
        }
        else
        {
            effect.transform.position = stackTarget12.position;
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

    public void ResetPos()
    {
        effect.transform.position = stackTarget11.position;
    }

    [ContextMenu("µð¹ö±ë")]
    public void Show_Debug()
    {
        Show();
    }
}
