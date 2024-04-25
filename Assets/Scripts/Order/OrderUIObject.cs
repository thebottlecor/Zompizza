using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderUIObject : MonoBehaviour
{

    public Image customer_profile;
    public TextMeshProUGUI customer_name;

    public TextMeshProUGUI order_detail;

    private OrderInfo info;

    public void UIUpdate(OrderInfo info)
    {
        this.info = info;

        customer_profile.sprite = DataManager.Instance.uiLibrary.customerProfile[info.customerIdx];
        customer_name.text = "Mike";

        order_detail.text = "detail";
    }

    public void OrderAccept()
    {
        info.accepted = true;

        gameObject.SetActive(false);
        info = null;

        OrderManager.Instance.OrderGoalUpdate();
    }

}
