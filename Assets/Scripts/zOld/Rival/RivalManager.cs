using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using DG.Tweening;

public class RivalManager : Singleton<RivalManager>
{

    public GameObject rankingObj;
    public RectTransform rankingRect;

    public float[] rating;

    public TextMeshProUGUI weeklyRankingTMP;
    public RankingObj[] rankingObjs;

    public Sprite[] medals;
    public Sprite[] icons;

    private bool playerWin;


    public struct RankingInfo
    {
        public int idx;
        public float rating;
    }

    public void Init()
    {
        weeklyRankingTMP.text = TextManager.Instance.GetCommons("WeeklyChoice");
        rating = new float[2];
    }

    public bool NextDay()
    {
        rating[0] += UnityEngine.Random.Range(6, 17) * 0.5f;
        rating[1] += UnityEngine.Random.Range(6, 17) * 0.5f;

        int day = GM.Instance.day; // ���̹� ����

        bool rivalDay = day == 6 || day == 13 || day == 21 || day == 28;

        if (rivalDay)
        {
            if (day >= 21) // ���� �������� ���
            {
                rating[1] = 0f;
            }
            if (day >= 28) // ������� ����ġŲ ���
            {
                rating[0] = 0f;
            }

            SetRanking();
            return true;
        }
        return false;
    }

    public void SetRanking()
    {
        playerWin = false;
        float player = GM.Instance.rating > 0 ? GM.Instance.rating : 0f;

        List<RankingInfo> rankings = new List<RankingInfo>(3)
        {
            new RankingInfo { idx = 2, rating = player },
            new RankingInfo { idx = 0, rating = rating[0] },
            new RankingInfo { idx = 1, rating = rating[1] }
        };

        float total = player + rating[0] + rating[1];
        if (total < 1f) total = 1f;

        var result = rankings.OrderByDescending(i => i.rating);

        int ranking = 0;
        foreach (var i in result)
        {
            if (ranking == 0 && i.idx == 2) playerWin = true;
            float percent = i.rating / total;
            if (i.rating <= 0f) percent = 0f;
            rankingObjs[ranking].Init(i.idx, ranking, i.rating, percent);
            ranking++;
        }
    }

    public void ShowRankingPanel()
    {
        rankingObj.SetActive(true);

        //AudioManager.Instance.PlaySFX(Sfx.newInfo);
        AudioManager.Instance.PlaySFX(GameEventManager.Instance.audioClips[2]);

        rankingRect.localScale = 0.01f * Vector3.one;
        rankingRect.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.OutElastic).SetUpdate(true);

        if (playerWin)
        {
            AudioManager.Instance.PlaySFX(Sfx.pizzaComplete);
            UIManager.Instance.shopUI.upgradeDirection.Show(1);
        }

        UINaviHelper.Instance.SetFirstSelect();
    }
    public void HideRankingPanel()
    {
        rankingObj.SetActive(false);
        GM.Instance.ShowWarningQueue();
    }
}
