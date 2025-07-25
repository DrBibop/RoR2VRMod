using UnityEngine.XR;

namespace VRMod.Inputs
{
    internal abstract class BaseInput
    {
        internal XRNode xrNode;
        protected InputDevice device;

        internal BaseInput(XRNode deviceNode)
        {
            this.xrNode = deviceNode;
        }

        protected bool CheckDevice()
        {
            if (device == null)
            {
                device = InputDevices.GetDeviceAtXRNode(xrNode);

                return device != null;
            }

            return true;
        }

        internal abstract void UpdateValues(Rewired.CustomController vrControllers);
    }
}
