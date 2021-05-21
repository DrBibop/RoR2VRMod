using Rewired;
using System.Collections.Generic;

namespace VRMod.ControllerMappings
{
    internal class GenericVRMap
    {
        protected int leftJoyID;
        protected int rightJoyID;

        internal bool Ready => leftJoyID != -1 && rightJoyID != -1;

        internal Dictionary<int, string> mapGlyphs;

        internal string cachedName { get; private set; }

        internal GenericVRMap(int leftID, int rightID, string name, bool useDefaultGlyphs = false)
        {
            if (useDefaultGlyphs)
            {
                mapGlyphs = new Dictionary<int, string>()
                {
                    { 0, "<sprite name=\"texVRGlyphs_LStick\">" },
                    { 1, "<sprite name=\"texVRGlyphs_LStick\">" },
                    { 2, "<sprite name=\"texVRGlyphs_RStick\">" },
                    { 3, "<sprite name=\"texVRGlyphs_RStick\">" },
                    { 4, "<sprite name=\"texVRGlyphs_LTrigger\">" },
                    { 5, "<sprite name=\"texVRGlyphs_RTrigger\">" },
                    { 6, "<sprite name=\"texVRGlyphs_LGrip\">" },
                    { 7, "<sprite name=\"texVRGlyphs_RGrip\">" },
                    { 8, "<sprite name=\"texVRGlyphs_LPrimary\">" },
                    { 9, "<sprite name=\"texVRGlyphs_RPrimary\">" },
                    { 10, "<sprite name=\"texVRGlyphs_LSecondary\">" },
                    { 11, "<sprite name=\"texVRGlyphs_RSecondary\">" },
                    { 12, "<sprite name=\"texVRGlyphs_LStickPress\">" },
                    { 13, "<sprite name=\"texVRGlyphs_RStickPress\">" }
                };
            }

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
