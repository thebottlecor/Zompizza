using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopExiter : MonoBehaviour
{
    public static EventHandler PlayerExitEvent;

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
