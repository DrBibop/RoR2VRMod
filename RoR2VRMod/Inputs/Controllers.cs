using Mono.Cecil.Cil;
using MonoMod.Cil;
using Rewired;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;
using VRMod.Inputs;
using VRMod.Inputs.Legacy;

namespace VRMod
{
    public class Controllers
    {
        private static CustomController vrControllers;
        private static CustomControllerMap vrGameplayMap;
        private static CustomControllerMap vrUIMap;

        private static bool hasRecentered;
        private static bool initializedMainPlayer;
        private static bool initializedLocalUser;

        private static List<SkillRemap> skillRemaps = new List<SkillRemap>()
        {
            new SkillRemap("LoaderBody", SkillSlot.Utility, SkillSlot.Special)
        };

        private static BaseInput[] inputs;
        private static List<BaseInput> modInputs = new List<BaseInput>();

        internal static int leftJoystickID { get; private set; }
        internal static int rightJoystickID { get; private set; }

        internal static int ControllerID => vrControllers.id;

        internal static void Init()
        {
            ReInput.InputSourceUpdateEvent += UpdateVRInputs;

            RoR2Application.onUpdate += Update;

            On.RoR2.UI.InputBindingControl.Awake += DisableControllerBinds;

            if (ModConfig.UseMotionControls.Value)
                On.RoR2.UI.MainMenu.MainMenuController.Start += ShowRecenterDialog;

            PlayerCharacterMasterController.onPlayerAdded += SubscribeToBodyEvents;

            On.RoR2.GamepadVibrationManager.Update += VRHaptics;

            if (ModConfig.ControllerMovementDirection.Value)
                IL.RoR2.PlayerCharacterMasterController.Update += ControllerMovementDirection;

            On.RoR2.UI.MainMenu.ProfileMainMenuScreen.SetMainProfile += (orig, self, profile) => 
            {
                orig(self, profile);
                if (initializedLocalUser)
                {
                    initializedLocalUser = false;
                    RoR2Application.onUpdate += Update;
                }
            };

            SetupControllerInputs();
        }

        private static void VRHaptics(On.RoR2.GamepadVibrationManager.orig_Update orig)
        {
            orig();

            LocalUser localUser = LocalUserManager.GetFirstLocalUser();

            if (localUser == null || localUser.userProfile == null || localUser.cameraRigController == null) return;

            float vibrationScale = localUser.userProfile.gamepadVibrationScale;

            Vector3 rawScreenShakeDisplacement = localUser.cameraRigController.rawScreenShakeDisplacement;

            GamepadVibrationManager.MotorValues motorValues = GamepadVibrationManager.CalculateMotorValuesForCameraDisplacement(vibrationScale, rawScreenShakeDisplacement);

            if (ModConfig.OculusMode.Value)
            {
                InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

                HapticCapabilities capabilities;

                if (leftHand.TryGetHapticCapabilities(out capabilities))
                {
                    if (capabilities.supportsImpulse)
                    {
                        leftHand.SendHapticImpulse(0, motorValues.deepMotor, 0.02f);
                    }
                }
                if (rightHand.TryGetHapticCapabilities(out capabilities))
                {
                    if (capabilities.supportsImpulse)
                    {
                        rightHand.SendHapticImpulse(0, motorValues.deepMotor, 0.02f);
                    }
                }
            }
            else
            {
                SteamVR_Actions.gameplay_Haptic.Execute(0, 0, 80, motorValues.quickMotor, SteamVR_Input_Sources.LeftHand);
                SteamVR_Actions.gameplay_Haptic.Execute(0, 0, 80, motorValues.quickMotor, SteamVR_Input_Sources.RightHand);
            }
        }

        public static void AddSkillRemap(string bodyName, SkillSlot skill1, SkillSlot skill2)
        {
            skillRemaps.Add(new SkillRemap(bodyName, skill1, skill2));
        }

        private static void SubscribeToBodyEvents(PlayerCharacterMasterController obj)
        {
            obj.master.onBodyStart += ApplyRemapsFromBody;
            obj.master.onBodyDestroyed += ApplyRemapsFromBody;
        }

        private static void ApplyRemapsFromBody(CharacterBody obj)
        {
            if (obj.master != LocalUserManager.GetFirstLocalUser().cachedMaster) return;
            ApplyRemaps(obj.name.Remove(obj.name.IndexOf("(Clone)")));
        }

        internal static void ApplyRemaps(string bodyName)
        {

            SkillRemap[] remaps = skillRemaps.Where(x => x.bodyName == bodyName).ToArray();

            foreach (SkillRemap remap in remaps)
            {
                ActionElementMap originalMap1 = vrGameplayMap.GetElementMapsWithAction(7 + (int)remap.skill1)[0];
                ActionElementMap originalMap2 = vrGameplayMap.GetElementMapsWithAction(7 + (int)remap.skill2)[0];

                int elementIdentifierId1 = originalMap1.elementIdentifierId;
                int elementIdentifierId2 = originalMap2.elementIdentifierId;

                Player player = LocalUserManager.GetFirstLocalUser().inputPlayer;

                bool result1 = player.controllers.maps.GetMap(vrControllers, vrGameplayMap.id).ReplaceElementMap(originalMap1.id, originalMap1.actionId, originalMap1.axisContribution, elementIdentifierId2, originalMap1.elementType, originalMap1.axisRange, originalMap1.invert);
                bool result2 = player.controllers.maps.GetMap(vrControllers, vrGameplayMap.id).ReplaceElementMap(originalMap2.id, originalMap2.actionId, originalMap2.axisContribution, elementIdentifierId1, originalMap2.elementType, originalMap2.axisRange, originalMap2.invert);

                if (!(result1 && result2))
                {
                    VRMod.StaticLogger.LogError("Failed to remap");
                }    
            }
        }

        private static void ControllerMovementDirection(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(x => x.MatchLdloca(6));
            c.GotoNext(x => x.MatchLdloca(6));

            c.Emit(OpCodes.Ldloc_S, (byte)6);
            c.EmitDelegate<Func<Vector2, Vector2>>((vector) =>
            {
                float angleDifference;

                if (MotionControls.HandsReady)
                {
                    Vector3 controllerDirection = MotionControls.GetHandByDominance(false).muzzle.forward;
                    Vector3 cameraDirection = Camera.main.transform.forward;

                    controllerDirection.y = 0;
                    cameraDirection.y = 0;

                    angleDifference = Vector3.SignedAngle(controllerDirection, cameraDirection, Vector3.up);
                }
                else
                {
                    Quaternion controllerRotation = InputTracking.GetLocalRotation(XRNode.LeftHand);
                    Quaternion headRotation = Camera.main.transform.localRotation;

                    angleDifference = headRotation.eulerAngles.y - controllerRotation.eulerAngles.y;
                }

                return Quaternion.Euler(new Vector3(0, 0, angleDifference)) * vector;
            });
            c.Emit(OpCodes.Stloc_S, (byte)6);
        }

        private static void ShowRecenterDialog(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);

            if (hasRecentered) return;

            hasRecentered = true;

            string glyphString = ControllerGlyphs.GetGlyph(25);
            
            SimpleDialogBox dialogBox = SimpleDialogBox.Create(null);
            dialogBox.headerToken = new SimpleDialogBox.TokenParamsPair()
            {
                token = "Recenter",
                formatParams = null
            };

            ControllerGlyphs.ApplySpriteAsset(dialogBox.descriptionLabel);

            dialogBox.descriptionToken = new SimpleDialogBox.TokenParamsPair()
            {
                token = "Use {0} to recenter your HMD.",
                formatParams = new object[] { glyphString }
            };

            dialogBox.AddCancelButton(CommonLanguageTokens.ok, Array.Empty<object>());
        }

        private static void DisableControllerBinds(On.RoR2.UI.InputBindingControl.orig_Awake orig, InputBindingControl self)
        {
            orig(self);

            if (ModConfig.UseMotionControls.Value && self.inputSource == MPEventSystem.InputSource.Gamepad && self.button)
            {
                self.button.interactable = false;
                self.button = null;
            }
        }

        private static void SetupControllerInputs()
        {
            vrControllers = RewiredAddons.CreateRewiredController();
            vrUIMap = RewiredAddons.CreateUIMap(vrControllers.id);
            vrGameplayMap = RewiredAddons.CreateGameplayMap(vrControllers.id);

            if (ModConfig.OculusMode.Value)
            {
                inputs = new BaseInput[]
                {
                    new LegacyAxisInput(true, 0, false, 0, 4), //LJoyX = MoveHor, UIHor
                    new LegacyAxisInput(true, 1, true, 1, 5), //LJoyY = MoveVer, UIVer
                    new LegacyAxisInput(false, 3, false, 2), //RJoyX = LookHor
                    new LegacyAxisInput(false, 4, true, 3), //RJoyY = LookVer
                    new LegacyButtonInput(true, 2, 12, 19), //X = Equipment, Ready
                    new LegacyReleaseAndHoldableButtonInput(true, 3, 24, 15), //Y = Pause, (Hold)Scoreboard/Profile
                    new LegacyButtonInput(false, 0, 6, 17), //A = Interact, Submit
                    new LegacyButtonInput(false, 1, 7, 18), //B = Jump, Cancel
                    new LegacyButtonInput(true, 8, 13), //LClick = Sprint
                    new LegacyButtonInput(false, 9, 14, 25), //RClick = Ping, Recenter
                    new LegacyAxisToButtonInput(true, 8, 9, 20), //LTrigger = Secondary, Tab Left
                    new LegacyAxisToButtonInput(true, 10, 10, 22), //LGrip = Utility, Submenu Left
                    new LegacyAxisToButtonInput(false, 9, 8, 21), //RTrigger = Primary, Tab Right
                    new LegacyAxisToButtonInput(false, 11, 11, 23) //RGrip = Special, Submenu Right
                };
            }
            else
            {
                inputs = new BaseInput[]
                {
                    new VectorInput(SteamVR_Actions.gameplay_Move, 0, 1),
                    new SimulatedVectorInput(SteamVR_Actions.gameplay_Look, 2, 3, null, SteamVR_Actions.gameplay_LookRight, null, SteamVR_Actions.gameplay_LookLeft),
                    new VectorInput(SteamVR_Actions.ui_Navigate, 4, 5),
                    new ButtonInput(SteamVR_Actions.gameplay_Interact, 6),
                    new ButtonInput(SteamVR_Actions.gameplay_Jump, 7),
                    new ButtonInput(SteamVR_Actions.gameplay_PrimarySkill, 8),
                    new ButtonInput(SteamVR_Actions.gameplay_SecondarySkill, 9),
                    new ButtonInput(SteamVR_Actions.gameplay_UtilitySkill, 10),
                    new ButtonInput(SteamVR_Actions.gameplay_SpecialSkill, 11),
                    new ButtonInput(SteamVR_Actions.gameplay_UseEquipment, 12),
                    new ButtonInput(SteamVR_Actions.gameplay_Sprint, 13),
                    new ButtonInput(SteamVR_Actions.gameplay_Ping, 14),
                    new HoldableButtonInput(SteamVR_Actions.gameplay_ScoreboardAndProfile, 15, SteamVR_Actions.gameplay_HoldScoreboardAndProfile),
                    new ButtonInput(SteamVR_Actions.ui_Submit, 17),
                    new ButtonInput(SteamVR_Actions.ui_Cancel, 18),
                    new ButtonInput(SteamVR_Actions.ui_ReadyAndContinue, 19),
                    new ButtonInput(SteamVR_Actions.ui_TabLeft, 20),
                    new ButtonInput(SteamVR_Actions.ui_TabRight, 21),
                    new ButtonInput(SteamVR_Actions.ui_SubmenuLeft, 22),
                    new ButtonInput(SteamVR_Actions.ui_SubmenuRight, 23),
                    new ReleaseButtonInput(SteamVR_Actions.ui_Pause, 24),
                    new ButtonInput(SteamVR_Actions.ui_RecenterHMD, 25)
                };

                var plugins = BepInEx.Bootstrap.Chainloader.PluginInfos;

                if (plugins.ContainsKey("com.KingEnderBrine.ExtraSkillSlots"))
                {
                    modInputs.Add(new ButtonInput(SteamVR_Actions.gameplay_ExtraSkill1, 26));
                    modInputs.Add(new ButtonInput(SteamVR_Actions.gameplay_ExtraSkill2, 27));
                    modInputs.Add(new ButtonInput(SteamVR_Actions.gameplay_ExtraSkill3, 28));
                    modInputs.Add(new ButtonInput(SteamVR_Actions.gameplay_ExtraSkill4, 29));
                }
                if (plugins.ContainsKey("com.evaisa.r2voicechat"))
                {
                    modInputs.Add(new ButtonInput(SteamVR_Actions.gameplay_PushToTalk, 30));
                }
                if (plugins.ContainsKey("com.cwmlolzlz.skills"))
                {
                    modInputs.Add(new ButtonInput(SteamVR_Actions.gameplay_BuySkill, 31));
                }
                if (plugins.ContainsKey("com.KingEnderBrine.ProperSave"))
                {
                    modInputs.Add(new ButtonInput(SteamVR_Actions.ui_ProperSaveLoad, 32));
                }
            }
        }

        private static void Update()
        {
            LocalUser localUser = LocalUserManager.GetFirstLocalUser();

            if (localUser != null)
            {
                if (AddVRController(localUser.inputPlayer))
                {
                    initializedLocalUser = true;
                    RoR2Application.onUpdate -= Update;
                }
            }
            else if (!initializedMainPlayer)
            {
                if (AddVRController(LocalUserManager.GetRewiredMainPlayer()))
                    initializedMainPlayer = true;
            }
        }

        internal static bool AddVRController(Player inputPlayer)
        {
            if (!inputPlayer.controllers.ContainsController(vrControllers))
            {
                inputPlayer.controllers.AddController(vrControllers, false);
                vrControllers.SetEnabled(true);
            }

            if (inputPlayer.controllers.maps.GetAllMaps(ControllerType.Custom).ToList().Count < 2)
            {
                if (inputPlayer.controllers.maps.GetMap(ControllerType.Custom, vrControllers.id, 2, 0) == null)
                    inputPlayer.controllers.maps.AddMap(vrControllers, vrUIMap);
                if (inputPlayer.controllers.maps.GetMap(ControllerType.Custom, vrControllers.id, 0, 0) == null)
                    inputPlayer.controllers.maps.AddMap(vrControllers, vrGameplayMap);
                if (!vrGameplayMap.enabled)
                    vrGameplayMap.enabled = true;
                if (!vrUIMap.enabled)
                    vrUIMap.enabled = true;
            }

            return inputPlayer.controllers.ContainsController(vrControllers) && inputPlayer.controllers.maps.GetAllMaps(ControllerType.Custom).ToList().Count >= 2;
        }

        private static void UpdateVRInputs()
        {
            if (ModConfig.OculusMode.Value)
            {
                string[] joyNames = Input.GetJoystickNames();

                if (!joyNames[leftJoystickID].ToLower().Contains("left") || !joyNames[rightJoystickID].ToLower().Contains("right"))
                {
                    leftJoystickID = -1;
                    rightJoystickID = -1;
                    for (int i = 0; i < joyNames.Length; i++)
                    {
                        string joyName = joyNames[i].ToLower();
                        if (joyName.Contains("left"))
                        {
                            leftJoystickID = i;
                        }

                        if (joyName.Contains("right"))
                        {
                            rightJoystickID = i;
                        }
                    }

                    if (leftJoystickID == -1 || rightJoystickID == -1) return;
                }
            }

            foreach (BaseInput input in inputs)
            {
                input.UpdateValues(vrControllers);
            }

            foreach (BaseInput input in modInputs)
            {
                input.UpdateValues(vrControllers);
            }
        }

        public struct SkillRemap
        {
            public string bodyName;
            public SkillSlot skill1;
            public SkillSlot skill2;

            public SkillRemap(string bodyName, SkillSlot skill1, SkillSlot skill2)
            {
                this.bodyName = bodyName;
                this.skill1 = skill1;
                this.skill2 = skill2;
            }
        }
    }
}
