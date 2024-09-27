using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PizzaAreaEnter : MonoBehaviour
{

    public static EventHandler PlayerArriveEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GM.Instance.pizzeriaStay = true;
            UIManager.Instance.installUIs.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GM.Instance.pizzeriaStay = false;
            UIManager.Instance.installUIs.SetActive(true);
        }
    }
}
