using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace VRMod
{
    internal static class RewiredAddons
    {
        internal static CustomController CreateRewiredController()
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
                    new ControllerElementIdentifier(25, "RecenterHMD", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(26, "ExtraSkill1", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(27, "ExtraSkill2", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(28, "ExtraSkill3", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(29, "ExtraSkill4", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(30, "PushToTalk", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(31, "BuySkill", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(32, "Load", "", "", ControllerElementType.Button, true),
                },
                new int[] { },
                new int[] { },
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
                new HardwareButtonInfo[] { },
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

            customController.useUpdateCallbacks = false;

            return customController;
        }

        internal static CustomControllerMap CreateUIMap(int controllerID)
        {
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
                new ActionElementMap(150, ControllerElementType.Button, 25, Pole.Positive, AxisRange.Positive, false), //Recenter
                new ActionElementMap(34, ControllerElementType.Button, 32, Pole.Positive, AxisRange.Positive, false) //Load
            };

            return CreateCustomMap("VRUI", 2, controllerID, uiElementMaps);
        }

        internal static CustomControllerMap CreateGameplayMap(int controllerID)
        {
            List<ActionElementMap> defaultElementMaps = new List<ActionElementMap>()
            {
                new ActionElementMap(0 , ControllerElementType.Axis  , 0 , Pole.Positive, AxisRange.Full, false), //MoveHor
                new ActionElementMap(1 , ControllerElementType.Axis  , 1 , Pole.Positive, AxisRange.Full, false), //MoveVer
                new ActionElementMap(16, ControllerElementType.Axis  , 2 , Pole.Positive, AxisRange.Full, false), //LookHor
                new ActionElementMap(17, ControllerElementType.Axis  , 3 , Pole.Positive, AxisRange.Full, false), //LookVer
                new ActionElementMap(4 , ControllerElementType.Button, 7 , Pole.Positive, AxisRange.Full, false), //Jump
                new ActionElementMap(5 , ControllerElementType.Button, 6, Pole.Positive, AxisRange.Full, false), //Interact
                new ActionElementMap(6 , ControllerElementType.Button, 12 , Pole.Positive, AxisRange.Full, false), //Equipment
                new ActionElementMap(7 , ControllerElementType.Button, (ModConfig.LeftDominantHand.Value ? 9 : 8) , Pole.Positive, AxisRange.Full, false), //Primary
                new ActionElementMap(8 , ControllerElementType.Button, (ModConfig.LeftDominantHand.Value ? 8 : 9) , Pole.Positive, AxisRange.Full, false), //Secondary
                new ActionElementMap(9 , ControllerElementType.Button, (ModConfig.LeftDominantHand.Value ? 11 : 10) , Pole.Positive, AxisRange.Full, false), //Utility
                new ActionElementMap(10, ControllerElementType.Button, (ModConfig.LeftDominantHand.Value ? 10 : 11) , Pole.Positive, AxisRange.Full, false), //Special
                new ActionElementMap(18, ControllerElementType.Button, 13, Pole.Positive, AxisRange.Full, false), //Sprint
                new ActionElementMap(19, ControllerElementType.Button, 15, Pole.Positive, AxisRange.Full, false), //Scoreboard or Profile
                new ActionElementMap(28, ControllerElementType.Button, 14, Pole.Positive, AxisRange.Full, false), //Ping
                new ActionElementMap(100, ControllerElementType.Button, 26, Pole.Positive, AxisRange.Full, false), //ExtraSkill1
                new ActionElementMap(101, ControllerElementType.Button, 27, Pole.Positive, AxisRange.Full, false), //ExtraSkill2
                new ActionElementMap(102, ControllerElementType.Button, 28, Pole.Positive, AxisRange.Full, false), //ExtraSkill3
                new ActionElementMap(103, ControllerElementType.Button, 29, Pole.Positive, AxisRange.Full, false), //ExtraSkill4
                new ActionElementMap(351, ControllerElementType.Button, 30, Pole.Positive, AxisRange.Full, false), //PushToTalk
                new ActionElementMap(400, ControllerElementType.Button, 31, Pole.Positive, AxisRange.Full, false) //BuySkill
            };

            return CreateCustomMap("VRDefault", 0, controllerID, defaultElementMaps);
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
    }
}
