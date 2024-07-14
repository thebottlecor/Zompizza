using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;

public class UINaviHelper_Ingame : MonoBehaviour
{
    [Header("인게임 - 옵션 저장값")]
    public UINavi utils_map_first;
    public UINavi[] utils_maps;

    public UINavi utils_close;

    [Header("인게임 - 가게 저장값")]
    public UINavi[] shops_first;
    public UINavi[] shops_orders_explores;
    public UINavi[] shops_managements;
    public UINavi[] shops_upgrades;
    public List<UINavi> shops_vehicles;
    public UINavi shops_closeStore; // 영업 종료전 직접 닫는 버튼
    public UINavi shops_close;

    public RectTransform virtualCursor;
    public TextMeshProUGUI[] virtualCursorTMP;

    [Header("인게임 - 특별")]
    public UINavi explore_first;
    public UINavi raid_first;
    public UINavi ranking_first;
    public UINavi nextDay_first;
    public UINavi shopCloseWarning_first;
    public UINavi gameOverWarning_first;

    public UINavi gameOver_first;
    public UINavi gameWin_first;

    public UINavi gameEvent_accept;
    public UINavi gameEvent_decline;

    [Header("항상 보여지는 인디케이터")]
    public PadKeyIndicator[] alwaysShow_PadUIs;

    private void Start()
    {
        if (UINaviHelper.Instance != null)
        {
            UINaviHelper.Instance.ingame = this;
            UINaviHelper.Instance.inputHelper.PadCheck();
        }
        virtualCursorTMP[0].text = TextManager.Instance.GetCommons("VirtualCursor0");
        virtualCursorTMP[1].text = TextManager.Instance.GetCommons("VirtualCursor2");

        virtualCursorTMP[2].text = TextManager.Instance.GetCommons("VirtualCursor5");
        virtualCursorTMP[3].text = TextManager.Instance.GetCommons("VirtualCursor1");

        virtualCursorTMP[4].text = TextManager.Instance.GetCommons("VirtualCursor6");
        virtualCursorTMP[5].text = TextManager.Instance.GetCommons("VirtualCursor0");
    }

    public UINavi Utils_Map_Reconnection()
    {
        utils_close.ResetConnection();
        utils_maps[0].ResetConnection();
        utils_maps[1].ResetConnection();

        if (WorldMapManager.Instance.customerBtn.gameObject.activeSelf)
        {
            if (WorldMapManager.Instance.customerMode) // 주문으로 버튼 없음
            {
                utils_maps[0].left = utils_maps[1];
                utils_maps[0].right = utils_maps[1];

                utils_maps[1].left = utils_maps[0];
                utils_maps[1].right = utils_maps[0];
                utils_maps[1].up = utils_close;
                utils_maps[1].down = utils_close;

                utils_close.left = utils_maps[0];
                utils_close.right = utils_maps[0];
                utils_close.up = utils_maps[1];
                utils_close.down = utils_maps[1];

                return utils_maps[1];
            }
            else
            {
                utils_maps[0].left = utils_close;
                utils_maps[0].right = utils_close;

                utils_close.left = utils_maps[0];
                utils_close.right = utils_maps[0];

                return utils_maps[0];
            }
        }
        else
        {
            return utils_close;
        }
    }

    public UINavi Shop_Orders_Reconnection()
    {
        shops_close.ResetConnection();
        shops_closeStore.ResetConnection();

        var uiManager = UIManager.Instance;
        var shops = uiManager.shopUI;

        if (shops.explorePanel.activeSelf)
        {
            shops_close.left = shops_orders_explores[0];
            shops_close.right = shops_orders_explores[0];

            return shops_orders_explores[0];
        }
        else if (shops.orderPanel.activeSelf)
        {
            UINavi first = null;

            OrderUIObject prevObj = null;
            UINavi prev = null;

            bool canCloseStore = shops.shopCloseBtn.gameObject.activeSelf && shops.shopCloseBtn.enabled;
            bool maxLoad = OrderManager.Instance.IsMaxDelivery;

            var list = uiManager.orderUIObjects;
            for (int i = 0; i < list.Count; i++)
            {
                list[i].navi.ResetConnection();
                list[i].navi_ViewPos.ResetConnection();
                if (list[i].gameObject.activeSelf && list[i].OrderAcceptable())
                {
                    if (!maxLoad)
                    {
                        bool acceptEnabled = list[i].acceptButton.interactable;

                        if (first == null)
                        {
                            if (acceptEnabled) first = list[i].navi;
                            else first = list[i].navi_ViewPos;
                        }

                        if (prev != null)
                        {
                            prev.down = acceptEnabled ? list[i].navi : list[i].navi_ViewPos;
                            prevObj.navi_ViewPos.down = acceptEnabled ? list[i].navi : list[i].navi_ViewPos;
                            list[i].navi.up = prevObj.acceptButton.interactable ? prev : prevObj.navi_ViewPos;
                            list[i].navi_ViewPos.up = prevObj.acceptButton.interactable ? prev : prevObj.navi_ViewPos;
                        }

                        if (acceptEnabled)
                        {
                            if (canCloseStore)
                            {
                                list[i].navi.right = shops_closeStore;
                            }
                            else
                            {
                                list[i].navi.right = shops_close;
                            }
                            list[i].navi.left = list[i].navi_ViewPos;
                            list[i].navi_ViewPos.right = list[i].navi;
                        }
                        else
                        {
                            if (canCloseStore)
                            {
                                list[i].navi_ViewPos.right = shops_closeStore;
                            }
                            else
                            {
                                list[i].navi_ViewPos.right = shops_close;
                            }
                        }
                    }
                    else
                    {
                        if (first == null) first = list[i].navi_ViewPos;

                        if (prev != null)
                        {
                            prevObj.navi_ViewPos.down = list[i].navi_ViewPos;
                            list[i].navi_ViewPos.up = prevObj.navi_ViewPos;
                        }
                        if (canCloseStore)
                        {
                            list[i].navi_ViewPos.right = shops_closeStore;
                        }
                        else
                        {
                            list[i].navi_ViewPos.right = shops_close;
                        }
                    }
                    list[i].navi_ViewPos.left = shops_close;

                    prevObj = list[i];
                    prev = list[i].navi;
                }
            }

            if (canCloseStore)
            {
                shops_close.left = shops_closeStore;
                shops_closeStore.right = shops_close;

                // 첫번째 주문
                shops_close.right = first;
                shops_closeStore.left = first;
            }
            else
            {
                // 첫번째 주문
                shops_close.left = first;
                shops_close.right = first;
            }

            return first != null ? first : shops_close;
        }
        else
        {
            // 오븐 요리중
            return null;
        }
    }

    public UINavi Shop_Management_Reconnection()
    {
        shops_close.ResetConnection();
        shops_managements[0].ResetConnection();

        shops_managements[0].up = shops_close;
        shops_managements[0].down = shops_close;
        shops_managements[0].right = shops_close;
        shops_managements[0].left = shops_close;

        shops_close.up = shops_managements[0];
        shops_close.down = shops_managements[0];
        shops_close.right = shops_managements[0];
        shops_close.left = shops_managements[0];

        return shops_managements[0];
    }


    public UINavi Shop_Upgrade_Reconnection()
    {
        virtualCursor.gameObject.SetActive(true);
        var ui = UIManager.Instance.shopUI.GetCurrentResearchUI();
        virtualCursor.position = ui.icon.rectTransform.position;
        return Shop_UpgradeSelect(ui);
    }
    private UINavi Shop_UpgradeSelect(ResearchUI research)
    {
        UINavi first = research.GetComponent<UINavi>();

        first.ResetConnection();
        first.down = shops_close;

        shops_upgrades[0].ResetConnection();
        shops_upgrades[0].down = shops_close;
        shops_upgrades[0].right = first;
        shops_upgrades[0].left = first;

        shops_close.ResetConnection();
        shops_close.up = first;

        if (shops_upgrades[0].gameObject.activeSelf)
        {
            first.left = shops_upgrades[0];
            first.right = shops_upgrades[0];
            shops_close.left = shops_upgrades[0];
            shops_close.right = shops_upgrades[0];
        }
        else
        {

        }
        return first;
    }

    public UINavi Shop_Vehicle_Reconnection()
    {
        return Shop_VehicleSelect();
    }
    private UINavi Shop_VehicleSelect()
    {
        UINavi first = shops_vehicles[4];

        first.ResetConnection();
        first.down = shops_vehicles[2];
        first.right = shops_vehicles[5];

        shops_vehicles[5].ResetConnection();
        shops_vehicles[5].down = shops_vehicles[1];
        shops_vehicles[5].left = first;

        shops_vehicles[0].ResetConnection();
        shops_vehicles[0].up = first;
        shops_vehicles[0].down = shops_close;
        shops_vehicles[0].right = first;
        shops_vehicles[0].left = shops_vehicles[5];

        shops_vehicles[1].ResetConnection();
        shops_vehicles[1].up = first;
        shops_vehicles[1].left = shops_vehicles[2];
        shops_vehicles[1].down = shops_vehicles[3];

        shops_vehicles[2].ResetConnection();
        shops_vehicles[2].up = shops_vehicles[5];
        shops_vehicles[2].right = shops_vehicles[1];
        shops_vehicles[2].down = shops_vehicles[3];

        shops_vehicles[3].ResetConnection();
        shops_vehicles[3].up = shops_vehicles[2];
        shops_vehicles[3].left = shops_vehicles[2];
        shops_vehicles[3].right = shops_vehicles[1];
        shops_vehicles[3].down = shops_close;

        shops_close.ResetConnection();
        shops_close.up = shops_vehicles[3];
        shops_close.down = first;

        if (shops_vehicles[0].gameObject.activeSelf && shops_vehicles[0].gameObject.activeInHierarchy)
        {
            first.left = shops_vehicles[0];
            shops_vehicles[5].right = shops_vehicles[0];

            shops_vehicles[2].left = shops_vehicles[0];
            shops_vehicles[1].right = shops_vehicles[0];

            shops_close.left = shops_vehicles[0];
            shops_close.right = shops_vehicles[0];
        }
        else
        {

        }
        return first;
    }

    public UINavi GameEvent_Reconnection()
    {
        var ge = GameEventManager.Instance;

        UINavi first = null;

        if (ge.acceptBtn.gameObject.activeSelf)
        {
            first = gameEvent_accept;
        }
        else if (ge.declineBtn.gameObject.activeSelf)
        {
            first = gameEvent_decline;
        }

        return first;
    }

    public void Toggle_AlwaysShow_PadUIs(bool on, int padType)
    {
        if (on)
        {
            for (int i = 0; i < alwaysShow_PadUIs.Length; i++)
            {
                alwaysShow_PadUIs[i].UIUpdate(padType);
                alwaysShow_PadUIs[i].gameObject.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < alwaysShow_PadUIs.Length; i++)
            {
                alwaysShow_PadUIs[i].gameObject.SetActive(false);
            }
        }
    }
}
