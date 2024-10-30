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
    public Image customer_profile_bg;
    public TextMeshProUGUI customer_name;

    public TextMeshProUGUI order_detail;

    public Button acceptButton;
    public TextMeshProUGUI buttonTmp;

    public TextMeshProUGUI viewLocationTmp;

    public GameObject[] friendshipIcons;

    private OrderInfo info;

    public UINavi navi;
    public UINavi navi_ViewPos;

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
        customer_name.text = tm.GetSurvivorName(info.customerIdx + Constant.npcNameOffset);

        StringBuilder st = new StringBuilder();
        st.AppendFormat("<color=#002a8e><size=110%>{0}</size></color>\n", tm.GetSurvivorName(info.customerIdx + Constant.npcNameOffset));
        st.AppendFormat("{0} : {1:0.#}km ({2})\n", tm.GetCommons("Distance"), info.km, tm.GetCommons(OrderManager.Instance.orderGoals[info.goal].compassDir.ToString()));

        //float averageRating = OrderManager.Instance.customersInfos[info.goal].AverageRating();

        //if (GameEventManager.Instance.friendshipFixed > 0f) averageRating = GameEventManager.Instance.friendshipFixed;

        //if (averageRating >= Constant.friendShip3)
        //{
        //    friendshipIcons[0].SetActive(true);
        //    friendshipIcons[1].SetActive(true);
        //    friendshipIcons[2].SetActive(true);
        //}
        //else if (averageRating >= Constant.friendShip2)
        //{
        //    friendshipIcons[0].SetActive(true);
        //    friendshipIcons[1].SetActive(true);
        //    friendshipIcons[2].SetActive(false);
        //}
        //else if (averageRating >= Constant.friendShip1)
        //{
        //    friendshipIcons[0].SetActive(true);
        //    friendshipIcons[1].SetActive(false);
        //    friendshipIcons[2].SetActive(false);
        //}
        //else
        //{
        //    friendshipIcons[0].SetActive(false);
        //    friendshipIcons[1].SetActive(false);
        //    friendshipIcons[2].SetActive(false);
        //}

        // 이벤트성
        if (GameEventManager.Instance.friendshipFixed > 4f)
        {
            friendshipIcons[0].SetActive(true);
            friendshipIcons[1].SetActive(true);
            friendshipIcons[2].SetActive(true);
        }
        else
        {
            friendshipIcons[0].SetActive(false);
            friendshipIcons[1].SetActive(false);
            friendshipIcons[2].SetActive(false);
        }

        //if (info.bouns_friendship > 0)
        //    st.AppendFormat("{0} : {1}<size=90%><color=#550742>(+{2})</color></size>G", tm.GetCommons("Rewards"), info.rewards - info.bouns_friendship, info.bouns_friendship);
        //else
        //    st.AppendFormat("{0} : {1}G", tm.GetCommons("Rewards"), info.rewards);
        //st.AppendLine();



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

        /// 피자 재료 설명하는 부분
        //string subStr = tm.GetCommons("OrderDetail_Sub");
        //string subStr2 = tm.GetCommons("OrderDetail_Sub2");

        //for (int i = 0; i < info.pizzas.Count; i++)
        //{
        //    ///string pizza = string.Format("{0} {1}{2}", tm.GetCommons("Pizza"), info.pizzas[i].stack, string.IsNullOrEmpty(subStr) ? string.Empty : subStr);
        //    StringBuilder ele = new StringBuilder();
        //    int count = info.pizzas[i].ingredients.Count;
        //    foreach (var element in info.pizzas[i].ingredients)
        //    {
        //        ///ele.AppendFormat("<color=#002a8e><sprite={0}>{1} {2}</color>{3}", (int)element.Key + ingredientSpriteOffset, tm.GetIngredient(element.Key), element.Value, string.IsNullOrEmpty(subStr) ? string.Empty : subStr);
        //        ele.Append("<color=#002a8e>");
        //        ele.AppendFormat(subStr, (int)element.Key + Constant.ingredientSpriteOffset, tm.GetIngredient(element.Key), element.Value);
        //        count--;
        //        if (count > 0)
        //            ele.AppendFormat("{0}", string.IsNullOrEmpty(subStr2) ? ", " : subStr2);
        //    }
        //    st.AppendFormat(tm.GetCommons("OrderDetail"), ele, tm.GetCommons("Pizza"));

        //    if (i + 1 < info.pizzas.Count)
        //        st.Append("\n");
        //}
        /// 사용 안함
        /// 
        /// 대신 콤보를 표시
        ///

        int combo = info.comboSpecial;
        if (combo > 0)
            st.AppendFormat("<sprite=\"emoji\" index=10> {0} x{1:F1}", tm.GetCommons($"ComboSpecial{combo}"), OrderManager.Instance.ovenMiniGame.specialComboBonus[combo]);

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

        bool makable = true;
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
        //if (!OrderManager.Instance.CheckIngredient(info)) return;

        info.accepted = true;
        OrderManager.Instance.OrderAccepted(info);
        OrderReset();
        OrderManager.Instance.OrderGoalUpdate();

        AudioManager.Instance.PlaySFX(Sfx.okay);

        UIManager.Instance.shopUI.SnapTo(null);
        UINaviHelper.Instance.SetFirstSelect();
    }
    public bool OrderAcceptable()
    {
        return info != null && !info.accepted;
    }

    public void OrderReset()
    {
        gameObject.SetActive(false);
        ToggleHighlight(false);
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

        OrderManager.Instance.ovenMiniGame.SetHighlight(info);

        //for (int i = 0; i < info.pizzas.Count; i++)
        //{
        //    foreach (var item in info.pizzas[i].ingredients)
        //    {
        //        UIManager.Instance.ingredientUIPairs[item.Key].ToggleHighlight(true);
        //    }
        //}
        ToggleHighlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (info == null) return;

        OrderManager.Instance.ovenMiniGame.SetHighlight(info, false);

        //for (int i = 0; i < info.pizzas.Count; i++)
        //{
        //    foreach (var item in info.pizzas[i].ingredients)
        //    {
        //        UIManager.Instance.ingredientUIPairs[item.Key].ToggleHighlight(false);
        //    }
        //}
        ToggleHighlight(false);
    }

    public void ToggleHighlight(bool on)
    {
        if (on)
        {
            customer_profile_bg.color = DataManager.Instance.uiLib.order_select_Color;
        }
        else
        {
            customer_profile_bg.color = DataManager.Instance.uiLib.order_unselect_Color;
        }
    }
}
