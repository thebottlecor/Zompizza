using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEnterTrigger : MonoBehaviour
{

    public int idx;
    public static EventHandler<int> PlayerArriveEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (PlayerArriveEvent != null)
            {
                PlayerArriveEvent(this, idx);
            }
            Hide();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
