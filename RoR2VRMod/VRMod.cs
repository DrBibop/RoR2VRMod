using BepInEx;
using System.Security;
using System.Security.Permissions;
using UnityEngine.XR;
using System.Collections;
using System;
using R2API.Utils;
using BepInEx.Logging;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace VRMod
{
    [BepInPlugin("com.DrBibop.VRMod", "VRMod", "1.2.0")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    public class VRMod : BaseUnityPlugin
    {
        internal static ManualLogSource StaticLogger;

        private void Awake()
        {
            StaticLogger = Logger;

            ModConfig.Init();
            Inputs.Init();
            InputBehaviours.Init();
            SettingsAddon.Init();
            UIFixes.Init();
            CameraFixes.Init();

            On.RoR2.RoR2Application.OnLoad += (orig, self) =>
            {
                orig(self);
                StartCoroutine(SetVRDevice(ModConfig.ConfigUseOculus.Value));
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