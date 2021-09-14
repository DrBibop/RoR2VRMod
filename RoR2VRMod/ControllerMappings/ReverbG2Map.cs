using Rewired;

namespace VRMod.ControllerMappings
{
    internal class ReverbG2Map : GenericVRMap
    {
        internal ReverbG2Map(int leftID, int rightID, string name) : base(leftID, rightID, name, true) { base.holdableMenuButton.buttonId = 2; }

        internal override bool GetLeftPrimary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(leftJoyID, 3);
        }

        internal override bool GetRightPrimary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(rightJoyID, 1);
        }

        internal override bool GetRightSecondary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(rightJoyID, 17) && !base.GetRightJoyPress();
        }
    }
}
