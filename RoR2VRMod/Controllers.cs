using Mono.Cecil.Cil;
using MonoMod.Cil;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
        private static CustomControllerMap vrDefaultMap;
        private static CustomControllerMap vrUIMap;

        private static bool isUsingMotionControls;
        private static bool hasRecentered;
        private static bool initializedMainPlayer;

        private static TMP_SpriteAsset glyphsSpriteAsset;

        internal static string[] inputGlyphs;

        private static List<SkillRemap> skillRemaps = new List<SkillRemap>()
        {
            new SkillRemap("LoaderBody", SkillSlot.Utility, SkillSlot.Special)
        };

        private static BaseInput[] inputs;

        internal static int leftJoystickID { get; private set; }
        internal static int rightJoystickID { get; private set; }

        internal static void Init()
        {
            ReInput.InputSourceUpdateEvent += UpdateVRInputs;

            RoR2Application.onUpdate += Update;

            On.RoR2.UI.MPEventSystem.OnLastActiveControllerChanged += ChangedToCustom;

            On.RoR2.Glyphs.GetGlyphString_MPEventSystem_string_AxisRange_InputSource += GetCustomGlyphString;

            On.RoR2.InputBindingDisplayController.Awake += ApplySpriteAsset;

            On.RoR2.UI.ContextManager.Awake += ApplyContextSpriteAsset;

            On.RoR2.UI.InputBindingControl.Awake += DisableControllerBinds;

            On.RoR2.UI.MainMenu.MainMenuController.Start += ShowRecenterDialog;

            PlayerCharacterMasterController.onPlayerAdded += SubscribeToBodyEvents;

            if (ModConfig.ControllerMovementDirection.Value)
                IL.RoR2.PlayerCharacterMasterController.Update += ControllerMovementDirection;

            SetupControllerInputs();

            glyphsSpriteAsset = VRMod.VRAssetBundle.LoadAsset<TMP_SpriteAsset>("sprVRGlyphs");
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
                ActionElementMap originalMap1 = vrDefaultMap.GetElementMapsWithAction(7 + (int)remap.skill1)[0];
                ActionElementMap originalMap2 = vrDefaultMap.GetElementMapsWithAction(7 + (int)remap.skill2)[0];

                int elementIdentifierId1 = originalMap1.elementIdentifierId;
                int elementIdentifierId2 = originalMap2.elementIdentifierId;

                Player player = LocalUserManager.GetFirstLocalUser().inputPlayer;

                bool result1 = player.controllers.maps.GetMap(vrControllers, vrDefaultMap.id).ReplaceElementMap(originalMap1.id, originalMap1.actionId, originalMap1.axisContribution, elementIdentifierId2, originalMap1.elementType, originalMap1.axisRange, originalMap1.invert);
                bool result2 = player.controllers.maps.GetMap(vrControllers, vrDefaultMap.id).ReplaceElementMap(originalMap2.id, originalMap2.actionId, originalMap2.axisContribution, elementIdentifierId1, originalMap2.elementType, originalMap2.axisRange, originalMap2.invert);

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

            if (hasRecentered || !isUsingMotionControls) return;

            hasRecentered = true;

            string glyphString = inputGlyphs[25];
            
            SimpleDialogBox dialogBox = SimpleDialogBox.Create(null);
            dialogBox.headerToken = new SimpleDialogBox.TokenParamsPair()
            {
                token = "Recenter",
                formatParams = null
            };

            if (glyphsSpriteAsset)
                dialogBox.descriptionLabel.spriteAsset = glyphsSpriteAsset;

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

        private static void ApplyContextSpriteAsset(On.RoR2.UI.ContextManager.orig_Awake orig, ContextManager self)
        {
            orig(self);

            if (self.glyphTMP && glyphsSpriteAsset)
            {
                self.glyphTMP.spriteAsset = glyphsSpriteAsset;
            }
        }

        private static void ApplySpriteAsset(On.RoR2.InputBindingDisplayController.orig_Awake orig, InputBindingDisplayController self)
        {
            orig(self);

            if (glyphsSpriteAsset)
            {
                if (self.guiLabel)
                    self.guiLabel.spriteAsset = glyphsSpriteAsset;

                if (self.label)
                    self.label.spriteAsset = glyphsSpriteAsset;
            }
        }

        private static void SetupControllerInputs()
        {
            vrControllers = CreateVRControllers();
            vrControllers.useUpdateCallbacks = false;

            List<ActionElementMap> uiElementMaps = new List<ActionElementMap>()
            {
                new ActionElementMap(11, ControllerElementType.Button, 24, Pole.Positive, AxisRange.Positive, false), //Start
                new ActionElementMap(12, ControllerElementType.Axis  , 4 , Pole.Positive, AxisRange.Full, false), //UIHor
                new ActionElementMap(13, ControllerElementType.Axis  , 5 , Pole.Positive, AxisRange.Full, false), //UIVer
                new ActionElementMap(14, ControllerElementType.Button, 17 , Pole.Positive, AxisRange.Positive, false), //Submit
                new ActionElementMap(15, ControllerElementType.Button, 18, Pole.Positive, AxisRange.Positive, false), //Cancel
                new ActionElementMap(25, ControllerElementType.Button, 24, Pole.Positive, AxisRange.Positive, false), //Pause
                new ActionElementMap(29, ControllerElementType.Button, 20, Pole.Positive, AxisRange.Positive, false), //TabLeft
                new ActionElementMap(30, ControllerElementType.Button, 21, Pole.Positive, AxisRange.Positive, false), //TabRight
                new ActionElementMap(31, ControllerElementType.Button, 19, Pole.Positive, AxisRange.Positive, false), //AltSubmit
                new ActionElementMap(32, ControllerElementType.Button, 22, Pole.Positive, AxisRange.Positive, false), //SubmenuLeft
                new ActionElementMap(33, ControllerElementType.Button, 23, Pole.Positive, AxisRange.Positive, false), //SubmenuRight
                new ActionElementMap(150, ControllerElementType.Button, 25, Pole.Positive, AxisRange.Positive, false) //Recenter
            };

            vrUIMap = CreateCustomMap("VRUI", 2, vrControllers.id, uiElementMaps);

            List<ActionElementMap> defaultElementMaps = new List<ActionElementMap>()
            {
                new ActionElementMap(0 , ControllerElementType.Axis  , 0 , Pole.Positive, AxisRange.Full, false), //MoveHor
                new ActionElementMap(1 , ControllerElementType.Axis  , 1 , Pole.Positive, AxisRange.Full, false), //MoveVer
                new ActionElementMap(16, ControllerElementType.Axis  , 2 , Pole.Positive, AxisRange.Full, false), //LookHor
                new ActionElementMap(17, ControllerElementType.Axis  , 3 , Pole.Positive, AxisRange.Full, false), //LookVer
                new ActionElementMap(4 , ControllerElementType.Button, 7 , Pole.Positive, AxisRange.Full, false), //Jump
                new ActionElementMap(5 , ControllerElementType.Button, 6, Pole.Positive, AxisRange.Full, false), //Interact
                new ActionElementMap(6 , ControllerElementType.Button, 12 , Pole.Positive, AxisRange.Full, false), //Equipment
                new ActionElementMap(7 , ControllerElementType.Button, (ModConfig.LeftDominantHand.Value ? 9 : 8) , Pole.Positive, AxisRange.Positive, false), //Primary
                new ActionElementMap(8 , ControllerElementType.Button, (ModConfig.LeftDominantHand.Value ? 8 : 9) , Pole.Positive, AxisRange.Positive, false), //Secondary
                new ActionElementMap(9 , ControllerElementType.Button, (ModConfig.LeftDominantHand.Value ? 11 : 10) , Pole.Positive, AxisRange.Positive, false), //Utility
                new ActionElementMap(10, ControllerElementType.Button, (ModConfig.LeftDominantHand.Value ? 10 : 11) , Pole.Positive, AxisRange.Positive, false), //Special
                new ActionElementMap(18, ControllerElementType.Button, 13, Pole.Positive, AxisRange.Full, false), //Sprint
                new ActionElementMap(19, ControllerElementType.Button, 15, Pole.Positive, AxisRange.Full, false), //Scoreboard or Profile
                new ActionElementMap(28, ControllerElementType.Button, 14, Pole.Positive, AxisRange.Full, false) //Ping
            };

            vrDefaultMap = CreateCustomMap("VRDefault", 0, vrControllers.id, defaultElementMaps);

            if (ModConfig.OculusMode.Value)
            {
                inputs = new BaseInput[]
                {
                    new LegacyAxisInput(true, 0, false, 0, 4), //LJoyX = MoveHor, UIHor
                    new LegacyAxisInput(true, 1, true, 1, 5), //LJoyY = MoveVer, UIVer
                    new LegacyAxisInput(false, 3, false, 2), //RJoyX = LookHor
                    new LegacyAxisInput(false, 4, true, 3), //RJoyY = LookVer
                    new LegacyButtonInput(true, 2, 12, 19), //X = Equipment, Ready
                    new LegacyHoldableButtonInput(true, 3, 15, 24), //Y = Pause, (Hold)Scoreboard/Profile
                    new LegacyButtonInput(false, 0, 6, 17), //A = Interact, Submit
                    new LegacyButtonInput(false, 1, 7, 18), //B = Jump, Cancel
                    new LegacyButtonInput(true, 8, 13), //LClick = Sprint
                    new LegacyButtonInput(false, 9, 14, 25), //RClick = Ping, Recenter
                    new LegacyButtonInput(true, 14, 9, 20), //LTrigger = Secondary, Tab Left
                    new LegacyButtonInput(true, 4, 10, 22), //LGrip = Utility, Submenu Left
                    new LegacyButtonInput(false, 15, 8, 21), //RTrigger = Primary, Tab Right
                    new LegacyButtonInput(false, 5, 11, 23) //RGrip = Special, Submenu Right
                };

                inputGlyphs = ControllerGlyphs.standardGlyphs;
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
            }
        }

        private static string GetCustomGlyphString(On.RoR2.Glyphs.orig_GetGlyphString_MPEventSystem_string_AxisRange_InputSource orig, MPEventSystem eventSystem, string actionName, AxisRange axisRange, MPEventSystem.InputSource currentInputSource)
        {
            if (!eventSystem)
            {
                return "UNKNOWN";
            }
            if (currentInputSource == MPEventSystem.InputSource.Gamepad && isUsingMotionControls && vrControllers != null)
            {
                Glyphs.resultsList.Clear();
                eventSystem.player.controllers.maps.GetElementMapsWithAction(ControllerType.Custom, vrControllers.id, actionName, false, Glyphs.resultsList);

                if (Glyphs.resultsList.Count() > 0)
                {
                    ActionElementMap displayedMap = Glyphs.resultsList.First();
                    if (displayedMap.elementIdentifierId > 0 && displayedMap.elementIdentifierId < inputGlyphs.Length)
                    {
                        return inputGlyphs[displayedMap.elementIdentifierId];
                    }
                }
            }

            return orig(eventSystem, actionName, axisRange, currentInputSource);
        }
        
        private static CustomController CreateVRControllers()
        {
            HardwareControllerMap_Game hcMap = new HardwareControllerMap_Game(
                "VRControllers",
                new ControllerElementIdentifier[]
                {
                    new ControllerElementIdentifier(0, "MoveX", "MoveXPos", "MoveXNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(1, "MoveY", "MoveYPos", "MoveYNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(2, "LookX", "LookXPos", "LookXNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(3, "LookY", "LookYPos", "LookYNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(4, "NavigateX", "NavigateXPos", "NavigateXNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(5, "NavigateY", "NavigateYPos", "NavigateYNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(6, "Interact", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(7, "Jump", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(8, "PrimarySkill", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(9, "SecondarySkill", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(10, "UtilitySkill", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(11, "SpecialSkill", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(12, "Equipment", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(13, "Sprint", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(14, "Ping", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(15, "Info", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(16, "HoldInfo", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(17, "Submit", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(18, "Cancel", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(19, "Ready", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(20, "TabLeft", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(21, "TabRight", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(22, "SubmenuLeft", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(23, "SubmenuRight", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(24, "Pause", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(25, "RecenterHMD", "", "", ControllerElementType.Button, true)
                },
                new int[] {6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22 },
                new int[] { 0, 1, 2, 3, 4, 5 },
                new AxisCalibrationData[]
                {
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, false, true)
                },
                new AxisRange[]
                {
                    AxisRange.Full,
                    AxisRange.Full,
                    AxisRange.Full,
                    AxisRange.Full,
                    AxisRange.Full,
                    AxisRange.Full
                },
                new HardwareAxisInfo[]
                {
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None),
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None),
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None),
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None),
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None),
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None)
                },
                new HardwareButtonInfo[]
                {
                    new HardwareButtonInfo(false, false),
                    new HardwareButtonInfo(false, false),
                    new HardwareButtonInfo(false, false),
                    new HardwareButtonInfo(false, false),
                    new HardwareButtonInfo(false, false),
                    new HardwareButtonInfo(false, false),
                    new HardwareButtonInfo(false, false),
                    new HardwareButtonInfo(false, false),
                    new HardwareButtonInfo(false, false),
                    new HardwareButtonInfo(false, false),
                    new HardwareButtonInfo(false, false),
                    new HardwareButtonInfo(false, false),
                    new HardwareButtonInfo(false, false),
                    new HardwareButtonInfo(false, false),
                    new HardwareButtonInfo(false, false),
                    new HardwareButtonInfo(false, false)
                },
                null
            );

            ReInput.UserData.AddCustomController();
            CustomController_Editor newController = ReInput.UserData.customControllers.Last();
            newController.name = "VRControllers";
            foreach (ControllerElementIdentifier element in hcMap.elementIdentifiers.Values)
            {
                if (element.elementType == ControllerElementType.Axis)
                {
                    newController.AddAxis();
                    newController.elementIdentifiers.RemoveAt(newController.elementIdentifiers.Count - 1);
                    newController.elementIdentifiers.Add(element);
                    CustomController_Editor.Axis newAxis = newController.axes.Last();
                    newAxis.name = element.name;
                    newAxis.elementIdentifierId = element.id;
                    newAxis.deadZone = hcMap.hwAxisCalibrationData[newController.axisCount - 1].deadZone;
                    newAxis.zero = 0;
                    newAxis.min = hcMap.hwAxisCalibrationData[newController.axisCount - 1].min;
                    newAxis.max = hcMap.hwAxisCalibrationData[newController.axisCount - 1].max;
                    newAxis.invert = hcMap.hwAxisCalibrationData[newController.axisCount - 1].invert;
                    newAxis.axisInfo = hcMap.hwAxisInfo[newController.axisCount - 1];
                    newAxis.range = hcMap.hwAxisRanges[newController.axisCount - 1];
                }
                else if (element.elementType == ControllerElementType.Button)
                {
                    newController.AddButton();
                    newController.elementIdentifiers.RemoveAt(newController.elementIdentifiers.Count - 1);
                    newController.elementIdentifiers.Add(element);
                    CustomController_Editor.Button newButton = newController.buttons.Last();
                    newButton.name = element.name;
                    newButton.elementIdentifierId = element.id;
                }
            }

            CustomController customController = ReInput.controllers.CreateCustomController(newController.id);

            customController.erysJAiDEvGlnFNklzMyeuFJkdrW(new VRRumbleExtension(new VRRumbleExtensionSource()));

            return customController;
        }

        private static CustomControllerMap CreateCustomMap(string mapName, int categoryId, int controllerId, List<ActionElementMap> actionElementMaps)
        {
            ReInput.UserData.CreateCustomControllerMap(categoryId, controllerId, 0);

            ControllerMap_Editor newMap = ReInput.UserData.customControllerMaps.Last();
            newMap.name = mapName;

            foreach (ActionElementMap elementMap in actionElementMaps)
            {
                newMap.AddActionElementMap();
                ActionElementMap newElementMap = newMap.GetActionElementMap(newMap.ActionElementMaps.Count() - 1);
                newElementMap.actionId = elementMap.actionId;
                newElementMap.elementType = elementMap.elementType;
                newElementMap.elementIdentifierId = elementMap.elementIdentifierId;
                newElementMap.axisContribution = elementMap.axisContribution;
                if (elementMap.elementType == ControllerElementType.Axis)
                    newElementMap.axisRange = elementMap.axisRange;
                newElementMap.invert = elementMap.invert;
            }

            return ReInput.HskUxHpFZhrqieMHwBDWRMVxZrz.QCRMWRcLcHpJjvmLpFMRaWPZhee(categoryId, controllerId, 0);
        }

        private static void ChangedToCustom(On.RoR2.UI.MPEventSystem.orig_OnLastActiveControllerChanged orig, MPEventSystem self, Player player, Controller controller)
        {
            if (controller != null && controller.type == ControllerType.Custom)
            {
                isUsingMotionControls = true;
                self.currentInputSource = MPEventSystem.InputSource.Gamepad;
                return;
            }
            isUsingMotionControls = false;

            orig(self, player, controller);
        }

        private static void Update()
        {
            LocalUser localUser = LocalUserManager.GetFirstLocalUser();

            if (localUser != null)
            {
                if (AddVRController(localUser.inputPlayer))
                    RoR2Application.onUpdate -= Update;
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
                    inputPlayer.controllers.maps.AddMap(vrControllers, vrDefaultMap);
                if (!vrDefaultMap.enabled)
                    vrDefaultMap.enabled = true;
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
                    int leftJoystickId = -1;
                    int rightJoystickId = -1;
                    for (int i = 0; i < joyNames.Length; i++)
                    {
                        string joyName = joyNames[i].ToLower();
                        if (joyName.Contains("left"))
                        {
                            leftJoystickId = i;
                        }

                        if (joyName.Contains("right"))
                        {
                            rightJoystickId = i;
                        }
                    }

                    if (leftJoystickId == -1 || rightJoystickId == -1) return;
                }
            }

            if (inputGlyphs == null)
            {
                List<XRNodeState> states = new List<XRNodeState>();
                InputTracking.GetNodeStates(states);

                List<XRNodeState> results = states.Where(x => x.nodeType == XRNode.LeftHand).ToList();
                if (results.Count > 0)
                {
                    XRNodeState leftControllerState = results.First();
                    string controllerName = InputTracking.GetNodeName(leftControllerState.uniqueID);

                    if (controllerName.Contains("vive_controller"))
                        inputGlyphs = ControllerGlyphs.viveGlyphs;
                    else if (controllerName.Contains("holographic_controller"))
                        inputGlyphs = ControllerGlyphs.wmrGlyphs;
                    else
                        inputGlyphs = ControllerGlyphs.standardGlyphs;
                }
            }

            foreach (BaseInput input in inputs)
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
