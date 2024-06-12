using MTAssets.EasyMinimapSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WorldMapManager : Singleton<WorldMapManager>
{
    //Public variables
    public MinimapRenderer worldmap;

    public MinimapCamera worldMapCamera;

    public MinimapItem playerFieldOfView;

    public MinimapRenderer minimap;
    public Transform minimapItemsTarget;
    public MinimapItem pizzaShopPin;
    public Vector3 pizzaShopPinSize_minimap = new Vector3(900f, 0f, 900f);
    public float pizzaShopPinHighlight_minimap = 40f;
    public Vector3 pizzaShopPinSize_worldmap = new Vector3(600f, 0f, 600f);
    public float pizzaShopPinHighlight_worldmap = 60f;


    // My Custom
    public Vector2 worldmapLimitX;
    public Vector2 worldmapLimitZ;


    public SerializableDictionary<MinimapItem, int> customerPin;
    public Vector3 customerPin_InitScale = new Vector3(45f, 0f, 45f);
    public Vector3 customerPin_OverScale = new Vector3(60f, 0f, 60f);
    private MinimapItem mouseOverPin;
    private MinimapItem hightlightPin;

    public bool customerMode;
    private Vector3 customerPos;
    public Button customerBtn;
    public TextMeshProUGUI customerBtnText;
    public Button toShopBtn;
    public TextMeshProUGUI toShopBtnText;

    private void Start()
    {
        ToggleCustomerMode(false, new Vector3(0f, 0f, -1000f));
        //pizzaShopPin.gameObject.SetActive(false);

        toShopBtnText.text = TextManager.Instance.GetCommons("Return");
        toShopBtn.gameObject.SetActive(false);

        worldmap.gameObject.SetActive(false);
    }

    public void ToggleCustomerMode_Btn()
    {
        customerMode = !customerMode;
        ToggleCustomerMode(customerMode, new Vector3(0f, 0f, -1000f));
    }

    public void ToggleCustomerMode(bool on, Vector3 customerPos, int customerIdx = -1)
    {
        customerMode = on;
        this.customerPos = customerPos;

        if (hightlightPin != null)
        {
            hightlightPin.sizeOnMinimap = customerPin_InitScale;
            hightlightPin = null;
        }

        if (on)
        {
            var orderList = OrderManager.Instance.orderList;
            HashSet<int> idx = new HashSet<int>();
            for (int i = 0; i < orderList.Count; i++)
            {
                idx.Add(orderList[i].customerIdx);
            }
            foreach (var temp in customerPin)
            {
                if (idx.Contains(temp.Value))
                    temp.Key.gameObject.SetActive(true);
                else
                    temp.Key.gameObject.SetActive(false);

                if (customerIdx >= 0 && temp.Value == customerIdx)
                    hightlightPin = temp.Key;
            }
            toShopBtn.gameObject.SetActive(true);
            customerBtnText.text = TextManager.Instance.GetCommons("WorldmapMode0");
        }
        else
        {
            customerPos.z = -1000;
            foreach (var temp in customerPin)
            {
                temp.Key.gameObject.SetActive(false);
            }
            toShopBtn.gameObject.SetActive(false);
            customerBtnText.text = TextManager.Instance.GetCommons("WorldmapMode1");
        }

        UINaviHelper.Instance.ingame.Utils_Map_Reconnection();
    }

    public void OpenFullscreenMap()
    {
        //worldMapObj.SetActive(true);
        //worldMapCamera.gameObject.SetActive(true);
        //playerFieldOfView.enabled = false;

        //worldmap.renderContent = true;
        MinimapDataGlobal.SetMinimapItemsSizeGlobalMultiplier(1.5f);

        customerBtn.gameObject.SetActive(UIManager.Instance.shopUI.playerStay);
    }

    public void FirstFocus()
    {
        if (customerPos.z == -1000)
        {
            // 처음 열 때 플레이어 위치로 고정
            worldMapCamera.transform.position = minimapItemsTarget.position;
        }
        else
        {
            worldMapCamera.transform.position = customerPos;

            if (hightlightPin != null)
            {
                hightlightPin.sizeOnMinimap = customerPin_OverScale;
            }
        }
    }

    public void CloseFullscreenMap()
    {
        ToggleCustomerMode(false, new Vector3(0f, 0f, -1000f));

        //worldMapObj.SetActive(false);
        //worldMapCamera.gameObject.SetActive(false);
        //playerFieldOfView.enabled = true;

        //worldmap.renderContent = false;
        MinimapDataGlobal.SetMinimapItemsSizeGlobalMultiplier(1f);
    }

    public void OpenMinimap()
    {
        worldmap.gameObject.SetActive(false);
        minimap.gameObject.SetActive(true);
        for (int i = 0; i < minimap.minimapItemsToHightlight.Count; i++)
        {
            minimap.minimapItemsToHightlight[i].customGameObjectToFollowRotation = minimapItemsTarget;
        }
        //pizzaShopPin.gameObject.SetActive(false);
        pizzaShopPin.sizeOnMinimap = pizzaShopPinSize_minimap;
        pizzaShopPin.sizeOnHighlight = pizzaShopPinHighlight_minimap;
        pizzaShopPin.customGameObjectToFollowRotation = minimapItemsTarget;
    }
    public void CloseMinimap()
    {
        worldmap.gameObject.SetActive(true);
        minimap.gameObject.SetActive(false);
        for (int i = 0; i < minimap.minimapItemsToHightlight.Count; i++)
        {
            minimap.minimapItemsToHightlight[i].customGameObjectToFollowRotation = worldMapCamera.customGameObjectToFollowRotation;
        }
        //pizzaShopPin.gameObject.SetActive(true);
        pizzaShopPin.sizeOnMinimap = pizzaShopPinSize_worldmap;
        pizzaShopPin.sizeOnHighlight = pizzaShopPinHighlight_worldmap;
        pizzaShopPin.customGameObjectToFollowRotation = worldMapCamera.customGameObjectToFollowRotation;
    }

    public void OnDragInMinimapRendererArea(Vector3 onStartThisDragWorldPos, Vector3 onDraggingWorldPos)
    {
        if (UIManager.Instance.utilUI.loading) return;

        //Use the position of drag start and current position of drag to move the Minimap Camera of fullscreen minimap
        Vector3 deltaPositionToMoveMap = (onDraggingWorldPos - onStartThisDragWorldPos) * -1.0f;
        worldMapCamera.transform.position += (deltaPositionToMoveMap * 10.0f * Time.unscaledDeltaTime);

        worldMapCamera.transform.position = new Vector3(
            Mathf.Clamp(worldMapCamera.transform.position.x, worldmapLimitX.x, worldmapLimitX.y), 0f,
            Mathf.Clamp(worldMapCamera.transform.position.z, worldmapLimitZ.x, worldmapLimitZ.y));
    }

    public void OnClickInMinimapRendererArea(Vector3 clickWorldPos, MinimapItem clickedMinimapItem)
    {
        if (UIManager.Instance.utilUI.loading) return;

        //Show the Minimap Item Clicked
        if (clickedMinimapItem != null)
        {
            if (UIManager.Instance.shopUI.playerStay)
            {
                if (customerPin.ContainsKey(clickedMinimapItem))
                {
                    UIManager.Instance.shopUI.ShowOrder(customerPin[clickedMinimapItem]);
                    AudioManager.Instance.PlaySFX(Sfx.buttons);
                }
            }
        }
    }

    public void OnOverInMinimapRendererArea(bool isOverMinimapRendererArea, Vector3 mouseWorldPos, MinimapItem overMinimapItem)
    {
        if (UIManager.Instance.utilUI.loading) return;

        if (isOverMinimapRendererArea)
        {
            //Increase size of the selected item (avoid increase size of same minimap item various times)
            if (overMinimapItem != null)
            {
                if (overMinimapItem != mouseOverPin)
                {
                    mouseOverPin = overMinimapItem;
                    overMinimapItem.sizeOnMinimap = customerPin_OverScale;
                    AudioManager.Instance.PlaySFX(Sfx.btnHighlight);
                }
            }
            else
            {
                if (mouseOverPin != null)
                    mouseOverPin.sizeOnMinimap = customerPin_InitScale;
                mouseOverPin = null;
            }
        }
    }

    protected override void AddListeners()
    {
        InputHelper.WorldmapMoveEvent += OnWorldmapMove;
    }

    protected override void RemoveListeners()
    {
        InputHelper.WorldmapMoveEvent -= OnWorldmapMove;
    }

    private void OnWorldmapMove(object sender, InputAction.CallbackContext e)
    {
        if (UIManager.Instance.utilUI.loading) return;

        Vector2 input = e.ReadValue<Vector2>();

        if (input != null && e.performed)
        {
            worldmapMoveDir = new Vector3(input.x, 0f, input.y).normalized;
        }
        else
        {
            worldmapMoveDir = Vector3.zero;
        }
    }

    Vector3 worldmapMoveDir;

    private void Update()
    {
        if (UIManager.Instance.utilUI.loading) return;

        worldMapCamera.transform.position += -1000.0f * Time.unscaledDeltaTime * worldmapMoveDir;

        worldMapCamera.transform.position = new Vector3(
            Mathf.Clamp(worldMapCamera.transform.position.x, worldmapLimitX.x, worldmapLimitX.y), 0f,
            Mathf.Clamp(worldMapCamera.transform.position.z, worldmapLimitZ.x, worldmapLimitZ.y));
    }
}