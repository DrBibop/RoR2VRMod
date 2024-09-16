using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace VRMod
{
    internal static class RecenterController
    {
        private static GameObject instance;
        public static void Init()
        {
            instance = new GameObject("VRManager");

            instance.AddComponent<MPEventSystemProvider>().fallBackToMainEventSystem = true;

            InputResponse inputResponse = instance.AddComponent<InputResponse>();
            inputResponse.inputActionNames = new string[] { "RecenterHMD" };
            inputResponse.onPress = new UnityEvent();
            inputResponse.onPress.AddListener(Recenter);

            Object.DontDestroyOnLoad(instance);
        }

        private static void Recenter()
        {
            if (!Run.instance || PauseManager.isPaused)
            {
                /*if (ModConfig.InitialOculusModeValue)
                {*/
                    List<XRInputSubsystem> xrSubsystems = new List<XRInputSubsystem>();
                    SubsystemManager.GetInstances(xrSubsystems);

                    foreach (XRInputSubsystem xrSubsystem in xrSubsystems)
                    {
                        xrSubsystem.TryRecenter();
                    }
                /*}
                else
                {
                    Valve.VR.OpenVR.Chaperone.ResetZeroPose(ModConfig.InitialRoomscaleValue ? Valve.VR.ETrackingUniverseOrigin.TrackingUniverseStanding : Valve.VR.ETrackingUniverseOrigin.TrackingUniverseSeated);
                }*/
                    
            }
        }
    }
}
