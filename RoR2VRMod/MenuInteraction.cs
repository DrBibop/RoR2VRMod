using RoR2;
using RoR2.UI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VRMod
{
    //WIP
    internal class MenuInteraction
    {
        internal static readonly int uiLayer = LayerMask.NameToLayer("UI");

        private static Camera _cachedUICam;

        private static MPButton _selectedButton;

        private static Vector3 pointerHitPosition = Vector3.zero;

        private static MPButton selectedButton
        {
            get { return _selectedButton; }
            set
            {
                if (_selectedButton)
                {
                    _selectedButton.OnPointerExit(new PointerEventData(_selectedButton.eventSystem));
                }

                _selectedButton = value;

                if (_selectedButton)
                {
                    _selectedButton.OnPointerEnter(new PointerEventData(_selectedButton.eventSystem));
                }
            }
        }

        private static Camera cachedUICam
        {
            get
            {
                if (_cachedUICam == null)
                {
                    if (Camera.main)
                    {
                        SceneCamera sceneCamera = Camera.main.GetComponent<SceneCamera>();
                        if (sceneCamera)
                        {
                            _cachedUICam = sceneCamera.cameraRigController.uiCam;
                        }
                    }
                }
                return _cachedUICam;
            }
        }

        internal static void Init()
        {
            On.RoR2.UI.MPButton.Awake += AddCollider;
            //RoR2Application.onFixedUpdate += FixedUpdate;
            On.RoR2.UI.MainMenu.BaseMainMenuScreen.OnEnter += AddMenuCollider;
            On.RoR2.UI.MPInput.Update += EditPointerPosition;
        }

        private static void AddMenuCollider(On.RoR2.UI.MainMenu.BaseMainMenuScreen.orig_OnEnter orig, RoR2.UI.MainMenu.BaseMainMenuScreen self, RoR2.UI.MainMenu.MainMenuController mainMenuController)
        {
            orig(self, mainMenuController);
            BoxCollider collider = self.GetComponent<BoxCollider>();
            if (!collider)
            {
                RectTransform rect = self.transform as RectTransform;
                collider = self.gameObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(rect.sizeDelta.x, rect.sizeDelta.y, 1);
            }
        }

        private static void EditPointerPosition(On.RoR2.UI.MPInput.orig_Update orig, MPInput self)
        {
            if (!self.eventSystem.isCursorVisible)
            {
                return;
            }
            self.internalScreenPositionDelta = Vector2.zero;
            self._scrollDelta = new Vector2(0f, self.player.GetAxis(26));
            if (cachedUICam)
                self.internalMousePosition = cachedUICam.WorldToScreenPoint(pointerHitPosition);

        }

        private static void FixedUpdate()
        {
            if (Run.instance && !PauseManager.isPaused) return;

            Camera uiCam = cachedUICam;

            if (!uiCam) return;

            Ray ray;
            if (MotionControls.HandsReady)
            {
                HandController dominantHand = MotionControls.GetHandByDominance(true);

                ray = dominantHand.aimRay;
            }
            else
            {
                ray = new Ray(uiCam.transform.position, uiCam.transform.forward);
            }

            int layerMask = 1 << uiLayer;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, layerMask))
            {
                /*
                MPButton button = hit.collider.GetComponent<MPButton>();
                if(button)
                {
                    if (button != selectedButton)
                    {
                        selectedButton = button;
                    }
                }*/
                pointerHitPosition = hit.point;
            }
            /*else
            {
                selectedButton = null;
            }*/
        }

        private static void AddCollider(On.RoR2.UI.MPButton.orig_Awake orig, RoR2.UI.MPButton self)
        {
            orig(self);
            Vector2 rectSize = (self.transform as RectTransform).sizeDelta;
            BoxCollider newCollider = self.gameObject.AddComponent<BoxCollider>();
            newCollider.size = new Vector3(rectSize.x, rectSize.y, 1);
            if (self.gameObject.name.Contains("GenericHeaderButton"))
            {
                newCollider.center = Vector3.back;
            }
        }
    }
}
