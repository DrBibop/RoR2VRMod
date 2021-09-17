using Rewired;

namespace VRMod.Inputs.Legacy
{
    internal class LegacyButtonInput : LegacyInput
    {
        internal LegacyButtonInput(bool leftController, int inputIndex, params int[] inputIDs) : base(leftController, inputIndex, inputIDs) { }

        internal override void UpdateValues(CustomController vrControllers)
        {
            foreach (int inputID in inputIDs)
            {
                vrControllers.SetButtonValueById(inputID, UnityInputHelper.GetJoystickButtonValueByJoystickIndex(joystickID, inputIndex));
            }
        }
    }
}
