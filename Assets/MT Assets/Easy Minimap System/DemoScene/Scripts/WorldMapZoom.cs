using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MTAssets.EasyMinimapSystem
{
    public class WorldMapZoom : EventListener
    {
        //private float defaultZoomMultiplier = 0.0f;
        private float defaultFieldOfView = 0.0f;

        public MinimapCamera minimapCamera;
        public Slider zoomSlider;
        public float maxZoomPossible = 75.0f;
        //public float minItensMultiplierPossible = 0.25f;


        public float keyScrollSpeed = 0.1f;

        private SerializableDictionary<KeyMap, KeyMapping> HotKey => SettingManager.Instance.keyMappings;

        void Start()
        {
            //Get default param
            //defaultZoomMultiplier = MinimapDataGlobal.GetMinimapItemsSizeGlobalMultiplier();
            defaultFieldOfView = minimapCamera.fieldOfView;
        }

        private void Update()
        {
            if (!UIManager.Instance.utilUI.opened) return;

            if (pressed)
            {
                zoomSlider.value += zoomValue;
            }

            //Calculate zoom and apply
            //MinimapDataGlobal.SetMinimapItemsSizeGlobalMultiplier((defaultZoomMultiplier - (minItensMultiplierPossible * zoomSlider.value)));
            minimapCamera.fieldOfView = defaultFieldOfView - (maxZoomPossible * zoomSlider.value);
        }

        protected override void AddListeners()
        {
            InputHelper.WorldmapZoomEvent += OnWorldZoom;
        }

        protected override void RemoveListeners()
        {
            InputHelper.WorldmapZoomEvent -= OnWorldZoom;
        }

        bool pressed;
        float zoomValue;

        private void OnWorldZoom(object sender, InputAction.CallbackContext e)
        {
            if (!UIManager.Instance.utilUI.opened) return;

            float value = e.ReadValue<float>();

            pressed = e.performed;

            float scroll = keyScrollSpeed * value;

            scroll = Mathf.Clamp(scroll, -0.12f, 0.12f);

            zoomValue = scroll;
        }
    }
}