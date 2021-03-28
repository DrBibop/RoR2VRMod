using BepInEx;
using System.Security;
using System.Security.Permissions;
using UnityEngine.XR;
using System.Collections;
using System;
using R2API.Utils;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: ManualNetworkRegistration]
namespace VRMod
{
    [BepInPlugin("com.DrBibop.VRMod", "VRMod", "1.2.0")]
    public class VRMod : BaseUnityPlugin
    {
        private void Awake()
        {
            ModConfig.Init();
            SettingsAddon.Init();
            UIFixes.Init();
            CameraFixes.Init();

            On.RoR2.RoR2Application.Awake += (orig, self) =>
            {
                orig(self);
                if (XRSettings.loadedDeviceName != (ModConfig.ConfigUseOculus.Value ? "Oculus" : "OpenVR"))
                    StartCoroutine(SetVRDevice(ModConfig.ConfigUseOculus.Value));
            };
        }

        private IEnumerator SetVRDevice(bool useOculus)
        {
            XRSettings.LoadDeviceByName(useOculus ? "Oculus" : "OpenVR");
            yield return null;
            if (XRSettings.loadedDeviceName == (useOculus ? "Oculus" : "OpenVR"))
                XRSettings.enabled = true;
        }
    }
}

namespace R2API.Utils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ManualNetworkRegistrationAttribute : Attribute { }
}