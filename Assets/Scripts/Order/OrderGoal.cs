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
        goldText.text = $"+{gold}$";
        CoroutineHelper.StartCoroutine(HideText());

        EffectUpdate(false);
        AudioManager.Instance.PlaySFX(Sfx.money);
        AudioManager.Instance.PlaySFX(Sfx.complete);
        var source = DataManager.Instance.effectLib.dollarBoomEffect;
        Vector3 pos = this.transform.position;
        pos.y = 4f;
        var obj = Instantiate(source, pos, Quaternion.identity);
        Destroy(obj, 5f);
        Hide();
    }

    private IEnumerator HideText()
    {
        yield return CoroutineHelper.WaitForSeconds(1.9f);

        textCanvas.gameObject.SetActive(false);
    }

}
