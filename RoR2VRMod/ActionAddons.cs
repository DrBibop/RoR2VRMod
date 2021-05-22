using MonoMod.RuntimeDetour;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VRMod
{
    //Thank you KingEnderBrine. Your code from ExtraSkillSlots have been greatly helpful for this part.
    internal static class ActionAddons
    {
        internal static readonly ActionDef[] actionDefs = new ActionDef[]
        {
            new ActionDef() { id = 150, actionName = "RecenterHMD", token = "ACTION_RECENTER", actionType = InputActionType.Button, hasPosAndNeg = false, joystickMap = ControllerInput.DPadUp, keyboardMap = KeyboardKeyCode.RightControl }
        };

        private static ActionElementMap[] joystickActionElementMaps;
        private static ActionElementMap[] keyboardActionElementMaps;

        internal static void Init()
        {
            GenerateMapsFromActionDefs();

            foreach (ActionDef actionDef in actionDefs)
            {
                InputCatalog.actionToToken.Add(new InputCatalog.ActionAxisPair(actionDef.actionName, AxisRange.Full), actionDef.token);
                if (actionDef.hasPosAndNeg)
                {
                    InputCatalog.actionToToken.Add(new InputCatalog.ActionAxisPair(actionDef.actionName, AxisRange.Positive), actionDef.token + "_POSITIVE");
                    InputCatalog.actionToToken.Add(new InputCatalog.ActionAxisPair(actionDef.actionName, AxisRange.Negative), actionDef.token + "_NEGATIVE");
                }
            }

            var userDataInit = typeof(UserData).GetMethod("KFIfLMJhIpfzcbhqEXHpaKpGsgeZ", BindingFlags.NonPublic | BindingFlags.Instance);
            new Hook(userDataInit, (Action<Action<UserData>, UserData>)AddCustomActions);

            On.RoR2.UserProfile.LoadUserProfiles += AddBindingsToLoadedProfiles;
            On.RoR2.UserProfile.LoadDefaultProfile += (orig) =>
            {
                orig();
                AddMissingBindingsToProfile(UserProfile.defaultProfile);
            };
        }

        private static void GenerateMapsFromActionDefs()
        {
            List<ActionElementMap> joystickMaps = new List<ActionElementMap>();
            List<ActionElementMap> keyboardMaps = new List<ActionElementMap>();

            foreach (ActionDef actionDef in actionDefs)
            {
                if (actionDef.joystickMap != ControllerInput.None)
                {
                    ActionElementMap actionElementMap = new ActionElementMap(actionDef.id, ControllerElementType.Button, (int)actionDef.joystickMap, Pole.Positive, AxisRange.Full);
                    joystickMaps.Add(actionElementMap);
                }
                if (actionDef.keyboardMap != KeyboardKeyCode.None)
                {
                    ActionElementMap actionElementMap = new ActionElementMap(actionDef.id, ControllerElementType.Button, -1);
                    actionElementMap.keyboardKeyCode = actionDef.keyboardMap;
                    keyboardMaps.Add(actionElementMap);
                }
            }

            joystickActionElementMaps = joystickMaps.ToArray();
            keyboardActionElementMaps = keyboardMaps.ToArray();
        }

        private static void AddBindingsToLoadedProfiles(On.RoR2.UserProfile.orig_LoadUserProfiles orig)
        {
            orig();

            List<UserProfile> userProfiles = UserProfile.loadedUserProfiles.Values.ToList();
            foreach (UserProfile profile in userProfiles)
            {
                if (AddMissingBindingsToProfile(profile))
                    profile.RequestSave();
            }
        }

        private static bool AddMissingBindingsToProfile(UserProfile profile)
        {
            bool hasAddedBind = false;

            foreach (ActionElementMap map in joystickActionElementMaps)
            {
                List<ActionElementMap> actionElementMaps = profile.joystickMap.GetElementMaps().ToList();
                if (!actionElementMaps.Exists((x) => x.actionId == map.actionId))
                {
                    profile.joystickMap.AddElementMap(map);
                    hasAddedBind = true;
                }
            }

            foreach (ActionElementMap map in keyboardActionElementMaps)
            {
                List<ActionElementMap> actionElementMaps = profile.keyboardMap.GetElementMaps().ToList();
                if (!actionElementMaps.Exists((x) => x.actionId == map.actionId))
                {
                    profile.keyboardMap.AddElementMap(map);
                    hasAddedBind = true;
                }
            }

            return hasAddedBind;
        }

        private static void AddCustomActions(Action<UserData> orig, UserData self)
        {
            foreach (ActionDef actionDef in actionDefs)
            {
                InputAction inputAction = new InputAction();
                inputAction.id = actionDef.id;
                inputAction.name = actionDef.actionName;
                inputAction.type = actionDef.actionType;
                inputAction.descriptiveName = actionDef.actionName;
                if (actionDef.hasPosAndNeg)
                {
                    inputAction.positiveDescriptiveName = actionDef.actionName + "Positive";
                    inputAction.negativeDescriptiveName = actionDef.actionName + "Negative";
                }
                inputAction.behaviorId = 0;
                inputAction.userAssignable = true;
                inputAction.categoryId = 0;

                self.actions?.Add(inputAction);
            }

            ControllerMap_Editor controllerMap = self.joystickMaps?[0];
            ControllerMap_Editor keyboardMap = self.keyboardMaps?[0];
            if (controllerMap != null && keyboardMap != null)
            {
                foreach (ActionDef actionDef in actionDefs)
                {
                    if (controllerMap.actionElementMaps.Exists((x) => x.actionId == actionDef.id) || keyboardMap.actionElementMaps.Exists((x) => x.actionId == actionDef.id))
                        continue;

                    foreach (ActionElementMap map in joystickActionElementMaps)
                    {
                        controllerMap.actionElementMaps.Add(map);
                    }

                    foreach (ActionElementMap map in keyboardActionElementMaps)
                    {
                        keyboardMap.actionElementMaps.Add(map);
                    }
                }
            }

            orig(self);
        }

        internal struct ActionDef
        {
            internal int id;
            internal string actionName;
            internal string token;
            internal InputActionType actionType;
            internal bool hasPosAndNeg;
            internal KeyboardKeyCode keyboardMap;
            internal ControllerInput joystickMap;
        }

        internal enum ControllerInput
        {
            LeftStickX,
            LeftStickY,
            RightStickX,
            RightStickY,
            LeftTrigger,
            RightTrigger,
            A,
            B,
            X,
            Y,
            LeftBumper,
            RightBumper,
            Back,
            Start,
            LeftStickButton,
            RightStickButton,
            DPadUp,
            DPadRight,
            DPadDown,
            DPadLeft,
            LeftStick,
            RightStick,
            Guide,
            None
        }

        internal enum MouseInput
        {
            Horizontal,
            Vertical,
            ScrollWheel,
            LeftMouseButton,
            RightMouseButton,
            MiddleMouseButton
        }
    }
}
