using Rewired;

namespace VRMod.ControllerMappings
{
    internal class GenericOpenVRMap : GenericVRMap
    {
        internal GenericOpenVRMap(int leftID, int rightID, string name) : base(leftID, rightID, name) { }

        internal override bool GetLeftPrimary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(leftJoyID, 3);
        }

        internal override bool GetRightPrimary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(rightJoyID, 1);
        }

        internal override bool GetLeftSecondary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(leftJoyID, 2);
        }

        internal override bool GetRightSecondary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(rightJoyID, 0);
        }
    }
}
