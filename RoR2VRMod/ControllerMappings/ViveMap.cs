using Rewired;
using System.Collections.Generic;
using UnityEngine;

namespace VRMod.ControllerMappings
{
    internal class ViveMap : GenericVRMap
    {
        private DoublePressButton leftTouchpadDoublePress;

        internal ViveMap(int leftID, int rightID, string name) : base(leftID, rightID, name) 
        {
            leftTouchpadDoublePress = new DoublePressButton(leftID, 8);

            mapGlyphs = new Dictionary<int, string>()
                {
                    { 0, "<sprite name=\"texVRGlyphs_LTouch\">" },
                    { 1, "<sprite name=\"texVRGlyphs_LTouch\">" },
                    { 2, "<sprite name=\"texVRGlyphs_RTouchHor\">" },
                    { 3, "<sprite name=\"texVRGlyphs_RTouchHor\">" },
                    { 4, "<sprite name=\"texVRGlyphs_LTrigger\">" },
                    { 5, "<sprite name=\"texVRGlyphs_RTrigger\">" },
                    { 6, "<sprite name=\"texVRGlyphs_LGrip\">" },
                    { 7, "<sprite name=\"texVRGlyphs_RGrip\">" },
                    { 8, "<sprite name=\"texVRGlyphs_LTouchPress2\">" },
                    { 9, "<sprite name=\"texVRGlyphs_RTouchDown\">" },
                    { 10, "<sprite name=\"texVRGlyphs_LMenu\">" },
                    { 11, "<sprite name=\"texVRGlyphs_RTouchUp\">" },
                    { 12, "<sprite name=\"texVRGlyphs_LTouchPress\">" },
                    { 13, "<sprite name=\"texVRGlyphs_RMenu\">" },
                    { 14, "<sprite name=\"texVRGlyphs_LMenuHold\">" }
                };

            base.holdableMenuButton.buttonId = 2;
        }

        internal override float GetRightJoyX()
        {
            float rightJoyX = base.GetRightJoyX();
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(rightJoyID, 9) && Mathf.Abs(rightJoyX) > 0.5f ? rightJoyX : 0;
        }

        //There are too few inputs on these controllers so the top and bottom part of the touchpad are both already used.
        internal override float GetRightJoyY()
        {
            return 0;
        }

        internal override bool GetLeftPrimary()
        {
            return leftTouchpadDoublePress.HasDoublePressed;
        }

        internal override bool GetRightPrimary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(rightJoyID, 9) && Mathf.Abs(GetRightJoyX()) < 0.5f && base.GetRightJoyY() > 0.5f;
        }

        internal override bool GetRightSecondary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(rightJoyID, 9) && Mathf.Abs(GetRightJoyX()) < 0.5f && base.GetRightJoyY() < -0.5f;
        }

        internal override bool GetRightJoyPress()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(rightJoyID, 0);
        }
    }
}
