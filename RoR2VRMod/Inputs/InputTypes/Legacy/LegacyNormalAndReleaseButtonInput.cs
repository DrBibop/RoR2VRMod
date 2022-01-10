using Rewired;
using UnityEngine;

namespace VRMod.Inputs.Legacy
{
    internal class LegacyNormalAndReleaseButtonInput : LegacyInput
    {
        private float lastChangeTime = 0f;

        private bool lastState = false;
        private bool canRelease = false;

        internal LegacyNormalAndReleaseButtonInput(bool leftController, int inputIndex, params int[] inputIDs) : base(leftController, inputIndex, inputIDs) { }

        internal override void UpdateValues(CustomController vrControllers)
        {
            bool state = UnityInputHelper.GetJoystickButtonValueByJoystickIndex(joystickID, inputIndex);

            bool isReleasing = false;

            if (state != lastState)
            {
                lastChangeTime = Time.realtimeSinceStartup;
                lastState = state;
            }

            float timeSinceLastChange = Time.realtimeSinceStartup - lastChangeTime;

            if (state)
            {
                canRelease = timeSinceLastChange < 0.4f;
            }
            else
            {
                isReleasing = canRelease && timeSinceLastChange < 0.1f;
            }

            if (state)
                vrControllers.SetButtonValueById(inputIDs[0], state);

            if (isReleasing)
                vrControllers.SetButtonValueById(inputIDs[1], isReleasing);
        }
    }
}
