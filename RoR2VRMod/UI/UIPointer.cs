using RoR2;
using RoR2.UI;
using UnityEngine;

namespace VRMod
{
    internal class UIPointer
    {
        private static Camera _cachedUICam;

        private static float clickTime;

        private static Vector3 clickPosition;

        private static Vector3 pointerHitPosition;

        private static GameObject cursorPrefab;

        private static GameObject cursorInstance;

        private static Canvas lastHitCanvas;

        private static Camera cachedUICam
        {
            get
            {
                if (_cachedUICam == null || !_cachedUICam.isActiveAndEnabled)
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
            On.RoR2.UI.TooltipController.SetTooltip += MatchTooltipToCanvas;
            On.RoR2.UI.TooltipController.LateUpdate += PlaceTooltipOnCursor;

            cursorPrefab = VRMod.VRAssetBundle.LoadAsset<GameObject>("UICursor");
        }

        private static void PlaceTooltipOnCursor(On.RoR2.UI.TooltipController.orig_LateUpdate orig, TooltipController self)
        {
            if (cursorInstance)
            {
                self.tooltipCenterTransform.position = cursorInstance.transform.position;
            }
        }

        private static void MatchTooltipToCanvas(On.RoR2.UI.TooltipController.orig_SetTooltip orig, MPEventSystem eventSystem, TooltipProvider newTooltipProvider, Vector2 tooltipPosition)
        {
            orig(eventSystem, newTooltipProvider, tooltipPosition);

            if (!eventSystem.currentTooltip || !lastHitCanvas) return;

            if (cursorInstance)
            {
                Vector3 relativeCursorPosition = lastHitCanvas.transform.InverseTransformPoint(cursorInstance.transform.position);

                Vector2 vector2 = new Vector2(0f, 0f);
                vector2.x = ((relativeCursorPosition.x > 0f) ? 1f : 0f);
                vector2.y = ((relativeCursorPosition.y > 0f) ? 1f : 0f);
                eventSystem.currentTooltip.tooltipFlipTransform.anchorMin = vector2;
                eventSystem.currentTooltip.tooltipFlipTransform.anchorMax = vector2;
                eventSystem.currentTooltip.tooltipFlipTransform.pivot = vector2;
            }

            Canvas canvas = eventSystem.currentTooltip.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            RectTransform canvasTransform = canvas.transform as RectTransform;
            RectTransform lastHitTransform = lastHitCanvas.transform as RectTransform;

            canvasTransform.sizeDelta = lastHitTransform.sizeDelta;
            canvasTransform.pivot = lastHitTransform.pivot;
            canvasTransform.position = lastHitTransform.position;
            canvasTransform.rotation = lastHitTransform.rotation;
            canvasTransform.localScale = lastHitTransform.localScale;
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
                    if (!cursorInstance.activeSelf)
                        cursorInstance.SetActive(true);

                    cursorInstance.transform.position = pointerHitPosition;
                    cursorInstance.transform.rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
                }

                lastHitCanvas = hit.collider.GetComponent<Canvas>();
            }
            else if (cursorInstance.activeSelf)
            {
                cursorInstance.SetActive(false);
            }

            Vector3 mousePosition = uiCam.WorldToScreenPoint(pointerHitPosition);

            if (Time.realtimeSinceStartup - clickTime > 0.25f || (pointerHitPosition - clickPosition).magnitude > 1f)
                self.internalMousePosition = mousePosition;
        }
    }
}
