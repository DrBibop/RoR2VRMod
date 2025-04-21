using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.HudOverlay;
using RoR2.UI;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        private static readonly Vector2 gameDetailsResolution = new Vector2(450, 300);

        private static Camera cachedUICam;
        private static Vector3 camRotation;

        private static Canvas cachedMultiplayerCanvas;

        private static GameObject creditsCanvasPrefab;
        private static GameObject creditsCanvas;

        private static GameObject pickerCanvasPrefab;

        internal static GameObject queuedKickDialog;

        private static Transform voidFiendCorruptionMeter;

        private static GameObject pickerCanvas;

        internal static HUD livHUD;

        internal static void Init()
        {
            creditsCanvasPrefab = VRMod.VRAssetBundle.LoadAsset<GameObject>("CreditsCanvas");
            pickerCanvasPrefab = VRMod.VRAssetBundle.LoadAsset<GameObject>("PickerCanvas");

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
                CanvasToWorldSpace(self.gameObject, menuResolution, menuPosition, menuScale, true);
            };
            On.RoR2.UI.MainMenu.MultiplayerMenuController.Awake += (orig, self) =>
            {
                orig(self);
                self.gameObject.layer = LayerIndex.ui.intVal;
            };
            On.RoR2.UI.LogBook.LogBookController.Start += (orig, self) =>
            {
                orig(self);
                CanvasToWorldSpace(self.gameObject, hdResolution, menuPosition, menuScale, true);
            };
            On.RoR2.UI.EclipseRunScreenController.Awake += (orig, self) =>
            {
                orig(self);
                CanvasToWorldSpace(self.gameObject, hdResolution, menuPosition, menuScale, true);
            };
            On.RoR2.UI.CharacterSelectController.OnEnable += (orig, self) =>
            {
                orig(self);

                Transform topSideFade = self.transform.Find("TopSideFade");
                if (topSideFade)
                    topSideFade.gameObject.SetActive(false);

                Transform bottomSideFade = self.transform.Find("BottomSideFade");
                if (bottomSideFade)
                    bottomSideFade.gameObject.SetActive(false);

                // Needs to be done next frame for some reason. An outside source seems to be setting it back to camera screen space.
                RoR2Application.onNextUpdate += () => CanvasToWorldSpace(self.gameObject, hdResolution, characterSelectPosition, characterSelectScale, true);
            };
            On.RoR2.UI.PauseScreenController.OnEnable += (orig, self) =>
            {
                orig(self);
                if (!GetUICamera()) return;
                camRotation = new Vector3(0, cachedUICam.transform.eulerAngles.y, 0);
                CanvasToWorldSpace(self.gameObject, hdResolution, menuPosition, menuScale, true, true);
            };
            On.RoR2.UI.SimpleDialogBox.Start += (orig, self) =>
            {
                orig(self);
                CanvasToWorldSpace(self.rootObject, hdResolution, menuPosition, menuScale, true, PauseManager.isPaused);
            };
            On.RoR2.UI.GameEndReportPanelController.Awake += (orig, self) =>
            {
                orig(self);
                if (!GetUICamera()) return;
                camRotation = new Vector3(0, cachedUICam.transform.eulerAngles.y, 0);
                CanvasToWorldSpace(self.gameObject, hdResolution, menuPosition, menuScale, true, true);
            };
            On.RoR2.SplashScreenController.Start += (orig, self) =>
            {
                orig(self);
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = Color.black;
                GameObject splash = GameObject.Find("SpashScreenCanvas");
                if (splash)
                    CanvasToWorldSpace(splash, hdResolution, menuPosition, menuScale, false);
            };
            On.RoR2.UI.InfiniteTowerMenuController.Awake += (orig, self) =>
            {
                orig(self);
                CanvasToWorldSpace(self.gameObject, hdResolution, menuPosition, menuScale, true);
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

            On.RoR2.CreditsController.OnEnable += MoveCreditsToWorldSpace;

            On.RoR2.CameraRigController.OnDestroy += (orig, self) =>
            {
                orig(self);
                if (livHUD) GameObject.Destroy(livHUD.gameObject);
            };

            On.RoR2.GameOverController.GenerateReportScreen += (orig, self, hud) =>
            {
                if (hud != livHUD)
                    return orig(self, hud);
                else
                    return null;
            };

            On.RoR2.PickupPickerController.OnDisplayBegin += MovePickerPanelToWorld;

            On.RoR2.PickupPickerController.OnDisplayEnd += DestroyPickerPanelCanvas;

            On.RoR2.Networking.NetworkManagerSystem.HandleKick_NetworkMessage += (orig, netMsg) =>
            {
                orig(netMsg);
                UnparentDialogBoxInternal();
            };
            On.RoR2.Networking.NetworkManagerSystem.HandleKick_string += (orig, token) =>
            {
                orig(token);
                UnparentDialogBoxInternal();
            };

            On.RoR2.SceneCatalog.OnActiveSceneChanged += ChangeDialogScene;

            On.RoR2.VoidSurvivorController.OnOverlayInstanceAdded += MoveOverlayNextToHealth;

            if (ModConfig.InitialMotionControlsValue)
            {
                On.RoR2.UI.CrosshairManager.UpdateCrosshair += HideCrosshair;

                On.RoR2.UI.SniperTargetViewer.OnTransformParentChanged += FixHUDReference;
            }
        }

        private static void FixHUDReference(On.RoR2.UI.SniperTargetViewer.orig_OnTransformParentChanged orig, SniperTargetViewer self)
        {
            orig(self);
            if (!self.hud) self.hud = Utils.localCameraRig.hud;
        }

        private static void MoveOverlayNextToHealth(On.RoR2.VoidSurvivorController.orig_OnOverlayInstanceAdded orig, VoidSurvivorController self, OverlayController controller, GameObject instance)
        {
            orig(self, controller, instance);

            if (ModConfig.TempWristHUDValue && self.characterBody.IsLocalBody() && Utils.localCameraRig && Utils.localCameraRig.hud)
            {
                RectTransform healthbarTransform = Utils.localCameraRig.hud.healthBar.transform as RectTransform;
                instance.transform.SetParent(healthbarTransform);

                RectTransform instanceTransform = instance.transform as RectTransform;
                RectTransform fillRoot = instanceTransform.Find("FillRoot") as RectTransform;

                instanceTransform.localRotation = Quaternion.identity;
                instanceTransform.sizeDelta = new Vector2(fillRoot.sizeDelta.y, fillRoot.sizeDelta.y);

                fillRoot.localPosition = Vector3.zero;

                instanceTransform.anchorMin = new Vector2(1, 0.5f);
                instanceTransform.anchorMax = new Vector2(1, 0.5f);
                instanceTransform.pivot = new Vector2(1, 0.5f);

                instanceTransform.localPosition = new Vector3(0, healthbarTransform.sizeDelta.y / 2, 0);

                voidFiendCorruptionMeter = instanceTransform;

                RoR2Application.onNextUpdate += MoveOverlayAgainWhyDoINeedToDoThis;
            }
        }

        private static void MoveOverlayAgainWhyDoINeedToDoThis()
        {
            if (voidFiendCorruptionMeter)
            {
                Vector3 pos = voidFiendCorruptionMeter.localPosition;
                pos.x = 0;
                voidFiendCorruptionMeter.localPosition = pos;
            }
        }

        private static void HideCrosshair(On.RoR2.UI.CrosshairManager.orig_UpdateCrosshair orig, CrosshairManager self, CharacterBody targetBody, Vector3 crosshairWorldPosition, Camera uiCamera)
        {
            orig(self, targetBody, crosshairWorldPosition, uiCamera);

            if (self.crosshairController && self.crosshairController.gameObject.activeSelf)
            {
                self.crosshairController.gameObject.SetActive(false);
            }
        }

        private static void ChangeDialogScene(On.RoR2.SceneCatalog.orig_OnActiveSceneChanged orig, Scene oldScene, Scene newScene)
        {
            orig(oldScene, newScene);

            if (newScene.name == "title" && queuedKickDialog)
            {
                SceneManager.MoveGameObjectToScene(queuedKickDialog, newScene);
            }
        }
        
        private static void UnparentDialogBoxInternal()
        {
            SimpleDialogBox dialogBox = RoR2Application.instance.mainCanvas.GetComponentInChildren<SimpleDialogBox>();

            if (dialogBox)
            {
                dialogBox.rootObject.transform.SetParent(null);
                GameObject.DontDestroyOnLoad(dialogBox.rootObject);
                queuedKickDialog = dialogBox.rootObject;
            }
        }

        private static void DestroyPickerPanelCanvas(On.RoR2.PickupPickerController.orig_OnDisplayEnd orig, PickupPickerController self, NetworkUIPromptController networkUIPromptController, LocalUser localUser, CameraRigController cameraRigController)
        {
            orig(self, networkUIPromptController, localUser, cameraRigController);
            if (pickerCanvas) GameObject.Destroy(pickerCanvas);
        }

        private static void MovePickerPanelToWorld(On.RoR2.PickupPickerController.orig_OnDisplayBegin orig, PickupPickerController self, NetworkUIPromptController networkUIPromptController, LocalUser localUser, CameraRigController cameraRigController)
        {
            orig(self, networkUIPromptController, localUser, cameraRigController);

            if (self.panelInstance && GetUICamera())
            {
                pickerCanvas = GameObject.Instantiate(pickerCanvasPrefab);
                pickerCanvas.GetComponent<Canvas>().worldCamera = cachedUICam;
                pickerCanvas.transform.rotation = Quaternion.Euler(0, cachedUICam.transform.eulerAngles.y, 0);
                pickerCanvas.transform.position = pickerCanvas.transform.forward * 4;
                if (ModConfig.InitialRoomscaleValue)
                    pickerCanvas.transform.Translate(0, 1.8f, 0);

                RectTransform panelTransform = self.panelInstance.transform as RectTransform;
                panelTransform.SetParent(pickerCanvas.transform);
                panelTransform.localPosition = Vector3.zero;
                panelTransform.localRotation = Quaternion.identity;
                panelTransform.localScale = Vector3.one;
                panelTransform.offsetMin = Vector2.zero;
                panelTransform.offsetMax = Vector2.zero;

                LeTai.Asset.TranslucentImage.TranslucentImage translucentImage = self.panelInstance.gameObject.GetComponent<LeTai.Asset.TranslucentImage.TranslucentImage>();

                if (translucentImage) translucentImage.enabled = false;
            }
        }

        internal static void CreateLIVHUD(Camera livCamera)
        {
            CameraRigController cameraRig = Utils.localCameraRig;

            if (cameraRig && cameraRig.hud)
            {
                GameObject hudInstance = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/HUDSimple"));
                HUD hud = hudInstance.GetComponent<HUD>();
                hud.cameraRigController = cameraRig;
                Canvas canvas = hud.GetComponent<Canvas>();
                canvas.worldCamera = livCamera;
                canvas.planeDistance = 1f;
                GameObject.Destroy(hud.GetComponent<CrosshairManager>());
                hud.localUserViewer = cameraRig.localUserViewer;

                GameObject.Destroy(hudInstance.transform.Find("MainContainer/MainUIArea/CrosshairCanvas").gameObject);
                GameObject.Destroy(hudInstance.transform.Find("MainContainer/MainUIArea/Hitmarker").gameObject);

                int dummyLayer = LayerIndex.triggerZone.intVal;
                hudInstance.SetLayerRecursive(dummyLayer);

                livHUD = hud;
            }
        }

        private static void MoveCreditsToWorldSpace(On.RoR2.CreditsController.orig_OnEnable orig, CreditsController self)
        {
            orig(self);

            if (creditsCanvasPrefab)
            {
                creditsCanvas = GameObject.Instantiate(creditsCanvasPrefab, null);
                creditsCanvas.transform.position = new Vector3(0, 0, 8);
            }

            if (creditsCanvas && self.creditsPanelController)
            {
                Transform credits = self.creditsPanelController.scrollRect.transform;
                credits.SetParent(creditsCanvas.transform);
                credits.localPosition = Vector3.zero;
                credits.localRotation = Quaternion.identity;
                credits.localScale = Vector3.one;
                GameObject.Destroy(self.creditsPanelController.transform.Find("Backdrop").gameObject);
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
            rect.sizeDelta = gameDetailsResolution;
        }

        private static GameEndReportPanelController UnparentHUD(On.RoR2.GameOverController.orig_GenerateReportScreen orig, GameOverController self, HUD hud)
        {
            hud.transform.SetParent(null);
            hud.canvas.renderMode = RenderMode.ScreenSpaceCamera;
            return orig(self, hud);
        }

        private static bool GetUICamera()
        {
            if (cachedUICam == null || !cachedUICam.isActiveAndEnabled)
            {
                CameraRigController rig = Utils.localCameraRig;
                if (rig)
                {
                    cachedUICam = rig.uiCam;
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

        private static void CanvasToWorldSpace(GameObject uiObject, Vector2 resolution, Vector3 positionOffset, Vector3 scale, bool addCollider, bool followRotation = false)
        {
            if (!GetUICamera()) return;

            Canvas canvas = uiObject.GetComponent<Canvas>();

            if (!canvas)
            {
                canvas = uiObject.GetComponentInParent<Canvas>();
                if (!canvas)
                {
                    VRMod.StaticLogger.LogError("Could not set the render mode of " + uiObject.name + " to world space. Make sure a canvas is present on the object or one of its parents.");
                    VRMod.StaticLogger.LogError(System.Environment.StackTrace);
                    return;
                }
            }

            if (canvas == RoR2Application.instance.mainCanvas)
            {
                VRMod.StaticLogger.LogError("Attempted to set the global main canvas to world space. DO NOT DO THAT!");
                VRMod.StaticLogger.LogError(System.Environment.StackTrace);
                return;
            }

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

                if (ModConfig.InitialRoomscaleValue)
                    offset.y += 1.8f;

                if (uiObject.transform.parent)
                    uiObject.transform.parent.position = offset;

                uiObject.transform.position = offset;
                uiObject.transform.localScale = scale;

                RectTransform rect = uiObject.GetComponent<RectTransform>();
                if (rect)
                {
                    rect.pivot = menuPivot;
                    rect.sizeDelta = resolution;
                }
            }

            if (!addCollider) return;

            BoxCollider collider = canvas.GetComponent<BoxCollider>();
            if (!collider)
            {
                RectTransform rect = canvas.transform as RectTransform;
                collider = canvas.gameObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(rect.sizeDelta.x, rect.sizeDelta.y, 1);
            }
        }

        internal static void AdjustHUD(HUD hud)
        {
            if (ModConfig.InitialMotionControlsValue)
            {
                CrosshairManager crosshairManager = hud.GetComponent<CrosshairManager>();

                if (crosshairManager)
                {
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
            RectTransform rectTransform = hud.transform as RectTransform;
            rectTransform.sizeDelta = new Vector2(ModConfig.HUDWidth.Value, ModConfig.HUDHeight.Value);
            rectTransform.localScale = menuScale;
            rectTransform.SetParent(cachedUICam.transform);
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localPosition = new Vector3(0, 0, 12.35f);
            rectTransform.pivot = menuPivot;

            if (ModConfig.UseSmoothHUD.Value)
                hud.mainContainer.AddComponent<SmoothHUD>().Init(hud.cameraRigController.uiCam.transform);

            if (SceneCatalog.mostRecentSceneDef.sceneType == SceneType.Cutscene) return;

            if (ModConfig.TempWristHUDValue)
            {
                RectTransform healthCluster = springCanvas.Find("BottomLeftCluster/BarRoots") as RectTransform;
                healthCluster.pivot = new Vector2(0.5f, 0f);

                if (ModConfig.BetterHealthBar.Value)
                {
                    healthCluster.SetParent(springCanvas);
                    healthCluster.localRotation = Quaternion.identity;
                    healthCluster.offsetMin = new Vector2(300, healthCluster.offsetMin.y);
                    healthCluster.offsetMax = new Vector2(-300, healthCluster.offsetMax.y);

                    Vector3 notificationPosition = notificationArea.localPosition;
                    notificationPosition.y += 80;
                    notificationArea.localPosition = notificationPosition;

                    RectTransform spectatorLabel = springCanvas.Find("BottomCenterCluster/SpectatorLabel") as RectTransform;

                    Vector3 labelPosition = spectatorLabel.localPosition;
                    labelPosition.y += 68;
                    spectatorLabel.localPosition = labelPosition;
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

            if (ModConfig.TempWatchHUDValue)
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
        }

        private static void UpdateAllHealthBarPositionsVR(On.RoR2.UI.CombatHealthBarViewer.orig_UpdateAllHealthbarPositions orig, RoR2.UI.CombatHealthBarViewer self, Camera sceneCam, Camera uiCam)
        {
            if (sceneCam && uiCam)
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
}
