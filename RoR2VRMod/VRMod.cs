using BepInEx;
using System.Security;
using System.Security.Permissions;
using UnityEngine.XR;
using System.Collections;
using BepInEx.Logging;
using UnityEngine;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace VRMod
{
    [BepInPlugin("com.DrBibop.VRMod", "VRMod", "2.5.0")]
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
            SettingsAddon.Init();
            UIFixes.Init();
            CameraFixes.Init();
            CutsceneFixes.Init();
            if (ModConfig.InitialMotionControlsValue)
            {
                RoR2.RoR2Application.isModded = true;
                MotionControls.Init();
                MotionControlledAbilities.Init();
            }

            RoR2.RoR2Application.onLoad += () =>
            {
                StartCoroutine(InitVR(ModConfig.InitialOculusModeValue));
                RecenterController.Init();
            };
        }

        private IEnumerator InitVR(bool useOculus)
        {
            XRSettings.LoadDeviceByName(useOculus ? "Oculus" : "OpenVR");
            yield return null;
            if (XRSettings.loadedDeviceName != (useOculus ? "Oculus" : "OpenVR")) yield break;
            
            XRSettings.enabled = true;
            XRDevice.SetTrackingSpaceType(ModConfig.InitialRoomscaleValue ? TrackingSpaceType.RoomScale : TrackingSpaceType.Stationary);

            if (!useOculus)
            {
                Valve.VR.SteamVR_Settings.instance.trackingSpace = ModConfig.InitialRoomscaleValue ? Valve.VR.ETrackingUniverseOrigin.TrackingUniverseStanding : Valve.VR.ETrackingUniverseOrigin.TrackingUniverseSeated;
                Valve.VR.SteamVR.Initialize();
                Valve.VR.SteamVR_Actions.gameplay.Activate();
                Valve.VR.SteamVR_Actions.ui.Activate();
            }
            Controllers.Init();
            ControllerGlyphs.Init();
        }
    }
}