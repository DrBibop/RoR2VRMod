using Rewired;
using UnityEngine;
using Valve.VR;

namespace VRMod.Inputs
{
    internal class SimulatedVectorInput : VectorInput
    {
        private SteamVR_Action_Boolean upButton; 
        private SteamVR_Action_Boolean rightButton; 
        private SteamVR_Action_Boolean downButton; 
        private SteamVR_Action_Boolean leftButton;

        internal SimulatedVectorInput(SteamVR_Action_Vector2 vectorAction, int xAxisID, int yAxisID, SteamVR_Action_Boolean upButton, SteamVR_Action_Boolean rightButton, SteamVR_Action_Boolean downButton, SteamVR_Action_Boolean leftButton) : base(vectorAction, xAxisID, yAxisID)
        {
            this.upButton = upButton; 
            this.rightButton = rightButton;
            this.downButton = downButton; 
            this.leftButton = leftButton;
        }

        internal override void UpdateValues(CustomController vrController)
        {
            Vector2 result = Vector2.zero;
            if (vectorAction != null)
                result = vectorAction.axis;

            if (upButton != null && upButton.state)
                result.y += 1;

            if (rightButton != null && rightButton.state)
                result.x += 1;

            if (downButton != null && downButton.state)
                result.y -= 1;

            if (leftButton != null && leftButton.state)
                result.x -= 1;

            vrController.SetAxisValueById(xAxisID, result.x);
            vrController.SetAxisValueById(yAxisID, result.y);
        }
    }
}
