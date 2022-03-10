using Rewired;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine.XR;
using Valve.VR;

namespace VRMod
{
    internal static class ControllerGlyphs
    {
        internal static readonly string[] standardGlyphs = new string[]
        {
            "<sprite name=\"texVRGlyphs_LStick\">",          //MoveX
            "<sprite name=\"texVRGlyphs_LStick\">",          //MoveY
            "<sprite name=\"texVRGlyphs_RStick\">",          //LookX
            "<sprite name=\"texVRGlyphs_RStick\">",          //LookY
            "<sprite name=\"texVRGlyphs_LStick\">",          //NavigateX
            "<sprite name=\"texVRGlyphs_LStick\">",          //NavigateY
            "<sprite name=\"texVRGlyphs_RPrimary\">",        //Interact
            "<sprite name=\"texVRGlyphs_RSecondary\">",      //Jump
            "<sprite name=\"texVRGlyphs_RTrigger\">",        //PrimarySkill
            "<sprite name=\"texVRGlyphs_LTrigger\">",        //SecondarySkill
            "<sprite name=\"texVRGlyphs_LGrip\">",           //UtilitySkill
            "<sprite name=\"texVRGlyphs_RGrip\">",           //SpecialSkill
            "<sprite name=\"texVRGlyphs_LPrimary\">",        //Equipment
            "<sprite name=\"texVRGlyphs_LStickPress\">",     //Sprint 
            "<sprite name=\"texVRGlyphs_RStickPress\">",     //Ping
            "<sprite name=\"texVRGlyphs_LSecondaryHold\">",  //Info
            "<sprite name=\"texVRGlyphs_LSecondaryHold\">",  //HoldInfo
            "<sprite name=\"texVRGlyphs_RPrimary\">",        //Submit
            "<sprite name=\"texVRGlyphs_RSecondary\">",      //Cancel 
            "<sprite name=\"texVRGlyphs_LPrimary\">",        //Ready
            "<sprite name=\"texVRGlyphs_LTrigger\">",        //TabLeft
            "<sprite name=\"texVRGlyphs_RTrigger\">",        //TabRight
            "<sprite name=\"texVRGlyphs_LGrip\">",           //SubmenuLeft
            "<sprite name=\"texVRGlyphs_RGrip\">",           //SubmenuRight
            "<sprite name=\"texVRGlyphs_LSecondary\">",      //Pause 
            "<sprite name=\"texVRGlyphs_RStickPress\">",     //RecenterHMD
            "<sprite name=\"texVRGlyphs_Unknown\">",         //ExtraSkill1
            "<sprite name=\"texVRGlyphs_Unknown\">",         //ExtraSkill2
            "<sprite name=\"texVRGlyphs_Unknown\">",         //ExtraSkill3
            "<sprite name=\"texVRGlyphs_Unknown\">",         //ExtraSkill4
            "<sprite name=\"texVRGlyphs_Unknown\">",         //PushToTalk
            "<sprite name=\"texVRGlyphs_Unknown\">",         //BuySkill
            "<sprite name=\"texVRGlyphs_Unknown\">"          //Load
        };

        internal static readonly string[] viveGlyphs = new string[]
        {
            "<sprite name=\"texVRGlyphs_LTouch\">",          //MoveX
            "<sprite name=\"texVRGlyphs_LTouch\">",          //MoveY
            "<sprite name=\"texVRGlyphs_RTouchHor\">",       //LookX
            "<sprite name=\"texVRGlyphs_RTouchHor\">",       //LookY
            "<sprite name=\"texVRGlyphs_LTouch\">",          //NavigateX
            "<sprite name=\"texVRGlyphs_LTouch\">",          //NavigateY
            "<sprite name=\"texVRGlyphs_RTouchDown\">",      //Interact
            "<sprite name=\"texVRGlyphs_RTouchUp\">",        //Jump
            "<sprite name=\"texVRGlyphs_RTrigger\">",        //PrimarySkill
            "<sprite name=\"texVRGlyphs_LTrigger\">",        //SecondarySkill
            "<sprite name=\"texVRGlyphs_LGrip\">",           //UtilitySkill
            "<sprite name=\"texVRGlyphs_RGrip\">",           //SpecialSkill
            "<sprite name=\"texVRGlyphs_LTouchPress2\">",    //Equipment
            "<sprite name=\"texVRGlyphs_LTouchPress\">",     //Sprint 
            "<sprite name=\"texVRGlyphs_RMenu\">",           //Ping
            "<sprite name=\"texVRGlyphs_LMenuHold\">",       //Info
            "<sprite name=\"texVRGlyphs_LMenuHold\">",       //HoldInfo
            "<sprite name=\"texVRGlyphs_RTouchDown\">",      //Submit
            "<sprite name=\"texVRGlyphs_RTouchUp\">",        //Cancel 
            "<sprite name=\"texVRGlyphs_LTouchPress2\">",    //Ready
            "<sprite name=\"texVRGlyphs_LTrigger\">",        //TabLeft
            "<sprite name=\"texVRGlyphs_RTrigger\">",        //TabRight
            "<sprite name=\"texVRGlyphs_LGrip\">",           //SubmenuLeft
            "<sprite name=\"texVRGlyphs_RGrip\">",           //SubmenuRight
            "<sprite name=\"texVRGlyphs_LMenu\">",           //Pause 
            "<sprite name=\"texVRGlyphs_RMenu\">",           //RecenterHMD
            "<sprite name=\"texVRGlyphs_Unknown\">",         //ExtraSkill1
            "<sprite name=\"texVRGlyphs_Unknown\">",         //ExtraSkill2
            "<sprite name=\"texVRGlyphs_Unknown\">",         //ExtraSkill3
            "<sprite name=\"texVRGlyphs_Unknown\">",         //ExtraSkill4
            "<sprite name=\"texVRGlyphs_Unknown\">",         //PushToTalk
            "<sprite name=\"texVRGlyphs_Unknown\">",         //BuySkill
            "<sprite name=\"texVRGlyphs_Unknown\">"          //Load
        };

        internal static readonly string[] wmrGlyphs = new string[]
        {
            "<sprite name=\"texVRGlyphs_LTouch\">",          //MoveX
            "<sprite name=\"texVRGlyphs_LTouch\">",          //MoveY
            "<sprite name=\"texVRGlyphs_RStick\">",          //LookX
            "<sprite name=\"texVRGlyphs_RStick\">",          //LookY
            "<sprite name=\"texVRGlyphs_LStick\">",          //NavigateX
            "<sprite name=\"texVRGlyphs_LStick\">",          //NavigateY
            "<sprite name=\"texVRGlyphs_RTouchDown\">",      //Interact
            "<sprite name=\"texVRGlyphs_RTouchUp\">",        //Jump
            "<sprite name=\"texVRGlyphs_RTrigger\">",        //PrimarySkill
            "<sprite name=\"texVRGlyphs_LTrigger\">",        //SecondarySkill
            "<sprite name=\"texVRGlyphs_LGrip\">",           //UtilitySkill
            "<sprite name=\"texVRGlyphs_RGrip\">",           //SpecialSkill
            "<sprite name=\"texVRGlyphs_LTouchPress2\">",    //Equipment
            "<sprite name=\"texVRGlyphs_LTouchPress\">",     //Sprint 
            "<sprite name=\"texVRGlyphs_RMenu\">",           //Ping
            "<sprite name=\"texVRGlyphs_LMenuHold\">",       //Info
            "<sprite name=\"texVRGlyphs_LMenuHold\">",       //HoldInfo
            "<sprite name=\"texVRGlyphs_RTouchDown\">",      //Submit
            "<sprite name=\"texVRGlyphs_RTouchUp\">",        //Cancel 
            "<sprite name=\"texVRGlyphs_LTouchPress2\">",    //Ready
            "<sprite name=\"texVRGlyphs_LTrigger\">",        //TabLeft
            "<sprite name=\"texVRGlyphs_RTrigger\">",        //TabRight
            "<sprite name=\"texVRGlyphs_LGrip\">",           //SubmenuLeft
            "<sprite name=\"texVRGlyphs_RGrip\">",           //SubmenuRight
            "<sprite name=\"texVRGlyphs_LMenu\">",           //Pause 
            "<sprite name=\"texVRGlyphs_RMenu\">",           //RecenterHMD
            "<sprite name=\"texVRGlyphs_Unknown\">",         //ExtraSkill1
            "<sprite name=\"texVRGlyphs_Unknown\">",         //ExtraSkill2
            "<sprite name=\"texVRGlyphs_Unknown\">",         //ExtraSkill3
            "<sprite name=\"texVRGlyphs_Unknown\">",         //ExtraSkill4
            "<sprite name=\"texVRGlyphs_Unknown\">",         //PushToTalk
            "<sprite name=\"texVRGlyphs_Unknown\">",         //BuySkill
            "<sprite name=\"texVRGlyphs_Unknown\">"          //Load
        };

        private static TMP_SpriteAsset glyphsSpriteAsset;

        private static string[] currentGlyphs;

        private static bool isUsingMotionControls;

        internal static void Init()
        {
            On.RoR2.Glyphs.GetGlyphString_MPEventSystem_string_AxisRange_InputSource += GetCustomGlyphString;

            On.RoR2.InputBindingDisplayController.Awake += ApplyInputDisplaySpriteAsset;

            On.RoR2.UI.ContextManager.Awake += ApplyContextSpriteAsset;

            On.RoR2.UI.MPEventSystem.OnLastActiveControllerChanged += ChangedToCustom;

            glyphsSpriteAsset = VRMod.VRAssetBundle.LoadAsset<TMP_SpriteAsset>("sprVRGlyphs");

            if (ModConfig.InitialOculusModeValue)
            {
                currentGlyphs = standardGlyphs;
                return;
            }

            RoR2Application.onUpdate += FindControllerType;
        }

        private static void FindControllerType()
        {
            uint index = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);

            StringBuilder result = new StringBuilder();
            ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
            OpenVR.System.GetStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_ControllerType_String, result, 64, ref error);
            if (error == ETrackedPropertyError.TrackedProp_Success)
            {
                string resultString = result.ToString();

                if (resultString == "") return;

                if (resultString.Contains("vive_controller"))
                    currentGlyphs = viveGlyphs;
                else if (resultString.Contains("holographic_controller"))
                    currentGlyphs = wmrGlyphs;
                else
                    currentGlyphs = standardGlyphs;

                RoR2Application.onUpdate -= FindControllerType;
            }
        }

        private static string GetCustomGlyphString(On.RoR2.Glyphs.orig_GetGlyphString_MPEventSystem_string_AxisRange_InputSource orig, MPEventSystem eventSystem, string actionName, AxisRange axisRange, MPEventSystem.InputSource currentInputSource)
        {
            if (!eventSystem)
            {
                return "???";
            }
            if (isUsingMotionControls)
            {
                Glyphs.resultsList.Clear();
                eventSystem.player.controllers.maps.GetElementMapsWithAction(ControllerType.Custom, Controllers.ControllerID, actionName, false, Glyphs.resultsList);

                if (Glyphs.resultsList.Count() > 0)
                {
                    ActionElementMap displayedMap = Glyphs.resultsList.First();
                    if (displayedMap.elementIdentifierId > 0 && displayedMap.elementIdentifierId < currentGlyphs.Length)
                    {
                        return currentGlyphs[displayedMap.elementIdentifierId];
                    }
                }
            }

            return orig(eventSystem, actionName, axisRange, currentInputSource);
        }

        private static void ChangedToCustom(On.RoR2.UI.MPEventSystem.orig_OnLastActiveControllerChanged orig, MPEventSystem self, Player player, Controller controller)
        {
            if (controller != null && controller.type == ControllerType.Custom)
            {
                isUsingMotionControls = true;
                self.currentInputSource = MPEventSystem.InputSource.MouseAndKeyboard;
                return;
            }
            isUsingMotionControls = false;

            orig(self, player, controller);
        }

        private static void ApplyContextSpriteAsset(On.RoR2.UI.ContextManager.orig_Awake orig, ContextManager self)
        {
            orig(self);

            if (self.glyphTMP && glyphsSpriteAsset)
            {
                ApplySpriteAsset(self.glyphTMP);
            }
        }

        private static void ApplyInputDisplaySpriteAsset(On.RoR2.InputBindingDisplayController.orig_Awake orig, InputBindingDisplayController self)
        {
            orig(self);

            if (glyphsSpriteAsset)
            {
                if (self.guiLabel)
                    ApplySpriteAsset(self.guiLabel);

                if (self.label)
                    ApplySpriteAsset(self.label);
            }
        }

        internal static void ApplySpriteAsset(TextMeshPro tmp)
        {
            tmp.spriteAsset = glyphsSpriteAsset;
        }

        internal static void ApplySpriteAsset(TextMeshProUGUI tmp)
        {
            tmp.spriteAsset = glyphsSpriteAsset;
        }

        internal static string GetGlyph(uint index)
        {
            if (currentGlyphs == null || index >= currentGlyphs.Length) return "";

            return currentGlyphs[index];
        }
    }
}
