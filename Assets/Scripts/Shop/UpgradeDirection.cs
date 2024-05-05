using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeDirection : MonoBehaviour
{

    public ParticleSystem effect;
    private Coroutine hideCoroutine;

    public void Show()
    {
        effect.gameObject.SetActive(true);
        effect.Play(true);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(Hide(effect.main.duration));
    }

    private IEnumerator Hide(float time)
    {
        yield return CoroutineHelper.WaitForSecondsRealtime(time);

        effect.gameObject.SetActive(false);
        hideCoroutine = null;
    }
}
