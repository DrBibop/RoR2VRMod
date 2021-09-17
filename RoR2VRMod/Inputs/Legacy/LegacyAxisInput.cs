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
            foreach (int inputID in inputIDs)
            {
                float value = UnityInputHelper.GetJoystickAxisRawValueByJoystickIndex(joystickID, inputIndex);

                if (invert) value = -value;

                vrControllers.SetAxisValueById(inputID, value);
            }
        }
    }
}
