using Rewired;

namespace VRMod.Inputs.Legacy
{
    internal class LegacyAxisInput : LegacyInput
    {
        private bool invert;

        internal LegacyAxisInput(bool leftController, int inputIndex, bool invert, params int[] inputIDs) : base(leftController, inputIndex, inputIDs)
        {
            this.invert = invert;
        }

        internal override void UpdateValues(CustomController vrControllers)
        {
            float value = UnityInputHelper.GetJoystickAxisRawValueByJoystickIndex(joystickID, inputIndex);

            if (invert) value = -value;

            foreach (int inputID in inputIDs)
            {
                vrControllers.SetAxisValueById(inputID, value);
            }
        }
    }
}
