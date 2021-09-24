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

        internal override string BindingString => "";

        internal override bool IsBound => true;

        internal LegacyInput(bool leftController, int inputIndex, params int[] inputIDs)
        {
            this.leftController = leftController;
            this.inputIndex = inputIndex;
            this.inputIDs = inputIDs;
        }

        internal override void UpdateValues(CustomController vrControllers) { }
    }
}
