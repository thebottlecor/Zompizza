using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class ShopEnter : MonoBehaviour
{

    public static EventHandler PlayerArriveEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.transform.parent.GetComponent<PlayerController>().StopPlayer();

            if (PlayerArriveEvent != null)
            {
                PlayerArriveEvent(this, null);
            }
        }
    }

}
