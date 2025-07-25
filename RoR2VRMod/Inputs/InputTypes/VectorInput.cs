using Rewired;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRMod.Inputs
{
    internal class VectorInput : BaseInput
    {
        protected InputHelpers.Axis2D vector;
        protected int xAxisID;
        protected int yAxisID;

        internal VectorInput(XRNode deviceNode, InputHelpers.Axis2D vector, int xAxisID, int yAxisID) : base(deviceNode)
        {
            this.vector = vector;
            this.xAxisID = xAxisID;
            this.yAxisID = yAxisID;
        }

        protected Vector2 PollVector()
        {
            if (device.TryReadAxis2DValue(vector, out Vector2 value))
                return value;

            return Vector2.zero;
        }

        internal override void UpdateValues(CustomController vrController)
        {
            if (!CheckDevice()) return;

            Vector2 value = PollVector();
            vrController.SetAxisValueById(xAxisID, value.x);
            vrController.SetAxisValueById(yAxisID, value.y);
        }
    }
}
