using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameEventManager : Singleton<GameEventManager>
{

    public int currentEvent = -1;
    private bool[] triggeredEvents;

    public GameObject eventPanel;

    public Image profile;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    public TextMeshProUGUI acceptBtnText;
    public TextMeshProUGUI declineBtnText;

    public Button acceptBtn;
    public Button declineBtn;

    public float typingSpeed = 0.05f;
    private bool tmpCompleted;

    public float friendshipFixed;
    public bool hasCat;
    public GameObject cat;

    public AudioClip[] audioClips;

    public Sprite[] profileSprites;

    private TextManager tm => TextManager.Instance;

    private void Start()
    {
        triggeredEvents = new bool[3];
        Init();
    }

    IEnumerator TextPrint(string text, float delay, Action finishAction)
    {
        tmpCompleted = false;
        int count = 0;

        while (count != text.Length)
        {
            if (count < text.Length)
            {
                dialogueText.text += text[count].ToString();
                count++;
            }

            if (count % 4 == 0)
                AudioManager.Instance.PlaySFX(Sfx.inputFieldStart);

            yield return CoroutineHelper.WaitForSecondsRealtime(typingSpeed);
        }

        if (delay > 0f)
            yield return CoroutineHelper.WaitForSecondsRealtime(delay);

        tmpCompleted = true;

        finishAction();
    }


    public void Init()
    {
        currentEvent = -1;
        tmpCompleted = false;

        acceptBtnText.text = tm.GetCommons("EventAccept");
        declineBtnText.text = tm.GetCommons("EventDecline");

        cat.SetActive(false);
    }

    public void SetEvent(int idx)
    {
        if (triggeredEvents[idx]) return;
        triggeredEvents[idx] = true;

        acceptBtn.gameObject.SetActive(false);
        declineBtn.gameObject.SetActive(false);

        eventPanel.SetActive(true);

        AudioManager.Instance.PlaySFX(Sfx.newInfo);
        UINaviHelper.Instance.SetFirstSelect();

        currentEvent = idx;

        profile.sprite = profileSprites[idx];

        nameText.text = tm.GetCommons($"{idx}Event_name");
        dialogueText.text = string.Empty;

        StartCoroutine(TextPrint(tm.GetCommons($"{idx}Event_dialogue"), 0.25f, () =>
        {
            acceptBtn.gameObject.SetActive(true);
            declineBtn.gameObject.SetActive(true);
            UINaviHelper.Instance.SetFirstSelect();
        }));
    }

    public void AcceptTrigger()
    {
        if (!tmpCompleted) return;

        acceptBtn.gameObject.SetActive(false);
        declineBtn.gameObject.SetActive(false);

        UINaviHelper.Instance.SetFirstSelect();

        dialogueText.text = string.Empty;

        int value = AcceptEffect();

        string text = tm.GetCommons($"{currentEvent}Event_accept");

        switch (currentEvent)
        {
            case 0:
                text = string.Format(tm.GetCommons($"{currentEvent}Event_accept"), value);
                break;
            case 1:
                text = string.Format(tm.GetCommons($"{currentEvent}Event_accept"), value);
                break;
        }

        StartCoroutine(TextPrint(text, 1f, () =>
        {
            eventPanel.SetActive(false);
            UINaviHelper.Instance.SetFirstSelect();
        }));
    }

    private int AcceptEffect()
    {
        switch (currentEvent)
        {
            case 0:
                {
                    AudioManager.Instance.PlaySFX(audioClips[0]);
                    int max = Mathf.Min(10, GM.Instance.HasIngredient);
                    GM.Instance.IngredientSteal(max);
                    UIManager.Instance.UpdateIngredients();
                    return max;
                }
            case 1:
                {
                    AudioManager.Instance.PlaySFX(audioClips[2]);
                    int max = Mathf.Min(3000, GM.Instance.gold);
                    GM.Instance.AddGold(-1 * max, GM.GetGoldSource.upgrade);
                    friendshipFixed = 5f;
                    return -1 * max;
                }
            case 2:
                {
                    hasCat = true;
                    cat.SetActive(true);
                    AudioManager.Instance.PlaySFX(audioClips[4]);
                }
                break;
        }
        return int.MaxValue;
    }

    public void DeclineTrigger()
    {
        if (!tmpCompleted) return;

        acceptBtn.gameObject.SetActive(false);
        declineBtn.gameObject.SetActive(false);

        UINaviHelper.Instance.SetFirstSelect();

        dialogueText.text = string.Empty;

        DeclineEffect();

        StartCoroutine(TextPrint(tm.GetCommons($"{currentEvent}Event_decline"), 1f, () =>
        {
            eventPanel.SetActive(false);
            UINaviHelper.Instance.SetFirstSelect();
        }));
    }

    private void DeclineEffect()
    {
        switch (currentEvent)
        {
            case 0:
                AudioManager.Instance.PlaySFX(audioClips[1]);
                break;
            case 1:
                friendshipFixed = 1f;
                AudioManager.Instance.PlaySFX(audioClips[3]);
                break;
            case 2:
                AudioManager.Instance.PlaySFX(audioClips[5]);
                break;
        }
    }

    public void NextDay()
    {
        friendshipFixed = 0f;
    }

}
