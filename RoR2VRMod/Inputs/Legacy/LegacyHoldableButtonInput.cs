using Rewired;
using UnityEngine;

namespace VRMod.Inputs.Legacy
{
    internal class LegacyHoldableButtonInput : LegacyInput
    {
        private float lastChangeTime = 0f;

        private bool lastState = false;

        private bool released = false;
        private bool held = false;

        internal LegacyHoldableButtonInput(bool leftController, int inputIndex, params int[] inputIDs) : base(leftController, inputIndex, inputIDs) { }

        internal override void UpdateValues(CustomController vrControllers)
        {
            bool state = UnityInputHelper.GetJoystickButtonValueByJoystickIndex(joystickID, inputIndex);

            if (state != lastState)
            {
                lastChangeTime = Time.realtimeSinceStartup;
                lastState = state;
            }

            float timeSinceLastChange = Time.realtimeSinceStartup - lastChangeTime;

            released = !state && !held && timeSinceLastChange < 0.1f;
            held = state && timeSinceLastChange > 0.4f;

            vrControllers.SetButtonValueById(inputIDs[0], released);
            vrControllers.SetButtonValueById(inputIDs[1], held);
        }
    }
}
