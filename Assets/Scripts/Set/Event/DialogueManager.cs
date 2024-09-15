using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogueManager : Singleton<DialogueManager>
{

    public int currentEvent = -1;
    private int currentStep = 0;
    private bool[] triggeredEvents;

    public GameObject eventPanel;

    public Image profile;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    public Button nextBtn;

    public float typingSpeed = 0.05f;
    private bool tmpCompleted;

    public Sprite[] profileSprites;

    private int[] maxDialouge = new int[2] { 0, 1 };
    private int[][] profileIdx = new int[2][] { new int[] { 0, 0 }, new int[] { 1, 2, 3 } };

    private TextManager tm => TextManager.Instance;

    private void Start()
    {
        triggeredEvents = new bool[3];
        Init();
    }

    private void Update()
    {
        if (!tmpCompleted) return;
        if (!nextBtn.gameObject.activeSelf) return;

        if (Input.anyKeyDown)
        {
            AcceptTrigger();
        }
    }

    IEnumerator TextPrint(string text, float delay, Action finishAction)
    {
        tmpCompleted = false;
        int count = 0;
        int typingCount = 0;

        while (count != text.Length)
        {
            if (count < text.Length)
            {
                if (text[count].Equals('<'))
                {
                    int index = text.IndexOf('>', count);
                    if (index >= 0)
                    {
                        int length = index - count + 1;
                        dialogueText.text += text.Substring(count, length);
                        count += length;
                    }
                }
                else
                {
                    dialogueText.text += text[count].ToString();
                    count++;
                    typingCount++;

                    if (typingCount % 4 == 0)
                        AudioManager.Instance.PlaySFX(Sfx.inputFieldStart);
                }
            }

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
        currentStep = 0;
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

        //AudioManager.Instance.PlaySFX(Sfx.newInfo);
        UINaviHelper.Instance.SetFirstSelect();

        currentEvent = idx;
        currentStep = 0;

        //profile.sprite = profileSprites[idx];
        profile.sprite = profileSprites[profileIdx[idx][0]];

        nameText.text = tm.GetCommons($"{idx}Dialogue_name");
        dialogueText.text = string.Empty;

        StartCoroutine(TextPrint(tm.GetCommons($"{idx}Dialogue_text0"), 0.5f, () =>
        {
            nextBtn.gameObject.SetActive(true);
            UINaviHelper.Instance.SetFirstSelect();
        }));
    }

    public void AcceptTrigger()
    {
        if (!tmpCompleted) return;
        if (!nextBtn.gameObject.activeSelf) return;

        nextBtn.gameObject.SetActive(false);
        UINaviHelper.Instance.SetFirstSelect();

        dialogueText.text = string.Empty;

        string text = tm.GetCommons($"{currentEvent}Dialogue_text{currentStep + 1}");
        currentStep++;
        profile.sprite = profileSprites[profileIdx[currentEvent][currentStep]];
        if (currentStep > maxDialouge[currentEvent])
        {
            StartCoroutine(TextPrint(text, 1.5f, () =>
            {
                eventPanel.SetActive(false);
                GM.Instance.stop_control = false;
                Time.timeScale = 1f;

                switch (currentEvent)
                {
                    case 0:
                        TutorialManager.Instance.Step2_Before();
                        break;
                    case 1:
                        TutorialManager.Instance.Step2();
                        break;
                }

                UINaviHelper.Instance.SetFirstSelect();
            }));
        }
        else
        {
            StartCoroutine(TextPrint(text, 0.5f, () =>
            {
                nextBtn.gameObject.SetActive(true);
                UINaviHelper.Instance.SetFirstSelect();
            }));
        }
    }

}
