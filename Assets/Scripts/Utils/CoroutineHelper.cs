using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHelper : MonoBehaviour
{

    private static MonoBehaviour monoInstance;

    [RuntimeInitializeOnLoadMethod]
    private static void Initializer()
    {
        monoInstance = new GameObject($"[{nameof(CoroutineHelper)}]").AddComponent<CoroutineHelper>();
        DontDestroyOnLoad(monoInstance.gameObject);
    }

    public new static Coroutine StartCoroutine(IEnumerator coroutine)
    {
        return monoInstance.StartCoroutine(coroutine);
    }

    public new static void StopCoroutine(Coroutine coroutine)
    {
        monoInstance.StopCoroutine(coroutine);
    }


    public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

    private static readonly Dictionary<float, WaitForSeconds> waitForSeconds = new Dictionary<float, WaitForSeconds>();
    private static readonly Dictionary<float, WaitForSecondsRealtime> waitForSecondsRealtime = new Dictionary<float, WaitForSecondsRealtime>();

    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        if (!waitForSeconds.TryGetValue(seconds, out WaitForSeconds wfs))
            waitForSeconds.Add(seconds, wfs = new WaitForSeconds(seconds));
        return wfs;
    }

    public static WaitForSecondsRealtime WaitForSecondsRealtime(float seconds)
    {
        if (!waitForSecondsRealtime.TryGetValue(seconds, out WaitForSecondsRealtime wfs))
            waitForSecondsRealtime.Add(seconds, wfs = new WaitForSecondsRealtime(seconds));
        return wfs;
    }

    public static Coroutine SimpleCoroution(float seconds, Action action)
    {
        return StartCoroutine(SimpleAction(seconds, action));
    }
    public static Coroutine SimpleCoroution(Action action)
    {
        return StartCoroutine(SimpleAction(action));
    }

    private static IEnumerator SimpleAction(float seconds, Action action)
    {
        yield return WaitForSeconds(seconds);
        action();
    }
    private static IEnumerator SimpleAction(Action action)
    {
        yield return null;
        action();
    }
}