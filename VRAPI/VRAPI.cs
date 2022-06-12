using BepInEx;
using BepInEx.Logging;
using System;

namespace VRAPI
{
    [BepInPlugin("com.DrBibop.VRAPI", "VRAPI", "1.1.0")]
    [BepInDependency("com.DrBibop.VRMod", BepInDependency.DependencyFlags.SoftDependency)]
    public class VRAPI : BaseUnityPlugin
    {
        internal static ManualLogSource StaticLogger;

        private void Awake()
        {
            StaticLogger = Logger;

            if (VR.enabled && MotionControls.enabled)
            {
                SubscribeToHandPairEvent();
            }
        }

        private void SubscribeToHandPairEvent()
        {
            VRMod.MotionControls.onHandPairSet += (body) =>
            {
                if (MotionControls.onHandPairSet != null)
                {
                    MotionControls.onHandPairSet(body);
                }
            };
        }
    }
}
