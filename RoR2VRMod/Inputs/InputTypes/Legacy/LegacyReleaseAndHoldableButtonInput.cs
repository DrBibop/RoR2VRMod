using Rewired;
using UnityEngine;

namespace VRMod.Inputs.Legacy
{
    internal class LegacyReleaseAndHoldableButtonInput : LegacyInput
    {
        private float lastChangeTime = 0f;

        private bool lastState = false;
        private bool canRelease = false;

        internal LegacyReleaseAndHoldableButtonInput(bool leftController, int inputIndex, params int[] inputIDs) : base(leftController, inputIndex, inputIDs) { }

        internal override void UpdateValues(CustomController vrControllers)
        {
            bool state = UnityInputHelper.GetJoystickButtonValueByJoystickIndex(joystickID, inputIndex);

            bool isReleasing = false;
            bool isHolding = false;
            
            if (state != lastState)
            {
                lastChangeTime = Time.realtimeSinceStartup;
                lastState = state;
            }

            float timeSinceLastChange = Time.realtimeSinceStartup - lastChangeTime;

            if (state)
            {
                if (timeSinceLastChange < 0.4f)
                    canRelease = true;
                else
                {
                    isHolding = true;
                    canRelease = false;
                }
            }
            else
            {
                isReleasing = canRelease && timeSinceLastChange < 0.1f;
            }

            if (isReleasing)
                vrControllers.SetButtonValueById(inputIDs[0], isReleasing);

            if (isHolding)
                vrControllers.SetButtonValueById(inputIDs[1], isHolding);
        }
    }
}
