using Rewired;
using UnityEngine;

namespace VRMod.ControllerMappings
{
    internal class DoublePressButton
    {
        private int joystickId;
        private int buttonId;

        private float doublePressTimer = 0;

        private bool wasButtonHeld = false;

        private bool isButtonHeld = false;

        private bool hasDoublePressed = false;

        internal bool HasDoublePressed => hasDoublePressed;

        internal DoublePressButton(int joystickId, int buttonId)
        {
            this.joystickId = joystickId;
            this.buttonId = buttonId;

            RoR2.RoR2Application.onUpdate += Update;
        }

        ~DoublePressButton()
        {
            RoR2.RoR2Application.onUpdate -= Update;
        }

        internal void Update()
        {
            wasButtonHeld = isButtonHeld;
            isButtonHeld = UnityInputHelper.GetJoystickButtonValueByJoystickIndex(joystickId, buttonId);

            bool justPressed = isButtonHeld && !wasButtonHeld;

            if (hasDoublePressed)
            {
                if (!isButtonHeld)
                {
                    hasDoublePressed = false;
                    doublePressTimer = 0;
                }
                return;
            }

            if (doublePressTimer > 0 && doublePressTimer <= 0.3f)
            {
                doublePressTimer += Time.deltaTime;

                if (justPressed)
                    hasDoublePressed = true;
            }
            else if (justPressed)
                doublePressTimer = Time.deltaTime;

            return;
        }
    }
}
