using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using RoR2.CameraModes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.PostProcessing;

namespace VRMod
{
    public static class CameraFixes
    {
        private static bool isTurningLeft;
        private static bool wasTurningLeft;

        private static bool isTurningRight;
        private static bool wasTurningRight;

        private static bool justTurnedLeft => isTurningLeft && (!wasTurningLeft || timeSinceLastSnapTurn > ModConfig.SnapTurnHoldDelay.Value);
        private static bool justTurnedRight => isTurningRight && (!wasTurningRight || timeSinceLastSnapTurn > ModConfig.SnapTurnHoldDelay.Value);

        private static float timeSinceLastSnapTurn = 0f;

        private static int lastFrameCount = -1;

        private static GameObject spectatorCamera;
        private static GameObject spectatorScreen;

        private static Camera spectatorCameraComponent;

        private static GameObject spectatorCameraPrefab;
        private static GameObject spectatorScreenPrefab;

        private static Transform cachedCameraTargetTransform;

        private static List<ForcedVisibleRenderer> forcedVisibleRenderers;

        internal static LIV.SDK.Unity.LIV liv { get; private set; }

        private static Transform cameraOffset;

        internal static void Init()
        {
            spectatorCameraPrefab = VRMod.VRAssetBundle.LoadAsset<GameObject>("SpectatorCamera");
            spectatorCameraPrefab.GetComponent<Camera>().stereoTargetEye = StereoTargetEyeMask.None;

            spectatorScreenPrefab = VRMod.VRAssetBundle.LoadAsset<GameObject>("SpectatorScreen");

            On.RoR2.MatchCamera.Awake += (orig, self) =>
            {
                self.matchFOV = false;
                orig(self);
            };

            IL.RoR2.CameraRigController.SetCameraState += RemoveFOVChange;
            On.RoR2.CameraRigController.SetCameraState += SetCameraStateOverride;

            IL.RoR2.CameraRigController.Update += UpdateLIVHUDTargetMaster;

            IL.RoR2.CameraModes.CameraModePlayerBasic.CollectLookInputInternal += GetVRLookInput;
            IL.RoR2.CameraModes.CameraModePlayerBasic.UpdateInternal += RemoveRecoilAndCameraPitch;

            On.RoR2.CameraRigController.Start += InitCamera;

            if (ModConfig.InitialFirstPersonValue)
            {
                On.RoR2.Run.Update += SetBodyInvisible;

                On.RoR2.CameraRigController.OnDestroy += (orig, self) =>
                {
                    orig(self);
                    if (VRCameraWrapper.instance)
                        GameObject.Destroy(VRCameraWrapper.instance.gameObject);
                };

                On.RoR2.CharacterModel.SetEquipmentDisplay += HideFloatingEquipment;

                On.RoR2.ItemFollower.Start += HideFloatingItems;

                On.RoR2.HealingFollowerController.OnStartClient += HideWoodsprite;

                IL.RoR2.CharacterBody.UpdateSingleTemporaryVisualEffect_refTemporaryVisualEffect_GameObject_float_bool_string += HideTempEffect;
                IL.RoR2.CharacterBody.UpdateSingleTemporaryVisualEffect_refTemporaryVisualEffect_string_float_bool_string += HideTempEffect;

                On.EntityStates.VagrantNovaItem.BaseVagrantNovaItemState.OnEnter += HideSparks;

                On.EntityStates.LaserTurbine.LaserTurbineBaseState.OnEnter += HideDisc;

                On.RoR2.UI.HudObjectiveTargetSetter.OnEnable += FixHUDReference;

                IL.RoR2.CameraModes.CameraModePlayerBasic.ApplyLookInputInternal += ChangeNetworkAngles;
            }

            On.RoR2.CameraModes.CameraModePlayerBasic.GetCrosshairRaycastRay += GetVRCrosshairRaycastRay;

            new ILHook(typeof(ThreeEyedGames.DecaliciousRenderer).GetMethod("OnPreRender", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), OnPreRenderIL);

            On.RoR2.SurvivorPodController.UpdateCameras += (orig, self, gameObject) => { };

            On.RoR2.CameraRigController.SetOverrideCam += RemoveCameraLerp;

            On.RoR2.PositionAlongBasicBezierSpline.Update += ForceEndOfCurve;

            if (ModConfig.InitialRoomscaleValue && !ModConfig.InitialOculusModeValue)
                LIV.SDK.Unity.SDKShaders.LoadShaders();

            RoR2Application.onLoad += ReplaceLoaderSmokeRampTextures;

            new Hook(typeof(PostProcessProfile).GetMethod("OnEnable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), (Action<Action<PostProcessProfile>, PostProcessProfile>)RemoveDepthOfField);
        }

        private static void RemoveDepthOfField(Action<PostProcessProfile> orig, PostProcessProfile self)
        {
            orig(self);

            DepthOfField dofSetting = self.GetSetting<DepthOfField>();

            if (dofSetting)
            {
                dofSetting.active = false;
            }
        }

        private static void ChangeNetworkAngles(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(x => x.MatchStfld<RoR2.Networking.NetworkedViewAngles>("viewAngles"));
            c.Index -= 2;

            c.RemoveRange(2);

            c.EmitDelegate<Func<RoR2.PitchYawPair>>(() => 
            {
                Vector3 lookVector = Camera.main.transform.forward;
                float x = Mathf.Sqrt(lookVector.x * lookVector.x + lookVector.z * lookVector.z);
                return new PitchYawPair(Mathf.Atan2(-lookVector.y, x) * 57.29578f, Mathf.Repeat(Mathf.Atan2(lookVector.x, lookVector.z) * 57.29578f, 360f));
            });
        }

        private static void FixHUDReference(On.RoR2.UI.HudObjectiveTargetSetter.orig_OnEnable orig, RoR2.UI.HudObjectiveTargetSetter self)
        {
            orig(self);

            if (self.hud == null && Utils.localCameraRig && Utils.localCameraRig.hud)
                self.hud = Utils.localCameraRig.hud;
        }

        private static void RemoveRecoilAndCameraPitch(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            
            c.GotoNext(x => x.MatchLdfld(typeof(CharacterCameraParamsData), "minPitch"));

            c.RemoveRange(2);

            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Func<CharacterCameraParamsData, CameraModePlayerBasic, float>>((camParams, camMode) => 
            {
                return ModConfig.TempLockedCameraPitchValue && !camMode.isSpectatorMode ? 0 : camParams.minPitch.value;
            });

            c.GotoNext(x => x.MatchLdfld(typeof(CharacterCameraParamsData), "maxPitch"));

            c.RemoveRange(2);

            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Func<CharacterCameraParamsData, CameraModePlayerBasic, float>>((camParams, camMode) =>
            {
                return ModConfig.TempLockedCameraPitchValue && !camMode.isSpectatorMode ? 0 : camParams.maxPitch.value;
            });

            c.GotoNext(x => x.MatchLdloc(10));

            c.RemoveRange(10);
        }

        private static void UpdateLIVHUDTargetMaster(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(x => x.MatchCallvirt(typeof(RoR2.UI.HUD), "set_targetMaster"));

            c.Index++;
            c.Emit(OpCodes.Ldloc_S, (byte)5);
            c.EmitDelegate<Action<CharacterMaster>>((master) => 
            {
                if (UIFixes.livHUD) UIFixes.livHUD.targetMaster = master;
            });
        }

        private static void ReplaceLoaderSmokeRampTextures()
        {
            Texture2D smokeRamp = VRMod.VRAssetBundle.LoadAsset<Texture2D>("SmokeRamp"); ;

            EntityStateConfiguration state = LegacyResourcesAPI.Load<EntityStateConfiguration>("entitystateconfigurations/EntityStates.Loader.BaseChargeFist");

            if (state)
            {
                HG.GeneralSerializer.SerializedField effectField = state.serializedFieldsCollection.serializedFields.First(x => x.fieldName == "chargeVfxPrefab");

                GameObject effectPrefab = effectField.fieldValue.objectValue as GameObject;

                Transform dustObject = effectPrefab.transform.Find("Dust");

                if (dustObject)
                {
                    ReplaceSmokeRampTexture(dustObject, smokeRamp);
                }
            }

            state = LegacyResourcesAPI.Load<EntityStateConfiguration>("entitystateconfigurations/EntityStates.Loader.BaseSwingChargedFist");

            if (state)
            {
                HG.GeneralSerializer.SerializedField effectField = state.serializedFieldsCollection.serializedFields.First(x => x.fieldName == "swingEffectPrefab");

                GameObject effectPrefab = effectField.fieldValue.objectValue as GameObject;

                Transform dustObject = effectPrefab.transform.Find("Dust");

                if (dustObject)
                {
                    ReplaceSmokeRampTexture(dustObject, smokeRamp);
                }
            }

            GameObject hookPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/LoaderHook");

            if (hookPrefab)
            {
                Transform dustObject = hookPrefab.transform.Find("FistMesh/RopeFront/Dust");

                if (dustObject)
                {
                    ReplaceSmokeRampTexture(dustObject, smokeRamp);
                }

                dustObject = hookPrefab.transform.Find("RopeEnd/Dust");

                if (dustObject)
                {
                    ReplaceSmokeRampTexture(dustObject, smokeRamp);
                }
            }
        }

        private static void ReplaceSmokeRampTexture(Transform effect, Texture2D rampTexture)
        {
            Material smokeMat = effect.GetComponent<ParticleSystemRenderer>().material;

            if (smokeMat)
            {
                smokeMat.SetTexture("_RemapTex", rampTexture);
                smokeMat.SetFloat("_AlphaBias", 0.005f);
                smokeMat.SetFloat("_InvFade", 0f);
            }
        }

        private static void ForceEndOfCurve(On.RoR2.PositionAlongBasicBezierSpline.orig_Update orig, PositionAlongBasicBezierSpline self)
        {
            if (self.GetComponent<PlayableDirector>())
            {
                self.normalizedPositionAlongCurve = 1;
            }
            orig(self);
        }

        private static void RemoveCameraLerp(On.RoR2.CameraRigController.orig_SetOverrideCam orig, CameraRigController self, ICameraStateProvider newOverrideCam, float lerpDuration)
        {
            orig(self, newOverrideCam, 0);
        }

        private static void OnPreRenderIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(x => x.MatchCall<ThreeEyedGames.DecaliciousRenderer>("DrawUnlitDecals"));

            c.Index++;

            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Func<ThreeEyedGames.DecaliciousRenderer, bool>>((self) =>
            {
                if (self._camera.stereoTargetEye == StereoTargetEyeMask.None) return true;

                bool result = lastFrameCount == Time.renderedFrameCount;
                if (result)
                {
                    lastFrameCount = Time.renderedFrameCount;
                }
                return result;
            });

            int lastIndex = c.Index;

            c.GotoNext(x => x.MatchLdfld<ThreeEyedGames.DecaliciousRenderer>("_limitToGameObjects"));

            c.Index--;

            ILLabel label = c.MarkLabel();

            c.Index = lastIndex;

            c.Emit(OpCodes.Brfalse_S, label);
        }

        private static void HideFloatingItems(On.RoR2.ItemFollower.orig_Start orig, ItemFollower self)
        {
            CharacterModel componentInParent = self.GetComponentInParent<CharacterModel>();
            if (componentInParent)
            {
                if (componentInParent.body.IsLocalBody())
                {
                    self.enabled = false;
                    return;
                }
            }

            orig(self);
        }

        private static void HideDisc(On.EntityStates.LaserTurbine.LaserTurbineBaseState.orig_OnEnter orig, EntityStates.LaserTurbine.LaserTurbineBaseState self)
        {
            orig(self);
            if (self.ownerBody.IsLocalBody())
            {
                self.laserTurbineController.showTurbineDisplay = false;
            }
        }

        private static void HideSparks(On.EntityStates.VagrantNovaItem.BaseVagrantNovaItemState.orig_OnEnter orig, EntityStates.VagrantNovaItem.BaseVagrantNovaItemState self)
        {
            orig(self);
            if (self.attachedBody.IsLocalBody())
            {
                if (self.chargeSparks)
                {
                    self.chargeSparks.Stop();
                }
            }
        }

        private static void HideTempEffect(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(x => x.MatchStfld<TemporaryVisualEffect>("radius"));

            c.Index++;

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldind_Ref);
            c.EmitDelegate<Action<CharacterBody, TemporaryVisualEffect>>((body, effect) =>
            {
                if (body.IsLocalBody())
                {
                    Renderer[] renderers = effect.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        renderer.enabled = false;
                    }
                }
            });
        }

        /// <summary>
        /// Prevents the mod from disabling the specified renderer in the body.
        /// </summary>
        /// <param name="bodyName">The name of the character body object.</param>
        /// <param name="rendererObjectName">The name of the renderer object.</param>
        public static void PreventRendererDisable(string bodyName, string rendererObjectName)
        {
            if (forcedVisibleRenderers == null)
                forcedVisibleRenderers = new List<ForcedVisibleRenderer>();

            forcedVisibleRenderers.Add(new ForcedVisibleRenderer(bodyName, rendererObjectName));
        }

        private static void HideWoodsprite(On.RoR2.HealingFollowerController.orig_OnStartClient orig, HealingFollowerController self)
        {
            orig(self);

            if (self.ownerBodyObject != LocalUserManager.GetFirstLocalUser().cachedBodyObject) return;

            Renderer[] renderers = self.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }
        }

        private static void HideFloatingEquipment(On.RoR2.CharacterModel.orig_SetEquipmentDisplay orig, CharacterModel self, EquipmentIndex newEquipmentIndex)
        {
            EquipmentIndex[] equipmentsToHide = new EquipmentIndex[]
            {
                RoR2Content.Equipment.Blackhole.equipmentIndex,
                RoR2Content.Equipment.Saw.equipmentIndex,
                RoR2Content.Equipment.PassiveHealing.equipmentIndex,
                RoR2Content.Equipment.Meteor.equipmentIndex
            };
            
            if (!self.body.master.IsLocalMaster() || !equipmentsToHide.Contains(newEquipmentIndex))
            {
                orig(self, newEquipmentIndex);
            }
        }

        private static void InitCamera(On.RoR2.CameraRigController.orig_Start orig, CameraRigController self)
        {
            orig(self);

            if (self.sceneCam.cullingMask == (self.sceneCam.cullingMask | (1 << LayerIndex.triggerZone.intVal)))
                self.sceneCam.cullingMask &= ~(1 << LayerIndex.triggerZone.intVal);

            if (self.sceneCam.cullingMask == (self.sceneCam.cullingMask | (1 << LayerIndex.noDraw.intVal)))
                self.sceneCam.cullingMask &= ~(1 << LayerIndex.noDraw.intVal);

            if (self.gameObject.scene.name == "intro" && self.sceneCam.cullingMask == (self.sceneCam.cullingMask | (1 << LayerIndex.ui.intVal)))
                self.sceneCam.cullingMask &= ~(1 << LayerIndex.ui.intVal);

            if (Run.instance && ModConfig.UseConfortVignette.Value)
            {
                self.uiCam.gameObject.AddComponent<ConfortVignette>();
            }

            GameObject cameraOffsetObject = new GameObject("Camera Offset");
            cameraOffset = cameraOffsetObject.transform;
            cameraOffset.transform.SetParent(self.transform);
            cameraOffset.transform.localPosition = Vector3.zero;
            cameraOffset.transform.localRotation = Quaternion.identity;
            cameraOffset.transform.localScale = Vector3.one;

            self.sceneCam.transform.SetParent(cameraOffset.transform);

            if (ModConfig.InitialRoomscaleValue && !Run.instance)
            {
                self.currentCameraState = self.desiredCameraState;
            }

            if (!ModConfig.InitialOculusModeValue && ModConfig.InitialRoomscaleValue)
            {
                if (liv) GameObject.Destroy(liv);

                liv = self.gameObject.AddComponent<LIV.SDK.Unity.LIV>();
                liv.stage = self.transform;
                liv.stageTransform = cameraOffset.transform;
                liv.HMDCamera = self.sceneCam;
                liv.excludeBehaviours = new string[]
                {
                "AkAudioListener",
                "Rigidbody",
                "AkGameObj",
                "CameraResolutionScaler",
                "TranslucentImageSource"
                };
                liv.spectatorLayerMask = self.sceneCam.cullingMask;

                liv.enabled = true;
            }

            if (self.hud)
                UIFixes.AdjustHUD(self.hud);

            RoR2Application.instance.mainCanvas.worldCamera = self.uiCam;

            if (FocusChecker.instance) FocusChecker.instance.UpdateCameraRig(self);

            if (UIFixes.queuedKickDialog)
            {
                Canvas dialogCanvas = UIFixes.queuedKickDialog.GetComponentInChildren<Canvas>();

                if (dialogCanvas)
                    dialogCanvas.worldCamera = self.uiCam;
            }
        }

        private static void GetVRLookInput(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            //Remove target info from stack. Why is it pushed so early anyway?
            c.GotoNext(x => 
                x.MatchLdflda(typeof(RoR2.CameraModes.CameraModeBase.CameraModeContext), "targetInfo")
            );

            c.Index--;

            c.RemoveRange(2);

            //Fixing brtrue label
            c.GotoNext(x =>
                x.MatchLdloca(6)
            );

            ILLabel sussyLabel = c.IncomingLabels.First();

            c.Index++;
            sussyLabel.Target = c.Next;
            c.Index--;

            //Replacing input vectors
            c.Remove();
            c.Index++;
            c.RemoveRange(6);

            c.EmitDelegate<Func<Rewired.Player, Vector2>>((player) => 
            {
                return ModConfig.InitialMotionControlsValue ? Vector2.zero : new Vector2(player.GetAxisRaw(ModConfig.SnapTurn.Value ? 26 : 2), ModConfig.TempLockedCameraPitchValue ? 0 : player.GetAxisRaw(3));
            });
            c.Emit(OpCodes.Stloc_S, (byte)6);

            c.Remove();
            c.Index++;
            c.RemoveRange(6);

            c.EmitDelegate<Func<Rewired.Player, Vector2>>((player) =>
            {
                return new Vector2(player.GetAxisRaw(16), ModConfig.TempLockedCameraPitchValue ? 0 : player.GetAxisRaw(17));
            });
            c.Emit(OpCodes.Stloc_S, (byte)7);

            int startIndex = c.Index;

            //Removing aim assist
            c.GotoNext(x => x.MatchCall(typeof(RoR2.CameraModes.CameraModePlayerBasic), "PerformAimAssist"));

            c.Index -= 3;

            c.RemoveRange(4);

            //Adding snap turn code
            c.GotoNext(x => x.MatchLdflda(typeof(RoR2.CameraModes.CameraModeBase.CollectLookInputResult), "lookInput"));

            c.Index--;

            int snapTurnIndex = c.Index;

            c.Emit(OpCodes.Ldarg_3);
            c.Emit(OpCodes.Ldloc_S, (byte)6);
            c.Emit(OpCodes.Ldloc_S, (byte)7);
            c.EmitDelegate<Func<Vector2, Vector2, Vector2>>((mouseVector, joystickVector) =>
            {
                wasTurningLeft = isTurningLeft;
                wasTurningRight = isTurningRight;

                isTurningLeft = joystickVector.x < -0.8f;
                isTurningRight = joystickVector.x > 0.8f;

                if (!ModConfig.MotionControlsEnabled)
                {
                    isTurningLeft = isTurningLeft || mouseVector.x < -0.8f;
                    isTurningRight = isTurningRight || mouseVector.x > 0.8f;
                }

                Vector2 result = Vector2.zero;

                if (justTurnedLeft)
                    result.x = -ModConfig.SnapTurnAngle.Value;
                else if (justTurnedRight)
                    result.x = ModConfig.SnapTurnAngle.Value;

                if ((isTurningLeft || isTurningRight) && timeSinceLastSnapTurn <= ModConfig.SnapTurnHoldDelay.Value)
                {
                    timeSinceLastSnapTurn += Time.deltaTime;
                }
                else
                {
                    timeSinceLastSnapTurn = 0;
                }

                return result;
            });
            c.Emit(OpCodes.Stfld, typeof(RoR2.CameraModes.CameraModeBase.CollectLookInputResult).GetField("lookInput"));
            
            //Removing sensitivity modifications;
            var labels = il.GetIncomingLabels(c.Next);

            c.RemoveRange(24);

            foreach (var label in labels)
            {
                label.Target = c.Next;
            }

            //Adding jump after smooth turn code
            ILLabel endLabel = c.MarkLabel();

            c.Index = snapTurnIndex;

            c.Emit(OpCodes.Br_S, endLabel);

            //Adding snap turn condition and jump to snap turn
            ILLabel snapTurnLabel = c.MarkLabel();

            c.Index = startIndex;

            c.EmitDelegate<Func<bool>>(() => { return ModConfig.SnapTurn.Value; });

            c.Emit(OpCodes.Brtrue_S, snapTurnLabel);
        }

        private static void SetBodyInvisible(On.RoR2.Run.orig_Update orig, Run self)
        {
            CharacterBody cachedBody = Utils.localBody;

            if (cachedBody)
            {
                Renderer[] renderers = cachedBody.modelLocator?.modelTransform?.gameObject.GetComponentsInChildren<Renderer>();

                if (forcedVisibleRenderers != null)
                {
                    string currentBodyName = cachedBody.name.Substring(0, cachedBody.name.IndexOf("(Clone)"));

                    ForcedVisibleRenderer[] visibleBodyRenderers = forcedVisibleRenderers.Where(x => x.bodyName == currentBodyName).ToArray();

                    renderers = renderers.Where(x => !Array.Exists(visibleBodyRenderers, vren => vren.rendererObjectName == x.gameObject.name)).ToArray();
                }
                
                if (renderers != null)
                {
                    foreach (Renderer renderer in renderers)
                    {
                        if (renderer.gameObject.activeSelf)
                        {
                            renderer.gameObject.SetActive(false);
                        }
                    }
                }
            }

            orig(self);
        }
        
        private static void SetCameraStateOverride(On.RoR2.CameraRigController.orig_SetCameraState orig, CameraRigController self, CameraState cameraState)
        {
            if (Run.instance)
            {
                if (self.cameraMode is RoR2.CameraModes.CameraModePlayerBasic)
                {
                    if (ModConfig.InitialFirstPersonValue)
                    {
                        if (!VRCameraWrapper.instance)
                        {
                            GameObject wrapperObject = new GameObject("VR Camera Rig");
                            VRCameraWrapper.instance = wrapperObject.AddComponent<VRCameraWrapper>();
                            VRCameraWrapper.instance.Init(self);
                        }

                        if (!self.cameraMode.IsSpectating(self) && self.cameraModeContext.targetInfo.isViewerControlled && self.targetBody)
                        {
                            VRCameraWrapper.instance.UpdateRotation(cameraState);
                            cameraState.rotation = self.sceneCam.transform.rotation;
                        }

                        if (self.targetBody.IsLocalBody())
                        {
                            if (!cachedCameraTargetTransform)
                            {
                                if (!ModConfig.InitialRoomscaleValue)
                                {
                                    ChildLocator childLocator = self.targetBody.modelLocator.modelTransform.GetComponent<ChildLocator>();
                                    if (childLocator)
                                    {
                                        cachedCameraTargetTransform = childLocator.FindChild("VRCamera");
                                    }
                                }

                                if (!cachedCameraTargetTransform)
                                {
                                    cachedCameraTargetTransform = new GameObject("VRCamera").transform;
                                    cachedCameraTargetTransform.SetParent(self.targetBody.transform);
                                    cachedCameraTargetTransform.localPosition = Vector3.zero;
                                    cachedCameraTargetTransform.localRotation = Quaternion.identity;

                                    CapsuleCollider collider = self.targetBody.GetComponent<CapsuleCollider>();

                                    if (collider)
                                    {
                                        if (ModConfig.InitialRoomscaleValue)
                                        {
                                            cachedCameraTargetTransform.Translate(collider.center + new Vector3(0, -collider.height / 2, 0), Space.Self);
                                        }
                                        else
                                        {
                                            cachedCameraTargetTransform.Translate(collider.center + new Vector3(0, (-collider.height / 2) + (ModConfig.HeightMultiplier.Value * collider.height), 0), Space.Self);
                                        }
                                        VRCameraWrapper.instance.transform.localScale = Vector3.one * ModConfig.HeightMultiplier.Value * (collider.height / 1.82f);
                                    }
                                }
                            }

                            VRCameraWrapper.instance.transform.position = self.hasOverride ? cameraState.position : cachedCameraTargetTransform.position;
                        }
                    }
                }

                if (!self.targetBody.IsLocalBody() && (SceneCatalog.mostRecentSceneDef.sceneType == SceneType.Stage || SceneCatalog.mostRecentSceneDef.sceneType == SceneType.Intermission) && Run.instance.livingPlayerCount > 0)
                {
                    if (!spectatorCamera)
                    {
                        Camera cameraReference = Camera.main;

                        bool cameraReferenceEnabled = cameraReference.enabled;
                        if (cameraReferenceEnabled)
                        {
                            cameraReference.enabled = false;
                        }
                        bool cameraReferenceActive = cameraReference.gameObject.activeSelf;
                        if (cameraReferenceActive)
                        {
                            cameraReference.gameObject.SetActive(false);
                        }

                        spectatorCamera = GameObject.Instantiate(cameraReference.gameObject, null);
                        
                        Component[] components = spectatorCamera.GetComponents<Component>();

                        foreach (Component component in components)
                        {
                            if (!(component is Transform) && !(component is Camera) && !(component is PostProcessLayer) && !(component is RoR2.PostProcess.SobelCommandBuffer && !(component is ThreeEyedGames.DecaliciousRenderer)))
                            {
                                Component.Destroy(component);
                            }
                        }

                        spectatorCameraComponent = spectatorCamera.GetComponent<Camera>();
                        spectatorCameraComponent.stereoTargetEye = StereoTargetEyeMask.None;
                        spectatorCameraComponent.targetTexture = spectatorCameraPrefab.GetComponent<Camera>().targetTexture;

                        if (cameraReferenceActive != cameraReference.gameObject.activeSelf)
                        {
                            cameraReference.gameObject.SetActive(cameraReferenceActive);
                        }
                        if (cameraReferenceEnabled != cameraReference.enabled)
                        {
                            cameraReference.enabled = cameraReferenceEnabled;
                        }

                        spectatorCamera.SetActive(true);
                        spectatorCameraComponent.enabled = true;
                        UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(spectatorCameraComponent, true);
                    }

                    if (!spectatorScreen)
                    { 
                        spectatorScreen = GameObject.Instantiate(spectatorScreenPrefab, null);
                        Utils.SetLayerRecursive(spectatorScreen, LayerIndex.ui.intVal);
                        spectatorScreen.transform.rotation = Quaternion.Euler(new Vector3(0, self.uiCam.transform.eulerAngles.y, 0));
                        spectatorScreen.transform.position = self.uiCam.transform.position + spectatorScreen.transform.forward * 2;
                    }

                    spectatorCamera.transform.position = cameraState.position;
                    spectatorCamera.transform.rotation = cameraState.rotation;
                    spectatorCameraComponent.fieldOfView = cameraState.fov;
                }
                else
                {
                    if (spectatorCamera)
                        GameObject.Destroy(spectatorCamera);

                    if (spectatorScreen)
                        GameObject.Destroy(spectatorScreen);
                }
            }

            orig(self, cameraState);
        }

        private static void RemoveFOVChange(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<CameraState>("fov")
                );

            c.Index -= 6;

            c.RemoveRange(9);
        }

        private static Ray GetVRCrosshairRaycastRay(On.RoR2.CameraModes.CameraModePlayerBasic.orig_GetCrosshairRaycastRay orig, RoR2.CameraModes.CameraModePlayerBasic self, ref RoR2.CameraModes.CameraModeBase.CameraModeContext context, Vector2 crosshairOffset, Vector3 raycastStartPlanePoint, ref CameraState cameraState)
        {
            if (MotionControls.HandsReady)
            {
                return MotionControls.GetHandByDominance(true).aimRay;
            }

            Camera sceneCam = context.cameraInfo.sceneCam;

            if (!sceneCam)
            {
                return default(Ray);
            }
            float fov = sceneCam.fieldOfView;
            float num = fov * sceneCam.aspect;
            Quaternion quaternion = Quaternion.Euler(crosshairOffset.y * fov, crosshairOffset.x * num, 0f);
            quaternion = sceneCam.transform.rotation * quaternion;
            return new Ray(Vector3.ProjectOnPlane(sceneCam.transform.position - raycastStartPlanePoint, sceneCam.transform.rotation * Vector3.forward) + raycastStartPlanePoint, quaternion * Vector3.forward);
        }

        internal struct ForcedVisibleRenderer
        {
            internal string bodyName;
            internal string rendererObjectName;

            internal ForcedVisibleRenderer(string bodyName, string rendererObjectName)
            {
                this.bodyName = bodyName;
                this.rendererObjectName = rendererObjectName;
            }
        }
    }
}
