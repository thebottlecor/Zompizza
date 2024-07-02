using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class ShopEnter : MonoBehaviour
{

    //public GameObject indicator;

    public static EventHandler PlayerArriveEvent;
    //public static EventHandler PlayerExitEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var player = GM.Instance.player;

            player.StopPlayer();

            //Vector3 dir = (transform.position - player.transform.position).normalized;
            //float dist = 0.5f;
            //Vector3 adjustPos = player.transform.position + dir * dist;
            //GM.Instance.player.transform.DOMove(adjustPos, 1f).SetUpdate(true);

            //indicator.SetActive(false);

            if (PlayerArriveEvent != null)
            {
                PlayerArriveEvent(this, null);
            }
        }
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Player"))
    //    {
    //        if (PlayerExitEvent != null)
    //        {
    //            PlayerExitEvent(this, null);
    //        }
    //    }
    //}

}
