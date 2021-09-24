using RoR2;
using RoR2.UI;
using System;
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

        private static Camera cachedUICam;
        private static Vector3 camRotation;

        private static Canvas cachedMultiplayerCanvas;

        private static GameObject creditsCanvasPrefab;
        private static GameObject creditsCanvas;

        internal static void Init()
        {
            creditsCanvasPrefab = VRMod.VRAssetBundle.LoadAsset<GameObject>("CreditsCanvas");

            On.RoR2.UI.CombatHealthBarViewer.UpdateAllHealthbarPositions += UpdateAllHealthBarPositionsVR;

            On.RoR2.Indicator.PositionForUI += PositionForUIOverride;
            On.RoR2.PositionIndicator.UpdatePositions += UpdatePositionsOverride;
            On.RoR2.GameOverController.GenerateReportScreen += UnparentHUD;
            
            RoR2Application.onLoad += () =>
            {
                RoR2Application.instance.mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            };

            On.RoR2.UI.MainMenu.BaseMainMenuScreen.OnEnter += (orig, self, controller) =>
            {
                orig(self, controller);
                SetRenderMode(self.gameObject, menuResolution, menuPosition, menuScale);
            };
            On.RoR2.UI.MainMenu.MultiplayerMenuController.Awake += (orig, self) =>
            {
                orig(self);
                self.gameObject.layer = LayerMask.NameToLayer("UI");
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
                if (!GetUICamera()) return;
                camRotation = new Vector3(0, cachedUICam.transform.eulerAngles.y, 0);
                SetRenderMode(self.gameObject, hdResolution, menuPosition, menuScale, true);
            };
            On.RoR2.UI.SimpleDialogBox.Start += (orig, self) =>
            {
                orig(self);
                SetRenderMode(self.transform.root.gameObject, hdResolution, menuPosition, menuScale, PauseManager.isPaused);
            };
            On.RoR2.UI.GameEndReportPanelController.Awake += (orig, self) =>
            {
                orig(self);
                if (!GetUICamera()) return;
                camRotation = new Vector3(0, cachedUICam.transform.eulerAngles.y, 0);
                SetRenderMode(self.gameObject, hdResolution, menuPosition, menuScale, true);
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

            
            On.RoR2.RemoteGameBrowser.RemoteGameBrowserController.Awake += (orig, self) =>
            {
                orig(self);
                cachedMultiplayerCanvas = self.GetComponentInParent<Canvas>();
            };

            On.RoR2.RemoteGameDetailsPanelController.Awake += OpenPopupInMenuCanvas;

            On.RoR2.UI.MainMenu.ProfileMainMenuScreen.OpenCreateProfileMenu += SetAddProfileButtonAsDefaultFallback;

            On.RoR2.UI.CreditsPanelController.OnEnable += MoveCreditsToWorldSpace;
        }

        private static void MoveCreditsToWorldSpace(On.RoR2.UI.CreditsPanelController.orig_OnEnable orig, CreditsPanelController self)
        {
            orig(self);

            if (creditsCanvasPrefab)
            {
                creditsCanvas = GameObject.Instantiate(creditsCanvasPrefab, null);
                creditsCanvas.transform.position = new Vector3(0, 0, 8);
            }

            if (creditsCanvas)
            {
                Transform credits = self.scrollRect.transform;
                credits.SetParent(creditsCanvas.transform);
                credits.localPosition = Vector3.zero;
                credits.localRotation = Quaternion.identity;
                credits.localScale = Vector3.one;
                GameObject.Destroy(self.transform.Find("Backdrop").gameObject);
            }
        }

        private static void SetAddProfileButtonAsDefaultFallback(On.RoR2.UI.MainMenu.ProfileMainMenuScreen.orig_OpenCreateProfileMenu orig, RoR2.UI.MainMenu.ProfileMainMenuScreen self, bool firstTime)
        {
            orig(self, firstTime);
            self.submitProfileNameButton.defaultFallbackButton = true;
        }

        private static void OpenPopupInMenuCanvas(On.RoR2.RemoteGameDetailsPanelController.orig_Awake orig, RemoteGameDetailsPanelController self)
        {
            orig(self);
            if (cachedMultiplayerCanvas)
                self.transform.SetParent(cachedMultiplayerCanvas.transform);

            RectTransform rect = (self.transform as RectTransform);

            rect.localPosition = Vector3.zero;
            rect.rotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.sizeDelta = menuResolution / 2;
        }

        private static GameEndReportPanelController UnparentHUD(On.RoR2.GameOverController.orig_GenerateReportScreen orig, GameOverController self, HUD hud)
        {
            hud.transform.SetParent(null);
            hud.canvas.renderMode = RenderMode.ScreenSpaceCamera;
            return orig(self, hud);
        }

        private static bool GetUICamera()
        {
            if (cachedUICam == null)
            {
                if (Camera.main)
                {
                    SceneCamera sceneCamera = Camera.main.GetComponent<SceneCamera>();
                    if (sceneCamera)
                    {
                        cachedUICam = sceneCamera.cameraRigController.uiCam;
                    }
                }
            }
            return cachedUICam != null;
        }

        private static void UpdatePositionsOverride(On.RoR2.PositionIndicator.orig_UpdatePositions orig, UICamera uiCamera)
        {
            orig(uiCamera);
            if (!HUD.cvHudEnable.value || !PositionIndicator.cvPositionIndicatorsEnable.value)
                return;

            foreach (PositionIndicator indicator in PositionIndicator.instancesList)
            {
                Vector3 position = indicator.targetTransform ? indicator.targetTransform.position : indicator.defaultPosition;
                position.y += indicator.yOffset;

                bool isPingIndicator = PingIndicator.instancesList.Exists((x) => x.positionIndicator == indicator);

                Transform rigTransform = uiCamera.cameraRigController.sceneCam.transform.parent;
                Vector3 newPosition = rigTransform.InverseTransformPoint(position);
                indicator.transform.position = newPosition;
                indicator.transform.localScale = (isPingIndicator ? 1: 0.2f) * Vector3.Distance(uiCamera.transform.position, newPosition) * Vector3.one;
            }
        }

        private static void PositionForUIOverride(On.RoR2.Indicator.orig_PositionForUI orig, Indicator self, Camera sceneCamera, Camera uiCamera)
        {
            if (self.targetTransform)
            {
                Vector3 position = self.targetTransform.position;
                
                Vector3 vector = sceneCamera.transform.parent.InverseTransformPoint(position);
                if (self.visualizerTransform != null)
                {
                    self.visualizerTransform.position = vector;
                    self.visualizerTransform.rotation = Quaternion.LookRotation((vector - uiCamera.transform.position).normalized);
                    self.visualizerTransform.localScale = (self is EntityStates.Engi.EngiMissilePainter.Paint.EngiMissileIndicator ? 1 : 0.1f) * Vector3.Distance(sceneCamera.transform.position, position) * Vector3.one;
                }
            }
        }

        private static void SetRenderMode(GameObject uiObject, Vector2 resolution, Vector3 positionOffset, Vector3 scale, bool followRotation = false)
        {
            if (!GetUICamera()) return;

            Canvas canvas = uiObject.GetComponent<Canvas>();

            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = cachedUICam;

                Vector3 offset = positionOffset;

                if (followRotation)
                {
                    offset = Quaternion.Euler(camRotation) * offset;

                    if (uiObject.transform.root != uiObject.transform)
                        uiObject.transform.parent.rotation = Quaternion.Euler(uiObject.transform.parent.eulerAngles + camRotation);
                    else
                        uiObject.transform.rotation = Quaternion.Euler(uiObject.transform.eulerAngles + camRotation);

                }

                if (uiObject.transform.parent)
                    uiObject.transform.parent.position = cachedUICam.transform.position + offset;

                uiObject.transform.position = cachedUICam.transform.position + offset;
                uiObject.transform.localScale = scale;

                RectTransform rect = uiObject.GetComponent<RectTransform>();
                if (rect)
                {
                    rect.pivot = menuPivot;
                    rect.sizeDelta = resolution;
                }
            }
        }

        internal static void AdjustHUD(HUD hud)
        {
            if (ModConfig.UseMotionControls.Value)
            {
                CrosshairManager crosshairManager = hud.GetComponent<CrosshairManager>();

                if (crosshairManager)
                {
                    crosshairManager.container.gameObject.SetActive(false);

                    //Thanks HutchyBen
                    crosshairManager.hitmarker.enabled = false;
                }
            }

            Transform steamBuild = hud.mainContainer.transform.Find("SteamBuildLabel");
            if (steamBuild)
                steamBuild.gameObject.SetActive(false);

            RectTransform springCanvas = hud.mainUIPanel.transform.Find("SpringCanvas") as RectTransform;
            springCanvas.anchorMin = ModConfig.AnchorMin;
            springCanvas.anchorMax = ModConfig.AnchorMax;

            RectTransform notificationArea = hud.mainContainer.transform.Find("NotificationArea") as RectTransform;
            notificationArea.anchorMin = new Vector2(0.5f, ModConfig.AnchorMin.y);
            notificationArea.anchorMax = new Vector2(0.5f, ModConfig.AnchorMin.y);

            RectTransform mapNameCluster = hud.mainContainer.transform.Find("MapNameCluster") as RectTransform;
            mapNameCluster.anchorMin = new Vector2(0.5f, (ModConfig.AnchorMax.y - 0.5f) * 0.54f + 0.5f);
            mapNameCluster.anchorMax = new Vector2(0.5f, (ModConfig.AnchorMax.y - 0.5f) * 0.54f + 0.5f);

            RectTransform scoreboardPanel = springCanvas.Find("ScoreboardPanel") as RectTransform;
            scoreboardPanel.offsetMin = new Vector2(0, scoreboardPanel.offsetMin.y);
            scoreboardPanel.offsetMax = new Vector2(0, scoreboardPanel.offsetMax.y);

            if (!GetUICamera()) return;

            hud.canvas.renderMode = RenderMode.WorldSpace;
            RectTransform rectTransform = hud.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(ModConfig.HUDWidth.Value, ModConfig.HUDHeight.Value);
            rectTransform.localScale = menuScale;
            hud.transform.SetParent(cachedUICam.transform);
            hud.transform.localRotation = Quaternion.identity;
            hud.transform.position = new Vector3(0, 0, 12.35f);
            rectTransform.pivot = menuPivot;


            if (ModConfig.WristHUD.Value)
            {
                RectTransform healthCluster = springCanvas.Find("BottomLeftCluster/BarRoots") as RectTransform;
                healthCluster.pivot = new Vector2(0.5f, 0f);

                if (ModConfig.BetterHealthBar.Value)
                {
                    healthCluster.SetParent(springCanvas);
                    healthCluster.localRotation = Quaternion.identity;
                    healthCluster.offsetMin = new Vector2(300, healthCluster.offsetMin.y);
                    healthCluster.offsetMax = new Vector2(-300, healthCluster.offsetMax.y);
                }
                else
                {
                    healthCluster.localRotation = Quaternion.identity;
                    healthCluster.localPosition = Vector3.zero;
                    MotionControls.AddWristHUD(true, healthCluster);
                }

                RectTransform moneyCluster = springCanvas.Find("UpperLeftCluster") as RectTransform;
                moneyCluster.localRotation = Quaternion.identity;
                moneyCluster.pivot = new Vector2(0.5f, 1f);
                moneyCluster.localPosition = Vector3.zero;
                MotionControls.AddWristHUD(true, moneyCluster);

                RectTransform contextCluster = springCanvas.Find("RightCluster") as RectTransform;
                contextCluster.localRotation = Quaternion.identity;
                contextCluster.pivot = new Vector2(0f, 0f);
                contextCluster.localPosition = Vector3.zero;
                (contextCluster.GetChild(0) as RectTransform).pivot = new Vector2(0f, 2f);
                MotionControls.AddWristHUD(false, contextCluster);

                RectTransform cooldownsCluster = springCanvas.Find("BottomRightCluster") as RectTransform;
                cooldownsCluster.localRotation = Quaternion.identity;
                cooldownsCluster.pivot = new Vector2(0.2f, -0.6f);
                cooldownsCluster.localPosition = Vector3.zero;
                MotionControls.AddWristHUD(false, cooldownsCluster);

                MotionControls.SetSprintIcon(cooldownsCluster.Find("Scaler/SprintCluster/SprintIcon").GetComponent<Image>());
            }

            if (ModConfig.WatchHUD.Value)
            {
                Transform topCluster = springCanvas.Find("TopCenterCluster");
                GameObject clusterClone = GameObject.Instantiate(topCluster.gameObject, topCluster.parent);
                foreach (Transform child in clusterClone.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
                RectTransform cloneRect = clusterClone.transform as RectTransform;
                cloneRect.pivot = new Vector2(0.5f, 1f);
                cloneRect.localPosition = Vector3.zero;
                RectTransform inventoryCluster = topCluster.Find("ItemInventoryDisplayRoot") as RectTransform;
                inventoryCluster.SetParent(cloneRect);
                inventoryCluster.localPosition = Vector3.zero;
                MotionControls.AddWatchHUD(true, cloneRect);

                RectTransform chatCluster = springCanvas.Find("BottomLeftCluster/ChatBoxRoot") as RectTransform;
                chatCluster.localRotation = Quaternion.identity;
                chatCluster.pivot = new Vector2(0.5f, 0f);
                chatCluster.localPosition = Vector3.zero;
                MotionControls.AddWatchHUD(true, chatCluster);

                RectTransform difficultyCluster = springCanvas.Find("UpperRightCluster") as RectTransform;
                difficultyCluster.localRotation = Quaternion.identity;
                difficultyCluster.pivot = new Vector2(-1f, -2.5f);
                difficultyCluster.localPosition = Vector3.zero;
                MotionControls.AddWatchHUD(false, difficultyCluster);

                RectTransform alliesCluster = springCanvas.Find("LeftCluster") as RectTransform;
                alliesCluster.localRotation = Quaternion.identity;
                alliesCluster.pivot = new Vector2(1f, 0.5f);
                alliesCluster.localPosition = Vector3.zero;
                MotionControls.AddWatchHUD(false, alliesCluster);
            }

            if (ModConfig.SmoothHUD.Value)
                hud.gameObject.AddComponent<SmoothHUD>().Init(hud.cameraRigController);
        }

        private static void UpdateAllHealthBarPositionsVR(On.RoR2.UI.CombatHealthBarViewer.orig_UpdateAllHealthbarPositions orig, RoR2.UI.CombatHealthBarViewer self, Camera sceneCam, Camera uiCam)
        {
            foreach (CombatHealthBarViewer.HealthBarInfo healthBarInfo in self.victimToHealthBarInfo.Values)
            {
                Vector3 position = healthBarInfo.sourceTransform.position;
                position.y += healthBarInfo.verticalOffset;
                Vector3 vector = sceneCam.transform.parent.InverseTransformPoint(position);
                healthBarInfo.healthBarRootObjectTransform.position = vector;
                healthBarInfo.healthBarRootObjectTransform.localScale = 0.1f * Vector3.Distance(uiCam.transform.position, vector) * Vector3.one;
            }
        }
    }
}
