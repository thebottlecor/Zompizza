using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using UnityEngine.EventSystems;

public class OrderUIObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public Image customer_profile;
    public TextMeshProUGUI customer_name;

    public TextMeshProUGUI order_detail;

    public Button acceptButton;
    public TextMeshProUGUI buttonTmp;

    public TextMeshProUGUI viewLocationTmp;

    private OrderInfo info;

    private TextManager tm => TextManager.Instance;

    private void Start()
    {
        acceptButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ButtonSound();
        });
    }

    public void UIUpdate(OrderInfo info)
    {
        this.info = info;

        customer_profile.sprite = DataManager.Instance.uiLib.customerProfile[info.customerIdx];
        customer_name.text = tm.GetNames(info.customerIdx + Constant.npcNameOffset);

        StringBuilder st = new StringBuilder();
        st.AppendFormat("{0} : {1:0.#}km", tm.GetCommons("Distance"), info.km);
        //st.AppendFormat("{0:0.##}", info.distance);
        st.AppendLine();
        st.AppendFormat("{0} : {1}$", tm.GetCommons("Rewards"), info.rewards);
        st.AppendLine();

        // 글자로 풀어서 표시
        //for (int i = 0; i < info.pizzas.Count; i++)
        //{
        //    string subStr = tm.GetCommons("OrderDetail_Sub");

        //    string pizza = string.Format("<color=#002a8e><sprite=0>{0} {1}</color>{2}", tm.GetCommons("Pizza"), info.pizzas[i].stack, string.IsNullOrEmpty(subStr) ? string.Empty : subStr);
        //    StringBuilder ele = new StringBuilder();
        //    int count = info.pizzas[i].ingredients.Count;
        //    foreach (var element in info.pizzas[i].ingredients)
        //    {
        //        ele.AppendFormat("<color=#002a8e><sprite={0}>{1} {2}</color>{3}", (int)element.Key + ingredientSpriteOffset, tm.GetIngredient(element.Key), element.Value, string.IsNullOrEmpty(subStr) ? string.Empty : subStr);
        //        count--;
        //        if (count > 0)
        //            ele.Append(", ");
        //    }
        //    st.AppendFormat(tm.GetCommons("OrderDetail"), ele, pizza);

        //    if (i + 1 < info.pizzas.Count)
        //        st.Append("\n");
        //}

        string subStr = tm.GetCommons("OrderDetail_Sub");
        string subStr2 = tm.GetCommons("OrderDetail_Sub2");

        for (int i = 0; i < info.pizzas.Count; i++)
        {
            //string pizza = string.Format("{0} {1}{2}", tm.GetCommons("Pizza"), info.pizzas[i].stack, string.IsNullOrEmpty(subStr) ? string.Empty : subStr);
            StringBuilder ele = new StringBuilder();
            int count = info.pizzas[i].ingredients.Count;
            foreach (var element in info.pizzas[i].ingredients)
            {
                //ele.AppendFormat("<color=#002a8e><sprite={0}>{1} {2}</color>{3}", (int)element.Key + ingredientSpriteOffset, tm.GetIngredient(element.Key), element.Value, string.IsNullOrEmpty(subStr) ? string.Empty : subStr);
                ele.Append("<color=#002a8e>");
                ele.AppendFormat(subStr, (int)element.Key + Constant.ingredientSpriteOffset, tm.GetIngredient(element.Key), element.Value);
                count--;
                if (count > 0)
                    ele.AppendFormat("{0}", string.IsNullOrEmpty(subStr2) ? ", " : subStr2);
            }
            st.AppendFormat(tm.GetCommons("OrderDetail"), ele, tm.GetCommons("Pizza"));

            if (i + 1 < info.pizzas.Count)
                st.Append("\n");
        }

        // 아이콘으로만 표시
        //for (int i = 0; i < info.pizzas.Count; i++)
        //{
        //    st.AppendFormat("<sprite=0> {0} :", info.pizzas[i].stack);
        //    foreach (var element in info.pizzas[i].ingredients)
        //    {
        //        st.AppendFormat(" <sprite={0}> {1}", (int)element.Key + ingredientSpriteOffset, element.Value);
        //    }
        //    if (i + 1 < info.pizzas.Count)
        //        st.Append("\n");
        //}

        order_detail.text = st.ToString();

        viewLocationTmp.text = tm.GetCommons("ViewCustomer");
    }

    public void ButtonUpdate()
    {
        if (info == null || info.accepted) return;

        bool makable = OrderManager.Instance.CheckIngredient(info);
        bool loadLimit = false;

        if (OrderManager.Instance.IsMaxDelivery)
        {
            makable = false;
            loadLimit = true;
        }

        acceptButton.interactable = makable;
        if (makable)
            buttonTmp.text = tm.GetCommons("Accept");
        else
        {   
            if (loadLimit)
                buttonTmp.text = tm.GetCommons("AcceptDisable2");
            else
                buttonTmp.text = tm.GetCommons("AcceptDisable");
        }
    }

    public void OrderAccept()
    {
        if (OrderManager.Instance.IsMaxDelivery) return;

        info.accepted = true;

        OrderManager.Instance.OrderAccepted(info);

        OrderReset();

        OrderManager.Instance.OrderGoalUpdate();
    }

    public void OrderReset()
    {
        gameObject.SetActive(false);
        info = null;
    }

    public void ViewLocation()
    {
        if (info == null) return;

        Vector3 pos = OrderManager.Instance.orderGoals[info.goal].transform.position;

        WorldMapManager.Instance.ToggleCustomerMode(true, pos, info.customerIdx);
        UIManager.Instance.utilUI.OpenWorldMap();
        UIManager.Instance.ButtonSound();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (info == null) return;
        if (UIManager.Instance.isDirecting) return;

        for (int i = 0; i < info.pizzas.Count; i++)
        {
            foreach (var item in info.pizzas[i].ingredients)
            {
                UIManager.Instance.ingredientUIPairs[item.Key].ToggleHighlight(true);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (info == null) return;

        for (int i = 0; i < info.pizzas.Count; i++)
        {
            foreach (var item in info.pizzas[i].ingredients)
            {
                UIManager.Instance.ingredientUIPairs[item.Key].ToggleHighlight(false);
            }
        }
    }
}
