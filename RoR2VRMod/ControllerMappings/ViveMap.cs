using Rewired;
using UnityEngine;

namespace VRMod.ControllerMappings
{
    internal class ViveMap : GenericVRMap
    {
        private DoublePressButton leftTouchpadDoublePress;

        internal ViveMap(int leftID, int rightID, string name) : base(leftID, rightID, name) 
        {
            leftTouchpadDoublePress = new DoublePressButton(leftID, 8);
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

        internal override bool GetLeftSecondary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(leftJoyID, 2);
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
