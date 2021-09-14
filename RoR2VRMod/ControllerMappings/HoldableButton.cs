using Rewired;
using UnityEngine;

namespace VRMod.ControllerMappings
{
    internal class HoldableButton
    {
        private int joystickId;
        internal int buttonId;

        private float holdDelay;

        private float currHoldTime = 0f;

        private float releaseTime = 0f;

        internal bool HasShortPressed => releaseTime > 0f;

        internal bool IsHolding => currHoldTime > holdDelay;

        internal HoldableButton(int joystickId, int buttonId, float holdDelay = 0.4f)
        {
            this.joystickId = joystickId;
            this.buttonId = buttonId;
            this.holdDelay = holdDelay;

            RoR2.RoR2Application.onUpdate += Update;
        }

        ~HoldableButton()
        {
            RoR2.RoR2Application.onUpdate -= Update;
        }

        private void Update()
        {
            if (releaseTime > 0f)
                releaseTime -= Time.unscaledDeltaTime;

            bool isButtonHeld = UnityInputHelper.GetJoystickButtonValueByJoystickIndex(joystickId, buttonId);

            if (isButtonHeld && currHoldTime < holdDelay)
                currHoldTime += Time.unscaledDeltaTime;
            else if (!isButtonHeld)
            {
                if (0f < currHoldTime && currHoldTime < holdDelay)
                {
                    releaseTime = 0.1f;
                }

                currHoldTime = 0f;
            }
        }
    }
}
