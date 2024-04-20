using MTAssets.EasyMinimapSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapManager : Singleton<WorldMapManager>
{
    //Public variables
    public MinimapRenderer worldmap;

    public MinimapCamera worldMapCamera;

    public MinimapItem playerFieldOfView;

    public MinimapRenderer minimap;
    public Transform minimapItemsTarget;

    // My Custom
    public Vector2 worldmapLimitX;
    public Vector2 worldmapLimitZ;

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.M) == true && fullScreenMapObj.activeSelf == false)
    //        OpenFullscreenMap();
    //    if (Input.GetKeyDown(KeyCode.Escape) == true)
    //        if (fullScreenMapObj.activeSelf == true)
    //            CloseFullscreenMap();
    //}

    public void OpenFullscreenMap()
    {
        //worldMapObj.SetActive(true);
        //worldMapCamera.gameObject.SetActive(true);
        //playerFieldOfView.enabled = false;

        //worldmap.renderContent = true;
        MinimapDataGlobal.SetMinimapItemsSizeGlobalMultiplier(1.5f);
    }

    public void FocusPlayerPos()
    {
        // 처음 열 때 플레이어 위치로 고정
        worldMapCamera.transform.position = minimapItemsTarget.position;
    }

    public void CloseFullscreenMap()
    {
        //worldMapObj.SetActive(false);
        //worldMapCamera.gameObject.SetActive(false);
        //playerFieldOfView.enabled = true;

        //worldmap.renderContent = false;
        MinimapDataGlobal.SetMinimapItemsSizeGlobalMultiplier(1f);
    }

    public void OpenMinimap()
    {
        minimap.gameObject.SetActive(true);
        for (int i = 0; i < minimap.minimapItemsToHightlight.Count; i++)
        {
            minimap.minimapItemsToHightlight[i].customGameObjectToFollowRotation = minimapItemsTarget;
        }
    }
    public void CloseMinimap()
    {
        minimap.gameObject.SetActive(false);
        for (int i = 0; i < minimap.minimapItemsToHightlight.Count; i++)
        {
            minimap.minimapItemsToHightlight[i].customGameObjectToFollowRotation = worldMapCamera.customGameObjectToFollowRotation;
        }
    }

    public void OnDragInMinimapRendererArea(Vector3 onStartThisDragWorldPos, Vector3 onDraggingWorldPos)
    {
        //Use the position of drag start and current position of drag to move the Minimap Camera of fullscreen minimap
        Vector3 deltaPositionToMoveMap = (onDraggingWorldPos - onStartThisDragWorldPos) * -1.0f;
        worldMapCamera.transform.position += (deltaPositionToMoveMap * 10.0f * Time.unscaledDeltaTime);

        worldMapCamera.transform.position = new Vector3(
            Mathf.Clamp(worldMapCamera.transform.position.x, worldmapLimitX.x, worldmapLimitX.y), 0f,
            Mathf.Clamp(worldMapCamera.transform.position.z, worldmapLimitZ.x, worldmapLimitZ.y));
    }
}