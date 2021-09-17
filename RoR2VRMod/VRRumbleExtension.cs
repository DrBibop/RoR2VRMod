using Rewired;
using Rewired.Interfaces;
using System;
using Valve.VR;

namespace VRMod
{
    internal class VRRumbleExtension : Controller.Extension, IControllerVibrator
    {
        private float motorValue;

        public int vibrationMotorCount => 2;

        internal VRRumbleExtension(IControllerExtensionSource source) : base(source) { }

        public override Controller.Extension Clone()
        {
            return this;
        }

        public float GetVibration(int motorIndex)
        {
            return motorValue;
        }

        public void SetVibration(int motorIndex, float motorLevel)
        {
            SetVibration(motorIndex, motorLevel, 0.1f);
        }

        public void SetVibration(int motorIndex, float motorLevel, float duration)
        {
            SetVibration(motorIndex, motorLevel, duration, false);
        }

        public void SetVibration(int motorIndex, float motorLevel, bool stopOtherMotors)
        {
            SetVibration(motorIndex, motorLevel, 0.1f, stopOtherMotors);
        }

        public void SetVibration(int motorIndex, float motorLevel, float duration, bool stopOtherMotors)
        {
            if (motorIndex > 0) return;

            motorValue = motorLevel;

            SteamVR_Actions.gameplay_Haptic.Execute(0, duration, 100, motorLevel, SteamVR_Input_Sources.LeftHand);
            SteamVR_Actions.gameplay_Haptic.Execute(0, duration, 100, motorLevel, SteamVR_Input_Sources.RightHand);
        }

        public override void SourceUpdated(IControllerExtensionSource source) { }

        public void StopVibration() { }

        public override void UpdateData(UpdateLoopType updateLoop) { }
    }
}
