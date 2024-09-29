using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using MTAssets.EasyMinimapSystem;
using UnityEngine.UI;
using TMPro;

public class OrderGoal : MonoBehaviour
{

    public int index;
    public GameObject goalEffectObj;
    public MinimapItem minimapItem;
    public MinimapItem minimapItem_customer;

    public Canvas textCanvas;
    public TextMeshProUGUI goldText;

    private Target target;

    public GameObject[] giftDummy;
    public GiftGoal[] giftGoals;


    public static EventHandler<int> PlayerArriveEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (PlayerArriveEvent != null)
            {
                PlayerArriveEvent(this, index);

            }
        }
    }

    public void Init(int idx)
    {
        index = idx;

        if (DataManager.Instance != null)
            minimapItem.spriteColor = DataManager.Instance.uiLib.customerPinColor[idx];

        target = GetComponent<Target>();
        target.targetColor = minimapItem.spriteColor;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        goalEffectObj.SetActive(false);
    }
    public void Show()
    {
        gameObject.SetActive(true);
        goalEffectObj.SetActive(true);
    }

    public void EffectUpdate(bool on)
    {
        goalEffectObj.SetActive(on);
    }

    public void SuccessEffect(int gold, float rating)
    {
        textCanvas.gameObject.SetActive(true);
        goldText.text = $"+{gold}G";
        CoroutineHelper.StartCoroutine(HideText());
        CoroutineHelper.StartCoroutine(GiftBoxEffect());

        EffectUpdate(false);
        AudioManager.Instance.PlaySFX(Sfx.money);
        AudioManager.Instance.PlaySFX(Sfx.complete);
        var source = DataManager.Instance.effectLib.dollarBoomEffect;
        Vector3 pos = this.transform.position;
        pos.y += 4f;
        var obj = Instantiate(source, pos, Quaternion.identity);
        Destroy(obj, 5f);
        Hide();
    }

    private IEnumerator HideText()
    {
        yield return CoroutineHelper.WaitForSeconds(1.9f);

        textCanvas.gameObject.SetActive(false);
    }

    private IEnumerator GiftBoxEffect()
    {
        int min = Mathf.Min(giftGoals.Length + 1, 3 + 1);
        int rand = UnityEngine.Random.Range(1, min);
        giftGoals.Shuffle();

        Sequence sequence = DOTween.Sequence().SetUpdate(false).SetAutoKill(true);
        sequence.AppendCallback(() =>
        {
            for (int i = 0; i < rand; i++)
            {
                giftDummy[i].SetActive(true);
                Vector3 pos = transform.position;
                pos.y += 1f;
                giftDummy[i].transform.position = pos;

                giftDummy[i].transform.DOMoveX(giftGoals[i].transform.position.x, 0.75f).SetEase(Ease.InQuad);
                giftDummy[i].transform.DOMoveZ(giftGoals[i].transform.position.z, 0.75f).SetEase(Ease.InQuad);

                giftDummy[i].transform.DOMoveY(5f, 0.375f).SetEase(Ease.OutQuad);
            }
        });
        sequence.AppendInterval(0.375f);
        sequence.AppendCallback(() =>
        {
            for (int i = 0; i < rand; i++)
            {
                giftDummy[i].transform.DOMoveY(1f, 0.375f).SetEase(Ease.InQuad);
            }
        });
        sequence.AppendInterval(0.375f);
        sequence.AppendCallback(() =>
        {
            for (int i = 0; i < giftDummy.Length; i++)
            {
                giftDummy[i].SetActive(false);
            }
        });

        yield return CoroutineHelper.WaitForSeconds(0.76f);

        for (int i = 0; i < rand; i++)
        {
            giftGoals[i].Show();
        }
    }

    [ContextMenu("거리 표시")]
    private void ShowDist()
    {
        var gm = FindObjectOfType<GM>();
        float dist = (transform.position - gm.pizzeriaPos.position).magnitude;
        float km = dist * Constant.distanceScale; // 게임상 거리 200 = 1km

        float timeLimit = (Constant.delivery_timeLimit_1km * km) + Constant.delivery_timeLimit_base;

        Debug.Log($" 거리 : {km:0.##}km\n                     시간 : {timeLimit:F0}s");
    }
}
