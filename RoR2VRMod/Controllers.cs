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
using VRMod.ControllerMappings;

namespace VRMod
{
    internal class Controllers
    {
        private static CustomController vrControllers;
        private static CustomControllerMap vrDefaultMap;
        private static CustomControllerMap vrUIMap;

        private static GenericVRMap controllerMap;

        private static bool isUsingMotionControls = false;

        private static TMP_SpriteAsset glyphsSpriteAsset;

        internal static void Init()
        {
            ReInput.InputSourceUpdateEvent += UpdateVRInputs;

            RoR2Application.onUpdate += Update;

            On.RoR2.UI.MPEventSystem.OnLastActiveControllerChanged += ChangedToCustom;

            On.RoR2.Glyphs.GetGlyphString_MPEventSystem_string_AxisRange_InputSource += GetCustomGlyphString;

            On.RoR2.InputBindingDisplayController.Awake += ApplySpriteAsset;

            On.RoR2.UI.ContextManager.Awake += ApplyContextSpriteAsset;

            On.RoR2.UI.InputBindingControl.Awake += DisableControllerBinds;

            SetupControllerInputs();

            glyphsSpriteAsset = VRMod.VRAssetBundle.LoadAsset<TMP_SpriteAsset>("sprVRGlyphs");
        }

        private static void DisableControllerBinds(On.RoR2.UI.InputBindingControl.orig_Awake orig, InputBindingControl self)
        {
            orig(self);

            if (ModConfig.UseMotionControls.Value && self.inputSource == MPEventSystem.InputSource.Gamepad)
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

            if (self.guiLabel && glyphsSpriteAsset)
            {
                self.guiLabel.spriteAsset = glyphsSpriteAsset;
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
                    new ControllerElementIdentifier(13, "RightStickPress", "", "", ControllerElementType.Button, true)
                },
                new int[] { 8, 9, 10, 11, 12, 13 },
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
                if (name.Contains("rift"))
                {
                    controllerMap = new RiftOpenVRMap(leftID, rightID, name);
                    return;
                }

                if (name.Contains("vive"))
                {
                    controllerMap = new ViveMap(leftID, rightID, name);
                    return;
                }

                if (name.Contains("windows"))
                {
                    controllerMap = new TrackpadWMRMap(leftID, rightID, name);
                    return;
                }
            }

            controllerMap = new GenericVRMap(leftID, rightID, name, true);
        }
    }
}
