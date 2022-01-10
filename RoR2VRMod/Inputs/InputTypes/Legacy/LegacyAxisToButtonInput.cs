using Rewired;

namespace VRMod.Inputs.Legacy
{
    internal class LegacyAxisToButtonInput : LegacyButtonInput
    {
        internal LegacyAxisToButtonInput(bool leftController, int inputIndex, params int[] inputIDs) : base(leftController, inputIndex, inputIDs) { }

        internal override void UpdateValues(CustomController vrControllers)
        {
            float value = UnityInputHelper.GetJoystickAxisRawValueByJoystickIndex(joystickID, inputIndex);

            bool state = value > 0.7f;

            if (!state) return;

            foreach (int inputID in inputIDs)
            {
                vrControllers.SetButtonValueById(inputID, state);
            }
        }
    }
}
