using Rewired;
using System.Collections.Generic;

namespace VRMod.ControllerMappings
{
    class TrackpadWMRMap : GenericVRMap
    {
        private DoublePressButton leftTouchpadDoublePress;

        internal TrackpadWMRMap(int leftID, int rightID, string name) : base(leftID, rightID, name)
        {
            mapGlyphs = new Dictionary<int, string>()
                {
                    { 0, "<sprite name=\"texVRGlyphs_LTouch\">" },
                    { 1, "<sprite name=\"texVRGlyphs_LTouch\">" },
                    { 2, "<sprite name=\"texVRGlyphs_RStick\">" },
                    { 3, "<sprite name=\"texVRGlyphs_RStick\">" },
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

            leftTouchpadDoublePress = new DoublePressButton(leftID, 8);

            base.holdableMenuButton.buttonId = 2;
        }

        internal override float GetLeftJoyX()
        {
            return UnityInputHelper.GetJoystickAxisRawValueByJoystickIndex(leftJoyID, 16);
        }

        internal override float GetLeftJoyY()
        {
            return UnityInputHelper.GetJoystickAxisRawValueByJoystickIndex(leftJoyID, 17);
        }

        internal override bool GetLeftPrimary()
        {
            return leftTouchpadDoublePress.HasDoublePressed;
        }

        internal override bool GetRightPrimary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(rightJoyID, 9) && UnityInputHelper.GetJoystickAxisRawValueByJoystickIndex(rightJoyID, 19) > 0.5f;
        }

        internal override bool GetRightSecondary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(rightJoyID, 9) && UnityInputHelper.GetJoystickAxisRawValueByJoystickIndex(rightJoyID, 19) < -0.5f;
        }

        internal override bool GetRightJoyPress()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(leftJoyID, 0);
        }
    }
}
