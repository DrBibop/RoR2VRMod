using BepInEx;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UI;
using QModManager;
using Mono.Cecil.Cil;
using RoR2.UI;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace DrBibop
{
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.DrBibop.VRMod", "VRMod", "1.1.0")]
    public class VRMod : BaseUnityPlugin
    {
        private static readonly Vector3 menuPosition = new Vector3(0, 0, 15);
        private static readonly Vector3 characterSelectPosition = new Vector3(0, 0, 5);

        private static readonly Vector3 menuScale = new Vector3(0.01f, 0.01f, 0.01f);
        private static readonly Vector3 characterSelectScale = new Vector3(0.005f, 0.005f, 0.005f);

        private static readonly Vector2 menuPivot = new Vector2(0.5f, 0.5f);

        private static readonly Vector2 menuResolution = new Vector2(1500, 1000);
        private static readonly Vector2 hdResolution = new Vector2(1920, 1080);

        private static Camera uiCamera;

        private void Awake()
        {
            if (!VREnabler.ConfigVREnabled.Value)
                return;

            On.RoR2.UI.HUD.Awake += AdjustHUDAnchors;
            On.RoR2.CameraRigController.GetCrosshairRaycastRay += GetVRCrosshairRaycastRay;

            On.RoR2.UI.MainMenu.BaseMainMenuScreen.OnEnter += (orig, self, controller) =>
            {
                orig(self, controller);
                SetRenderMode(self.gameObject, menuResolution, menuPosition, menuScale);
            };
            On.RoR2.UI.LogBook.LogBookController.Start += (orig, self) =>
            {
                orig(self);
                SetRenderMode(self.gameObject, hdResolution, menuPosition, menuScale);
            };
            On.RoR2.UI.EclipseRunScreenController.Start += (orig, self) =>
            {
                orig(self);
                SetRenderMode(self.gameObject, hdResolution, menuPosition, menuScale);
            };
            On.RoR2.UI.CharacterSelectController.Start += (orig, self) =>
            {
                orig(self);
                SetRenderMode(self.gameObject, hdResolution, characterSelectPosition, characterSelectScale);
            };
            On.RoR2.UI.PauseScreenController.OnEnable += (orig, self) =>
            {
                orig(self);
                SetRenderMode(self.gameObject, hdResolution, menuPosition, menuScale);
            };
            On.RoR2.UI.SimpleDialogBox.Start += (orig, self) =>
            {
                orig(self);
                SetRenderMode(self.transform.root.gameObject, hdResolution, menuPosition, menuScale);
            };
            On.RoR2.UI.GameEndReportPanelController.Awake += (orig, self) =>
            {
                orig(self);
                SetRenderMode(self.gameObject, hdResolution, menuPosition, menuScale);
            };
            On.RoR2.SplashScreenController.Start += (orig, self) =>
            {
                orig(self);
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = Color.black;
                GameObject splash = GameObject.Find("SpashScreenCanvas");
                if (splash)
                    SetRenderMode(splash, hdResolution, menuPosition, menuScale);
            };

            On.RoR2.GameOverController.Awake += (orig, self) =>
            {
                orig(self);
                self.gameEndReportPanelPrefab.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            };

            On.RoR2.MatchCamera.Awake += (orig, self) =>
            {
                self.matchFOV = false;
                orig(self);
            };

            On.RoR2.UI.CombatHealthBarViewer.UpdateAllHealthbarPositions += UpdateAllHealthBarPositionsVR;

            On.RoR2.PositionIndicator.UpdatePositions += UpdatePositionsVR;

            IL.RoR2.CameraRigController.SetCameraState += SetCameraStateIL;
        }

        private void UpdatePositionsVR(On.RoR2.PositionIndicator.orig_UpdatePositions orig, UICamera uiCamera)
        {
			Camera sceneCam = uiCamera.cameraRigController.sceneCam;
			Camera camera = uiCamera.camera;
			Rect pixelRect = camera.pixelRect;
			Vector2 center = pixelRect.center;
			pixelRect.size *= 0.95f;
			pixelRect.center = center;
			Vector2 center2 = pixelRect.center;
			float num = 1f / (pixelRect.width * 0.5f);
			float num2 = 1f / (pixelRect.height * 0.5f);
			Quaternion rotation = uiCamera.transform.rotation;
			CameraRigController cameraRigController = uiCamera.cameraRigController;
			Transform y = null;
			if (cameraRigController && cameraRigController.target)
			{
				CharacterBody component = cameraRigController.target.GetComponent<CharacterBody>();
				if (component)
				{
					y = component.coreTransform;
				}
				else
				{
					y = cameraRigController.target.transform;
				}
			}
			for (int i = 0; i < PositionIndicator.instancesList.Count; i++)
			{
				PositionIndicator positionIndicator = PositionIndicator.instancesList[i];
				bool flag = false;
				bool flag2 = false;
				bool active = false;
				float num3 = 0f;
				if (PositionIndicator.cvPositionIndicatorsEnable.value)
				{
					Vector3 a = positionIndicator.targetTransform ? positionIndicator.targetTransform.position : positionIndicator.defaultPosition;
					if (!positionIndicator.targetTransform || (positionIndicator.targetTransform && positionIndicator.targetTransform != y))
					{
						active = true;
						Vector3 vector = sceneCam.WorldToScreenPoint(a + new Vector3(0f, positionIndicator.yOffset, 0f));
						bool flag3 = vector.z <= 0f;
						bool flag4 = !flag3 && pixelRect.Contains(vector);
						if (!flag4)
						{
                            Vector3 center3 = new Vector3(center2.x, center2.y, 0);
							Vector2 vector2 = vector - center3;
							float a2 = Mathf.Abs(vector2.x * num);
							float b = Mathf.Abs(vector2.y * num2);
							float d = Mathf.Max(a2, b);
							vector2 /= d;
							vector2 *= (flag3 ? -1f : 1f);
							vector = vector2 + center2;
							if (positionIndicator.shouldRotateOutsideViewObject)
							{
								num3 = Mathf.Atan2(vector2.y, vector2.x) * 57.29578f;
							}
						}
						flag = flag4;
						flag2 = !flag4;
						vector.z = 12.35f;
						Vector3 position = camera.ScreenToWorldPoint(vector);
						positionIndicator.transform.SetPositionAndRotation(position, rotation);
                        positionIndicator.transform.localScale = 12.35f * Vector3.one;
					}
				}
				if (positionIndicator.alwaysVisibleObject)
				{
					positionIndicator.alwaysVisibleObject.SetActive(active);
				}
				if (positionIndicator.insideViewObject == positionIndicator.outsideViewObject)
				{
					if (positionIndicator.insideViewObject)
					{
						positionIndicator.insideViewObject.SetActive(flag || flag2);
					}
				}
				else
				{
					if (positionIndicator.insideViewObject)
					{
						positionIndicator.insideViewObject.SetActive(flag);
					}
					if (positionIndicator.outsideViewObject)
					{
						positionIndicator.outsideViewObject.SetActive(flag2);
						if (flag2 && positionIndicator.shouldRotateOutsideViewObject)
						{
							positionIndicator.outsideViewObject.transform.localEulerAngles = new Vector3(0f, 0f, num3 + positionIndicator.outsideViewRotationOffset);
						}
					}
				}
			}
        }

        private void UpdateAllHealthBarPositionsVR(On.RoR2.UI.CombatHealthBarViewer.orig_UpdateAllHealthbarPositions orig, RoR2.UI.CombatHealthBarViewer self, Camera sceneCam, Camera uiCam)
        {
            foreach (CombatHealthBarViewer.HealthBarInfo healthBarInfo in self.victimToHealthBarInfo.Values)
            {
                Vector3 position = healthBarInfo.sourceTransform.position;
                position.y += healthBarInfo.verticalOffset;
                Vector3 vector = sceneCam.WorldToScreenPoint(position);
                Vector3 position2 = uiCam.ScreenToWorldPoint(vector);
                healthBarInfo.healthBarRootObjectTransform.position = position2;
                healthBarInfo.healthBarRootObjectTransform.localScale = 0.1f * Vector3.Distance(sceneCam.transform.position, position) * Vector3.one;
            }
        }

        private void SetCameraStateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<CameraState>("fov")
                );

            ILLabel breakLabel = c.IncomingLabels.ToList().First<ILLabel>();

            while (c.Next.OpCode.Code != Code.Ret)
            {
                c.Remove();
            }

            ILLabel retLabel = c.MarkLabel();

            c.GotoPrev(
                x => x.MatchBr(breakLabel)
                );
            c.Remove();
            c.Emit(OpCodes.Br_S, retLabel);
        }

        private void SetRenderMode(GameObject uiObject, Vector2 resolution, Vector3 positionOffset, Vector3 scale)
        {
            if (!uiCamera)
            {
                GameObject cameraObject = Camera.main.transform.parent.gameObject;
                uiCamera = cameraObject.GetComponent<CameraRigController>().uiCam;
            }

            Canvas canvas = uiObject.GetComponent<Canvas>();

            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = uiCamera;

                if (transform.parent)
                    uiObject.transform.parent.position = uiCamera.transform.position + positionOffset;

                uiObject.transform.position = uiCamera.transform.position + positionOffset;
                uiObject.transform.localScale = scale;

                RectTransform rect = uiObject.GetComponent<RectTransform>();
                if (rect)
                {
                    rect.pivot = menuPivot;
                    rect.sizeDelta = resolution;
                }
            }
        }

        private void AdjustHUDAnchors(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);
            
            RectTransform mainRect = self.mainContainer.GetComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.25f, 0.25f);
            mainRect.anchorMax = new Vector2(0.75f, 0.75f);
            CanvasScaler scaler = self.canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.scaleFactor = 0.8f;

            Transform uiArea = mainRect.transform.Find("MainUIArea");

            if (uiArea)
            {
                Transform[] uiElementsToLower = new Transform[3]
                {
                    uiArea.Find("UpperRightCluster"),
                    uiArea.Find("UpperLeftCluster"),
                    uiArea.Find("TopCenterCluster")
                };

                foreach (Transform uiTransform in uiElementsToLower)
                {
                    if (!uiTransform)
                        continue;

                    RectTransform rect = uiTransform.GetComponent<RectTransform>();
                    Vector3 newPos = rect.position;
                    newPos.y -= 150;
                    rect.position = newPos;
                }
            }
        }

        private Ray GetVRCrosshairRaycastRay(On.RoR2.CameraRigController.orig_GetCrosshairRaycastRay orig, RoR2.CameraRigController self, Vector2 crosshairOffset, Vector3 raycastStartPlanePoint)
        {
            if (!self.sceneCam)
            {
                return default(Ray);
            }
            float fieldOfView = self.sceneCam.fieldOfView;
            float num = fieldOfView * self.sceneCam.aspect;
            Quaternion quaternion = Quaternion.Euler(crosshairOffset.y * fieldOfView, crosshairOffset.x * num, 0f);
            quaternion = self.sceneCam.transform.rotation * quaternion;
            return new Ray(Vector3.ProjectOnPlane(self.sceneCam.transform.position - raycastStartPlanePoint, self.sceneCam.transform.rotation * Vector3.forward) + raycastStartPlanePoint, quaternion * Vector3.forward);
        }
    }
}