using Rewired;
using UnityEngine;

namespace VRMod.ControllerMappings
{
    class TrackpadWMRMap : GenericVRMap
    {
        private DoublePressButton leftTouchpadDoublePress;

        internal TrackpadWMRMap(int leftID, int rightID, string name) : base(leftID, rightID, name)
        {
            leftTouchpadDoublePress = new DoublePressButton(leftID, 8);
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

        internal override bool GetLeftSecondary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(leftJoyID, 2);
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
