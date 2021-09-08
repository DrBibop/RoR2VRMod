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
using VRMod.ControllerMappings;

namespace VRMod
{
    public class Controllers
    {
        private static CustomController vrControllers;
        private static CustomControllerMap vrDefaultMap;
        private static CustomControllerMap vrUIMap;

        private static GenericVRMap controllerMap;

        private static bool isUsingMotionControls;
        private static bool hasRecentered;
        private static bool initializedMainPlayer;

        private static TMP_SpriteAsset glyphsSpriteAsset;

        private static List<SkillRemap> skillRemaps = new List<SkillRemap>()
        {
            new SkillRemap("LoaderBody", SkillSlot.Utility, SkillSlot.Special)
        };

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

            string glyphString;
            if (controllerMap.mapGlyphs.TryGetValue(13, out glyphString))
            {
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
                new ActionElementMap(11, ControllerElementType.Button, 10, Pole.Positive, AxisRange.Full, false),
                new ActionElementMap(12, ControllerElementType.Axis  , 0 , Pole.Positive, AxisRange.Full, false),
                new ActionElementMap(13, ControllerElementType.Axis  , 1 , Pole.Positive, AxisRange.Full, false),
                new ActionElementMap(14, ControllerElementType.Button, 9 , Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(15, ControllerElementType.Button, 11, Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(25, ControllerElementType.Button, 10, Pole.Positive, AxisRange.Full, false),
                new ActionElementMap(29, ControllerElementType.Button, 4 , Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(30, ControllerElementType.Button, 5 , Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(31, ControllerElementType.Button, 8, Pole.Positive, AxisRange.Full, false),
                new ActionElementMap(32, ControllerElementType.Button, 6 , Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(33, ControllerElementType.Button, 7 , Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(150, ControllerElementType.Button, 13 , Pole.Positive, AxisRange.Positive, false)
            };

            vrUIMap = CreateCustomMap("VRUI", 2, vrControllers.id, uiElementMaps);


            List<ActionElementMap> defaultElementMaps = new List<ActionElementMap>()
            {
                new ActionElementMap(0 , ControllerElementType.Axis  , 0 , Pole.Positive, AxisRange.Full, false),
                new ActionElementMap(1 , ControllerElementType.Axis  , 1 , Pole.Positive, AxisRange.Full, false),
                new ActionElementMap(16, ControllerElementType.Axis  , 2 , Pole.Positive, AxisRange.Full, false),
                new ActionElementMap(17, ControllerElementType.Axis  , 3 , Pole.Positive, AxisRange.Full, false),
                new ActionElementMap(4 , ControllerElementType.Button, 11 , Pole.Positive, AxisRange.Full, false),
                new ActionElementMap(5 , ControllerElementType.Button, 9, Pole.Positive, AxisRange.Full, false),
                new ActionElementMap(6 , ControllerElementType.Button, 8 , Pole.Positive, AxisRange.Full, false),
                new ActionElementMap(7 , ControllerElementType.Button, (ModConfig.LeftDominantHand.Value ? 4 : 5) , Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(8 , ControllerElementType.Button, (ModConfig.LeftDominantHand.Value ? 5 : 4) , Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(9 , ControllerElementType.Button, (ModConfig.LeftDominantHand.Value ? 7 : 6) , Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(10, ControllerElementType.Button, (ModConfig.LeftDominantHand.Value ? 6 : 7) , Pole.Positive, AxisRange.Positive, false),
                new ActionElementMap(18, ControllerElementType.Button, 12, Pole.Positive, AxisRange.Full, false),
                new ActionElementMap(19, ControllerElementType.Button, 14, Pole.Positive, AxisRange.Full, false),
                new ActionElementMap(28, ControllerElementType.Button, 13, Pole.Positive, AxisRange.Full, false)
            };

            vrDefaultMap = CreateCustomMap("VRDefault", 0, vrControllers.id, defaultElementMaps);
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
                    string result;
                    if (controllerMap.mapGlyphs.TryGetValue(displayedMap.elementIdentifierId, out result))
                    {
                        return result;
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
                    new ControllerElementIdentifier(0, "LeftStickX", "LeftStickXPos", "LeftStickXNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(1, "LeftStickY", "LeftStickYPos", "LeftStickYNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(2, "RightStickX", "RightStickXPos", "RightStickXNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(3, "RightStickY", "RightStickYPos", "RightStickYNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(4, "LeftTrigger", "", "", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(5, "RightTrigger", "", "", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(6, "LeftGrip", "", "", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(7, "RightGrip", "", "", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(8, "LeftPrimary", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(9, "RightPrimary", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(10, "LeftSecondary", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(11, "RightSecondary", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(12, "LeftStickPress", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(13, "RightStickPress", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(14, "LeftSecondaryHold", "", "", ControllerElementType.Button, true),
                },
                new int[] { 8, 9, 10, 11, 12, 13, 14 },
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7 },
                new AxisCalibrationData[]
                {
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, true, true),
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, true, true),
                    new AxisCalibrationData(true, 0.1f, 0, 0, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, 0, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, 0, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, 0, 1, false, true)
                },
                new AxisRange[]
                {
                    AxisRange.Full,
                    AxisRange.Full,
                    AxisRange.Full,
                    AxisRange.Full,
                    AxisRange.Positive,
                    AxisRange.Positive,
                    AxisRange.Positive,
                    AxisRange.Positive
                },
                new HardwareAxisInfo[]
                {
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None),
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None),
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

            return ReInput.controllers.CreateCustomController(newController.id);
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
            string[] joyNames = Input.GetJoystickNames();
            if (controllerMap == null|| !controllerMap.CheckJoyNames(joyNames))
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

                SelectMapFromName(joyNames[leftJoystickId].ToLower(), leftJoystickId, rightJoystickId);
            }

            vrControllers.SetAxisValue(0, controllerMap.GetLeftJoyX());
            vrControllers.SetAxisValue(1, controllerMap.GetLeftJoyY());
            vrControllers.SetAxisValue(2, controllerMap.GetRightJoyX());
            vrControllers.SetAxisValue(3, controllerMap.GetRightJoyY());
            vrControllers.SetAxisValue(4, controllerMap.GetLeftTrigger());
            vrControllers.SetAxisValue(5, controllerMap.GetRightTrigger());
            vrControllers.SetAxisValue(6, controllerMap.GetLeftGrip());
            vrControllers.SetAxisValue(7, controllerMap.GetRightGrip());

            vrControllers.SetButtonValue(0, controllerMap.GetLeftPrimary());
            vrControllers.SetButtonValue(1, controllerMap.GetRightPrimary());
            vrControllers.SetButtonValue(2, controllerMap.GetLeftSecondary());
            vrControllers.SetButtonValue(3, controllerMap.GetRightSecondary());
            vrControllers.SetButtonValue(4, controllerMap.GetLeftJoyPress());
            vrControllers.SetButtonValue(5, controllerMap.GetRightJoyPress());
            vrControllers.SetButtonValue(6, controllerMap.GetLeftSecondaryHold());
        }

        private static void SelectMapFromName(string name, int leftID, int rightID)
        {
            if (controllerMap != null && controllerMap.cachedName == name)
            {
                controllerMap.SetJoystickIDs(leftID, rightID);
                return;
            }

            if (!ModConfig.ConfigUseOculus.Value)
            {
                if (name.Contains("vive") && !name.Contains("cosmos"))
                {
                    controllerMap = new ViveMap(leftID, rightID, name);
                    return;
                }

                if (name.Contains("0x066a"))
                {
                    controllerMap = new ReverbG2Map(leftID, rightID, name);
                    return;
                }

                if (name.Contains("windows"))
                {
                    controllerMap = new TrackpadWMRMap(leftID, rightID, name);
                    return;
                }

                controllerMap = new GenericOpenVRMap(leftID, rightID, name);
                return;
            }

            controllerMap = new GenericVRMap(leftID, rightID, name, true);
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
