using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class OrderGoal : MonoBehaviour
{

    public int index;
    public GameObject goalEffectObj;
    public GameObject successEffectObj;

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

    public void Hide()
    {
        gameObject.SetActive(false);
        goalEffectObj.SetActive(false);
        successEffectObj.SetActive(false);
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

    public void SuccessEffect()
    {
        EffectUpdate(false);
        successEffectObj.SetActive(true);
        successEffectObj.transform.DOShakePosition(5f, 0.01f).OnComplete(() =>
        {
            Hide();
        });
    }

}
