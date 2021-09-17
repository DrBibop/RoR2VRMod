using System;
using System.Collections.Generic;
using System.Text;

namespace VRMod
{
    internal static class ControllerGlyphs
    {
        internal static string[] standardGlyphs = new string[]
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
            "<sprite name=\"texVRGlyphs_RStickPress\">"      //RecenterHMD
        };

        internal static string[] viveGlyphs = new string[]
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
            "<sprite name=\"texVRGlyphs_RMenu\">"            //RecenterHMD
        };

        internal static string[] wmrGlyphs = new string[]
        {
            "<sprite name=\"texVRGlyphs_LTouch\">",          //MoveX
            "<sprite name=\"texVRGlyphs_LTouch\">",          //MoveY
            "<sprite name=\"texVRGlyphs_RStick\">",       //LookX
            "<sprite name=\"texVRGlyphs_RStick\">",       //LookY
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
            "<sprite name=\"texVRGlyphs_RMenu\">"            //RecenterHMD
        };
    }
}
