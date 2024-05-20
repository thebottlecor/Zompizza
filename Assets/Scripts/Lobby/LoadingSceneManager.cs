using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager : Singleton<LoadingSceneManager>
{
    [HideInInspector] public string nextScene;

    private int loading = 0;
    public bool IsSceneLoading => loading > 0;

    public GameStartInfo StartInfo { get; private set; }

    private readonly string LoadingSceneName = "loading";

    public static EventHandler<string> SceneLoadCompletedEvent;
    public static EventHandler<float> SceneLoadingEvent;

    public void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        StartCoroutine(Co_LoadScene());
    }

    public void LobbyStart(GameStartInfo startInfo, string sceneName = "asu")
    {
        nextScene = sceneName;
        this.StartInfo = startInfo;

        if (StartInfo.tutorial)
        {
            StartCoroutine(Co_PrologueStart());
        }
        else
        {
            StartCoroutine(Co_LobbyStart());
        }
    }

    public void PrologueStart()
    {
        nextScene = "asu";
        StartCoroutine(Co_LobbyStart());
    }

    public void ToLobby()
    {
        if (WorldMapManager.Instance != null)
        {
            WorldMapManager.Instance.CloseFullscreenMap();
        }

        SettingManager.Instance.ReturnToParent();
        LoadScene("lobby");
        Time.timeScale = 1f;
    }

    IEnumerator Co_PrologueStart()
    {
        // 프롤로그 시작
        var asyncLoad = SceneManager.LoadSceneAsync(4);
        asyncLoad.allowSceneActivation = false;
        float timer = 0f;
        float t = 0f;
        while (!asyncLoad.allowSceneActivation)
        {
            yield return null;
            timer += Time.unscaledDeltaTime;
            if (asyncLoad.progress < 0.9f)
            {
                t = Mathf.Lerp(t, asyncLoad.progress, timer);
                if (t >= asyncLoad.progress)
                    timer = 0f;
            }
            else
            {

                timer += Time.unscaledDeltaTime;
                t = Mathf.Lerp(0.9f, 1f, timer);
                if (t >= 1)
                {
                    asyncLoad.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }

    IEnumerator Co_LobbyStart()
    {
        BeforeLoad();

        yield return SceneManager.LoadSceneAsync(LoadingSceneName);

        var asyncLoad = SceneManager.LoadSceneAsync(nextScene);
        asyncLoad.allowSceneActivation = false;
        float timer = 0f;
        float t = 0f;
        while (!asyncLoad.allowSceneActivation)
        {
            yield return null;
            timer += Time.unscaledDeltaTime;
            if (asyncLoad.progress < 0.9f)
            {
                t = Mathf.Lerp(t, asyncLoad.progress, timer);
                if (t >= asyncLoad.progress)
                    timer = 0f;
            }
            else
            {

                timer += Time.unscaledDeltaTime;
                t = Mathf.Lerp(0.9f, 1f, timer);
                if (t >= 1)
                {
                    asyncLoad.allowSceneActivation = true;
                    LoadCompletedEvent();
                    yield break;
                }
            }
            if (SceneLoadingEvent != null)
                SceneLoadingEvent(this, t);
        }
        LoadCompletedEvent();
    }

    IEnumerator Co_LoadScene()
    {
        BeforeLoad();

        yield return SceneManager.LoadSceneAsync(LoadingSceneName);
        var asyncLoad = SceneManager.LoadSceneAsync(nextScene);
        asyncLoad.allowSceneActivation = false;
        float timer = 0f;
        float t = 0f;
        while (!asyncLoad.isDone)
        {  
            yield return null;
            if (asyncLoad.progress < 0.9f)
            {
                t = Mathf.Lerp(t, asyncLoad.progress, timer);
                if (t >= asyncLoad.progress)
                    timer = 0f;
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                t = Mathf.Lerp(0.9f, 1f, timer);
                if (t >= 1)
                {
                    asyncLoad.allowSceneActivation = true;
                    LoadCompletedEvent();
                    yield break;
                }
            }
            if (SceneLoadingEvent != null)
                SceneLoadingEvent(this, t);
        }
        LoadCompletedEvent();
    }

    void BeforeLoad()
    {
        loading++;
    }

    void LoadCompletedEvent()
    {
        if (SceneLoadCompletedEvent != null)
            SceneLoadCompletedEvent(this, nextScene);

        loading--;
    }

    public override void CallAfterAwake()
    {

    }
    public override void CallAfterStart(ConfigData config)
    {

    }
}
