using Rewired;
using UnityEngine;
using Valve.VR;

namespace VRMod.Inputs
{
    internal class HoldableButtonInput : ButtonInput
    {
        private SteamVR_Action_Boolean holdableButtonAction;

        internal HoldableButtonInput(SteamVR_Action_Boolean buttonAction, int buttonID, SteamVR_Action_Boolean holdableButtonAction) : base(buttonAction, buttonID)
        {
            this.holdableButtonAction = holdableButtonAction;
        }

        internal override void UpdateValues(CustomController vrController)
        {
            vrController.SetButtonValueById(buttonID, buttonAction.state || (holdableButtonAction.state && Time.realtimeSinceStartup - holdableButtonAction.changedTime > 0.4f));
        }
    }
}
