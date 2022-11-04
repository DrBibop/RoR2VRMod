using BepInEx;
using System.Security;
using System.Security.Permissions;
using UnityEngine.XR;
using System.Collections;
using BepInEx.Logging;
using UnityEngine;
using Valve.VR;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace VRMod
{
    [BepInPlugin("com.DrBibop.VRMod", "VRMod", "2.8.2")]
    [BepInDependency("com.Moffein.BanditTweaks", BepInDependency.DependencyFlags.SoftDependency)]
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
            FocusChecker.Init();
            if (ModConfig.InitialMotionControlsValue)
            {
                RoR2.RoR2Application.isModded = true;
                MotionControls.Init();
                MotionControlledAbilities.Init();
                EntityStateAnimationParameter.Init();
            }

            RoR2.RoR2Application.onLoad += () =>
            {
                StartCoroutine(InitVR(ModConfig.InitialOculusModeValue));
                RecenterController.Init();
                UIPointer.Init();
                Haptics.HapticsManager.Init();
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
                SteamVR_Settings.instance.trackingSpace = ModConfig.InitialRoomscaleValue ? ETrackingUniverseOrigin.TrackingUniverseStanding : ETrackingUniverseOrigin.TrackingUniverseSeated;
                SteamVR_Settings.instance.pauseGameWhenDashboardVisible = false;
                SteamVR_Settings.instance.lockPhysicsUpdateRateToRenderFrequency = false;
                SteamVR.Initialize();
                SteamVR_Actions.gameplay.Activate();
                SteamVR_Actions.ui.Activate();
            }
            Controllers.Init();
            ControllerGlyphs.Init();
        }
    }
}