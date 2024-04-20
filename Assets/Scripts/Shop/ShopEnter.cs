using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class ShopEnter : MonoBehaviour
{

    public GameObject indicator;

    public static EventHandler PlayerArriveEvent;
    public static EventHandler PlayerExitEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.transform.parent.GetComponent<PlayerController>().StopPlayer();
            indicator.SetActive(false);

            if (PlayerArriveEvent != null)
            {
                PlayerArriveEvent(this, null);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (PlayerExitEvent != null)
            {
                PlayerExitEvent(this, null);
            }
        }
    }

}
