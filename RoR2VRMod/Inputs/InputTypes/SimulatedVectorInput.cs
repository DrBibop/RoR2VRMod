using Rewired;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRMod.Inputs
{
    internal class SimulatedVectorInput : VectorInput
    {
        private InputHelpers.Button upButton; 
        private InputHelpers.Button rightButton; 
        private InputHelpers.Button downButton; 
        private InputHelpers.Button leftButton;

        internal SimulatedVectorInput(XRNode deviceNode, InputHelpers.Axis2D vector, int xAxisID, int yAxisID, InputHelpers.Button upButton, InputHelpers.Button rightButton, InputHelpers.Button downButton, InputHelpers.Button leftButton) : base(deviceNode, vector, xAxisID, yAxisID)
        {
            this.upButton = upButton; 
            this.rightButton = rightButton;
            this.downButton = downButton; 
            this.leftButton = leftButton;
        }

        internal override void UpdateValues(CustomController vrController)
        {
            if (!CheckDevice()) return;

            Vector2 result = PollVector();

            if (upButton != InputHelpers.Button.None && device.IsPressed(upButton, out bool upValue) && upValue)
                result.y += 1;

            if (rightButton != InputHelpers.Button.None && device.IsPressed(rightButton, out bool rightValue) && rightValue)
                result.x += 1;

            if (downButton != InputHelpers.Button.None && device.IsPressed(downButton, out bool downValue) && downValue)
                result.y -= 1;

            if (leftButton != InputHelpers.Button.None && device.IsPressed(leftButton, out bool leftValue) && leftValue)
                result.x -= 1;

            result.x = Mathf.Clamp(result.x, -1, 1);
            result.y = Mathf.Clamp(result.y, -1, 1);

            vrController.SetAxisValueById(xAxisID, result.x);
            vrController.SetAxisValueById(yAxisID, result.y);
        }
    }
}
