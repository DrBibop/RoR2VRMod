using Rewired;
using UnityEngine;
using Valve.VR;

namespace VRMod.Inputs
{
    internal class ReleaseButtonInput : ButtonInput
    {
        private bool canRelease = false;

        internal ReleaseButtonInput(SteamVR_Action_Boolean buttonAction, int buttonID) : base(buttonAction, buttonID) { }

        internal override void UpdateValues(CustomController vrController)
        {
            bool isReleasing = false;
            if (buttonAction.state)
            {
                canRelease = Time.realtimeSinceStartup - buttonAction.changedTime < 0.4f;
            }
            else
            {
                isReleasing = canRelease && Time.realtimeSinceStartup - buttonAction.changedTime < 0.1f;
            }

            vrController.SetButtonValueById(buttonID, isReleasing);
        }
    }
}
