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

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        weeklyRankingTMP.text = TextManager.Instance.GetCommons("WeeklyChoice");
        rating = new float[2];
    }

    public bool NextDay()
    {
        rating[0] += UnityEngine.Random.Range(10, 21) * 0.5f;
        rating[1] += UnityEngine.Random.Range(10, 21) * 0.5f;

        if ((GM.Instance.day + 1) % 7 == 0)
        {
            SetRanking();
            return true;
        }
        return false;
    }

    public void SetRanking()
    {
        playerWin = false;
        float player = GM.Instance.rating > 0 ? GM.Instance.rating : 0f;

        List<RankingInfo> rankings = new List<RankingInfo>(3);

        rankings.Add(new RankingInfo { idx = 2, rating = player });
        rankings.Add(new RankingInfo { idx = 0, rating = rating[0] });
        rankings.Add(new RankingInfo { idx = 1, rating = rating[1] });

        float total = player + rating[0] + rating[1];

        var result = rankings.OrderByDescending(i => i.rating);

        int ranking = 0;
        foreach (var i in result)
        {
            if (ranking == 0 && i.idx == 2) playerWin = true;
            rankingObjs[ranking].Init(i.idx, ranking, i.rating, i.rating / total);
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
