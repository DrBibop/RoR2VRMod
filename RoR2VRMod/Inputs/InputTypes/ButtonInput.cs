using Rewired;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRMod.Inputs
{
    internal class ButtonInput : BaseInput
    {
        protected InputHelpers.Button button;
        protected int buttonID;

        internal ButtonInput(XRNode deviceNode, InputHelpers.Button button, int buttonID) : base(deviceNode)
        {
            this.button = button;
            this.buttonID = buttonID;
        }

        protected bool PollButtonState()
        {
            if (device.IsPressed(button, out bool value))
                return value;

            return false;
        }

        internal override void UpdateValues(CustomController vrController)
        {
            if (!CheckDevice()) return;

            vrController.SetButtonValueById(buttonID, PollButtonState());
        }
    }
}
