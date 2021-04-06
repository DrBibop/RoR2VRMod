using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace VRMod
{
    internal static class UIFixes
    {
        private static readonly Vector3 menuPosition = new Vector3(0, 0, 15);
        private static readonly Vector3 characterSelectPosition = new Vector3(0, 0, 5);

        private static readonly Vector3 menuScale = new Vector3(0.01f, 0.01f, 0.01f);
        private static readonly Vector3 characterSelectScale = new Vector3(0.005f, 0.005f, 0.005f);

        private static readonly Vector2 menuPivot = new Vector2(0.5f, 0.5f);

        private static readonly Vector2 menuResolution = new Vector2(1500, 1000);
        private static readonly Vector2 hdResolution = new Vector2(1920, 1080);

        private static Camera uiCamera;

        internal static void Init()
        {
            On.RoR2.UI.HUD.Awake += AdjustHUDAnchors;

            On.RoR2.TeleporterInteraction.Awake += AdjustTPIconSize;
            On.RoR2.UI.PingIndicator.RebuildPing += AdjustPingIconSize;
            On.RoR2.TeamComponent.Start += AdjustTeamIconSize;

            On.RoR2.UI.CombatHealthBarViewer.UpdateAllHealthbarPositions += UpdateAllHealthBarPositionsVR;

            IL.RoR2.PositionIndicator.UpdatePositions += RemoveZReplacement;

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
        }

        private static void SetRenderMode(GameObject uiObject, Vector2 resolution, Vector3 positionOffset, Vector3 scale)
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

                if (uiObject.transform.parent)
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

        private static void AdjustHUDAnchors(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);

            CanvasScaler scaler = self.canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.scaleFactor = ModConfig.UIScale.Value;

            Transform[] uiElements = new Transform[] {
                self.mainUIPanel.transform.Find("SpringCanvas"),
                self.mainContainer.transform.Find("NotificationArea"),
                self.mainContainer.transform.Find("MapNameCluster")
            };

            foreach (Transform uiElement in uiElements)
            {
                RectTransform rect = uiElement.GetComponent<RectTransform>();
                rect.anchorMin = ModConfig.AnchorMin;
                rect.anchorMax = ModConfig.AnchorMax;
            }
        }

        private static void RemoveZReplacement(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchStfld<Vector3>("z")
                );

            c.Index--;

            c.Remove();

            c.Emit(OpCodes.Ldc_R4, 12.25f);
        }

        private static void AdjustTeamIconSize(On.RoR2.TeamComponent.orig_Start orig, TeamComponent self)
        {
            orig(self);
            if (self.indicator)
                self.indicator.transform.localScale = 2 * Vector3.one;
        }

        private static void AdjustPingIconSize(On.RoR2.UI.PingIndicator.orig_RebuildPing orig, PingIndicator self)
        {
            orig(self);
            self.positionIndicator.transform.localScale = 12.35f * Vector3.one;
        }

        private static void AdjustTPIconSize(On.RoR2.TeleporterInteraction.orig_Awake orig, TeleporterInteraction self)
        {
            orig(self);
            self.teleporterPositionIndicator.transform.localScale = 4 * Vector3.one;
        }

        private static void UpdateAllHealthBarPositionsVR(On.RoR2.UI.CombatHealthBarViewer.orig_UpdateAllHealthbarPositions orig, RoR2.UI.CombatHealthBarViewer self, Camera sceneCam, Camera uiCam)
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
    }
}
