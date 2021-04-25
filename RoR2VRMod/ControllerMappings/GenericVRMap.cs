using Rewired;

namespace VRMod.ControllerMappings
{
    internal class GenericVRMap
    {
        protected int leftJoyID;
        protected int rightJoyID;

        internal bool Ready => leftJoyID != -1 && rightJoyID != -1;

        internal string cachedName { get; private set; }

        private bool subscribedUpdate;

        internal GenericVRMap(int leftID, int rightID, string name)
        {
            SetJoystickIDs(leftID, rightID);
            cachedName = name;
        }

        internal bool CheckJoyNames(string[] joyNames)
        {
            return joyNames[leftJoyID].Contains("Left") && joyNames[rightJoyID].Contains("Right");
        }

        internal void SetJoystickIDs(int leftID, int rightID)
        {
            leftJoyID = leftID;
            rightJoyID = rightID;
        }

        internal virtual float GetLeftJoyX()
        {
            return UnityInputHelper.GetJoystickAxisRawValueByJoystickIndex(leftJoyID, 0);
        }

        internal virtual float GetLeftJoyY()
        {
            return UnityInputHelper.GetJoystickAxisRawValueByJoystickIndex(leftJoyID, 1);
        }

        internal virtual float GetRightJoyX()
        {
            return UnityInputHelper.GetJoystickAxisRawValueByJoystickIndex(rightJoyID, 3);
        }

        internal virtual float GetRightJoyY()
        {
            return UnityInputHelper.GetJoystickAxisRawValueByJoystickIndex(rightJoyID, 4);
        }

        internal virtual float GetLeftTrigger()
        {
            return UnityInputHelper.GetJoystickAxisRawValueByJoystickIndex(leftJoyID, 8);
        }

        internal virtual float GetRightTrigger()
        {
            return UnityInputHelper.GetJoystickAxisRawValueByJoystickIndex(rightJoyID, 9);
        }

        internal virtual float GetLeftGrip()
        {
            return UnityInputHelper.GetJoystickAxisRawValueByJoystickIndex(leftJoyID, 10);
        }

        internal virtual float GetRightGrip()
        {
            return UnityInputHelper.GetJoystickAxisRawValueByJoystickIndex(rightJoyID, 11);
        }

        internal virtual bool GetLeftPrimary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(leftJoyID, 2);
        }

        internal virtual bool GetRightPrimary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(rightJoyID, 0);
        }

        internal virtual bool GetLeftSecondary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(leftJoyID, 3);
        }

        internal virtual bool GetRightSecondary()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(rightJoyID, 1);
        }

        internal virtual bool GetLeftJoyPress()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(leftJoyID, 8);
        }

        internal virtual bool GetRightJoyPress()
        {
            return UnityInputHelper.GetJoystickButtonValueByJoystickIndex(rightJoyID, 9);
        }
    }
}
