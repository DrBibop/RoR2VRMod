using BepInEx;
using System.Security;
using System.Security.Permissions;
using UnityEngine.XR;
using System.Collections;
using System;
using BepInEx.Logging;
using R2API.Utils;
using RoR2;
using UnityEngine;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: ManualNetworkRegistration]
namespace VRMod
{
    [BepInPlugin("com.DrBibop.VRMod", "VRMod", "1.3.0")]
    public class VRMod : BaseUnityPlugin
    {
        internal static ManualLogSource StaticLogger;

        internal static AssetBundle VRAssetBundle;

        private void Awake()
        {
            StaticLogger = Logger;

            VRAssetBundle = AssetBundle.LoadFromMemory(Properties.Resources.vrmodassets);

            ModConfig.Init();
            ActionAddons.Init();
            VRManager.Init();
            SettingsAddon.Init();
            UIFixes.Init();
            CameraFixes.Init();
            if (ModConfig.UseMotionControls.Value)
            {
                RoR2.RoR2Application.isModded = true;
                MotionControls.Init();
                MotionControlledAbilities.Init();
            }

            RoR2.RoR2Application.onLoad += () =>
            {
                StartCoroutine(SetVRDevice(ModConfig.ConfigUseOculus.Value));
                VRManager.Init();
                Controllers.Init();
            };
        }

        private IEnumerator SetVRDevice(bool useOculus)
        {
            XRSettings.LoadDeviceByName(useOculus ? "Oculus" : "OpenVR");
            yield return null;
            if (XRSettings.loadedDeviceName == (useOculus ? "Oculus" : "OpenVR"))
            {
                XRSettings.enabled = true;
                XRDevice.SetTrackingSpaceType(TrackingSpaceType.Stationary);
            }
        }
    }
}

namespace R2API.Utils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ManualNetworkRegistrationAttribute : Attribute { }
}