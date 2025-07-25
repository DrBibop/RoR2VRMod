using Rewired;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRMod.Inputs
{
    internal class ReleaseButtonInput : ButtonInput
    {
        private bool canRelease = false;
        private bool prevState;
        private float pressTime;

        internal ReleaseButtonInput(XRNode deviceNode, InputHelpers.Button button, int buttonID) : base(deviceNode, button, buttonID) { }

        internal override void UpdateValues(CustomController vrController)
        {
            if (!CheckDevice()) return;

            bool isReleasing = false;
            bool state = PollButtonState();
            if (state)
            {
                if (!prevState)
                    pressTime = Time.realtimeSinceStartup;

                canRelease = Time.realtimeSinceStartup - pressTime < 0.4f;
            }
            else
            {
                isReleasing = canRelease && Time.realtimeSinceStartup - pressTime < 0.1f;
            }

            vrController.SetButtonValueById(buttonID, isReleasing);

            prevState = state;
        }
    }
}
