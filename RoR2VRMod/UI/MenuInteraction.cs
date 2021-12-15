using MonoMod.RuntimeDetour;
using Rewired.Integration.UnityUI;
using Rewired.UI;
using RoR2;
using RoR2.UI;
using System;
using UnityEngine;

namespace VRMod
{
    internal class MenuInteraction
    {
        private static Camera _cachedUICam;

        private static float clickTime;

        private static Vector3 clickPosition;

        private static Vector3 pointerHitPosition;

        private static GameObject cursorPrefab;

        private static GameObject cursorInstance;

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
            On.RoR2.UI.MPInput.Update += EditPointerPosition;

            On.RoR2.UI.MainMenu.BaseMainMenuScreen.OnEnter += (orig, self, menuController) =>
            {
                orig(self, menuController);
                AddMenuCollider(self.gameObject);
            };
            On.RoR2.UI.CharacterSelectController.Awake += (orig, self) =>
            {
                orig(self);
                AddMenuCollider(self.gameObject);
            };
            On.RoR2.UI.LogBook.LogBookController.Start += (orig, self) =>
            {
                orig(self);
                AddMenuCollider(self.gameObject);
            };
            On.RoR2.UI.EclipseRunScreenController.Start += (orig, self) =>
            {
                orig(self);
                AddMenuCollider(self.gameObject);
            };
            On.RoR2.UI.PauseScreenController.OnEnable += (orig, self) =>
            {
                orig(self);
                AddMenuCollider(self.gameObject);
            };
            On.RoR2.UI.SimpleDialogBox.Start += (orig, self) =>
            {
                orig(self);
                AddMenuCollider(self.transform.root.gameObject);
            };
            On.RoR2.UI.GameEndReportPanelController.Awake += (orig, self) =>
            {
                orig(self);
                AddMenuCollider(self.gameObject);
            };

            cursorPrefab = VRMod.VRAssetBundle.LoadAsset<GameObject>("UICursor");
        }

        private static void AddMenuCollider(GameObject canvasObject)
        {
            BoxCollider collider = canvasObject.GetComponent<BoxCollider>();
            if (!collider)
            {
                RectTransform rect = canvasObject.transform as RectTransform;
                collider = canvasObject.gameObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(rect.sizeDelta.x, rect.sizeDelta.y, 1);
            }
        }

        private static void EditPointerPosition(On.RoR2.UI.MPInput.orig_Update orig, MPInput self)
        {
            if (!cachedUICam || !Utils.isUsingUI)
            {
                if (cursorInstance && cursorInstance.activeSelf)
                    cursorInstance.SetActive(false);

                orig(self);
                return;
            }

            if (!cursorInstance && cursorPrefab)
            {
                cursorInstance = GameObject.Instantiate(cursorPrefab);
            }

            if (!cursorInstance.activeSelf)
                cursorInstance.SetActive(true);

            self.internalScreenPositionDelta = Vector2.zero;
            self._scrollDelta = new Vector2(0f, self.player.GetAxis(26));

            if (self.GetMouseButtonDown(0))
            {
                clickTime = Time.realtimeSinceStartup;
                clickPosition = pointerHitPosition;
            }
            else if (self.GetMouseButtonUp(0))
            {
                clickTime = 0;
            }

            Camera uiCam = cachedUICam;

            Ray ray;
            if (MotionControls.HandsReady)
            {
                HandController dominantHand = MotionControls.GetHandByDominance(true);

                ray = dominantHand.uiRay;
            }
            else
            {
                ray = new Ray(uiCam.transform.position, uiCam.transform.forward);
            }

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, LayerIndex.ui.mask))
            {
                pointerHitPosition = hit.point;

                if (cursorInstance)
                {
                    cursorInstance.transform.position = pointerHitPosition;
                    cursorInstance.transform.rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
                }
            }

            Vector3 mousePosition = uiCam.WorldToScreenPoint(pointerHitPosition);

            if (Time.realtimeSinceStartup - clickTime > 0.25f || (pointerHitPosition - clickPosition).magnitude > 1f)
                self.internalMousePosition = mousePosition;
        }
    }
}
