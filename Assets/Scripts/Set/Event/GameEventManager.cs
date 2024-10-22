using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameEventManager : Singleton<GameEventManager>
{
    [Serializable]
    public struct SaveData
    {
        public float friendshipFixed;
        public bool hasCat;
        public bool ninjaPriceUp;
        public bool fatherOffering;
        public bool mysteryAid;
        public int humanityPoint; 
    }
    public SaveData Save()
    {
        SaveData data = new()
        {
            friendshipFixed = this.friendshipFixed,
            hasCat = this.hasCat,
            ninjaPriceUp = this.ninjaPriceUp,
            fatherOffering = this.fatherOffering,
            mysteryAid = this.mysteryAid,
            humanityPoint = this.humanityPoint,
        };
        return data;
    }
    public void Load(SaveData data)
    {
        friendshipFixed = data.friendshipFixed;

        hasCat = data.hasCat;
        cat.SetActive(hasCat);

        ninjaPriceUp = data.ninjaPriceUp;
        fatherOffering = data.fatherOffering;
        mysteryAid = data.mysteryAid;
        humanityPoint = data.humanityPoint;
    }

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

    // 이벤트에 의한 일회성 호감도 고정
    public float friendshipFixed; 
    public bool hasCat; 
    public bool ninjaPriceUp; 
    public bool fatherOffering;
    public bool mysteryAid;
    public int humanityPoint;  // 최대 인류애점수 8점

    public GameObject cat;

    public AudioClip[] audioClips;

    public Sprite[] profileSprites;

    private TextManager tm => TextManager.Instance;

    public void Init()
    {
        triggeredEvents = new bool[9];
        currentEvent = -1;
        tmpCompleted = false;

        acceptBtnText.text = tm.GetCommons("EventAccept");
        declineBtnText.text = tm.GetCommons("EventDecline");

        cat.SetActive(false);
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

    public bool SetEvent(int idx)
    {
        if (triggeredEvents[idx]) return false;
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

        return true;
    }

    public void AcceptTrigger()
    {
        if (!tmpCompleted) return;

        acceptBtn.gameObject.SetActive(false);
        declineBtn.gameObject.SetActive(false);

        UINaviHelper.Instance.SetFirstSelect();

        dialogueText.text = string.Empty;

        int value = AcceptEffect();

        string text;
        switch (currentEvent)
        {
            case 0:
            case 1:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
                text = string.Format(tm.GetCommons($"{currentEvent}Event_accept"), value);
                break;
            default:
                text = tm.GetCommons($"{currentEvent}Event_accept");
                break;
        }

        StartCoroutine(TextPrint(text, 1f, () =>
        {
            EventHide();
        }));
    }

    private int AcceptEffect()
    {
        switch (currentEvent)
        {
            case 0:
                return Event_Grandma(1);
            case 1:
                return Event_Oldman(1000);
            case 2:
                {
                    hasCat = true;
                    cat.SetActive(true);
                    humanityPoint++; // 고양이 입양
                    AudioManager.Instance.PlaySFX(audioClips[4]);
                }
                break;
            case 3:
                {
                    ninjaPriceUp = true;
                    GM.Instance.AddRating(-5f, GM.GetRatingSource.notComplete); // 첫번째 치킨 닌자
                    AudioManager.Instance.PlaySFX(audioClips[3]);
                    return 10;
                }
            case 4:
                return Event_Grandma(5);
            case 5:
                return Event_Oldman(3000);
            case 6:
                {
                    fatherOffering = true;
                    humanityPoint++; // 신부님 기부
                    AudioManager.Instance.PlaySFX(audioClips[2]);
                    return 500;
                }
            case 7:
                {
                    int money = 10000;
                    GM.Instance.AddGold(money, GM.GetGoldSource.delivery); // 두번째 치킨 닌자
                    GM.Instance.AddRating(-10f, GM.GetRatingSource.notComplete); // 두번째 치킨 닌자
                    AudioManager.Instance.PlaySFX(audioClips[3]);
                    return money;
                }
            case 8:
                {
                    mysteryAid = true;
                    return 10 + humanityPoint * 5;
                }
        }
        return int.MaxValue;
    }

    private int Event_Oldman(int money)
    {
        AudioManager.Instance.PlaySFX(audioClips[2]);
        int max = Mathf.Min(money, GM.Instance.gold);
        GM.Instance.AddGold(-1 * max, GM.GetGoldSource.villager);
        friendshipFixed = 5f;
        humanityPoint++; // 첫번째 기부
        return -1 * max;
    }

    private int Event_Grandma(int ingredient)
    {
        AudioManager.Instance.PlaySFX(audioClips[0]);
        int max = Mathf.Min(ingredient, GM.Instance.HasIngredient);
        GM.Instance.IngredientSteal(max);
        UIManager.Instance.UpdateIngredients();
        UIManager.Instance.OrderUIBtnUpdate();
        humanityPoint++; // 첫번째 고아원
        return max;
    }

    public void DeclineTrigger()
    {
        if (!tmpCompleted) return;

        acceptBtn.gameObject.SetActive(false);
        declineBtn.gameObject.SetActive(false);

        UINaviHelper.Instance.SetFirstSelect();

        dialogueText.text = string.Empty;

        int value = DeclineEffect();

        string text;
        switch (currentEvent)
        {
            case 3:
            case 8:
                text = string.Format(tm.GetCommons($"{currentEvent}Event_decline"), value);
                break;
            default:
                text = tm.GetCommons($"{currentEvent}Event_decline");
                break;
        }

        StartCoroutine(TextPrint(text, 1f, () =>
        {
            EventHide();
        }));
    }

    public void EventHide()
    {
        eventPanel.SetActive(false);
        UINaviHelper.Instance.SetFirstSelect();

        VillagerManager.Instance.CreateSOS();
        TutorialManager.Instance.NoMoreEvented();
    }

    private int DeclineEffect()
    {
        switch (currentEvent)
        {
            case 0:
            case 4:
                AudioManager.Instance.PlaySFX(audioClips[1]);
                break;
            case 1:
            case 5:
                friendshipFixed = 1f;
                AudioManager.Instance.PlaySFX(audioClips[3]);
                break;
            case 2:
                AudioManager.Instance.PlaySFX(audioClips[5]);
                break;
            case 3:
                GM.Instance.AddRating(10f, GM.GetRatingSource.notComplete); // 첫번째 치킨 닌자 - 거절
                humanityPoint++; // 첫번째 치킨 닌자 - 거절
                AudioManager.Instance.PlaySFX(audioClips[2]);
                return 10;
            case 6:
                AudioManager.Instance.PlaySFX(audioClips[3]);
                break;
            case 7:
                humanityPoint++; // 두번째 치킨 닌자 - 거절
                AudioManager.Instance.PlaySFX(audioClips[2]);
                break;
            case 8:
                {
                    mysteryAid = true;
                    return 10 + humanityPoint * 5;
                }
        }
        return int.MaxValue;
    }

    public void NextDay()
    {
        friendshipFixed = 0f;
    }
    public void OfferingDaily()
    {
        if (fatherOffering)
        {
            int value = Mathf.Min(500, GM.Instance.gold);
            GM.Instance.AddGold(-1 * value, GM.GetGoldSource.villager); // 신부님 매일 기부
        }
    }
    public float MysteryAidEffect()
    {
        if (mysteryAid)
        {
            float baseValue = 0.1f;
            baseValue += humanityPoint * 0.05f;
            return (1f - baseValue);
        }
        return 1f;
    }

}
