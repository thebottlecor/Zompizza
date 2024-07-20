using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogueManager : Singleton<DialogueManager>
{

    public int currentEvent = -1;
    private bool[] triggeredEvents;

    public GameObject eventPanel;

    public Image profile;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    public Button nextBtn;

    public float typingSpeed = 0.05f;
    private bool tmpCompleted;

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
    }

    public void SetDialogue(int idx)
    {
        if (triggeredEvents[idx]) return;
        triggeredEvents[idx] = true;

        nextBtn.gameObject.SetActive(false);

        eventPanel.SetActive(true);
        GM.Instance.stop_control = true;
        Time.timeScale = 0f;

        AudioManager.Instance.PlaySFX(Sfx.newInfo);
        UINaviHelper.Instance.SetFirstSelect();

        currentEvent = idx;

        profile.sprite = profileSprites[idx];

        nameText.text = tm.GetCommons($"{idx}Event_name");
        dialogueText.text = string.Empty;

        StartCoroutine(TextPrint(tm.GetCommons($"{idx}Event_dialogue"), 0.25f, () =>
        {
            nextBtn.gameObject.SetActive(true);
            UINaviHelper.Instance.SetFirstSelect();
        }));
    }

    public void AcceptTrigger()
    {
        if (!tmpCompleted) return;

        nextBtn.gameObject.SetActive(false);

        UINaviHelper.Instance.SetFirstSelect();

        dialogueText.text = string.Empty;

        string text = tm.GetCommons($"{currentEvent}Event_accept");

        StartCoroutine(TextPrint(text, 1f, () =>
        {
            eventPanel.SetActive(false);
            GM.Instance.stop_control = false;
            Time.timeScale = 1f;

            switch (currentEvent)
            {
                case 0:
                    TutorialManager.Instance.Step2();
                    break;
            }

            UINaviHelper.Instance.SetFirstSelect();
        }));
    }

}
