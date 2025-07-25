using Rewired;

namespace VRMod.Inputs.Legacy
{
    internal class LegacyInput : BaseInput
    {
        private bool leftController;
        protected int inputIndex;
        protected int[] inputIDs;

        protected int joystickID
        {
            get
            {
                return leftController ? Controllers.leftJoystickID : Controllers.rightJoystickID;
            }
        }

        internal LegacyInput(bool leftController, int inputIndex, params int[] inputIDs) : base(UnityEngine.XR.XRNode.CenterEye)
        {
            this.leftController = leftController;
            this.inputIndex = inputIndex;
            this.inputIDs = inputIDs;
        }

        internal override void UpdateValues(CustomController vrControllers) { }
    }
}
